using System;
using System.Collections.Generic;
using System.Linq;
using EasySave.ExecutionManagement;
using EasySave.View;
using EasySave.Models;
using EasySave.StateManagement; // <-- ajouter pour BackupStateManager
using Tool.Utils;

namespace EasySave.ViewModels
{
    public class Controller
    {
        private BackupExecutionManager backupManager = new BackupExecutionManager();

        // Map des jobs vers leurs BackupStateManager
        private readonly Dictionary<int, BackupStateManager> _stateManagers = new();

        public Controller(string[] args)
        {
            if (args.Length == 0)
            {
                this.createUI().showMenu();
            }
            else if (args[0].Length == 3)
            {
                char first = args[0][0];
                char middle = args[0][1];
                char last = args[0][2];

                if (char.IsDigit(first) && char.IsDigit(last))
                {
                    int fId = (int)char.GetNumericValue(first);
                    int lId = (int)char.GetNumericValue(last);

                    if (getJobs().Count >= fId && getJobs().Count >= lId)
                    {
                        if (middle == '-')
                        {
                            //lancer la sauvegarde de fId et lId
                        }
                        else if (middle == ',')
                        {
                            // faire un for qui lance les sauvegard de fId et lId
                        }
                    }
                }
<<<<<<< HEAD
<<<<<<< Updated upstream
                else
=======
            }
        }

        // Expose the job list to the View for validation purposes
        public List<BackupJob> getJobs() => backupManager.getBackupJobList();

        public void startBackupJob(int jobId)
        {
            if (getJobs().Count > jobId)
            {
                backupManager.ExecuteJob(jobId);
            }
            //else { displayMessage("y a pas de job"); }
        }

        public ConsoleUI createUI()
        {
            return new ConsoleUI(this);
        }

        public void displayMessage(string message) { Console.WriteLine(message); }
        public void getUIMessage(string message) { }

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
                if (job.status.Status == BackupStateResum.INACTIVE || job.status.Status == BackupStateResum.ERROR)
>>>>>>> Stashed changes
                {
                    this.displayMessage("Error: input must be in the form '3-2' or '4,9'.");
                }
=======
>>>>>>> origin/dev
            }
        }

        // Expose the job list to the View for validation purposes
        public List<BackupJob> getJobs() => backupManager.getBackupJobList();

        public void startBackupJob(int jobId)
        {
            if (getJobs().Count > jobId)
            {
                backupManager.ExecuteJob(jobId);
            }
            //else { displayMessage("y a pas de job"); }
        }

        public ConsoleUI createUI()
        {
            return new ConsoleUI(this);
        }

        public void displayMessage(string message) { Console.WriteLine(message); }
        public void getUIMessage(string message) { }

        // Placeholder for job existence check
        public bool JobExists(string jobName)
        {
<<<<<<< HEAD
<<<<<<< Updated upstream
            return true; // placeholder 
        }

        //private BackupJob[] getBackupJobById(int[] jobId) { }

        public void createBackupJob( string jobName, string sourcePath, string destPath, BackupTypes type) {

=======
>>>>>>> Stashed changes
=======
            return getJobs().Any(job => job.name == jobName);
        }

        public string[] getJobName()
        {
            var jobNames = new List<string>();
            foreach (var job in getJobs())
            {
                if (job.status.Status == BackupStateResum.INACTIVE || job.status.Status == BackupStateResum.ERROR)
                {
                    jobNames.Add(job.name);
                }
            }
            return jobNames.ToArray();
        }

        public void createBackupJob(string jobName, string sourcePath, string destPath, BackupTypes type)
        {
>>>>>>> origin/dev
            backupManager.createBackupJob(jobName, sourcePath, destPath, type);

            // Créer le BackupStateManager correspondant pour ce job
            int jobId = backupManager.getBackupJobList().Count - 1;
            var job = backupManager.getBackupJobList()[jobId];
            _stateManagers[jobId] = new BackupStateManager(job);
        }

        public BackupStateManager getJobStateManager(int jobId)
        {
            if (_stateManagers.ContainsKey(jobId))
                return _stateManagers[jobId];
            return null;
        }
    }
}