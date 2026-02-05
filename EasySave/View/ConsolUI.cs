using System;
using EasySave.Messaging;

namespace EasySave.View
{
    public class ConsoleUI : IUserInterface
    {
        private readonly MessageProvider messageProvider;

        public ConsoleUI(MessageProvider provider)
        {
            messageProvider = provider;
        }

        public void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("       EasySave - Menu             ");
            Console.WriteLine("1. Create a backup job             ");
            Console.WriteLine("2. Execute a job                   ");
            Console.WriteLine("3. Execute all jobs                ");
            Console.WriteLine("4. View job states                 ");
            Console.WriteLine("5. Delete a job                    ");
            Console.WriteLine("6. Exit                            ");
            Console.Write("\nYour choice: ");
        }

        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void Display(Message message)
        {
            DisplayMessage(messageProvider.Resolve(message));
        }

        public void Attach()
        {
            // TODO: Implement update observation
        }

        public void DisplayProgress()
        {
            // TODO: Display progress
        }
    }
}