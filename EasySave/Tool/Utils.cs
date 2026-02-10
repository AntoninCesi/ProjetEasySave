using System;

namespace Tool.Utils
{
    public enum BackupType { COMPLET, DIFFERENTIAL } // COMPLET matches diagram's 'COMPLET'
    public enum BackupStateResum { INACTIVE, ACTIVE, ERRROR, FINISHED } // Matches diagram states
}
