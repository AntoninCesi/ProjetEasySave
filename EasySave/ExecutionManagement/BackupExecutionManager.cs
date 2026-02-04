using System;
using EasySave.Models;
using EasySave.Strategies;
using EasySave.StateManagement;

namespace EasySave.ExecutionManagement
{
    public class BackupExecutionManager
    {
        private readonly BackupStateManager _stateManager;

        public BackupExecutionManager(BackupStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void ExecuteJob(BackupJob job)
        {
            IBackupStrategy strategy = job.Type == BackupType.COMPLET
                ? new FullBackupStrategy()
                : new DifferentialBackupStrategy();

            job.Status = BackupStatus.ACTIVE;

            strategy.Execute(job, (fileName, progress) => {
                job.Progress = progress;
                NotifyState(job);
            });

            job.Status = BackupStatus.FINISHED;
            NotifyState(job);
        }

        private void NotifyState(BackupJob job)
        {
            // We call .Update(state) to correspond to the IBackupStateObserver interface
                        _stateManager.Update(new BackupState
            {
                Name = job.Name,
                LastActionTimestamp = DateTime.Now,
                Status = job.Status,
                TotalFiles = job.TotalFiles,
                TotalSize = job.TotalSize,
                Progress = job.Progress,
                SourcePath = job.SourcePath,
                DestinationPath = job.DestinationPath
            });
        }
    }
}