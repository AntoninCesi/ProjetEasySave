using System;
using System.IO;
using System.Linq;
using EasySave.Messaging;
using EasySave.Models;
using EasySave.StateManagement;
using Tool.Utils;
using EasySave.ViewModels;

namespace EasySave.View
{
    public class ConsoleUI : IUserInterface
    {
        private readonly MessageProvider messageProvider;
        private readonly Controller _controller;

        // Verrou pour écrire dans la console sans chevauchement
        private readonly object _consoleLock = new object();

        public ConsoleUI(Controller controller)
        {
            _controller = controller;
            bool isFrench = AskLanguage();
            messageProvider = new MessageProvider(isFrench);
        }

        public void showMenu()
        {
            bool quit = false;
            while (!quit)
            {
                Console.Clear();
                displayMessage(new Message(MessageType.MenuTitle));
                displayMessage(new Message(MessageType.MenuOption1));
                displayMessage(new Message(MessageType.MenuOption2));
                displayMessage(new Message(MessageType.MenuOption3));
                displayMessage(new Message(MessageType.MenuOption4));
                Console.Write(messageProvider.Resolve(new Message(MessageType.MenuPrompt)));

                string? choice = Console.ReadLine()?.Trim();
                switch (choice)
                {
                    case "1": createBackupJob(BackupTypes.FULL); break;
                    case "2": createBackupJob(BackupTypes.DIFFERENTIAL); break;
                    case "3": startBackupJob(); break;
                    case "4": quit = true; displayMessage(new Message(MessageType.Goodbye)); break;
                    default: displayMessage(new Message(MessageType.InvalidChoice)); break;
                }

                if (!quit)
                {
                    Console.WriteLine();
                    displayMessage(new Message(MessageType.BackToMenu));
                    Console.ReadKey(true);
                }
            }
        }

        private void createBackupJob(BackupTypes type)
        {
            Console.Write(messageProvider.Resolve(new Message(MessageType.AskSourceDirectory)));
            string sourcePath = Console.ReadLine() ?? string.Empty;

            Console.Write(messageProvider.Resolve(new Message(MessageType.AskDestinationDirectory)));
            string destinationPath = Console.ReadLine() ?? string.Empty;

            Console.Write(messageProvider.Resolve(new Message(MessageType.JobName)));
            string jobName = Console.ReadLine() ?? string.Empty;

            // Crée le job via le controller
            _controller.createBackupJob(jobName, sourcePath, destinationPath, type);
        }

        private void startBackupJob()
        {
            var jobNames = _controller.getJobName()?.ToList();

            if (jobNames == null || jobNames.Count == 0)
            {
                displayMessage(new Message(MessageType.NoJobAvailable));
                return;
            }

            Console.WriteLine(messageProvider.Resolve(new Message(MessageType.SelectJob)));
            for (int i = 0; i < jobNames.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jobNames[i]}");
            }

            Console.Write("> ");
            string? input = Console.ReadLine();

            if (!int.TryParse(input, out int choice) || choice < 1 || choice > jobNames.Count)
            {
                displayMessage(new Message(MessageType.InvalidChoice));
                return;
            }

            int jobId = choice - 1;

            // S'abonner aux notifications de progression avant de lancer le job
            AttachJobProgress(jobId);

            // Lancer le job via le controller
            _controller.startBackupJob(jobId);

            displayMessage(new Message(MessageType.BackupStarted));
        }

        // Connecte l'UI à l'observer du job pour afficher la progression
        private void AttachJobProgress(int jobId)
        {
            var stateManager = _controller.getJobStateManager(jobId); // Controller renvoie un BackupStateManager
            if (stateManager == null) return;

            stateManager.ProgressChanged += (filesSaved, totalSize, elapsed, jobName) =>
            {
                lock (_consoleLock)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(
                        $"[{jobName}] Fichiers: {filesSaved}, Taille: {totalSize / 1024} KB, Temps: {elapsed:c}    "
                    );
                }
            };
        }


        public void displayMessage(string message) => Console.WriteLine(message);
        public void displayMessage(Message message) => Console.WriteLine(messageProvider.Resolve(message));
        public void attach() { }
        public void displayProgress() { }

        private bool AskLanguage()
        {
            while (true)
            {
                Console.Clear();
                Console.Write("Choose language / Choisir la langue (FR/EN): ");
                string? input = Console.ReadLine()?.Trim().ToUpperInvariant();
                if (input == "FR") return true;
                if (input == "EN") return false;
                Console.WriteLine("Invalid input. Please enter 'FR' or 'EN'.");
                Console.ReadKey(true);
            }
        }
    }
}
