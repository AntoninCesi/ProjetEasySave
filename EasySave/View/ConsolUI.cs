using EasySave.Messaging;
using EasySave.Resources;
using EasySave.ViewModels;
using EasySave.Models;
using System;
using Tool.Utils;

namespace EasySave.View
{
    public class ConsoleUI
    {
        private readonly MessageProvider _messageProvider;
        private readonly MainViewModel _controller;

        // Lock partagé entre ConsoleUI et ConsoleProgressBarManager
        private readonly object _consoleLock = new object();

        private readonly ConsoleProgressBarManager _progressManager;

        public ConsoleUI(string[] arg)
        {
            // Créer le manager en lui passant le lock partagé
            _progressManager = new ConsoleProgressBarManager(
                barCount: 10,
                barWidth: 40,
                consoleLock: _consoleLock
            );

            _controller = new MainViewModel(arg);

            if (arg.Length == 0)
            {
                _messageProvider = new MessageProvider();

                _controller.OnMessageReceived += (sender, msg) =>
                {
                    DisplayMessage(msg);
                };

                SetupLanguage();
            }
        }

        private void SetupLanguage()
        {
            lock (_consoleLock)
            {
                Console.Clear();
                Console.WriteLine("1 - Français");
                Console.WriteLine("2 - English");
                Console.Write("> ");
            }

            string choice = Console.ReadLine() ?? "2";
            LanguageManager.Instance.ChangeLanguage(choice == "1" ? "fr-FR" : "en-US");

            ShowMenu();
        }

        private void ShowMenu()
        {
            bool quit = false;

            while (!quit)
            {
                lock (_consoleLock)
                {
                    Console.Clear();
                    // Réserver les lignes du bas APRÈS le Clear
                    _progressManager.ReserveLines();
                }

                DisplayMessage(new Message(MessageType.MenuTitle));
                DisplayMessage(new Message(MessageType.MenuOption1));
                DisplayMessage(new Message(MessageType.MenuOption2));
                DisplayMessage(new Message(MessageType.MenuOption3));
                DisplayMessage(new Message(MessageType.MenuOption4));

                string prompt = _messageProvider.Resolve(new Message(MessageType.MenuPrompt));
                lock (_consoleLock) { Console.Write(prompt); }

                string? choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1": CreateBackupJob(BackupTypes.FULL); break;
                    case "2": CreateBackupJob(BackupTypes.DIFFERENTIAL); break;
                    case "3": LaunchBackup(); break;
                    case "4":
                        DisplayMessage(new Message(MessageType.Goodbye));
                        quit = true;
                        break;
                    default:
                        DisplayMessage(new Message(MessageType.InvalidChoice));
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void LaunchBackup()
        {
            if (!_controller.thereJobs())
            {
                DisplayMessage(new Message(MessageType.NoJobAvailable));
                Console.ReadKey();
                return;
            }

            // Afficher la liste des jobs disponibles
            lock (_consoleLock)
            {
                string[] jobNames = _controller.getJobName();
                for (int i = 0; i < jobNames.Length; i++)
                    Console.WriteLine($"  {i} - {jobNames[i]}");

                Console.Write(_messageProvider.Resolve(new Message(MessageType.MenuPrompt)));
            }

            string choice = Console.ReadLine() ?? "";

            // Abonner les observers
            foreach (var job in _controller.getJobs())
            {
                var observer = job.progressObserver;
                int jobIndex = Array.IndexOf(_controller.getJobName(), job.name);

                observer.OnProgressChanged += progress =>
                {
                    _progressManager.UpdateProgress(jobIndex, progress);
                };

                observer.OnStatusChanged += status =>
                {
                    if (status == BackupStateResum.ON)
                        DisplayMessage(new Message(MessageType.BackupStarted));
                };
            }

            // Lancer les jobs
            _controller.HandleCommandLineArgs(choice);

            // Attendre une touche une fois tout terminé
            lock (_consoleLock)
            {
                // Placer le curseur sous la zone menu, au-dessus des barres
                int safeTop = Math.Max(0, _progressManager.BarStartLine - 2);
                Console.SetCursorPosition(0, safeTop);
                DisplayMessage(new Message(MessageType.BackupEnd));
            }

            Console.ReadKey();
        }

        private void CreateBackupJob(BackupTypes type)
        {
            lock (_consoleLock) { Console.Clear(); }

            DisplayMessage(new Message(MessageType.JobName));
            string name = Console.ReadLine() ?? "Job_" + DateTime.Now.Ticks;

            DisplayMessage(new Message( MessageType.AskSourceDirectory));
            string source = Console.ReadLine() ?? "";

            DisplayMessage(new Message( MessageType.AskDestinationDirectory));
            string dest = Console.ReadLine() ?? "";

            _controller.createBackupJob(name, source, dest, type);
        }

        /// <summary>Affiche un message dans la zone menu (safe vis-à-vis des barres).</summary>
        public void DisplayMessage(Message message)
            => PrintLine(_messageProvider.Resolve(message));


        /// <summary>
        /// Écrit une ligne en sauvegardant / restaurant la position du curseur
        /// pour ne pas interférer avec les barres de progression situées en bas.
        /// </summary>
        private void PrintLine(string text)
        {
            lock (_consoleLock)
            {
                int savedLeft = Console.CursorLeft;
                int savedTop = Console.CursorTop;

                // Si on risque d'écraser une barre, on monte
                if (savedTop >= _progressManager.BarStartLine)
                {
                    Console.SetCursorPosition(0, Math.Max(0, _progressManager.BarStartLine - 1));
                }

                Console.WriteLine(text);

                // Ne pas restaurer si on a simplement avancé normalement
                // (restauration uniquement lors d'une mise à jour en arrière-plan)
            }
        }
    }
}