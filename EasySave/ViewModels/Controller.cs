using System;
using System.Collections.Generic;
using System.Text;
using EasySave.ExecutionManagement;

namespace EasySave.ViewModels
{
    public class Controller
    {

        private BackupExecutionManager backupManager = new BackupExecutionManager();

        public Controller(string[] args)
        {
            Console.WriteLine("ici");
            if (args.Length == 0)
            {
                this.createUI();
            }

            if (args[0].Length == 3)
            {
                Console.WriteLine(args[0].Length);
                char first = args[0][0];
                char middle = args[0][1];
                char last = args[0][2];

                if (char.IsDigit(first) && char.IsDigit(last) && (middle == '-' || middle == ','))
                {
                    if (this.isBackupJobExist(first) && this.isBackupJobExist(last))
                    {
                        if (middle == '-')
                        {
                            this.executBackupJob(new int[] { first, last });
                        }
                        else if (middle == ',')
                        {
                            List<int> toExecute = new List<int>();

                            for (int i = first; i <= last; i++)
                            {
                                if (!this.isBackupJobExist(i))
                                {
                                    this.displayMessage(""); // error
                                }
                                else
                                {
                                    toExecute.Add(i);
                                }
                            }
                            this.executBackupJob(toExecute.ToArray());
                        }
                    }
                }
                else
                {
                    this.displayMessage("Error: input must be in the form '3-2' or '4,9'.");
                }
            }
        }

        public void createUI() { }
        public void displayMessage(string message) { }
        public void getUIMessage(string message) { }

        private bool isBackupJobExist(int jobId)
        {
            return true; // placeholder pour éviter erreur de compilation
        }

        //private BackupJob[] getBackupJobById(int[] jobId) { }

        private void createBackupJob(int jobId, string jobName, string sourcePath, string destPath) { }
        private void executBackupJob(int[] jobId) { }
        private void deleateBackupJob(int jobId) { }
    }
}
