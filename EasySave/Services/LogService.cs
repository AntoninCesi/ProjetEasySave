using System;
using System.IO;
using System.Text.Json;

namespace EasySave.Services
{
    public class LogService
    {
        private readonly string _logFolderPath;

        public LogService()
        {
            // Logs stored in "Logs" folder inside app directory
            _logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (!Directory.Exists(_logFolderPath))
            {
                Directory.CreateDirectory(_logFolderPath);
            }
        }

        public void WriteLog(string jobName, string source, string target, long fileSize, long transferTimeMs)
        {
            try
            {
                // Convert paths to UNC format if possible
                string sourceUNC = ConvertToUNC(source);
                string targetUNC = ConvertToUNC(target);

                var logEntry = new
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    BackupName = jobName,
                    SourceFile = sourceUNC,
                    TargetFile = targetUNC,
                    FileSize = fileSize,
                    TransferTimeMs = transferTimeMs // Negative if error
                };

                // One log file per day (e.g., 2026-02-05.json)
                string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
                string filePath = Path.Combine(_logFolderPath, fileName);

                // Format JSON with indentation
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(logEntry, options);

                // Append to daily log file
                File.AppendAllText(filePath, jsonString + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOG ERROR] Failed to write log: {ex.Message}");
            }
        }

        private string ConvertToUNC(string path)
        {
            try
            {
                // If already UNC path (starts with \\), return as is
                if (path.StartsWith(@"\\"))
                {
                    return path;
                }

                // Convert local paths to UNC format if possible
                // Ex: C:\folder → \\localhost\C$\folder
                if (Path.IsPathRooted(path))
                {
                    string driveLetter = Path.GetPathRoot(path).TrimEnd('\\', ':');
                    string restOfPath = path.Substring(Path.GetPathRoot(path).Length);
                    return $@"\\localhost\{driveLetter}$\{restOfPath}";
                }

                return path;
            }
            catch
            {
                return path; // Return original path on error
            }
        }
    }
}