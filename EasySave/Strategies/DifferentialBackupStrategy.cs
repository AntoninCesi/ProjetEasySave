using System;
using System.IO;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJob job, ProgressCallback callback)
        {
            // Differential logic: only copy if files are different or new
            var files = Directory.GetFiles(job.SourcePath, "*.*", SearchOption.AllDirectories);
            int processedFiles = 0;

            foreach (var file in files)
            {
                // Logic to compare files would go here...
                processedFiles++;
                int progress = (int)((double)processedFiles / files.Length * 100);

                // Notify progress
                callback?.Invoke(Path.GetFileName(file), progress);
            }
        }
    }
}