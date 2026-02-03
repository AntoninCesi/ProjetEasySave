using EasySave.Models;

namespace EasySave.Strategies;

public interface IBackupStrategy
{
    // The Action allows the strategy to "send back" info to the caller for logs/state
    void Execute(BackupJob job, Action<string, string, long, long> onFileCopied);
}