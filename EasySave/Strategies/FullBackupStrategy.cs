using EasySave.Models;
using System.Diagnostics;

namespace EasySave.Strategies;

public class FullBackupStrategy : IBackupStrategy
{
    public void Execute(BackupJob job, Action<string, string, long, long> onFileCopied)
    {
        var files = Directory.GetFiles(job.SourcePath, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            string destFile = file.Replace(job.SourcePath, job.TargetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);

            var stopWatch = Stopwatch.StartNew();
            File.Copy(file, destFile, true);
            stopWatch.Stop();

            onFileCopied(file, destFile, new FileInfo(file).Length, stopWatch.ElapsedMilliseconds);
        }
    }
}