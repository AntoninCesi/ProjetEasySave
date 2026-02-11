using EasySave.Models;

namespace EasySave.StateManagement
{
    public interface IBackupStateObserver
    {
        // Method to update the observer with the latest backup state
        void Update(BackupState state);
    }
}