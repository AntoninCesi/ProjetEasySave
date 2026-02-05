using System;

namespace EasySave.Models
{
    public class FileInfo
    {
        public string fileName { get; set; }
        public string filePath { get; set; }
        public long fileSize { get; set; }
        public bool isDirectory { get; set; }
        public DateTime lastModified { get; set; }
    }
}