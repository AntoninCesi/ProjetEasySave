using System;
using System.IO;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJob job, ProgressCallback callback)
        {
            var files = Directory.GetFiles(job.SourcePath, "*.*", SearchOption.AllDirectories);
            int totalFiles = files.Length;
            int processedFiles = 0;

            foreach (var file in files)
            {
                // Perform file copy logic here
                processedFiles++;
                int progress = (int)((double)processedFiles / totalFiles * 100);

                // Trigger callback to notify the manager (Observer)
                callback?.Invoke(Path.GetFileName(file), progress);
            }
        }
    }
}