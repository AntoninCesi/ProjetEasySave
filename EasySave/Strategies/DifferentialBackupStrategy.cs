namespace EasySave.Strategies;

using EasySave.Models;
using System.Diagnostics;


public class DifferentialBackupStrategy : IBackupStrategy
{
    public void Execute(BackupJob job, Action<string, string, long, long> onFileCopied)
    {
        var files = Directory.GetFiles(job.SourcePath, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            string destFile = file.Replace(job.SourcePath, job.TargetPath);
            var sourceInfo = new FileInfo(file);

            // Differential logic: skip if file exists and is identical
            if (File.Exists(destFile))
            {
                var destInfo = new FileInfo(destFile);
                if (sourceInfo.Length == destInfo.Length && sourceInfo.LastWriteTime == destInfo.LastWriteTime)
                    continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
            var stopWatch = Stopwatch.StartNew();
            File.Copy(file, destFile, true);
            stopWatch.Stop();

            onFileCopied(file, destFile, sourceInfo.Length, stopWatch.ElapsedMilliseconds);
        }
    }
}