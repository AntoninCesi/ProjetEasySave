using System;
using EasySave.Models;
using Tool.Utils;

namespace EasySave.Strategies
{
    /// <summary>
    /// Factory pour créer et exécuter les stratégies de sauvegarde
    /// Version optimisée : l'observateur est maintenant dans BackupJob
    /// </summary>
    public class BackupStrategyFactory
    {
        /// <summary>
        /// Exécute une sauvegarde selon le type spécifié dans le job
        /// </summary>
        public static void ExecuteBackup(BackupJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            // Créer et exécuter la stratégie appropriée
            IBackupStrategy strategy = CreateStrategy(job.type);
            strategy.Execute(job);
        }

        /// <summary>
        /// Crée une instance de la stratégie appropriée
        /// </summary>
        public static IBackupStrategy CreateStrategy(BackupTypes backupType)
        {
            switch (backupType)
            {
                case BackupTypes.FULL:
                    return new FullBackupStrategy();

                case BackupTypes.DIFFERENTIAL:
                    return new DifferentialBackupStrategy();

                default:
                    throw new ArgumentException($"Type de sauvegarde non supporté : {backupType}");
            }
        }
    }
}