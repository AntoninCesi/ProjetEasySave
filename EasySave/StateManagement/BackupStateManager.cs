using System;
using EasySave.Models;

namespace EasySave.StateManagement
{
    // implement observer to manage JSON dump
    public class BackupStateManager : IBackupStateObserver
    {
        public void Update(BackupState state)
        {
            // C'est ici que tu appelleras ta logique JSON (UpdateState)
            Console.WriteLine($"[JSON DUMP] Update of {state.Name} - Progression: {state.Progress}%");
        }
    }
}