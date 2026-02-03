namespace EasySave.Models;

public class BackupState
{
	public string BackupName { get; set; } = "";
	public DateTime LastActionTimestamp { get; set; }
	public BackupStatus Status { get; set; }

	public int TotalFiles { get; set; }
	public long TotalSizeBytes { get; set; }

	public float Progress { get; set; } // 0..1
	public int RemainingFiles { get; set; }
	public long RemainingSizeBytes { get; set; }

	public string CurrentSourceUNC { get; set; } = "";
	public string CurrentDestinationUNC { get; set; } = "";
}