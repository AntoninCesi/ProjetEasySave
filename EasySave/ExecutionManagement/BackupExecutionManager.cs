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

            // ⚠️ VÉRIFICATION DU LOGICIEL MÉTIER AVANT DE DÉMARRER (v2.0 requirement)
            if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
            {
                Console.WriteLine($"❌ Impossible de démarrer {job.name} : logiciel métier en cours d'exécution");

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

                    job.status.Status = BackupStateResum.FINISHED;
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

        public async Task ExecuteAllJob()
        {
            var tasks = new List<Task>();

            for (int i = 0; i < _listBackupJob.Count; i++)
                tasks.Add(ExecuteJob(i));

            await Task.WhenAll(tasks);
        }
    }
}