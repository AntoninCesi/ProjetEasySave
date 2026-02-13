using System;
using System.Diagnostics;
using System.Linq;

namespace EasySave.Services
{
    /// <summary>
    /// Service to monitor and detect if business-critical software is running.
    /// Implements Singleton pattern for centralized process monitoring.
    /// </summary>
    public class BusinessSoftwareMonitor
    {
        private static BusinessSoftwareMonitor? _instance;
        
        /// <summary>
        /// Gets the singleton instance of BusinessSoftwareMonitor
        /// </summary>
        public static BusinessSoftwareMonitor Instance => _instance ??= new BusinessSoftwareMonitor();

        /// <summary>
        /// Private constructor to enforce Singleton pattern
        /// </summary>
        private BusinessSoftwareMonitor() { }

        /// <summary>
        /// Checks if the configured business software is currently running
        /// </summary>
        /// <param name="processName">Name of the process to check (without .exe)</param>
        /// <returns>True if the software is running, false otherwise</returns>
        public bool IsRunning(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return false;

            try
            {
                // Get all processes with matching name
                var processes = Process.GetProcessesByName(processName);
                bool isRunning = processes.Length > 0;

                // Clean up process objects
                foreach (var process in processes)
                {
                    process.Dispose();
                }

                return isRunning;
            }
            catch (Exception ex)
            {
                // Log error but don't block backups if we can't check
                Console.WriteLine($"Error checking business software: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if business software from app settings is running
        /// </summary>
        /// <returns>True if configured business software is running</returns>
        public bool IsBusinessSoftwareRunning()
        {
            var settings = Models.AppSettings.Instance;
            return IsRunning(settings.BusinessSoftware);
        }

        /// <summary>
        /// Gets the list of all running processes (for debugging)
        /// </summary>
        /// <returns>Array of process names currently running</returns>
        public string[] GetRunningProcesses()
        {
            try
            {
                return Process.GetProcesses()
                    .Select(p => p.ProcessName)
                    .OrderBy(name => name)
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }
}
