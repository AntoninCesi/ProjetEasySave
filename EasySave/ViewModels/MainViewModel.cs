using EasySave.Models;
using EasySave.Services;
using System.Diagnostics;

namespace EasySave.ViewModels;

public class MainViewModel
{
    private readonly List<BackupJob> _jobs = new();
    private readonly LogService _logService = new();
    private readonly StateService _stateService = new();

    public void ExecuteBackup(int jobIndex)
    {
        if (jobIndex < 0 || jobIndex >= _jobs.Count) return;

        var job = _jobs[jobIndex];

        // Start tracking state
        _stateService.UpdateState(job.Name, "Active", 0, 0, 0, 0, job.SourcePath, job.TargetPath);

        try
        {
            var files = Directory.GetFiles(job.SourcePath, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                string destFile = file.Replace(job.SourcePath, job.TargetPath);
                string destDir = Path.GetDirectoryName(destFile)!;

                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                var stopWatch = Stopwatch.StartNew();
                File.Copy(file, destFile, true);
                stopWatch.Stop();

                // Log the action for each file
                _logService.WriteLog(job.Name, file, destFile, fileInfo.Length, stopWatch.ElapsedMilliseconds);
            }
        }
        finally
        {
            // Set to Inactive when finished
            _stateService.UpdateState(job.Name, "Inactive", 0, 0, 0, 0, "", "");
        }
    }
}