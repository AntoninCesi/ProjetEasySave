using System;
using System.IO;
using EasySave.Models;
using BackupManagerNamespace;


class Program
{
    static void Main()
    {
        string path = @"C:\Users\jenni\Documents\testeasy1";
        BackupManager back = new BackupManager();

        back.CopyFileImage(path, @"C:\Users\jenni\Documents\testeasy2");
    }
}
