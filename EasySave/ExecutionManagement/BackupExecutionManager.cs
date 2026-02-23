using System;
using EasySave.Models;
using EasySave.Strategies;
using EasySave.StateManagement;
using EasySave.Services;
using Tool.Utils;

namespace EasySave.ExecutionManagement
{
    public class BackupExecutionManager
    {
        // La liste des jobs reste ici
        private readonly List<BackupJob> _listBackupJob = new();

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

            // Crée un StateManager pour suivre ce job (UI / observer)
            var stateManager = new BackupStateManager(job);

            int jobId = _listBackupJob.Count - 1;
            Console.WriteLine(_listBackupJob[jobId].ToString());
        }

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

                return;  // On n'exécute PAS le job
            }

            await Task.Run(() =>
            {
                try
                {
                    Console.WriteLine($"▶️  Démarrage du job {job.name}");
                    BackupStrategyFactory.ExecuteBackup(job);

                    // Si on arrive ici sans annulation, c'est un succès
                    if (!job.IsCancelled)
                    {
                        job.status.Status = BackupStateResum.END;
                        job.status.LastActionTimestamp = DateTime.Now;
                        Console.WriteLine($"✅ Job {job.name} terminé avec succès !");
                    }
                }
                catch (OperationCanceledException)
                {
                    // Annulation par Stop ou par logiciel métier
                    job.status.Status = BackupStateResum.ERROR;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"⏹️  Job {job.name} annulé");
                }
                catch (Exception ex)
                {
                    job.status.Status = BackupStateResum.ERROR;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"Erreur dans le job {job.name} : {ex.Message}");
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