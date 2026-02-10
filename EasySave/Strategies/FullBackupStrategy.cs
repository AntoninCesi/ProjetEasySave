using System;
using System.IO;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJob job, ProgressCallback callback)
        {
            var directory = new DirectoryInfo(job.sourcePath);
            var files = directory.GetFiles();
            int total = files.Length;

            for (int i = 0; i < total; i++)
            {
                var f = files[i];

                // Create the custom FileInfo DTO requested by the colleague
                var fileData = new EasySave.Models.FileInfo
                {
                    fileName = f.Name,
                    filePath = f.FullName,
                    fileSize = f.Length,
                    isDirectory = false,
                    lastModified = f.LastWriteTime
                };

                string destFile = Path.Combine(job.destinationPath, f.Name);

                // Ensure destination directory exists
                if (!Directory.Exists(job.destinationPath))
                {
                    Directory.CreateDirectory(job.destinationPath);
                }

                // Perform physical file copy
                System.IO.File.Copy(f.FullName, destFile, true);

                // Send progress update to the Manager
                callback?.Invoke(fileData, (total - i - 1));
            }
        }
    }
}