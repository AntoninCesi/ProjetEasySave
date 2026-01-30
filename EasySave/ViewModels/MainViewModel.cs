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

        // Start tracking state as "Active"
        _stateService.UpdateState(job.Name, "Active", 0, 0, 0, 0, job.SourcePath, job.TargetPath);

        try
        {
            // Get all files from source
            var files = Directory.GetFiles(job.SourcePath, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                string destFile = file.Replace(job.SourcePath, job.TargetPath);
                string destDir = Path.GetDirectoryName(destFile)!;

                // Basic check for differential backup: only copy if file changed
                if (job.Type == BackupType.Differential && File.Exists(destFile))
                {
                    var targetInfo = new FileInfo(destFile);
                    // If size and last modified date are same, we skip it
                    if (fileInfo.Length == targetInfo.Length && fileInfo.LastWriteTime == targetInfo.LastWriteTime)
                    {
                        continue;
                    }
                }

                // Create directory if it doesn't exist
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                // Start timer for logs
                var stopWatch = Stopwatch.StartNew();
                File.Copy(file, destFile, true);
                stopWatch.Stop();

                // Log the action for each file copied
                _logService.WriteLog(job.Name, file, destFile, fileInfo.Length, stopWatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            // You could add a simple console message here for debugging
            Console.WriteLine($"Error during backup: {ex.Message}");
        }
        finally
        {
            // Always set to "Inactive" when finished or if it crashes
            _stateService.UpdateState(job.Name, "Inactive", 0, 0, 0, 0, "", "");
        }
    }
}