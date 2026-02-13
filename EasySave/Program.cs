using System;
<<<<<<< HEAD
<<<<<<< Updated upstream
using System.Threading.Tasks;
using EasyLog;

namespace EasySave
=======
=======
>>>>>>> origin/dev
using EasySave.Models;
using EasySave.ViewModels;
using Tool.Utils;
using EasySave.View;
using System.Windows;

namespace EasySave;

<<<<<<< HEAD
class Program
>>>>>>> Stashed changes
{
    [STAThread]
    static void Main(string[] args)
    {
<<<<<<< Updated upstream
        static void Main(string[] args)
        {
            var logger = new EasyLogger();

            Parallel.For(0, 100, i =>
            {
                logger.Write(LogEvent.FileCopied(
                    "JOB_PARALLEL",
                    $"src_{i}.txt",
                    $"dst_{i}.txt",
                    i * 100,
                    i
                ));
            });
            Console.WriteLine("100 logs écrits en parallèle.");
            Console.ReadKey();
        }
    }
}
=======
        // On crée l'application et on affiche la fenêtre
        Application app = new Application();
        MainWindow window = new MainWindow();
        app.Run(window);
    }
}
>>>>>>> Stashed changes
=======
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
>>>>>>> origin/dev
