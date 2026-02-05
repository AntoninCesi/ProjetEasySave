using EasySave.Models;

namespace EasySave.Strategies
{
    // Delegate to send the custom FileInfo object and the count of remaining files
    public delegate void ProgressCallback(EasySave.Models.FileInfo file, int filesRemaining);

    public interface IBackupStrategy
    {
        void Execute(BackupJob job, ProgressCallback callback);
    }
}