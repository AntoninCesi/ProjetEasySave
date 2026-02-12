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
        private readonly BackupStateManager _stateManager;
        
        public BackupExecutionManager()
        {
            _stateManager = new BackupStateManager();
        }

        /// <summary>
        /// Crée un nouveau job de sauvegarde
        /// </summary>
        public void createBackupJob(string name, string sourcePath, string destinationPath, BackupTypes type)
        {
            _stateManager.listBackupJob.Add(new BackupJob
            {
                name = name,
                sourcePath = sourcePath,
                destinationPath = destinationPath,
                type = type
            });

            int jobId = _stateManager.listBackupJob.Count - 1;
            Console.WriteLine(_stateManager.listBackupJob[jobId].ToString());
        }

        /// <summary>
        /// Exécute un job de manière synchrone (pour tests)
        /// </summary>
        public void ExecuteJobSync(int jobId)
        {
            Console.WriteLine("= Test de la Factory =\n");
            BackupStrategyFactory.ExecuteBackup(_stateManager.getJobById(jobId));
            Console.WriteLine("Sauvegarde terminée !");
        }

        /// <summary>
        /// Retourne la liste de tous les jobs
        /// </summary>
        public List<BackupJob> getBackupJobList()
        {
            return _stateManager.listBackupJob;
        }

        /// <summary>
        /// Exécute un job spécifique de manière asynchrone dans un thread séparé
        /// </summary>
        public async Task ExecuteJob(int jobId)
        {
            var job = _stateManager.getJobById(jobId);
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
                    
                    // Appelle la factory qui exécute la stratégie
                    BackupStrategyFactory.ExecuteBackup(job);

                    // Job terminé avec succès
                    job.status.Status = BackupStateResum.FINISHED;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"Job {job.name} terminé avec succès !");
                }
                catch (Exception ex)
                {
                    // Gestion de l'erreur pour ce job uniquement
                    job.status.Status = BackupStateResum.ERROR;
                    job.status.LastActionTimestamp = DateTime.Now;
                    Console.WriteLine($"Erreur dans le job {job.name} : {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Exécute tous les jobs de la liste simultanément
        /// </summary>
        public async Task ExecuteAllJobs()
        {
            var tasks = new List<Task>();

            for (int i = 0; i < _stateManager.listBackupJob.Count; i++)
            {
                tasks.Add(ExecuteJob(i));
            }

            // Attend que tous les jobs soient terminés
            await Task.WhenAll(tasks);
        }
    }
}
