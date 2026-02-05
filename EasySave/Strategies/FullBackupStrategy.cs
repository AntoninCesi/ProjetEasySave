using System;
using System.IO;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        private int filesProcessed = 0;
        private int totalFiles = 0;

        public void Execute(BackupJob job, ProgressCallback callback)
        {
            try
            {
                var directory = new DirectoryInfo(job.SourcePath);

                if (!directory.Exists)
                    throw new DirectoryNotFoundException($"Source directory not found: {job.SourcePath}");

                // Create destination directory
                if (!Directory.Exists(job.DestinationPath))
                {
                    Directory.CreateDirectory(job.DestinationPath);
                }

                // Count all files (including subdirectories)
                totalFiles = CountAllFiles(directory);
                filesProcessed = 0;

                // Copy recursively
                CopyDirectoryRecursive(directory, job.DestinationPath, callback);
            }
            catch (Exception ex)
            {
                throw new Exception($"Full backup failed: {ex.Message}", ex);
            }
        }

        private int CountAllFiles(DirectoryInfo dir)
        {
            int count = dir.GetFiles().Length;

            foreach (var subDir in dir.GetDirectories())
            {
                count += CountAllFiles(subDir);
            }

            return count;
        }

        private void CopyDirectoryRecursive(DirectoryInfo sourceDir, string destDirPath, ProgressCallback callback)
        {
            // Create destination directory
            if (!Directory.Exists(destDirPath))
            {
                Directory.CreateDirectory(destDirPath);
            }

            // Copy all files from current directory
            foreach (System.IO.FileInfo file in sourceDir.GetFiles())
            {
                string destFile = Path.Combine(destDirPath, file.Name);

                // Copy file
                file.CopyTo(destFile, true);

                filesProcessed++;
                int remaining = totalFiles - filesProcessed;

                // Create FileInfo DTO
                var fileData = new EasySave.Models.FileInfo
                {
                    fileName = file.Name,
                    filePath = file.FullName,
                    fileSize = file.Length,
                    isDirectory = false,
                    lastModified = file.LastWriteTime
                };

                // Notify progress
                callback?.Invoke(fileData, remaining);
            }

            // Process subdirectories recursively
            foreach (DirectoryInfo subDir in sourceDir.GetDirectories())
            {
                string newDestDir = Path.Combine(destDirPath, subDir.Name);
                CopyDirectoryRecursive(subDir, newDestDir, callback);
            }
        }
    }
}