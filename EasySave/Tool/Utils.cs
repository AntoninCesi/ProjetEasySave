using System;

namespace Tool.Utils
{
    public enum BackupTypes { FULL, DIFFERENTIAL } // COMPLET matches diagram's 'COMPLET'
    public enum BackupStateResum { INACTIVE, ACTIVE, ERROR, FINISHED } // Matches diagram states
}