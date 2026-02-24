using System;
<<<<<<< HEAD
using System.Collections.Generic;
using System.Threading.Tasks;
=======
using System.Threading;
>>>>>>> 29742782d5f8c515a37c523f4200e5c09c305aa9
using EasySave.Models;
using EasySave.Services;
using EasySave.StateManagement;
using EasySave.Strategies;
using Tool.Utils;

namespace EasySave.ExecutionManagement
{
<<<<<<< HEAD
	public class BackupExecutionManager
	{
		private readonly List<BackupJob> _listBackupJob = new();

		// IMPORTANT : on conserve les state managers pour éviter qu’ils soient GC collectés
		private readonly List<BackupStateManager> _stateManagers = new();
=======
    public class BackupExecutionManager
    {
        private readonly List<BackupJob> _listBackupJob = new();

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
>>>>>>> 29742782d5f8c515a37c523f4200e5c09c305aa9

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

<<<<<<< HEAD
			_listBackupJob.Add(job);
=======
            var stateManager = new BackupStateManager(job);
>>>>>>> 29742782d5f8c515a37c523f4200e5c09c305aa9

			// Crée et conserve un StateManager pour suivre ce job (UI / observer)
			_stateManagers.Add(new BackupStateManager(job));

<<<<<<< HEAD
			int jobId = _listBackupJob.Count - 1;
			Console.WriteLine(_listBackupJob[jobId].ToString());
		}
=======
        public List<BackupJob> getBackupJobList() => _listBackupJob;

        /// Supprime un job de la liste
       
        public void deleteBackupJob(int jobId)
        {
            if (jobId >= 0 && jobId < _listBackupJob.Count)
            {
                var job = _listBackupJob[jobId];

                // Nettoyer les ressources
                job.Dispose();

                _listBackupJob.RemoveAt(jobId);
                Console.WriteLine($"Job supprimé de la liste (index {jobId})");
            }
        }

        /// Retourne le nombre de jobs
        public int getJobCount()
        {
            return _listBackupJob.Count;
        }


        /// Met en pause un job en cours d'exécution

        public void PauseJob(int jobId)
        {
            var job = getJobById(jobId);
            if (job == null) return;

            if (job.status.Status == BackupStateResum.ON)
            {
                job.PauseEvent.Reset(); // Mettre en pause
                job.status.Status = BackupStateResum.PAUSED;
                job.progressObserver.UpdateStatus(BackupStateResum.PAUSED);
                Console.WriteLine($" Job {job.name} mis en pause");
            }
        }

      
        /// Reprend un job en pause
        
        public void ResumeJob(int jobId)
        {
            var job = getJobById(jobId);
            if (job == null) return;

            if (job.status.Status == BackupStateResum.PAUSED)
            {
                job.PauseEvent.Set(); // Reprendre
                job.status.Status = BackupStateResum.ON;
                job.progressObserver.UpdateStatus(BackupStateResum.ON);
                Console.WriteLine($" Job {job.name} repris");
            }
        }

        
        /// Arrête complètement un job
        
        public void StopJob(int jobId)
        {
            var job = getJobById(jobId);
            if (job == null) return;

            if (job.status.Status == BackupStateResum.ON || job.status.Status == BackupStateResum.PAUSED)
            {
                job.CancellationTokenSource.Cancel(); // Annuler
                job.PauseEvent.Set(); // S'assurer que le job n'est pas bloqué en pause
                Console.WriteLine($"Job {job.name} arrêté");
            }
        }

        
        /// Met en pause TOUS les jobs en cours
        
        public void PauseAllJobs()
        {
            for (int i = 0; i < _listBackupJob.Count; i++)
            {
                if (_listBackupJob[i].status.Status == BackupStateResum.ON)
                {
                    PauseJob(i);
                }
            }
            Console.WriteLine("⏸️  Tous les jobs actifs mis en pause");
        }

        
        /// Reprend TOUS les jobs en pause
        
        public void ResumeAllJobs()
        {
            for (int i = 0; i < _listBackupJob.Count; i++)
            {
                if (_listBackupJob[i].status.Status == BackupStateResum.PAUSED)
                {
                    ResumeJob(i);
                }
            }
            Console.WriteLine("▶️  Tous les jobs en pause repris");
        }

        
        /// Arrête TOUS les jobs en cours
        
        public void StopAllJobs()
        {
            for (int i = 0; i < _listBackupJob.Count; i++)
            {
                if (_listBackupJob[i].status.Status == BackupStateResum.ON ||
                    _listBackupJob[i].status.Status == BackupStateResum.PAUSED)
                {
                    StopJob(i);
                }
            }
            Console.WriteLine("⏹️  Tous les jobs arrêtés");
        }







        public void ExecuteJobAsyncExecuteJob(int jobId)
        {
            BackupStrategyFactory.ExecuteBackup(getJobById(jobId));
>>>>>>> 29742782d5f8c515a37c523f4200e5c09c305aa9

		// (Je conserve ta méthode, mais sécurisée)
		public void ExecuteJobAsyncExecuteJob(int jobId)
		{
			var job = getJobById(jobId);
			if (job == null) return;

<<<<<<< HEAD
			BackupStrategyFactory.ExecuteBackup(job);
		}

		public List<BackupJob> getBackupJobList()
		{
			return _listBackupJob;
		}
=======
        public BackupJob getJobById(int jobId)
        {
            if (jobId < 0 || jobId >= _listBackupJob.Count)
                return null;
>>>>>>> 29742782d5f8c515a37c523f4200e5c09c305aa9

		public BackupJob getJobById(int jobId)
		{
			if (jobId < 0 || jobId >= _listBackupJob.Count)
				return null;

<<<<<<< HEAD
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
=======
        public async Task ExecuteJob(int jobId)
        {
            var job = getJobById(jobId);
            if (job == null)
            {
                Console.WriteLine($"Job {jobId} not found!");
                return;
            }

            // Réinitialiser les contrôles pour une nouvelle exécution
            job.CancellationTokenSource = new CancellationTokenSource();
            job.CancellationTokenSource = new CancellationTokenSource();
            job.PauseEvent = new ManualResetEventSlim(true);

            // VÉRIFICATION DU LOGICIEL MÉTIER AVANT DE DÉMARRER 
            if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
            {
                Console.WriteLine($"Impossible de démarrer {job.name} : logiciel métier en cours d'exécution");

                // Logger l'événement

                LogService.Instance.LogBusinessSoftwareEvent(job.name,
                    "Backup launch blocked - Business software is running");
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    Console.WriteLine($"Starting job {job.name}");

                    // Pass shared controls directly via constructor
                    IBackupStrategy strategy = BackupStrategyFactory.CreateStrategy(
                        job.type,
                        _largeFileSemaphore,
                        ref _pendingPriorityFiles
                    );

                    strategy.Execute(job);

>>>>>>> 29742782d5f8c515a37c523f4200e5c09c305aa9

<<<<<<< HEAD
                    job.status.Status = BackupStateResum.ON;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"Job {job.name} completed successfully!");

                    // Si on arrive ici sans annulation, c'est un succès
                    if (!job.IsCancelled)
                    {
                        job.status.Status = BackupStateResum.END;
                        job.status.LastActionTimestamp = DateTime.Now;
                        Console.WriteLine($"✅ Job {job.name} terminé avec succès !");
                    }

                }
                catch (OperationCanceledException ex)
                {

                    job.status.Status = BackupStateResum.ERROR;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"Job {job.name} stopped: {ex.Message}");

                    // Annulation par Stop ou par logiciel métier
                    job.status.Status = BackupStateResum.ERROR;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"⏹️  Job {job.name} annulé");

                }
                catch (Exception ex)
                {
                    job.status.Status = BackupStateResum.ERROR;
                    job.status.LastActionTimestamp = DateTime.Now;

                    Console.WriteLine($"Error in job {job.name}: {ex.Message}");

                    Console.WriteLine($"Erreur dans le job {job.name} : {ex.Message}");

                }
            }, job.CancellationTokenSource.Token);
        }
=======
			await Task.Run(() =>
			{
				try
				{
					Console.WriteLine($"▶️  Démarrage du job {job.name}");
>>>>>>> feature/dlltype2

<<<<<<< HEAD
					// ✅ LOG: job started
					LogService.Instance.JobStarted(job.name);
=======
        /// <summary>
        /// Executes all jobs in parallel.
        /// Both shared controls (_largeFileSemaphore and _pendingPriorityFiles)
        /// are passed to each strategy to enforce the two parallel transfer rules.
        /// </summary>
        public async Task ExecuteAllJob()
        {
            var tasks = new List<Task>();
>>>>>>> 29742782d5f8c515a37c523f4200e5c09c305aa9

					BackupStrategyFactory.ExecuteBackup(job);

<<<<<<< HEAD
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
=======
            await Task.WhenAll(tasks);
        }
    }
}
>>>>>>> 29742782d5f8c515a37c523f4200e5c09c305aa9
