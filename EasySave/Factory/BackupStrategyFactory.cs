using System;
using EasySave.Models;

namespace EasySave.Strategies
{
    
    /// Factory pour créer et exécuter les stratégies de sauvegarde
 
    public class BackupStrategyFactory
    {
        
        /// Exécute une sauvegarde selon le type spécifié
       
        /// <param name="backupType">Type de sauvegarde (Full ou Differential)</param>
        /// <param name="job">Job de sauvegarde contenant les paramètres</param>
        public static void ExecuteBackup(BackupType backupType, BackupJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            switch (backupType)
            {
                case BackupType.Full:
                    ExecuteFullBackup(job);
                    break;

                case BackupType.Differential:
                    ExecuteDifferentialBackup(job);
                    break;

                default:
                    throw new ArgumentException($"Type de sauvegarde non supporté : {backupType}");
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
        
        /// <param name="backupType">Type de sauvegarde</param>
        /// <returns>Instance de la stratégie correspondante</returns>
        /// 

        public static IBackupStrategy CreateStrategy(BackupType backupType)
        {
            switch (backupType)
            {
                case BackupType.Full:
                    return new FullBackupStrategy();

                case BackupType.Differential:
                    return new DifferentialBackupStrategy();

                default:
                    throw new ArgumentException($"Type de sauvegarde non supporté : {backupType}");
            }
        }
    }
}
