using EasyLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public sealed class EasyLogger
{
    private static readonly object _globalLock = new();
    private readonly string _baseFolder;
    private readonly LogFormatter _formatter = new();

    public EasyLogger(string? baseFolder = null)
    {
        _baseFolder = string.IsNullOrWhiteSpace(baseFolder)
            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
            : baseFolder;

        Directory.CreateDirectory(_baseFolder);
    }

    public void Write(LogEvent ev)
    {
        var entry = _formatter.Format(ev);
        string filePath = Path.Combine(_baseFolder, $"{DateTime.Now:yyyy-MM-dd}.json");
        string json = JsonSerializer.Serialize(entry, JsonDefaults.Options);

        lock (_globalLock)
        {
            File.AppendAllText(filePath, json + Environment.NewLine, Encoding.UTF8);
        }
    }
}
