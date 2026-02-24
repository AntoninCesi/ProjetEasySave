using System;
using System.Collections.Generic;
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

		// (Je conserve ta méthode, mais sécurisée)
		public void ExecuteJobAsyncExecuteJob(int jobId)
		{
			var job = getJobById(jobId);
			if (job == null) return;

			BackupStrategyFactory.ExecuteBackup(job);
		}

		public List<BackupJob> getBackupJobList()
		{
			return _listBackupJob;
		}

		public BackupJob getJobById(int jobId)
		{
			if (jobId < 0 || jobId >= _listBackupJob.Count)
				return null;

			return _listBackupJob[jobId];
		}

		public async Task ExecuteJob(int jobId)
		{
			var job = getJobById(jobId);
			if (job == null)
			{
				Console.WriteLine($"Job {jobId} introuvable !");
				return;
			}

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

<<<<<<< HEAD
                    job.status.Status = BackupStateResum.ON;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"✅ Job {job.name} terminé avec succès !");
                }
                catch (OperationCanceledException ex)
                {
                    // Exception levée si logiciel métier détecté PENDANT la sauvegarde
                    job.status.Status = BackupStateResum.ERROR;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"⏹️  Job {job.name} arrêté : {ex.Message}");
                }
                catch (Exception ex)
                {
                    job.status.Status = BackupStateResum.ERROR;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"❌ Erreur dans le job {job.name} : {ex.Message}");
                }
            });
        }
=======
			await Task.Run(() =>
			{
				try
				{
					Console.WriteLine($"▶️  Démarrage du job {job.name}");
>>>>>>> feature/dlltype2

					// ✅ LOG: job started
					LogService.Instance.JobStarted(job.name);

					BackupStrategyFactory.ExecuteBackup(job);

					job.status.Status = BackupStateResum.FINISHED;
					job.status.LastActionTimestamp = DateTime.Now;

					// ✅ LOG: job completed
					LogService.Instance.JobCompleted(job.name);

					Console.WriteLine($"✅ Job {job.name} terminé avec succès !");
				}
				catch (OperationCanceledException ex)
				{
					job.status.Status = BackupStateResum.ERROR;
					job.status.LastActionTimestamp = DateTime.Now;

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

					LogService.Instance.LogBusinessSoftwareEvent(
						job.name,
						$"Backup error: {ex.Message}"
					);

					Console.WriteLine($"❌ Erreur dans le job {job.name} : {ex.Message}");
				}
			});
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