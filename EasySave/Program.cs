<<<<<<< HEAD
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
=======
﻿using System;
using System.Windows;
using EasySave.Services;
using EasySave.Models;
using EasySave.View;

namespace EasySave
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			// 🔹 Recharge le logger selon les settings actuels
			LogService.Instance.ReloadFromSettings();

			// 🔹 Lancement UI
			Application app = new Application();
			MainWindow window = new MainWindow();
			app.Run(window);
		}
	}
>>>>>>> feature/dlltype2
}