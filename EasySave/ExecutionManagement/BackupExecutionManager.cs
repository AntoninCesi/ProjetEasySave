using System;
using EasySave.Models;
using EasySave.Strategies;
using EasySave.Services;

namespace EasySave.ExecutionManagement
{
    // Renamed to BackupExecutionManager as per diagram
    public class BackupExecutionManager
    {
        private readonly StateService _stateService;
        private readonly LogService _logService;

        public BackupExecutionManager(StateService stateService, LogService logService)
        {
            _stateService = stateService;
            _logService = logService;
        }

        public void ExecuteJob(BackupJob job)
        {
            IBackupStrategy strategy = job.Type == BackupType.COMPLET
                ? new FullBackupStrategy()
                : new DifferentialBackupStrategy();

            job.Status = BackupStatus.ACTIVE;

            strategy.Execute(job, (fileName, progress) => {
                job.Progress = progress;
                // Calls the intermediate notification method
                NotifyState(job, fileName);
            });

            job.Status = BackupStatus.FINISHED;
            _logService.WriteLog(job.Name, job.SourcePath, job.DestinationPath, job.TotalSize, 0);
        }

        // Added NotifyState method to match diagram's 'notifyState(job : BackupJob)'
        private void NotifyState(BackupJob job, string currentFileName)
        {
            _stateService.UpdateState(
                job.Name,
                job.Status.ToString(),
                job.TotalFiles,
                job.TotalSize,
                0,
                0,
                job.SourcePath,
                job.DestinationPath
            );
        }
    }
}