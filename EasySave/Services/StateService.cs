using System.Text.Json;
using EasySave.Models;

namespace EasySave.Services;

public class StateService
{
    private readonly string _stateFilePath;

    public StateService()
    {
        // The state file is stored in the app directory
        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "State");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        _stateFilePath = Path.Combine(folderPath, "state.json");
    }

    public void UpdateState(string jobName, string status, int totalFiles, long totalSize, int remainingFiles, long remainingSize, string currentSource, string currentDest)
    {
        var state = new
        {
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            JobName = jobName,
            Status = status, //"Active", "Inactive"
            TotalFiles = totalFiles,
            TotalSize = totalSize,
            RemainingFiles = remainingFiles,
            RemainingSize = remainingSize,
            Progress = totalFiles > 0 ? (100 * (totalFiles - remainingFiles) / totalFiles) : 0,
            CurrentSource = currentSource,
            CurrentDestination = currentDest
        };

        // Format for Notepad readability
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(state, options);

        // We overwrite the file every time to have the real-time status
        File.WriteAllText(_stateFilePath, jsonString);
    }
}