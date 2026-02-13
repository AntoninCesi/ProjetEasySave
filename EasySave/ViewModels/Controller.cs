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
            return getJobs().Select(job => job.name).ToArray();
        }

        public void createBackupJob(string jobName, string sourcePath, string destPath, BackupTypes type)
        {
            backupManager.createBackupJob(jobName, sourcePath, destPath, type);

            // Créer le BackupStateManager correspondant pour ce job
            int jobId = backupManager.getBackupJobList().Count - 1;
            var job = backupManager.getBackupJobList()[jobId];
            _stateManagers[jobId] = new BackupStateManager(job);
        }

        // Nouvelle méthode pour exposer le state manager à la UI
        public BackupStateManager getJobStateManager(int jobId)
        {
            if (_stateManagers.ContainsKey(jobId))
                return _stateManagers[jobId];
            return null;
        }
    }
}
