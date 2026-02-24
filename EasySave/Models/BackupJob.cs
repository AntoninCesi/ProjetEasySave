using Tool.Utils;
using EasySave.Strategies;
using System.Threading;

namespace EasySave.Models
{
    public class BackupJob
    {
        public string name { get; set; } = string.Empty;
        public string sourcePath { get; set; } = string.Empty;
        public string destinationPath { get; set; } = string.Empty;
        public BackupTypes type { get; set; }
        public BackupState status { get; set; } = new BackupState();

        // Observator
        public BackupProgressObserver progressObserver { get; set; } = new BackupProgressObserver();


        //Ajout de Pause et Cancel
        public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
        public ManualResetEventSlim PauseEvent { get; set; } = new ManualResetEventSlim(true); // true = pas en pause au départ
        public bool IsCancelled => CancellationTokenSource?.IsCancellationRequested ?? false;
        public bool IsPaused => !(PauseEvent?.IsSet ?? true);

        public override string ToString()
        {
            return
                $"Backup Job : {name}\n" +
                $"Type : {type}\n" +
                $"Source : {sourcePath}\n" +
                $"Destination : {destinationPath}\n" +
                $"Status : {status.Status}\n";
        }

        // Méthode de nettoyage pour libérer les ressources
        public void Dispose()
        {
            CancellationTokenSource?.Dispose();
            PauseEvent?.Dispose();
        }
    }
}