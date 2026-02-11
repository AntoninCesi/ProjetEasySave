using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using EasyLog;
using EasySave.Commands;
using EasySave.Models;
using EasySave.Resources;
using EasySave.Services;

namespace EasySave.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BackupJob> BackupJobs { get; set; }
        private readonly List<BackupState> _states = new();
        private readonly StateService _stateService = new();

        // Support multi-langue
        public LanguageManager Lang => LanguageManager.Instance;
        public event PropertyChangedEventHandler? PropertyChanged;

        // Commandes pour les boutons
        public ICommand ChangeLanguageCommand { get; }
        public ICommand CreateJobCommand { get; }
        public ICommand RunSelectionCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public MainViewModel()
        {
            BackupJobs = new ObservableCollection<BackupJob>();

            // Initialiser les commandes
            ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);
            CreateJobCommand = new RelayCommand(_ => CreateNewJob());
            RunSelectionCommand = new RelayCommand(_ => RunSelectedBackups());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

            // Données de test
            BackupJobs.Add(new BackupJob
            {
                name = "TestJob_Full",
                sourcePath = @"C:\Temp\SourceTest",
                destinationPath = @"C:\Temp\TargetTest"
            });
        }

        // Changer de langue
        private void ChangeLanguage(string? languageCode)
        {
            Console.WriteLine($"ChangeLanguage appelée avec : {languageCode}"); // TEST

            if (string.IsNullOrEmpty(languageCode)) return;

            Console.WriteLine($"Changement vers : {languageCode}");

            LanguageManager.Instance.ChangeLanguage(languageCode);
            OnPropertyChanged(nameof(Lang));

            Console.WriteLine("Langue changée !");
        }

        // Créer un nouveau travail
        private void CreateNewJob()
        {
            var newJob = new BackupJob
            {
                name = $"NewJob_{BackupJobs.Count + 1}",
                sourcePath = string.Empty,
                destinationPath = string.Empty
            };
            BackupJobs.Add(newJob);
        }

        // Lancer les sauvegardes sélectionnées
        private void RunSelectedBackups()
        {
            for (int i = 0; i < BackupJobs.Count; i++)
            {
                ExecuteBackup(i);
            }
        }

        // Ouvrir les paramètres
        private void OpenSettings()
        {
            // TODO: Ouvrir la fenêtre de paramètres
        }

        // TON CODE EXISTANT - INCHANGÉ
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

        // TON CODE EXISTANT - INCHANGÉ
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}