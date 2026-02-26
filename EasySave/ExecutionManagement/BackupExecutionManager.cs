using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.Services;
using EasySave.StateManagement;
using EasySave.Strategies;
using Tool.Utils;

namespace EasySave.ExecutionManagement
{
	public class BackupExecutionManager
	{
		private readonly List<BackupJob> _listBackupJob = new();

		// IMPORTANT : on conserve les state managers pour éviter qu’ils soient GC collectés
		private readonly List<BackupStateManager> _stateManagers = new();

		/// <summary>
		/// Semaphore shared across all parallel jobs.
		/// Ensures only one large file (> MaxParallelFileSizeKo) is transferred at a time.
		/// </summary>
		private readonly SemaphoreSlim _largeFileSemaphore = new(1, 1);

		/// <summary>
		/// Counter shared across all parallel jobs.
		/// Tracks how many priority files are still pending globally.
		/// Non-priority transfers must wait until this reaches zero.
		/// </summary>
		private int _pendingPriorityFiles = 0;

		public void createBackupJob(string name, string sourcePath, string destinationPath, BackupTypes type)
		{
			var job = new BackupJob
			{
				name = name,
				sourcePath = sourcePath,
				destinationPath = destinationPath,
				type = type,
				progressObserver = new BackupProgressObserver()
			};

			_listBackupJob.Add(job);

			// Crée et conserve un StateManager pour suivre ce job (UI / observer)
			_stateManagers.Add(new BackupStateManager(job));

			int jobId = _listBackupJob.Count - 1;
			Console.WriteLine(_listBackupJob[jobId].ToString());
		}

		public List<BackupJob> getBackupJobList() => _listBackupJob;

		public BackupJob getJobById(int jobId)
		{
			if (jobId < 0 || jobId >= _listBackupJob.Count)
				return null;

			return _listBackupJob[jobId];
		}

		public int getJobCount() => _listBackupJob.Count;

		public void deleteBackupJob(int jobId)
		{
			if (jobId < 0 || jobId >= _listBackupJob.Count)
				return;

			var job = _listBackupJob[jobId];

			// Nettoyer les ressources si Dispose() existe
			try { job.Dispose(); } catch { }

			_listBackupJob.RemoveAt(jobId);
			Console.WriteLine($"Job supprimé de la liste (index {jobId})");
		}

		// ---------------------------
		// Pause / Resume / Stop (JOB)
		// ---------------------------

		public void PauseJob(int jobId)
		{
			var job = getJobById(jobId);
			if (job == null) return;

			if (job.status.Status == BackupStateResum.ACTIVE)
			{
				job.PauseEvent.Reset(); // pause
				job.status.Status = BackupStateResum.PAUSED;

				// compat (UpdateStatus alias -> NotifyStatusChanged)
				job.progressObserver.UpdateStatus(BackupStateResum.PAUSED);

				Console.WriteLine($"⏸️  Job {job.name} mis en pause");
			}
		}

		public void ResumeJob(int jobId)
		{
			var job = getJobById(jobId);
			if (job == null) return;

			if (job.status.Status == BackupStateResum.PAUSED)
			{
				job.PauseEvent.Set(); // resume
				job.status.Status = BackupStateResum.ACTIVE;
				job.progressObserver.UpdateStatus(BackupStateResum.ACTIVE);

				Console.WriteLine($"▶️  Job {job.name} repris");
			}
		}

		public void StopJob(int jobId)
		{
			var job = getJobById(jobId);
			if (job == null) return;

			if (job.status.Status == BackupStateResum.ACTIVE || job.status.Status == BackupStateResum.PAUSED)
			{
				job.CancellationTokenSource.Cancel(); // stop
				job.PauseEvent.Set(); // éviter blocage si pause
				Console.WriteLine($"⏹️  Job {job.name} arrêté");
			}
		}

		// ---------------------------
		// Pause / Resume / Stop (ALL)
		// ---------------------------

		public void PauseAllJobs()
		{
			for (int i = 0; i < _listBackupJob.Count; i++)
				PauseJob(i);

			Console.WriteLine("⏸️  Tous les jobs actifs mis en pause");
		}

		public void ResumeAllJobs()
		{
			for (int i = 0; i < _listBackupJob.Count; i++)
				ResumeJob(i);

			Console.WriteLine("▶️  Tous les jobs en pause repris");
		}

		public void StopAllJobs()
		{
			for (int i = 0; i < _listBackupJob.Count; i++)
				StopJob(i);

			Console.WriteLine("⏹️  Tous les jobs arrêtés");
		}

		// ---------------------------
		// Execution
		// ---------------------------

		// (Je conserve ta méthode, mais sécurisée)
		public void ExecuteJobAsyncExecuteJob(int jobId)
		{
			var job = getJobById(jobId);
			if (job == null) return;

			BackupStrategyFactory.ExecuteBackup(job);
		}

		public async Task ExecuteJob(int jobId)
		{
			var job = getJobById(jobId);
			if (job == null)
			{
				Console.WriteLine($"Job {jobId} introuvable !");
				return;
			}

			// Réinitialiser les contrôles pour une nouvelle exécution
			job.CancellationTokenSource = new CancellationTokenSource();
			job.PauseEvent = new ManualResetEventSlim(true); // non-paused

			// ⚠️ Vérification logiciel métier AVANT démarrage
			if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
			{
				Console.WriteLine($"❌ Impossible de démarrer {job.name} : logiciel métier en cours d'exécution");

				LogService.Instance.LogBusinessSoftwareEvent(
					job.name,
					"Backup launch blocked - Business software is running"
				);

				return;
			}

			await Task.Run(() =>
			{
				try
				{
					Console.WriteLine($"▶️  Démarrage du job {job.name}");

					job.status.Status = BackupStateResum.ACTIVE;
					job.status.LastActionTimestamp = DateTime.Now;
					job.progressObserver.UpdateStatus(BackupStateResum.ACTIVE);

					// ✅ LOG: job started
					LogService.Instance.JobStarted(job.name);

					// Utiliser la stratégie "parallèle" si la factory le supporte
					// (signature: CreateStrategy(type, semaphore, ref pending))
					IBackupStrategy strategy = BackupStrategyFactory.CreateStrategy(
						job.type,
						_largeFileSemaphore,
						ref _pendingPriorityFiles
					);

					strategy.Execute(job);

					job.status.Status = BackupStateResum.FINISHED;
					job.status.LastActionTimestamp = DateTime.Now;
					job.progressObserver.UpdateStatus(BackupStateResum.FINISHED);

					// ✅ LOG: job completed
					LogService.Instance.JobCompleted(job.name);

					Console.WriteLine($"✅ Job {job.name} terminé avec succès !");
				}
				catch (OperationCanceledException ex)
				{
					job.status.Status = BackupStateResum.ERROR;
					job.status.LastActionTimestamp = DateTime.Now;
					job.progressObserver.UpdateStatus(BackupStateResum.ERROR);

					LogService.Instance.LogBusinessSoftwareEvent(
						job.name,
						$"Backup canceled: {ex.Message}"
					);

					Console.WriteLine($"⏹️  Job {job.name} arrêté : {ex.Message}");
				}
				catch (Exception ex)
				{
					job.status.Status = BackupStateResum.ERROR;
					job.status.LastActionTimestamp = DateTime.Now;
					job.progressObserver.UpdateStatus(BackupStateResum.ERROR);

					LogService.Instance.LogBusinessSoftwareEvent(
						job.name,
						$"Backup error: {ex.Message}"
					);

					Console.WriteLine($"❌ Erreur dans le job {job.name} : {ex.Message}");
				}
			}, job.CancellationTokenSource.Token);
		}

		public async Task ExecuteAllJob()
		{
			var tasks = new List<Task>();

			for (int i = 0; i < _listBackupJob.Count; i++)
				tasks.Add(ExecuteJob(i));

			await Task.WhenAll(tasks);
		}
	}
}