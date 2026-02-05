using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.ViewModels
{
    internal class Controller 
    {
        public Controller(string[] args) {

            if (args.Length == 0)
            {
                this.createBackupJob()
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
                        if (middle == '-')
                        {
                            this.executBackupJob([first, middle]);
                        }
                    else if (middle == ",")
                        {
                            int [] toExecute = [] 
                            for (int i = first; i <= last; i++)
                            {
                                if (!this.isBackupJobExist(i))
                                {
                                    this.displayMessage("")//error
                                }

                                else
                                    toExecute.Add(i);
                            }
                            this.executBackupJob(toExecute);
                        }
                }
                else
                {
                    Console.WriteLine("Error: input must be in the form '3-2' or '4,9'.");
                }
            }
            
        }
        
        }
        public void createUI(){}
        public void displayMessage(string message) { }
        public void getUIMessage(string message) { }
        private bool isBackupJobExist(int jobId) { }
        //private BackupJob[] getBackupJobById(int[] jobId) { }
        private void createBackupJob(int jobId, string jobName, string sourcePath, string destPath) { }
        private void executBackupJob(int[] jobId) { }
        private void deleateBackupJob(int jobId) { }
    }
}
