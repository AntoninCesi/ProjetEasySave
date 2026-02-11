using System.Collections.ObjectModel;
using EasySave.Models;

namespace EasySave.WPF.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<BackupJob> BackupJobs { get; set; }

        public MainViewModel()
        {
            BackupJobs = new ObservableCollection<BackupJob>();

            // On ajoute des données manuellement pour vérifier que le tableau marche
            BackupJobs.Add(new BackupJob { name = "Test_Full", sourcePath = "C:/Source", destinationPath = "D:/Dest" });
            BackupJobs.Add(new BackupJob { name = "Test_Diff", sourcePath = "C:/Data", destinationPath = "D:/Backup" });
        }
    }
}