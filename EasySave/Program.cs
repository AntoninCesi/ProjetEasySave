using System;
using EasySave.Models;
using EasySave.Strategies;
using Tool.Utils;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Test Système Optimisé ===\n");

            // ========================================
            // 1. Créer un BackupJob
            // ========================================
            BackupJob job = new BackupJob
            {
                name = "Test Backup",
                sourcePath = @"C:\Users\jenni\Downloads",
                destinationPath = @"C:\Users\jenni\Documents\CESI\CESI2025-2026\Genie logiciel\s3",
                type = BackupTypes.FULL,
                status = new BackupState()
            };

            // L'observateur est déjà créé automatiquement dans le job !

            // ========================================
            // 2. S'abonner aux événements de progression
            // ========================================
            job.progressObserver.OnProgressChanged += (progressData) =>
            {
                // Mettre à jour le BackupState du job
                job.status.Progress = progressData.FilesSaved;
                job.status.LastActionTimestamp = DateTime.Now;

                // Afficher la progression
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Fichiers: {progressData.FilesSaved} - " +
                                $"Taille: {progressData.TotalSize / 1024.0:F2} KB - " +
                                $"Dernier fichier: {progressData.LastFileDuration.TotalMilliseconds:F2}ms");
            };

            // ========================================
            // 3. Exécuter la sauvegarde
            // ========================================
            Console.WriteLine(" Sauvegarde en cours...\n");

            try
            {
                // Créer et exécuter la stratégie
                FullBackupStrategy strategy = new FullBackupStrategy();
                strategy.Execute(job);

                Console.WriteLine("\n Sauvegarde terminée !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur : {ex.Message}");
            }

            // ========================================
            // 4. Afficher les stats finales
            // ========================================
            var finalProgress = job.progressObserver.GetProgress();

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("STATISTIQUES FINALES :");
            Console.WriteLine($"  Fichiers sauvegardés : {finalProgress.FilesSaved}");
            Console.WriteLine($"  Taille totale        : {finalProgress.TotalSize / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"  Temps total          : {finalProgress.TotalBackupTime:hh\\:mm\\:ss}");
            Console.WriteLine($"  Vitesse moyenne      : {(finalProgress.TotalSize / 1024.0 / finalProgress.TotalBackupTime.TotalSeconds):F2} KB/s");
            Console.WriteLine($"  État du job          : {job.status.Status}");
            Console.WriteLine(new string('=', 60));

            Console.ReadKey();
        }
    }
}