<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Models;

public class BackupJob
{
	public string Name { get; set; } = string.Empty;
	public string SourcePath { get; set; } = string.Empty;
	public string TargetPath { get; set; } = string.Empty;
	public BackupType Type { get; set; }
}
=======
﻿namespace EasySave.Models
{
    public enum BackupType { COMPLET, DIFFERENTIAL } // COMPLET matches diagram's 'COMPLET'
    public enum BackupStatus { INACTIVE, ACTIVE, ERRROR, FINISHED } // Matches diagram states

    public class BackupJob
    {
        public string Name { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public BackupType Type { get; set; }
        public BackupStatus Status { get; set; } = BackupStatus.INACTIVE;
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int Progress { get; set; }
    }
}
>>>>>>> origin/dev
