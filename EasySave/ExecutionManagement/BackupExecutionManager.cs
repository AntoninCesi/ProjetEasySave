using System;
using EasySave.Models;
using EasySave.Strategies;
using EasySave.StateManagement;
using Tool.Utils;

namespace EasySave.ExecutionManagement
{
    
    public class BackupExecutionManager
    {
        private readonly BackupStateManager _stateManager;

      
        public BackupExecutionManager() {
        
            _stateManager = new BackupStateManager();
        
        }

        public void createBackupJob(string name, string sourcePath, string destinationPath, BackupTypes type)
        {
            if (_stateManager.listBackupJob.Count >= 5)
            {
                Console.WriteLine("Error");
            }
            else
            {
                _stateManager.listBackupJob.Add(new BackupJob
                {
                    name = name,
                    sourcePath = sourcePath,
                    destinationPath = destinationPath,
                    type = type
                });

                int idJob = _stateManager.listBackupJob.Count - 1; 

                Console.WriteLine(_stateManager.listBackupJob[idJob].ToString());
            }
            
        }


    }
}