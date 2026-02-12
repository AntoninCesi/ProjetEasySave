using System;
using EasySave.Models;
using Tool.Utils;

namespace EasySave.Strategies
{
    
    /// Factory pour créer et exécuter les stratégies de sauvegarde
 
    public class BackupStrategyFactory
    {

        /// <summary>
        /// Exécute une sauvegarde avec un observateur de progression
        /// </summary>
        public static BackupProgressObserver ExecuteBackupWithObserver(BackupJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            BackupProgressObserver observer = new BackupProgressObserver();

            switch (job.type)
            {
                case BackupTypes.FULL:
                    {
                        FullBackupStrategy strategy = new FullBackupStrategy();
                        strategy.SetProgressObserver(observer);
                        strategy.Execute(job);
                        return strategy.GetProgressObserver();
                    }

                case BackupTypes.DIFFERENTIAL:
                    {
                        DifferentialBackupStrategy strategy = new DifferentialBackupStrategy();
                        strategy.SetProgressObserver(observer);
                        strategy.Execute(job);
                        return strategy.GetProgressObserver();
                    }

                default:
                    throw new ArgumentException($"Type de sauvegarde non supporté : {job.type}");
            }
        }

        /// <summary>
        /// Exécute une sauvegarde selon le type spécifié
        /// </summary>
        public static void ExecuteBackup(BackupJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            switch (job.type)
            {
                case BackupTypes.FULL:
                    ExecuteFullBackup(job);
                    break;

                case BackupTypes.DIFFERENTIAL:
                    ExecuteDifferentialBackup(job);
                    break;

                default:
                    throw new ArgumentException($"Type de sauvegarde non supporté : {job.type}");
            }
        }



        /// Exécute une sauvegarde complète

        private static void ExecuteFullBackup(BackupJob job)
        {
            FullBackupStrategy strategy = new FullBackupStrategy();
            strategy.Execute(job);
        }

        /// Exécute une sauvegarde différentielle
        
        private static void ExecuteDifferentialBackup(BackupJob job)
        {
            DifferentialBackupStrategy strategy = new DifferentialBackupStrategy();
            strategy.Execute(job);
        }

        
        /// Crée une instance de la stratégie appropriée sans l'exécuter
        
     

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
