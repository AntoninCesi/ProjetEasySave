using EasySave.ViewModels;

var vm = new MainViewModel();
vm.ExecuteBackup(0);

Console.WriteLine("Backup terminé. Appuie sur une touche...");
Console.ReadKey();
﻿using System;
using System.Collections.Generic;
using EasySave.ViewModels;


class Program
{
    static void Main(string[] args)
    {
        Controller controller = new Controller(args);

    }
        
}