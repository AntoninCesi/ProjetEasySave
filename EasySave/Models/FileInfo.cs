using System;

namespace EasySave.Models
{
    public class FileInfo
    {
        public string fileName { get; set; }
        public string filePath { get; set; }
        public long fileSize { get; set; }
        public bool isDirectory { get; set; }
        public DateTime lastModified { get; set; }
        
        // État de la sauvegarde du fichier
        public FileBackupStatus backupStatus { get; set; } = FileBackupStatus.Pending;
        
        // Temps de sauvegarde du fichier
        public TimeSpan backupDuration { get; set; } = TimeSpan.Zero;
        
        // Timestamp de début de sauvegarde
        public DateTime backupStartTime { get; set; }
        
        // Timestamp de fin de sauvegarde
        public DateTime backupEndTime { get; set; }
    }
    
    /// <summary>
    /// État de sauvegarde d'un fichier
    /// </summary>
    public enum FileBackupStatus
    {
        Pending,      // En attente
        InProgress,   // En cours de sauvegarde
        Completed,    // Sauvegardé avec succès
        Skipped,      // Ignoré (déjà à jour)
        Failed        // Échec
    }
}