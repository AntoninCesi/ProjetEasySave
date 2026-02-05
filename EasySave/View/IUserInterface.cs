using EasySave.Messaging;

namespace EasySave.View
{
    public interface IUserInterface
    {
        void ShowMenu();
        void DisplayMessage(string message);
        void Display(Message message);
        void Attach();
        void DisplayProgress();
    }
}