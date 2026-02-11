using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<BackupJob> BackupJobs { get; set; }
        private readonly List<BackupState> _states = new();
        private readonly StateService _stateService = new();

        public MainViewModel()
        {
            BackupJobs = new ObservableCollection<BackupJob>();

            // On utilise les noms en minuscules de tes collègues
            BackupJobs.Add(new BackupJob
            {
                name = "TestJob_Full",
                sourcePath = @"C:\Temp\SourceTest",
                destinationPath = @"C:\Temp\TargetTest"
            });
        }

        public void ExecuteBackup(int jobIndex)
        {
            if (jobIndex < 0 || jobIndex >= BackupJobs.Count) return;

            var job = BackupJobs[jobIndex];

            var state = _states.FirstOrDefault(s => s.JobName == job.name);
            if (state == null)
            {
                state = new BackupState { JobName = job.name };
                _states.Add(state);
            }

            state.Status = BackupStatus.ACTIF;
            state.LastActionTimestamp = DateTime.Now;
            state.CurrentSourceUNC = job.sourcePath;
            state.CurrentDestinationUNC = job.destinationPath;

            try
            {
                if (!Directory.Exists(job.sourcePath)) return;

                var files = Directory.GetFiles(job.sourcePath, "*.*", SearchOption.AllDirectories);

                state.TotalFiles = files.Length;

                // CORRECTION CS0104 : On utilise le nom complet pour éviter le conflit avec votre modèle FileInfo
                state.TotalSizeBytes = files.Sum(f => new System.IO.FileInfo(f).Length);

                state.RemainingFiles = state.TotalFiles;
                state.RemainingSizeBytes = state.TotalSizeBytes;
                state.Progress = 0f;

                _stateService.SaveStates(_states);

                foreach (var file in files)
                {
                    // Utilisation du FileInfo système
                    var sysFileInfo = new System.IO.FileInfo(file);
                    string destFile = file.Replace(job.sourcePath, job.destinationPath);
                    string destDir = Path.GetDirectoryName(destFile)!;

                    if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                    state.CurrentSourceUNC = file;
                    state.CurrentDestinationUNC = destFile;

                    var stopWatch = Stopwatch.StartNew();
                    File.Copy(file, destFile, true);
                    stopWatch.Stop();

                    EasyLog.EasyLog.Instance.WriteLog(
                        DateTime.Now,
                        job.name,
                        file,
                        destFile,
                        sysFileInfo.Length,
                        stopWatch.ElapsedMilliseconds
                    );

                    UpdateStateProgress(state, sysFileInfo.Length);
                }

                state.Status = BackupStatus.TERMINE;
                _stateService.SaveStates(_states);
            }
            catch (Exception)
            {
                state.Status = BackupStatus.EN_ERREUR;
                _stateService.SaveStates(_states);
            }
        }

        private void UpdateStateProgress(BackupState state, long fileSize)
        {
            state.RemainingFiles -= 1;
            state.RemainingSizeBytes -= fileSize;
            state.Progress = state.TotalFiles > 0
                ? (float)(state.TotalFiles - state.RemainingFiles) / state.TotalFiles
                : 0f;
            state.LastActionTimestamp = DateTime.Now;
            _stateService.SaveStates(_states);
        }
    }
}