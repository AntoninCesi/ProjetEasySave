using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class LogEvent
{
    public LogEventType Type { get; }
    public string JobName { get; }
    public string? SourcePath { get; private set; }
    public string? TargetPath { get; private set; }
    public long FileSizeBytes { get; private set; }
    public long TransferTimeMs { get; private set; }
    public string? ErrorMessage { get; private set; }

    private LogEvent(LogEventType type, string jobName)
    {
        Type = type;
        JobName = jobName;
    }

    public static LogEvent FileCopied(string jobName, string src, string dst, long sizeBytes, long timeMs)
        => new(LogEventType.FileCopied, jobName)
        {
            SourcePath = src,
            TargetPath = dst,
            FileSizeBytes = sizeBytes,
            TransferTimeMs = timeMs
        };

    public static LogEvent JobStarted(string jobName)
        => new(LogEventType.JobStarted, jobName);

    public static LogEvent JobCompleted(string jobName)
        => new(LogEventType.JobCompleted, jobName);

    public static LogEvent Error(string jobName, string message)
        => new(LogEventType.Error, jobName) { ErrorMessage = message };
}
