using EasyLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class LogFormatter
{
    public LogEntry Format(LogEvent ev)
    {
        return new LogEntry
        {
            Timestamp = DateTime.Now,
            BackupName = ev.JobName,
            SourcePath = ev.SourcePath ?? "",
            TargetPath = ev.TargetPath ?? "",
            FileSizeBytes = ev.FileSizeBytes,
            TransferTimeMs = ev.TransferTimeMs,
            ThreadId = Environment.CurrentManagedThreadId,
            // Si tu ajoutes ErrorMessage dans LogEntry, tu le mets ici.
        };
    }
}