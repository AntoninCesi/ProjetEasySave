using System;

namespace Tool.Utils
{
	public enum BackupTypes { FULL, DIFFERENTIAL }

	/// <summary>
	/// Backup states (compatibilité maximale entre anciennes et nouvelles branches).
	/// OFF/ON/END = legacy
	/// ACTIVE/FINISHED/INACTIVE = aliases pour le nouveau code
	/// </summary>
	public enum BackupStateResum
	{
		// Legacy
		OFF = 0,
		ON = 1,
		PAUSED = 2,
		ERROR = 3,
		END = 4,

		// Aliases (nouveau code / autres branches)
		INACTIVE = OFF,
		ACTIVE = ON,
		FINISHED = END
	}

    public enum LogStorageMode
    {
        Local,
        External,
        Both
    }
}