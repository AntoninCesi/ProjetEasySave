using EasySave.Models;
using System;
using Tool.Utils;


public class BackupState
{
	public int Id { get; set; }
	public string JobName { get; set; } = string.Empty;
	public DateTime LastActionTimestamp { get; set; }

	public BackupStatus Status { get; set; }

	public int TotalFiles { get; set; }
	public long TotalSizeBytes { get; set; }

	public int RemainingFiles { get; set; }
	public long RemainingSizeBytes { get; set; }

	public float Progress { get; set; }

	public string CurrentSourceUNC { get; set; } = string.Empty;
	public string CurrentDestinationUNC { get; set; } = string.Empty;
}


namespace EasySave.Models
{
    public class BackupState
    {
        //1methode
        public BackupStateResum Status { get; set; } = BackupStateResum.INACTIVE;

        //2methode
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        //3methode
        public DateTime LastActionTimestamp { get; set; }
        public int RemainingFiles { get; set; }
        public long RemainingSize { get; set; }
        public int Progress { get; set; }
        //4methode
        public string SourcePath { get; set; } = string.Empty;
        //5methode
        public string DestinationPath { get; set; } = string.Empty;
    }
}