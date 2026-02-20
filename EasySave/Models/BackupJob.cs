using Tool.Utils;
using EasySave.Strategies;

namespace EasySave.Models
{
    public class BackupJob
    {
        public string name { get; set; } = string.Empty;
        public string sourcePath { get; set; } = string.Empty;
        public string destinationPath { get; set; } = string.Empty;
        public BackupTypes type { get; set; }
        public BackupState status { get; set; } = new BackupState();

        // Observateur de progression propre à ce job
        public BackupProgressObserver progressObserver { get; set; } = new BackupProgressObserver();

        public override string ToString()
        {
            return
                $"Backup Job : {name}\n" +
                $"Type : {type}\n" +
                $"Source : {sourcePath}\n" +
                $"Destination : {destinationPath}\n" +
                $"Status : {status.Status}\n";
        }
    }
}