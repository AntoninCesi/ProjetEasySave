using System;
using EasySave.Models;
using EasySave.Strategies;
using EasySave.StateManagement;
using Tool.Utils;

namespace EasySave.ExecutionManagement
{
    
    public class BackupExecutionManager
    {
        //private List<BackupJob> listBackupJob = new List<BackupJob>();
        private readonly BackupStateManager _stateManager;
        public BackupExecutionManager() {
        
            _stateManager = new BackupStateManager();
        
        }

        public void createBackupJob(string name, string sourcePath, string destinationPath, BackupTypes type)
        {
           
             _stateManager.listBackupJob.Add(new BackupJob
             {
                 name = name,
                 sourcePath = sourcePath,
                 destinationPath = destinationPath,
                 type = type
             });

             int jobId = _stateManager.listBackupJob.Count - 1; 

             Console.WriteLine(_stateManager.listBackupJob[jobId].ToString());
            
        }
        public void ExecuteJob(int jobId)
        {
            Console.WriteLine("= Test de la Factory =\n");
            BackupStrategyFactory.ExecuteBackup(_stateManager.getJobById(jobId));
            Console.WriteLine("Sauvegarde terminée !");
        }
        public List<BackupJob> getBackupJobList()
        {
            return _stateManager.listBackupJob;
        }

        /*public void ExecuteJob(BackupJob job)
        {
            // Choose the right strategy based on job type
            IBackupStrategy strategy = job.Type == BackupType.COMPLET
                ? (IBackupStrategy)new FullBackupStrategy()
                : (IBackupStrategy)new DifferentialBackupStrategy();

            job.Status = BackupStatus.ACTIVE;

            // Execution with the callback to catch each file process
            strategy.Execute(job, (fileData, remaining) => {
                // 1. Trigger the event for the UI/ViewModel
                OnFileProcess?.Invoke(this, fileData);

                // 2. Update the state (JSON logging)
                NotifyState(job, fileData, remaining);
            });

            job.Status = BackupStatus.FINISHED;

            // Final update to notify completion
            _stateManager.Update(new BackupState
            {
                Name = job.Name,
                Status = job.Status
            });
        }*/
        /*private void NotifyState(BackupJob job, EasySave.Models.FileInfo
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * file, int remaining)
        {
            // Mapping job data and current file data to the state model
            _stateManager.Update(new BackupState
            {
                Name = job.Name,
                LastActionTimestamp = DateTime.Now,
                Status = job.Status,
                TotalFiles = job.TotalFiles,
                TotalSize = job.TotalSize,
                SourcePath = file.filePath,
                // FilesRemaining = remaining // Uncomment if BackupState supports this field
            });
        }*/
    }
}