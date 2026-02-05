using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using EasySave.Models;
using EasySave.Strategies;
using EasySave.StateManagement;
using EasySave.Services;

namespace EasySave.ExecutionManagement
{
    public class BackupExecutionManager
    {
        private readonly BackupStateManager _stateManager;
        private readonly LogService _logService;

        public event EventHandler<EasySave.Models.FileInfo> OnFileProcess;

        public BackupExecutionManager(BackupStateManager stateManager, LogService logService)
        {
            _stateManager = stateManager;
            _logService = logService;
        }

        public void ExecuteJob(BackupJob job)
        {
            Stopwatch jobStopwatch = Stopwatch.StartNew();

            try
            {
                // Calculate TotalFiles and TotalSize
                var dir = new DirectoryInfo(job.SourcePath);
                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException($"Source path not found: {job.SourcePath}");
                }

                job.TotalFiles = CountAllFiles(dir);
                job.TotalSize = CalculateTotalSize(dir);

                // Choose strategy
                IBackupStrategy strategy = job.Type == BackupType.FULL
                    ? (IBackupStrategy)new FullBackupStrategy()
                    : (IBackupStrategy)new DifferentialBackupStrategy();

                job.Status = BackupStatus.ACTIVE;
                int processedFiles = 0;

                // Execute with callback
                strategy.Execute(job, (fileData, remaining) => {
                    // Measure file transfer time
                    Stopwatch fileStopwatch = Stopwatch.StartNew();

                    processedFiles++;
                    job.Progress = job.TotalFiles > 0 ? (processedFiles * 100) / job.TotalFiles : 0;

                    fileStopwatch.Stop();
                    long transferTime = fileStopwatch.ElapsedMilliseconds;

                    // 1. Trigger event
                    OnFileProcess?.Invoke(this, fileData);

                    // 2. Log operation
                    _logService.WriteLog(
                        job.Name,
                        fileData.filePath,
                        Path.Combine(job.DestinationPath, fileData.fileName),
                        fileData.fileSize,
                        transferTime
                    );

                    // 3. Update state
                    NotifyState(job, fileData, remaining);
                });

                job.Status = BackupStatus.FINISHED;
                job.Progress = 100;

                jobStopwatch.Stop();
                Console.WriteLine($"\n✓ Backup '{job.Name}' completed in {jobStopwatch.ElapsedMilliseconds}ms");

                // Final update
                _stateManager.Update(new BackupState
                {
                    Name = job.Name,
                    LastActionTimestamp = DateTime.Now,
                    Status = job.Status,
                    TotalFiles = job.TotalFiles,
                    TotalSize = job.TotalSize,
                    RemainingFiles = 0,
                    RemainingSize = 0,
                    Progress = 100,
                    SourcePath = job.SourcePath,
                    DestinationPath = job.DestinationPath
                });
            }
            catch (Exception ex)
            {
                job.Status = BackupStatus.ERROR;
                Console.WriteLine($"\n✗ Error during backup '{job.Name}': {ex.Message}");

                // Log error (negative time)
                _logService.WriteLog(job.Name, job.SourcePath, job.DestinationPath, 0, -1);

                _stateManager.Update(new BackupState
                {
                    Name = job.Name,
                    LastActionTimestamp = DateTime.Now,
                    Status = BackupStatus.ERROR,
                    SourcePath = job.SourcePath,
                    DestinationPath = job.DestinationPath
                });
            }
        }

        private int CountAllFiles(DirectoryInfo dir)
        {
            int count = dir.GetFiles().Length;
            foreach (var subDir in dir.GetDirectories())
            {
                count += CountAllFiles(subDir);
            }
            return count;
        }

        private long CalculateTotalSize(DirectoryInfo dir)
        {
            long size = dir.GetFiles().Sum(f => f.Length);
            foreach (var subDir in dir.GetDirectories())
            {
                size += CalculateTotalSize(subDir);
            }
            return size;
        }

        private void NotifyState(BackupJob job, EasySave.Models.FileInfo file, int remaining)
        {
            long processedSize = job.TotalSize - (remaining > 0 ? (job.TotalSize / job.TotalFiles) * remaining : 0);
            long remainingSize = job.TotalSize - processedSize;

            _stateManager.Update(new BackupState
            {
                Name = job.Name,
                LastActionTimestamp = DateTime.Now,
                Status = job.Status,
                TotalFiles = job.TotalFiles,
                TotalSize = job.TotalSize,
                RemainingFiles = remaining,
                RemainingSize = remainingSize,
                Progress = job.Progress,
                SourcePath = file.filePath,
                DestinationPath = job.DestinationPath
            });
        }
    }
}