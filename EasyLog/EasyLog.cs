using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EasyLog
{
    /// <summary>
    /// DLL de log journalier (1 fichier par jour) pour EasySave.
    /// Format : JSON Lines (1 objet JSON par ligne).
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

        /// <summary>
        /// API simple : écrit 1 entrée de log (append) dans le fichier du jour.
        /// </summary>
        public void WriteLog(LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            string filePath = Path.Combine(_baseFolder, $"{DateTime.Now:yyyy-MM-dd}.json");
            string json = JsonSerializer.Serialize(entry, JsonDefaults.Options);

            lock (_lock)
            {
                File.AppendAllText(filePath, json + Environment.NewLine, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Overload pratique (compatibilité avec ton ancienne signature).
        /// </summary>
        public void WriteLog(
            DateTime timestamp,
            string backupName,
            string sourcePath,
            string targetPath,
            long fileSizeBytes,
            long transferTimeMs)
        {
            WriteLog(new LogEntry
            {
                Timestamp = timestamp,
                BackupName = backupName ?? "",
                SourcePath = sourcePath ?? "",
                TargetPath = targetPath ?? "",
                FileSizeBytes = fileSizeBytes,
                TransferTimeMs = transferTimeMs
            });
        }

        /// <summary>
        /// (Optionnel) Expose le dossier de log (utile pour debug).
        /// </summary>
        public string GetLogFolderPath() => _baseFolder;
    }
}
