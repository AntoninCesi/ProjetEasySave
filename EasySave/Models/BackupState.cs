using EasySave.Models;
using System;
using Tool.Utils;
using System.ComponentModel;




namespace EasySave.Models
{
    public class BackupState : INotifyPropertyChanged
    {
        //1methode

        private BackupStateResum _status;

        public BackupStateResum Status
        {
            get => _status;
            set
            {
                _status = value;
                // Cette ligne dit à la MainWindow : "Hé, la colonne State doit être rafraîchie !"
                OnPropertyChanged(nameof(Status));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        //2methode
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        //3methode
        public DateTime LastActionTimestamp { get; set; }
        public int RemainingFiles { get; set; }
        public long RemainingSize { get; set; }
        public int Progress { get; set; }
        //4methode
        public string SourcePath { get; set; } = string.Empty;
        //5methode
        public string DestinationPath { get; set; } = string.Empty;
    }
}