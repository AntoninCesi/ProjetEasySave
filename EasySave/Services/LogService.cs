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
			// Dossier logs : reste cohérent avec ce que tu avais (bin/.../Logs)
			string baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
			Directory.CreateDirectory(baseFolder);

			// On lit le format depuis les settings (string "JSON"/"XML")
			string fmt = Models.AppSettings.Instance.LogFormat ?? "JSON";

			LogFormat format = fmt.Equals("XML", StringComparison.OrdinalIgnoreCase)
				? LogFormat.Xml
				: LogFormat.Json;

			return new EasyLogger(format, baseFolder);
		}

		/// <summary>
		/// Log d'un fichier copié (signature conservée pour compatibilité avec ton code existant)
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
		/// Log d'événement "business software"
		/// </summary>
		public void LogBusinessSoftwareEvent(string jobName, string eventMessage)
		{
			_logger.Write(LogEvent.Error(
				jobName ?? "",
				eventMessage ?? ""
			));
		}

		// Optionnel : logs "début/fin job" (pratique si tu veux les ajouter vite)
		public void JobStarted(string jobName) => _logger.Write(LogEvent.JobStarted(jobName ?? ""));
		public void JobCompleted(string jobName) => _logger.Write(LogEvent.JobCompleted(jobName ?? ""));
	}
}