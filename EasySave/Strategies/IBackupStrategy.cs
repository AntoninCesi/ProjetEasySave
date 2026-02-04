using EasySave.Models;

namespace EasySave.Strategies
{
    public delegate void ProgressCallback(string fileName, int progress);

    public interface IBackupStrategy
    {
        void Execute(BackupJob job, ProgressCallback callback);
    }
}