using System;
using System.Threading;
using EasySave.Models;
using EasySave.Strategies;
using EasySave.StateManagement;
using EasySave.Services;
using Tool.Utils;

namespace EasySave.ExecutionManagement
{
    public class BackupExecutionManager
    {
        private readonly List<BackupJob> _listBackupJob = new();

        /// <summary>
        /// Semaphore shared across all parallel jobs.
        /// Ensures only one large file (> MaxParallelFileSizeKo) is transferred at a time.
        /// </summary>
        private readonly SemaphoreSlim _largeFileSemaphore = new(1, 1);

        /// <summary>
        /// Counter shared across all parallel jobs.
        /// Tracks how many priority files are still pending globally.
        /// Non-priority transfers must wait until this reaches zero.
        /// </summary>
        private int _pendingPriorityFiles = 0;

        public void createBackupJob(string name, string sourcePath, string destinationPath, BackupTypes type)
        {
            var job = new BackupJob
            {
                name = name,
                sourcePath = sourcePath,
                destinationPath = destinationPath,
                type = type,
                progressObserver = new BackupProgressObserver()
            };

            _listBackupJob.Add(job);

            var stateManager = new BackupStateManager(job);

            int jobId = _listBackupJob.Count - 1;
            Console.WriteLine(_listBackupJob[jobId].ToString());
        }

        public List<BackupJob> getBackupJobList() => _listBackupJob;

        public BackupJob getJobById(int jobId)
        {
            if (jobId < 0 || jobId >= _listBackupJob.Count)
                return null;

            return _listBackupJob[jobId];
        }

        public async Task ExecuteJob(int jobId)
        {
            var job = getJobById(jobId);
            if (job == null)
            {
                Console.WriteLine($"Job {jobId} not found!");
                return;
            }

            if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
            {
                Console.WriteLine($"Cannot start {job.name}: business software is running");
                LogService.Instance.LogBusinessSoftwareEvent(job.name,
                    "Backup launch blocked - Business software is running");
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    Console.WriteLine($"Starting job {job.name}");

                    // Pass shared controls directly via constructor
                    IBackupStrategy strategy = BackupStrategyFactory.CreateStrategy(
                        job.type,
                        _largeFileSemaphore,
                        ref _pendingPriorityFiles
                    );

                    strategy.Execute(job);

                    job.status.Status = BackupStateResum.ON;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"Job {job.name} completed successfully!");
                }
                catch (OperationCanceledException ex)
                {
                    job.status.Status = BackupStateResum.ERROR;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"Job {job.name} stopped: {ex.Message}");
                }
                catch (Exception ex)
                {
                    job.status.Status = BackupStateResum.ERROR;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"Error in job {job.name}: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Executes all jobs in parallel.
        /// Both shared controls (_largeFileSemaphore and _pendingPriorityFiles)
        /// are passed to each strategy to enforce the two parallel transfer rules.
        /// </summary>
        public async Task ExecuteAllJob()
        {
            var tasks = new List<Task>();

            for (int i = 0; i < _listBackupJob.Count; i++)
                tasks.Add(ExecuteJob(i));

            await Task.WhenAll(tasks);
        }
    }
}
