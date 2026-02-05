using System;
using System.IO;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        private int filesProcessed = 0;
        private int totalFiles = 0;

        public void Execute(BackupJob job, ProgressCallback callback)
        {
            try
            {
                System.IO.DirectoryInfo sourceDir = new System.IO.DirectoryInfo(job.SourcePath);

                if (!sourceDir.Exists)
                    throw new DirectoryNotFoundException($"Source directory not found: {job.SourcePath}");

                // Create destination directory
                if (!Directory.Exists(job.DestinationPath))
                {
                    Directory.CreateDirectory(job.DestinationPath);
                }

                // Count all files
                totalFiles = CountAllFiles(sourceDir);
                filesProcessed = 0;

                // Copy recursively (differential mode)
                CopyDirectoryDifferential(sourceDir, job.DestinationPath, callback);
            }
            catch (Exception ex)
            {
                throw new Exception($"Differential backup failed: {ex.Message}", ex);
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

        private void CopyDirectoryDifferential(DirectoryInfo sourceDir, string destDirPath, ProgressCallback callback)
        {
            // Create destination directory
            if (!Directory.Exists(destDirPath))
            {
                Directory.CreateDirectory(destDirPath);
            }

            // Copy modified/new files
            foreach (System.IO.FileInfo sourceFile in sourceDir.GetFiles())
            {
                string destPath = Path.Combine(destDirPath, sourceFile.Name);

                // Differential logic: copy only if new or modified
                bool shouldCopy = !File.Exists(destPath) ||
                                  sourceFile.LastWriteTime > File.GetLastWriteTime(destPath);

                if (shouldCopy)
                {
                    sourceFile.CopyTo(destPath, true);
                }

                filesProcessed++;
                int remaining = totalFiles - filesProcessed;

                // Create FileInfo DTO
                EasySave.Models.FileInfo fileData = new EasySave.Models.FileInfo
                {
                    fileName = sourceFile.Name,
                    filePath = sourceFile.FullName,
                    fileSize = sourceFile.Length,
                    isDirectory = false,
                    lastModified = sourceFile.LastWriteTime
                };

                callback?.Invoke(fileData, remaining);
            }

            // Process subdirectories recursively
            foreach (DirectoryInfo subDir in sourceDir.GetDirectories())
            {
                string newDestDir = Path.Combine(destDirPath, subDir.Name);
                CopyDirectoryDifferential(subDir, newDestDir, callback);
            }
        }
    }
}