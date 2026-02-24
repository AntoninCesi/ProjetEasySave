using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLog
{
    /// <summary>
    /// DTO public : 1 entrée de log (1 fichier copié / 1 événement).
    /// Sera sérialisé en JSON.
    /// </summary>
    public sealed class LogEntry
    {
        public DateTime Timestamp { get; set; }

        public string BackupName { get; set; } = "";

        public string SourcePath { get; set; } = "";

        public string TargetPath { get; set; } = "";

        public long FileSizeBytes { get; set; }

        public long TransferTimeMs { get; set; }

        public int ThreadId { get; set; }
    }
}