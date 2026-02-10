using System;
using EasySave.Models;

namespace EasySave.StateManagement
{
    // Minimal serializer interface used by the manager (default JSON implementation provided)
    public class BackupStateManager : IBackupStateObserver
    {
        public void Update(BackupState state)
        {
            // C'est ici que tu appelleras ta logique JSON (UpdateState)
            Console.WriteLine($"[JSON DUMP] Update of {state.Name} - Progression: {state.Progress}%");
        }
    }
}