using System;
using System.IO;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJob job, ProgressCallback callback)
        {
            // Use System.IO.DirectoryInfo to avoid confusion with your own models
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(job.SourcePath);

            // Explicitly use System.IO.FileInfo for the Windows file system tools
            System.IO.FileInfo[] files = di.GetFiles();
            int totalFiles = files.Length;
            int count = 0;

            foreach (System.IO.FileInfo sourceFile in files)
            {
                string destPath = Path.Combine(job.DestinationPath, sourceFile.Name);

                // Differential logic: copy only if file doesn't exist or was modified
                if (!File.Exists(destPath) || sourceFile.LastWriteTime > File.GetLastWriteTime(destPath))
                {
                    // Ensure destination directory exists
                    if (!Directory.Exists(job.DestinationPath))
                    {
                        Directory.CreateDirectory(job.DestinationPath);
                    }

                    File.Copy(sourceFile.FullName, destPath, true);
                }

                count++;
                int remaining = totalFiles - count;

                // Create your custom FileInfo object (the DTO) to send to the Manager
                // Note: We use the full namespace to be 100% sure there's no error
                EasySave.Models.FileInfo fileData = new EasySave.Models.FileInfo
                {
                    fileName = sourceFile.Name,
                    filePath = sourceFile.FullName,
                    fileSize = sourceFile.Length,
                    isDirectory = false,
                    lastModified = sourceFile.LastWriteTime
                };

                // Invoke the callback with the custom object and the number of remaining files
                callback?.Invoke(fileData, remaining);
            }
        }
    }
}