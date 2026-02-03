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

		public (FileInfo[], long) BrowseSourceDirectory(string path)
		{
			long totalLength = 0;
            DirectoryInfo di = new DirectoryInfo(path);
            // Get a reference to each file in that directory.
            FileInfo[] fiArr = di.GetFiles();
			if (fiArr.Length == 0)
			{
				Console.WriteLine("Aucun fichier trouvé");
			}
			else
			{
				// Display the names and sizes of the files.
				Console.WriteLine("The directory {0} contains the following files:", di.Name);
				foreach (FileInfo f in fiArr)
				{
                    Console.WriteLine("The size of {0} is {1} bytes.\n  ce qu'est f     {2}", f.Name, f.Length, f);
					totalLength += f.Length;
                }
					
					
			}
			return (fiArr, totalLength);
        }
	}
}