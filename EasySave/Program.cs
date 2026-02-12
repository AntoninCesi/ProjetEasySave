using System;
using EasySave.ViewModels;
using EasyLog;

namespace EasySave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Test EasyLog
            EasyLog.EasyLog.Instance.WriteLog(
                DateTime.Now,
                "TEST_EASYLOG",
                @"C:\test\source.txt",
                @"D:\backup\source.txt",
                1024,
                150
            );

            Console.WriteLine("✅ Test EasyLog écrit. On lance la backup...");

            var vm = new MainViewModel();
            vm.ExecuteBackup(0);

            Console.WriteLine("Backup terminé. Appuie sur une touche...");
            Console.ReadKey();
        }
    }
}
