using EasySave.ViewModels;

var vm = new MainViewModel();
vm.ExecuteBackup(0);

Console.WriteLine("Backup terminé. Appuie sur une touche...");
Console.ReadKey();
