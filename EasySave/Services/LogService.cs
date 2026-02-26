using System;
using System.Collections.Generic;
using System.IO;
using EasyLog;

namespace EasySave.Services
{
	public sealed class LogService
	{
		private static LogService? _instance;

		public static LogService Instance => _instance ??= new LogService();

		private EasyLogger _logger;

		private LogService()
		{
			_logger = CreateLoggerFromSettings();
		}

		/// <summary>
		/// Recrée le logger en relisant AppSettings (appelé quand on Save les Settings).
		/// </summary>
		public void ReloadFromSettings()
		{
			_logger = CreateLoggerFromSettings();
		}

        private static EasyLogger CreateLoggerFromSettings()
        {
            var s = Models.AppSettings.Instance;
            string fmt = s.LogFormat ?? "JSON";
            LogFormat format = fmt.Equals("XML", StringComparison.OrdinalIgnoreCase)
                ? LogFormat.Xml : LogFormat.Json;

            string baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            var sinks = new List<ILogSink>();

            bool useLocal = s.LogDestination is "Local" or "Both";
            bool useDocker = s.LogDestination is "Docker" or "Both";


            if (useLocal)
                sinks.Add(new LocalFileSink(baseFolder));
            if (useDocker)
                sinks.Add(new DockerSocketSink(s.DockerHost, s.DockerPort));

            return new EasyLogger(format, sinks.ToArray());
        }

        /// <summary>
		/// Log d'un fichier copié (signature conservée)
		/// </summary>
		public void WriteLog(string jobName, string source, string target, long fileSize, long transferTime)
		{
			_logger.Write(LogEvent.FileCopied(
				jobName ?? "",
				source ?? "",
				target ?? "",
				fileSize,
				transferTime
			));
		}

		/// <summary>
		/// Log d'événement "business software" (sans transfert de fichier)
		/// </summary>
		public void LogBusinessSoftwareEvent(string jobName, string eventMessage)
		{
			// Si tu veux un type dédié "BusinessEvent", crée-le dans EasyLog.
			// En attendant, on loggue comme Error/Info selon ton modèle.
			_logger.Write(LogEvent.Error(
				jobName ?? "",
				eventMessage ?? ""
			));
		}

		// Optionnel : logs "début/fin job"
		public void JobStarted(string jobName) => _logger.Write(LogEvent.JobStarted(jobName ?? ""));
		public void JobCompleted(string jobName) => _logger.Write(LogEvent.JobCompleted(jobName ?? ""));
	}
}