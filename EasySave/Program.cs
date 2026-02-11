
using EasySave.ViewModels;
using System;
using EasySave.Models;
using EasySave.Strategies;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("= Test de la Factory =\n");

            // Créer un job de sauvegarde
            BackupJob job = new BackupJob
            {
                sourcePath = @"C:\SourceTEST",        
                destinationPath = @"C:\DestTEST"    
            };

            /* 
            Exécuter une sauvegarde complète
            Console.WriteLine("Exécution de la sauvegarde complète...");
            BackupStrategyFactory.ExecuteBackup(BackupType.Full, job);
            Console.WriteLine("Sauvegarde terminée !");

            Console.ReadKey();
            
             
            */

             //Exécuter une sauvegarde différentielle 
            Console.WriteLine("Exécution de la sauvegarde diff...");
            BackupStrategyFactory.ExecuteBackup(BackupType.Differential, job);
            Console.WriteLine("Sauvegarde terminée !");

            

            BackupJob job2 = new BackupJob
            {
                sourcePath = @"C:\SourceTEST2",
                destinationPath = @"C:\DestTEST2"



            };

            Console.WriteLine("Exécution de la sauvegarde complète...");
            BackupStrategyFactory.ExecuteBackup(BackupType.Full, job2);
            Console.WriteLine("Sauvegarde terminée !");
            Console.ReadKey();


        }
}
}






/*class Program
{
    static void Main(string[] args)
    {
        Controller controller = new Controller(args);
        
    }
        
}*/

