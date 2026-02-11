using System;
using System.Collections.Generic;
using System.Text;
using EasySave.ExecutionManagement;
using EasySave.View;
using Tool.Utils;

namespace EasySave.ViewModels
{
    public class Controller
    {

        private BackupExecutionManager backupManager = new BackupExecutionManager();
        

        public Controller(string[] args)
        {
            //Console.WriteLine("ici");
            if (args.Length == 0)
            {
                this.createUI().showMenu();
            }

            else if (args[0].Length == 3)
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

        public ConsoleUI createUI() {

            return new ConsoleUI(this);
            
        }
        public void displayMessage(string message) { }
        public void getUIMessage(string message) { }

        private bool isBackupJobExist(int jobId)
        {
            return true; // placeholder 
        }

        //private BackupJob[] getBackupJobById(int[] jobId) { }

        public void createBackupJob( string jobName, string sourcePath, string destPath, BackupTypes type) {

            backupManager.createBackupJob(jobName, sourcePath, destPath, type);
        }
        private void executBackupJob(int[] jobId) { }
        private void deleateBackupJob(int jobId) { }
    }
}
