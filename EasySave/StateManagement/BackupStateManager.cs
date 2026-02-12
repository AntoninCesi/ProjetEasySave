using System;
using EasySave.Models;
using Tool.Utils;

namespace EasySave.StateManagement
{
    // implement observer to manage JSON dump
    public class BackupStateManager  //IBackupStateObserver
    {
        public List<BackupJob> listBackupJob = new List<BackupJob>();

        public void Update(BackupState state)
        {
            // C'est ici que tu appelleras ta logique JSON (UpdateState)
            //Console.WriteLine($"[JSON DUMP] Update of {state.Name} - Progression: {state.Progress}%");
        }
        // 1st method: Update backup status
        public void UpdateStatus(int jobId, BackupStateResum newStatus)
        {
            listBackupJob[jobId].status.Status = newStatus;
            listBackupJob[jobId].status.LastActionTimestamp = DateTime.Now;
        }

        // 2nd method: Set total files and total size
        public void SetTotals(int jobId, int totalFiles, long totalSize)
        {
            listBackupJob[jobId].status.TotalFiles = totalFiles;
            listBackupJob[jobId].status.TotalSize = totalSize;
        }

        // 3rd method: Update progress and remaining data
        public void UpdateProgress(int jobId, int remainingFiles, long remainingSize, int progress)
        {
            listBackupJob[jobId].status.RemainingFiles = remainingFiles;
            listBackupJob[jobId].status.RemainingSize = remainingSize;
            listBackupJob[jobId].status.Progress = progress;
            listBackupJob[jobId].status.LastActionTimestamp = DateTime.Now;
        }

        // 4th method: Set source path
        public void SetSourcePath(int jobId, string sourcePath)
        {
            listBackupJob[jobId].status.SourcePath = sourcePath;
        }

        // 5th method: Set destination path
        public void SetDestinationPath(int jobId, string destinationPath)
        {
            listBackupJob[jobId].status.DestinationPath = destinationPath;
        }

        public BackupJob getJobById (int jobId) {  return listBackupJob[jobId]; }
    }
    
}