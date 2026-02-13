using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.Strategies;
using EasySave.StateManagement;
using Tool.Utils;

namespace EasySave.ExecutionManagement
{
    public class BackupExecutionManager
    {
<<<<<<< HEAD
        private readonly BackupStateManager _stateManager;
        
        public BackupExecutionManager()
        {
            _stateManager = new BackupStateManager();
        }
=======
        // La liste des jobs reste ici
        private readonly List<BackupJob> _listBackupJob = new();
>>>>>>> feature/backupstate

        /// <summary>
        /// Crée un nouveau job de sauvegarde
        /// </summary>
        public void createBackupJob(string name, string sourcePath, string destinationPath, BackupTypes type)
        {
<<<<<<< HEAD
            _stateManager.listBackupJob.Add(new BackupJob
=======
            var job = new BackupJob
>>>>>>> feature/backupstate
            {
                name = name,
                sourcePath = sourcePath,
                destinationPath = destinationPath,
                type = type,
                progressObserver = new BackupProgressObserver()
            };

<<<<<<< HEAD
            int jobId = _stateManager.listBackupJob.Count - 1;
            Console.WriteLine(_stateManager.listBackupJob[jobId].ToString());
        }

        /// <summary>
        /// Exécute un job de manière synchrone (pour tests)
        /// </summary>
        public void ExecuteJobSync(int jobId)
=======

            job.status.Status = BackupStateResum.INACTIVE;

            _listBackupJob.Add(job);

            // Crée un StateManager pour suivre ce job (UI / observer)
            var stateManager = new BackupStateManager(job);

            int jobId = _listBackupJob.Count - 1;
            Console.WriteLine(_listBackupJob[jobId].ToString());
        }

        public void ExecuteJobAsyncExecuteJob(int jobId)
>>>>>>> feature/backupstate
        {
            Console.WriteLine("= Test de la Factory =\n");
            BackupStrategyFactory.ExecuteBackup(getJobById(jobId));
            Console.WriteLine("Sauvegarde terminée !");
        }

<<<<<<< HEAD
        /// <summary>
        /// Retourne la liste de tous les jobs
        /// </summary>
=======
>>>>>>> feature/backupstate
        public List<BackupJob> getBackupJobList()
        {
            return _listBackupJob;
        }

<<<<<<< HEAD
        /// <summary>
        /// Exécute un job spécifique de manière asynchrone dans un thread séparé
        /// </summary>
=======
        public BackupJob getJobById(int jobId)
        {
            if (jobId < 0 || jobId >= _listBackupJob.Count)
                return null;

            return _listBackupJob[jobId];
        }

>>>>>>> feature/backupstate
        public async Task ExecuteJob(int jobId)
        {
            var job = getJobById(jobId);
            if (job == null)
            {
                Console.WriteLine($"Job {jobId} introuvable !");
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    Console.WriteLine($"Démarrage du job {job.name}");
<<<<<<< HEAD
                    
                    // Appelle la factory qui exécute la stratégie
=======
>>>>>>> feature/backupstate
                    BackupStrategyFactory.ExecuteBackup(job);

                    job.status.Status = BackupStateResum.FINISHED;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"Job {job.name} terminé avec succès !");
                }
                catch (Exception ex)
                {
                    job.status.Status = BackupStateResum.ERROR;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"Erreur dans le job {job.name} : {ex.Message}");
                }
            });
        }

<<<<<<< HEAD
        /// <summary>
        /// Exécute tous les jobs de la liste simultanément
        /// </summary>
        public async Task ExecuteAllJobs()
=======
        public async Task ExecuteAllJob()
>>>>>>> feature/backupstate
        {
            var tasks = new List<Task>();

            for (int i = 0; i < _listBackupJob.Count; i++)
                tasks.Add(ExecuteJob(i));

            await Task.WhenAll(tasks);
        }
    }
}
