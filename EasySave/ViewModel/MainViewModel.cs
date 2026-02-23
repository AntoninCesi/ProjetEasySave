using EasySave.ExecutionManagement;
using EasySave.Messaging;
using EasySave.Models;
using EasySave.StateManagement; 
using System;
using System.Collections.Generic;
using System.Linq;
using Tool.Utils;

namespace EasySave.ViewModels
{
    public class MainViewModel
    {
        private BackupExecutionManager backupManager = new BackupExecutionManager();

        // Map des jobs vers leurs BackupStateManager
        private readonly Dictionary<int, BackupStateManager> _stateManagers = new();

        public event EventHandler<Message> OnMessageReceived;

        public MainViewModel(string[] args)
        {
            if (args.Length > 0)
            {
                HandleCommandLineArgs(args[0]);
            }
        }

        public void HandleCommandLineArgs(string arg)
        {


                if (arg.Length == 3)
                {
                    int fId = (int)char.GetNumericValue(arg[0]);
                    char middle = arg[1];
                    int lId = (int)char.GetNumericValue(arg[2]);

                    if (middle == '-')
                    {
                        for (int i = fId; i <= lId; i++) startBackupJob(i);
                    }
                    else if (middle == ',')
                    {
                        startBackupJob(fId);
                        startBackupJob(lId);
                    }
                }
                else if (int.TryParse(arg, out int id))
                {
                    startBackupJob(id);
                }
            
        }
            

        private void NotifyView(MessageType type, params object[] args)
        {
            OnMessageReceived?.Invoke(this, new Message(type, args));
        }

        // Expose the job list to the View for validation purposes
        public List<BackupJob> getJobs() => backupManager.getBackupJobList();

        private void startBackupJob(int jobId)
        {
            var jobs = getJobs();
            if (jobId >= 0 && jobId < jobs.Count)
            {
                // Notify the View that the process has started
                NotifyView(MessageType.BackupStarted, jobs[jobId].name);

                // Execute the business logic
                backupManager.ExecuteJob(jobId);

                // You can add a success notification here if needed
                // NotifyView(MessageType.BackupFinished);
            }
            else
            {
                NotifyView(MessageType.NoJobAvailable);
            }
        }


        // Placeholder for job existence check
        public bool JobExists(string jobName)
        {
            return getJobs().Any(job => job.name == jobName);
        }

        public string[] getJobName()
        {
            var jobNames = new List<string>();
            foreach (var job in getJobs())
            {

                jobNames.Add(job.name);
            }
            return jobNames.ToArray();
        }

        public void createBackupJob(string jobName, string sourcePath, string destPath, BackupTypes type)
        {
            backupManager.createBackupJob(jobName, sourcePath, destPath, type);

            // Créer le BackupStateManager correspondant pour ce job
            int jobId = backupManager.getBackupJobList().Count - 1;
            var job = backupManager.getBackupJobList()[jobId];
            _stateManagers[jobId] = new BackupStateManager(job);
        }

        public bool thereJobs()
        {
            return getJobs().Count > 0;
        }
        public BackupStateManager getJobStateManager(int jobId)
        {
            if (_stateManagers.ContainsKey(jobId))
                return _stateManagers[jobId];
            return null;
        }
    }
}