using EasySave.Models;

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
