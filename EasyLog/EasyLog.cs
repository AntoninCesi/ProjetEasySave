using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EasyLog
{
	/// <summary>
	/// DLL de log journalier (1 fichier par jour) pour EasySave.
	/// Objectif : écrire des entrées en temps réel durant une sauvegarde.
	/// </summary>
	public sealed class EasyLog
	{
		private static readonly Lazy<EasyLog> _instance = new(() => new EasyLog());
		public static EasyLog Instance => _instance.Value;

		private readonly object _lock = new();
		private readonly string _baseFolder;

		private EasyLog()
		{
			// Emplacement "propre" serveur/entreprise (évite C:\temp)
			_baseFolder = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				"ProSoft", "EasySave", "Logs"
			);

			Directory.CreateDirectory(_baseFolder);
		}

		public void WriteLog(DateTime timestamp, string backupName, string sourceUNC, string destinationUNC, long fileSizeBytes, long transferTimeMs)
		{
			var entry = new LogEntry
			{
				Timestamp = timestamp,
				BackupName = backupName,
				SourceUNC = sourceUNC,
				DestinationUNC = destinationUNC,
				FileSizeBytes = fileSizeBytes,
				TransferTimeMs = transferTimeMs
			};

			string filePath = Path.Combine(_baseFolder, $"{DateTime.Now:yyyy-MM-dd}.json");
			string json = JsonSerializer.Serialize(entry);

			lock (_lock)
			{
				File.AppendAllText(filePath, json + Environment.NewLine, Encoding.UTF8);
			}
		}

		private sealed class LogEntry
		{
			public DateTime Timestamp { get; set; }
			public string BackupName { get; set; } = "";
			public string SourceUNC { get; set; } = "";
			public string DestinationUNC { get; set; } = "";
			public long FileSizeBytes { get; set; }
			public long TransferTimeMs { get; set; }
		}
	}
}