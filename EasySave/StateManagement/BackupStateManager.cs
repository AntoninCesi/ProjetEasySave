using System;
using System.IO;
using System.Text.Json;
using EasySave.Models;

namespace EasySave.StateManagement
{
    public class BackupStateManager : IBackupStateObserver
    {
        private readonly string _stateFolderPath;

        public BackupStateManager()
        {
            _stateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "States");

            if (!Directory.Exists(_stateFolderPath))
            {
                Directory.CreateDirectory(_stateFolderPath);
            }
        }

        public void Update(BackupState state)
        {
            try
            {
                // Create state entry
                var stateEntry = new
                {
                    Name = state.Name,
                    Timestamp = state.LastActionTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = state.Status.ToString(),
                    TotalFiles = state.TotalFiles,
                    TotalSize = state.TotalSize,
                    RemainingFiles = state.RemainingFiles,
                    RemainingSize = state.RemainingSize,
                    Progress = state.Progress,
                    SourcePath = state.SourcePath,
                    DestinationPath = state.DestinationPath
                };

                // One JSON file per job
                string fileName = $"{state.Name}_state.json";
                string filePath = Path.Combine(_stateFolderPath, fileName);

                // Write JSON with indentation
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(stateEntry, options);

                // Overwrite file with new state
                File.WriteAllText(filePath, jsonString);

                Console.WriteLine($"[STATE UPDATED] {state.Name} - Progress: {state.Progress}%");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to update state: {ex.Message}");
            }
        }
    }
}