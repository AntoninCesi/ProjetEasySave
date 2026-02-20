using System;
using EasySave.Models;
using EasySave.ViewModels;
using Tool.Utils;
using EasySave.View;
using System.Windows;

namespace EasySave;

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
        // On crée l'application et on affiche la fenêtre



                Application app = new Application();
                MainWindow window = new MainWindow();
                app.Run(window);
            
        }
    } 