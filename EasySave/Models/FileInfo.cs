using System;

namespace EasySave.Models
{
    public class FileInfos
    {

        public int FilesSaved { get; set; }
        public long TotalSize { get; set; }
        public TimeSpan TotalBackupTime { get; set; }
        public TimeSpan LastFileDuration { get; set; }

        public override string ToString()
        {
            return $"Fichiers: {FilesSaved} - Taille: {TotalSize / 1024.0:F2} KB - Temps: {TotalBackupTime:hh\\:mm\\:ss}";
        }
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