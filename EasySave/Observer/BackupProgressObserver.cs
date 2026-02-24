using System;
using System.Threading;
using EasySave.Models;
using Tool.Utils;

namespace EasySave.Strategies
{
	/// <summary>
	/// Observateur simplifié pour suivre la progression des sauvegardes
	/// Stocke : nombre de fichiers, taille totale, temps
	/// </summary>
	public class BackupProgressObserver
	{
		private readonly object _lock = new object();

		private int _filesSaved;
		private long _totalSize;
		private DateTime _backupStartTime;

		// Événement de progression
		public event Action<FileInfos> OnProgressChanged;

		// ✅ Événement de changement de statut (attendu par l'UI)
		public event Action<BackupStateResum> OnStatusChanged;

		// Événement d’erreur
		public event Action<string> OnErrorOccurred;

		public BackupProgressObserver()
		{
			_backupStartTime = DateTime.Now;
		}

		// ✅ Méthode utilitaire (facultative) pour déclencher OnStatusChanged
		public void NotifyStatusChanged(BackupStateResum newStatus)
		{
			ThreadPool.QueueUserWorkItem(_ =>
				OnStatusChanged?.Invoke(newStatus)
			);
		}

		public void NotifyFileSaved(long fileSize, TimeSpan fileDuration)
		{
			FileInfos progress;

			lock (_lock)
			{
				_filesSaved++;
				_totalSize += fileSize;

				progress = new FileInfos
				{
					FilesSaved = _filesSaved,
					TotalSize = _totalSize,
					TotalBackupTime = DateTime.Now - _backupStartTime,
					LastFileDuration = fileDuration
				};
			}

			// Notification asynchrone hors lock
			ThreadPool.QueueUserWorkItem(_ =>
				OnProgressChanged?.Invoke(progress)
			);
		}

		public void NotifyError(string message)
		{
			ThreadPool.QueueUserWorkItem(_ =>
				OnErrorOccurred?.Invoke(message)
			);
		}

		public FileInfos GetProgress()
		{
			lock (_lock)
			{
				return new FileInfos
				{
					FilesSaved = _filesSaved,
					TotalSize = _totalSize,
					TotalBackupTime = DateTime.Now - _backupStartTime,
					LastFileDuration = TimeSpan.Zero
				};
			}
		}

		public void Reset()
		{
			lock (_lock)
			{
				_filesSaved = 0;
				_totalSize = 0;
				_backupStartTime = DateTime.Now;
			}
		}
	}
}