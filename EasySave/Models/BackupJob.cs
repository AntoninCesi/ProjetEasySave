<<<<<<< HEAD

﻿using System;
using System.Collections.Generic;
using System.Text;
=======
﻿using Tool.Utils;
>>>>>>> 14659d94d58ab26d4cb9bb15b8df1a30796879bf

namespace EasySave.Models
{
<<<<<<< HEAD
	public string Name { get; set; } = string.Empty;
	public string SourcePath { get; set; } = string.Empty;
	public string TargetPath { get; set; } = string.Empty;
	public BackupType Type { get; set; }
}

﻿namespace EasySave.Models
{
    public enum BackupType { COMPLET, DIFFERENTIAL } // COMPLET matches diagram's 'COMPLET'
    public enum BackupStatus { INACTIVE, ACTIVE, ERRROR, FINISHED } // Matches diagram states

=======
>>>>>>> 14659d94d58ab26d4cb9bb15b8df1a30796879bf
    public class BackupJob
    {
        public string name { get; set; } = string.Empty;
        public string sourcePath { get; set; } = string.Empty;
        public string destinationPath { get; set; } = string.Empty;
        public BackupType type { get; set; }
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
<<<<<<< HEAD

=======
>>>>>>> 14659d94d58ab26d4cb9bb15b8df1a30796879bf
