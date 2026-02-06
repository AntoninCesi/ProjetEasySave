using System;

namespace EasySave.Models
{
    public class BackupState
    {
        public string Name { get; set; } = string.Empty;
        public DateTime LastActionTimestamp { get; set; }
        //public BackupStatus Status { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int RemainingFiles { get; set; }
        public long RemainingSize { get; set; }
        public int Progress { get; set; }
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
    }
}