using System;
using EasySave.Models;
using EasySave.ViewModels;
using EasySave.Services;

namespace EasySave
{
	class Program
	{
		static void Main(string[] args)
		{
			// Recharge le logger selon les settings actuels (n’impacte pas le reste)
			LogService.Instance.ReloadFromSettings();

			// Comportement historique de dev
			MainViewModel controller = new MainViewModel(args);
		}
	}
}