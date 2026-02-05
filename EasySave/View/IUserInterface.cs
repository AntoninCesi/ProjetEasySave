using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.View
{
    public interface IUserInterface
    {
        void showMenu();
        void displayMessage(string message);
        void attach();//to update progress
        void displayProgress();// to update progress
    }
}
