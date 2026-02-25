using System;
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
			// Dossier logs : bin/.../Logs
			string baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
			Directory.CreateDirectory(baseFolder);

			// Format depuis les settings (ex: "JSON" / "XML")
			string fmt = Models.AppSettings.Instance.LogFormat ?? "JSON";

			LogFormat format = fmt.Equals("XML", StringComparison.OrdinalIgnoreCase)
				? LogFormat.Xml
				: LogFormat.Json;

			return new EasyLogger(format, baseFolder);
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