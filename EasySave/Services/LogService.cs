/*using System.Text.Json;

namespace EasySave.Services;

public class LogService
{
    private readonly string _logFolderPath;

    public LogService()
    {
        // Logs are stored in a "Logs" folder inside the app directory
        _logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        // Create the directory if it doesn't exist yet
        if (!Directory.Exists(_logFolderPath))
        {
            Directory.CreateDirectory(_logFolderPath);
        }
    }

    public void WriteLog(string jobName, string source, string target, long fileSize, long transferTime)
    {
        // Prepare the data for the log entry
        var logEntry = new
        {
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            BackupName = jobName,
            SourceFile = source,
            TargetFile = target,
            FileSize = fileSize,
            DurationMs = transferTime
        };

        // One log file per day (e.g., 2026-01-30.json)
        string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
        string filePath = Path.Combine(_logFolderPath, fileName);

        // Format JSON with indentation so it's readable in Notepad
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(logEntry, options);

        // Append the JSON string to the daily log file
        File.AppendAllText(filePath, jsonString + Environment.NewLine);
    }
}
*/