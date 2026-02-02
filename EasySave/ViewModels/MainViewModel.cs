using EasySave.Models;
using EasySave.Services;   // Pour LogService et StateService
using EasySave.Strategies; // Pour tes moteurs de sauvegarde

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

        // 1. Choose strategy based on Enum
        IBackupStrategy strategy = job.Type == BackupType.Full
            ? new FullBackupStrategy()
            : new DifferentialBackupStrategy();

        // 2. Set state to Active
        _stateService.UpdateState(job.Name, "Active", 0, 0, 0, 0, job.SourcePath, job.TargetPath);

        try
        {
            // 3. Delegate execution to the strategy
            strategy.Execute(job, (src, dest, size, time) => {
                // This callback handles logging for every file copied
                _logService.WriteLog(job.Name, src, dest, size, time);
            });
        }
        finally
        {
            // 4. Reset state to Inactive
            _stateService.UpdateState(job.Name, "Inactive", 0, 0, 0, 0, "", "");
        }
    }
}