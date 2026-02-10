using System;
using System.IO;
using System.Linq;
using EasySave.Messaging;
using EasySave.ViewModels;
using Tool.Utils;

namespace EasySave.View
{
    public class ConsoleUI : IUserInterface
    {
        private readonly MessageProvider messageProvider;
        private Controller _controller;

        public ConsoleUI(Controller control)
        {
            _controller = control;
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
                    case "1": this.createBackupJob(BackupTypes.COMPLET); break;
                    case "2": this.createBackupJob(BackupTypes.DIFFERENTIAL); break;
                    case "3": displayMessage(new Message(MessageType.Loading)); break;
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
            // Data collection
            Console.Write(messageProvider.Resolve(new Message(MessageType.AskSourceDirectory)));
            string sourcePath = Console.ReadLine() ?? string.Empty;

            Console.Write(messageProvider.Resolve(new Message(MessageType.AskDestinationDirectory)));
            string destinationPath = Console.ReadLine() ?? string.Empty;

            Console.Write(messageProvider.Resolve(new Message(MessageType.JobName)));
            string jobName = Console.ReadLine() ?? string.Empty;

            // --- USER INPUT VALIDATIONS ---

            // 1. Validate that job name is not empty
            if (string.IsNullOrWhiteSpace(jobName))
            {
                displayMessage(new Message(MessageType.EmptyJobName));
                return;
            }

            // 2. Validate that job name is unique (using .name property from team's model)
            var jobs = _controller.GetJobs();
            if (jobs != null && jobs.Any(j => j.name != null && j.name.Equals(jobName, StringComparison.OrdinalIgnoreCase)))
            {
                displayMessage(new Message(MessageType.DuplicateJobName));
                return;
            }

            // 3. Validate that source directory actually exists
            if (!Directory.Exists(sourcePath))
            {
                displayMessage(new Message(MessageType.SourceDoesNotExist));
                return;
            }

            // 4. Validate that source and destination are different paths
            if (sourcePath.Trim().Equals(destinationPath.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                displayMessage(new Message(MessageType.SourceAndDestIdentical));
                return;
            }

            // 5. Warning if the source directory is empty
            if (Directory.Exists(sourcePath) && !Directory.EnumerateFileSystemEntries(sourcePath).Any())
            {
                displayMessage(new Message(MessageType.SourceEmpty));
            }

            // Validation passed: Trigger job creation via Controller
            _controller.createBackupJob(jobName, sourcePath, destinationPath, type);
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