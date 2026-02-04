using System;
using System.IO;

namespace EasySave.Models 
{

    public enum StorageType { LOCAL, EXTERNAL, NETWORK }

    public class BackupMedium
    {
        public StorageType storageType { get; set; }
        private string _drivePath; 

        public BackupMedium(string path, StorageType type)
        {
            _drivePath = Path.GetPathRoot(path);
            storageType = type;
        }

        public long availableSpace() // Correspond à availableSpace() : long
        {
            DriveInfo drive = new DriveInfo(_drivePath);
            return drive.IsReady ? drive.AvailableFreeSpace : 0;
        }

        public bool isConnected() // Correspond à isConnected() : bool
        {
            DriveInfo drive = new DriveInfo(_drivePath);
            return drive.IsReady;
        }
    }
}