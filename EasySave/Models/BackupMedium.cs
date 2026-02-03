using System;
using System.IO;

namespace EasySave
{
    public enum StorageType { Local, Externe, Reseau }

    public class BackupMedium
    {
        public StorageType storageType { get; set; }
        private string _drivePath;

        public BackupMedium(string path, StorageType type)
        {
            _drivePath = Path.GetPathRoot(path);
            storageType = type;
        }

        public long availableSpace()
        {
            DriveInfo drive = new DriveInfo(_drivePath);
            return drive.IsReady ? drive.AvailableFreeSpace : 0;
        }

        public bool isConnected()
        {
            DriveInfo drive = new DriveInfo(_drivePath);
            return drive.IsReady;
        }
    }
}