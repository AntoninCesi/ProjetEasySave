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

        public void Display(Message message)
        {
            Console.WriteLine(messageProvider.Resolve(message));
        }
    }
}
