using System;
using System.IO;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJob job, ProgressCallback callback)
        {
            DirectoryInfo di = new DirectoryInfo(job.SourcePath);
            FileInfo[] files = di.GetFiles();
            int count = 0;

            foreach (FileInfo sourceFile in files)
            {
                string destPath = Path.Combine(job.DestinationPath, sourceFile.Name);

                if (!File.Exists(destPath) || sourceFile.LastWriteTime > File.GetLastWriteTime(destPath))
                {
                    File.Copy(sourceFile.FullName, destPath, true);
                }

                count++;
                int progress = (int)((float)count / files.Length * 100);
                callback?.Invoke(sourceFile.Name, progress);
            }
        }
    }
}