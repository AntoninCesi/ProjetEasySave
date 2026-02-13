<<<<<<< HEAD
﻿/*using System.Text.Json;
=======
﻿using System.Text.Json;
using System.IO;
>>>>>>> origin/dev

namespace EasySave.Services;

public class LogService
{
    private static LogService? _instance;
    private readonly string _logFolderPath;

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static LogService Instance => _instance ??= new LogService();

    private LogService()
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
<<<<<<< HEAD
}
*/
=======

    /// <summary>
    /// Logs business software events (blocking, detection during backup)
    /// </summary>
    public void LogBusinessSoftwareEvent(string jobName, string eventMessage)
    {
        var logEntry = new
        {
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            BackupName = jobName,
            Event = eventMessage,
            FileSize = 0,
            DurationMs = -1  // -1 indicates this is an event, not a file transfer
        };

        string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
        string filePath = Path.Combine(_logFolderPath, fileName);

        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(logEntry, options);

        File.AppendAllText(filePath, jsonString + Environment.NewLine);
    }
}
>>>>>>> origin/dev
