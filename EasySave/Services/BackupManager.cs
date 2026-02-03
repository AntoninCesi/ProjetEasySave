using System;
using System.IO;
using System.Collections;

namespace BackupManagerNamespace
{
	public class BackupManager
	{
		//Attributs
		public char symbol { get; }
		public bool isLowerSymbol { get; }

		//Méthodes
		public void BrowseSourceDirectory(string path)
		{

            // Get all files in the directory
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                Console.WriteLine(file);
            }


        }
	}
}