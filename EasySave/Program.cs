using System;
using System.Collections.Generic;
using EasySave.ViewModels;
using System;
using System.Collections.Generic;
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
<<<<<<< HEAD
            Console.WriteLine("=== Test Temps Réel ===\n");
=======
            //Controller controller = new Controller(args);
>>>>>>> ec736776ddace35e3bd30d76ac2639d02c907c9a

            // Créer la stratégie
            FullBackupStrategy strategy = new FullBackupStrategy();

            // S'abonner aux événements
            strategy.GetProgressObserver().OnProgressChanged += (info) =>
            {
                Console.WriteLine($"[{info.ProgressPercentage,3}%] {info.CurrentFile?.fileName ?? "N/A"}");
            };

            // Créer et exécuter
            BackupJob job = new BackupJob
            {
                sourcePath = @"C:\Users\ademr\Downloads",
                destinationPath = @"C:\DestTEST",
                type = BackupTypes.FULL,
                status = new BackupState()
            };

            strategy.Execute(job);
            Console.WriteLine("\n✅ Terminé !");
            Console.ReadKey();
        }
    }
}