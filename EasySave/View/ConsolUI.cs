using System;
using EasySave.Messaging;
using EasySave.Resources;
using EasySave.ViewModels;
using Tool.Utils;

namespace EasySave.View
{
    public class ConsoleUI
    {
        private readonly MessageProvider _messageProvider;
        private readonly MainViewModel _controller;
        private readonly object _consoleLock = new object();

        public ConsoleUI(MainViewModel controller)
        {
            _controller = controller;
            _messageProvider = new MessageProvider(); // Plus besoin de passer isFrench

            // Initialisation de la langue au démarrage
            SetupLanguage();
        }

        private void SetupLanguage()
        {
            Console.Clear();
            Console.WriteLine("1 - Français");
            Console.WriteLine("2 - English");
            Console.Write("> ");

            string choice = Console.ReadLine() ?? "2";
            // On utilise le Singleton pour définir la langue globalement
            LanguageManager.Instance.ChangeLanguage(choice == "1" ? "fr-FR" : "en-US");
        }

        public void showMenu()
        {
            bool quit = false;
            while (!quit)
            {
                Console.Clear();
                // menu
                displayMessage(new Message(MessageType.MenuTitle));
                displayMessage(new Message(MessageType.MenuOption1));
                displayMessage(new Message(MessageType.MenuOption2));
                displayMessage(new Message(MessageType.MenuOption3));
                displayMessage(new Message(MessageType.MenuOption4));

                Console.Write(_messageProvider.Resolve(new Message(MessageType.MenuPrompt)));

                string? choice = Console.ReadLine()?.Trim();
                switch (choice)
                {
                    case "1": createBackupJob(BackupTypes.FULL); break;
                    case "2": createBackupJob(BackupTypes.DIFFERENTIAL); break;
                    case "3": launchBackup(); break;
                    case "4":
                        displayMessage(new Message(MessageType.Goodbye));
                        quit = true;
                        break;
                    default:
                        displayMessage(new Message(MessageType.InvalidChoice));
                        Console.ReadKey();
                        break;
                }
            }
        }

        public void displayMessage(Message message)
        {
            lock (_consoleLock)
            {
                Console.WriteLine(_messageProvider.Resolve(message));
            }
        }

        // Utilisation pour les messages simples ou formatés
        public void displayMessage(MessageType type, params object[] args)
            => displayMessage(new Message(type, args));

        private void createBackupJob(BackupTypes type)
        {
            Console.Clear();
            displayMessage(MessageType.JobName);
            string name = Console.ReadLine() ?? "Job_" + DateTime.Now.Ticks;

            displayMessage(MessageType.AskSourceDirectory);
            string source = Console.ReadLine() ?? "";

            displayMessage(MessageType.AskDestinationDirectory);
            string dest = Console.ReadLine() ?? "";

            _controller.createBackupJob(name, source, dest, type);
        }

        private void launchBackup()
        {
            var jobs = _controller.getJobName();
            if (jobs.Length == 0)
            {
                displayMessage(MessageType.NoJobAvailable);
                Console.ReadKey();
                return;
            }

            displayMessage(MessageType.SelectJob);
            for (int i = 0; i < jobs.Length; i++)
            {
                Console.WriteLine($"{i} - {jobs[i]}");
            }

            if (int.TryParse(Console.ReadLine(), out int choice) && choice < jobs.Length)
            {
                _controller.startBackupJob(choice);
                displayMessage(MessageType.BackupStarted, jobs[choice]);
            }
        }
    }
}