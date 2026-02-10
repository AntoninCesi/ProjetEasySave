using System;
using System.Collections.Generic;
using System.Text;
using EasySave.ExecutionManagement;
using EasySave.View;
using EasySave.Models;
using Tool.Utils;

namespace EasySave.ViewModels
{
    public class Controller
    {
        private BackupExecutionManager backupManager = new BackupExecutionManager();

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

                if (char.IsDigit(first) && char.IsDigit(last) && (middle == '-' || middle == ','))
                {
                    int fId = (int)char.GetNumericValue(first);
                    int lId = (int)char.GetNumericValue(last);

                    if (this.isBackupJobExist(fId) && this.isBackupJobExist(lId))
                    {
                        // Execution logic (handled by group members)
                    }
                }
            }
        }

        // Expose the job list to the View for validation purposes
        public List<BackupJob> GetJobs() => backupManager.BackupJobs;

        public ConsoleUI createUI()
        {
            return new ConsoleUI(this);
        }

        public void displayMessage(string message) { Console.WriteLine(message); }
        public void getUIMessage(string message) { }

        // Placeholder for job existence check
        private bool isBackupJobExist(int jobId) => true;

        public void createBackupJob(string jobName, string sourcePath, string destPath, BackupTypes type)
        {
            backupManager.createBackupJob(jobName, sourcePath, destPath, type);
        }
    }
}