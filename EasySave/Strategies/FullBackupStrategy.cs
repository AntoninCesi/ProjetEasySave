using System;
using System.IO;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJob job, ProgressCallback callback)
        {
            DirectoryInfo di = new DirectoryInfo(job.SourcePath);
            FileInfo[] files = di.GetFiles();
            int count = 0;

            foreach (FileInfo file in files)
            {
                string destFile = Path.Combine(job.DestinationPath, file.Name);
                File.Copy(file.FullName, destFile, true);

                count++;
                int progress = (int)((float)count / files.Length * 100);
                callback?.Invoke(file.Name, progress);
            }
        }
    }
}