using System;

namespace Tool.Utils
{
    public enum BackupTypes { FULL, DIFFERENTIAL } // COMPLET matches diagram's 'COMPLET'
<<<<<<< HEAD
<<<<<<< HEAD
    public enum BackupStateResum { OFF, ON, ERROR, END } // Matches diagram states
=======
    public enum BackupStateResum { OFF, ON, PAUSED, ERROR, END }
>>>>>>> 29742782d5f8c515a37c523f4200e5c09c305aa9
}
=======
    public enum BackupStateResum { INACTIVE, ACTIVE, ERROR, FINISHED } // Matches diagram states
}
>>>>>>> feature/dlltype2
