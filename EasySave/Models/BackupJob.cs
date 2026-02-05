namespace EasySave.Models
{
    public enum BackupType { FULL, DIFFERENTIAL }
    public enum BackupStatus { INACTIVE, ACTIVE, ERROR, FINISHED }

    public class BackupJob
    {
        public string Name { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public BackupType Type { get; set; }
        public BackupStatus Status { get; set; } = BackupStatus.INACTIVE;
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int Progress { get; set; }
    }
}