using System;
using System.Threading;
using EasySave.Models;
using Tool.Utils;

namespace EasySave.Strategies
{
    /// <summary>
    /// Factory for creating and executing backup strategies.
    /// Accepts shared parallel controls to inject into strategies via constructor.
    /// </summary>
    public class BackupStrategyFactory
    {
        /// <summary>
        /// Creates a strategy and immediately executes it.
        /// Used for single-job execution without parallel controls.
        /// </summary>
        public static void ExecuteBackup(BackupJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            CreateStrategy(job.type).Execute(job);
        }

        /// <summary>
        /// Creates a strategy without parallel controls (sequential use).
        /// </summary>
        public static IBackupStrategy CreateStrategy(BackupTypes backupType)
        {
            return backupType switch
            {
                BackupTypes.FULL => new FullBackupStrategy(),
                BackupTypes.DIFFERENTIAL => new DifferentialBackupStrategy(),
                _ => throw new ArgumentException($"Unsupported backup type: {backupType}")
            };
        }

        /// <summary>
        /// Creates a strategy with shared parallel controls injected via constructor.
        /// Used by BackupExecutionManager when running multiple jobs in parallel.
        /// </summary>
        public static IBackupStrategy CreateStrategy(
            BackupTypes backupType,
            SemaphoreSlim largeFileSemaphore,
            ref int pendingPriorityFiles)
        {
            return backupType switch
            {
                BackupTypes.FULL => new FullBackupStrategy(largeFileSemaphore, ref pendingPriorityFiles),
                BackupTypes.DIFFERENTIAL => new DifferentialBackupStrategy(largeFileSemaphore, ref pendingPriorityFiles),
                _ => throw new ArgumentException($"Unsupported backup type: {backupType}")
            };
        }
    }
}
