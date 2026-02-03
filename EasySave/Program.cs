using System;
using System.IO;
using EasySave.Models;
using BackupManagerNamespace;


class Program
{
    static void Main()
    {
        string path = @"C:\Users\jenni\Documents\-Echec-";
        BackupManager back = new BackupManager();

        back.BrowseSourceDirectory(path);
    }
}
