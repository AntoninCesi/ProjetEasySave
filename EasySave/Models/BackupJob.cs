using Tool.Utils;
<<<<<<< Updated upstream
=======
using EasySave.Strategies;
>>>>>>> Stashed changes

namespace EasySave.Models
{
    public class BackupJob
    {
        public string name { get; set; } = string.Empty;
        public string sourcePath { get; set; } = string.Empty;
        public string destinationPath { get; set; } = string.Empty;
        public BackupTypes type { get; set; }
        public BackupState status { get; set; } = new BackupState();
        public int totalFiles { get; set; }
        public long totalSize { get; set; }

        public override string ToString()
        {
            return
                $"Backup Job : {name}\n" +
                $"Type : {type}\n" +
                $"Source : {sourcePath}\n" +
                $"Destination : {destinationPath}\n" +
                $"Status : {status}\n" +
                $"Total files : {totalFiles}\n" +
                $"Total size : {totalSize} bytes\n";
        }
    }
}