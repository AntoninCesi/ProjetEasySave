using System;
using EasySave.Models;
using EasySave.ViewModels;
using Tool.Utils;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            MainViewModel controller = new MainViewModel(args);
        }
    }
}