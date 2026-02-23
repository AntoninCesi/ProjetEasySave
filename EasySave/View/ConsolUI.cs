using EasySave.Messaging;
using EasySave.Resources;
using EasySave.ViewModels;
using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Documents;
using Tool.Utils;

namespace EasySave.ViewModel
{
    public class ConsoleUI
    {
        private readonly MessageProvider _messageProvider;
        private readonly MainViewModel _controller;
        private readonly object _consoleLock = new object();

        public ConsoleUI(string[] arg)
        {
            if (arg.Length == 0)
            {
                _controller = new MainViewModel(arg);
                _messageProvider = new MessageProvider(); // message structures

                _controller.OnMessageReceived += (sender, msg) =>
                {
                    displayMessage(msg);
                }; //for the viewmodel

                // Initialize language
                SetupLanguage();
            }
            else
            {
                _controller = new MainViewModel(arg);
            }

        }

        private void SetupLanguage()
        {
            Console.Clear();
            Console.WriteLine("1 - Français");
            Console.WriteLine("2 - English");
            Console.Write("> ");

            string choice = Console.ReadLine() ?? "2";
            // singleton
            LanguageManager.Instance.ChangeLanguage(choice == "1" ? "fr-FR" : "en-US");
            showMenu();
        }

        private void showMenu()
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
                displayMessage(new Message(MessageType.MenuOption5));
                displayMessage(new Message(MessageType.MenuOption4));

                Console.Write(_messageProvider.Resolve(new Message(MessageType.MenuPrompt)));

                string? choice = Console.ReadLine()?.Trim();
                switch (choice)
                {
                    case "1": createBackupJob(BackupTypes.FULL); break;
                    case "2": createBackupJob(BackupTypes.DIFFERENTIAL); break;
                    case "3": LaunchBackup(); break;
                    case "4": LaunchBackup(); break;
                    case "5":
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

        private void LaunchBackup()
        {
            Console.Clear();

            if (_controller.thereJobs())
            {
                for (int i = 0; i < _controller.getJobName().Length; i++)
                {
                    Console.WriteLine($"{i} - {_controller.getJobName()[i]}");
                }
                Console.Write(_messageProvider.Resolve(new Message(MessageType.MenuPrompt)));
                string choice = Console.ReadLine();

                _controller.HandleCommandLineArgs(choice);
                Console.ReadKey(); 
            }
            else
            {
                displayMessage(new Message(MessageType.NoJobAvailable));
                Console.ReadKey(); 
            }
        }


        public void displayMessage(Message message)
        {
            lock (_consoleLock)
            {
                Console.WriteLine(_messageProvider.Resolve(message));
            }
        }

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

        

    }
}