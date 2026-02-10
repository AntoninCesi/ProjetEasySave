using System;
using EasySave.Messaging;
using EasySave.ViewModels;
using Tool.Utils;

namespace EasySave.View
{
    public class ConsoleUI : IUserInterface
    {
        private readonly MessageProvider messageProvider;
        private Controller _controller;

        // Constructor asks the language once and creates the provider
        public ConsoleUI(Controller control)
        {
            _controller = control;
            bool isFrench = AskLanguage();
            messageProvider = new MessageProvider(isFrench);
        }

        // Main menu display
        public void showMenu()
        {
            bool quit = false;

            while (!quit)
            {
                Console.Clear();

                // Display menu title from provider
                displayMessage(new Message(MessageType.MenuTitle));

                // Display menu options from provider
                displayMessage(new Message(MessageType.MenuOption1));
                displayMessage(new Message(MessageType.MenuOption2));
                displayMessage(new Message(MessageType.MenuOption3));
                displayMessage(new Message(MessageType.MenuOption4));

                // Display prompt
                Console.Write(messageProvider.Resolve(new Message(MessageType.MenuPrompt)));

                string? choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        this.createBackupJob(BackupType.COMPLET);
                        break;
                    case "2":
                        this.createBackupJob(BackupType.DIFFERENTIAL);
                        break;
                    case "3":
                        displayMessage(new Message(MessageType.Loading));
                        break;
                    case "4":
                        quit = true;
                        displayMessage(new Message(MessageType.Goodbye));
                        break;
                    default:
                        displayMessage(new Message(MessageType.InvalidChoice));
                        break;
                }

                if (!quit)
                {
                    Console.WriteLine();
                    displayMessage(new Message(MessageType.BackToMenu));
                    Console.ReadKey(true);
                }
            }
        }
        private void createBackupJob(BackupType type)
        {
            Console.Write(messageProvider.Resolve (new Message( MessageType.AskSourceDirectory)));
            string sourcePath = Console.ReadLine() ?? string.Empty;

            Console.Write(messageProvider.Resolve(new Message(MessageType.AskDestinationDirectory)));
            string destinationPath = Console.ReadLine() ?? string.Empty;

            Console.Write(messageProvider.Resolve(new Message(MessageType.JobName)));
            string jobName = Console.ReadLine() ?? string.Empty;

            _controller.createBackupJob(jobName, sourcePath, destinationPath,type);
        }

        // Display a string directly
        public void displayMessage(string message)
        {
            Console.WriteLine(message);
        }

        // Display a Message object using MessageProvider
        public void displayMessage(Message message)
        {
            Console.WriteLine(messageProvider.Resolve(message));
        }

        public void attach()
        {
            // Future: subscribe to progress events
        }

        public void displayProgress()
        {
            // Future: show progress
        }

        // Ask the language once
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
