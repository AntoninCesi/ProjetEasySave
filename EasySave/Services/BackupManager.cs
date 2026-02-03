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

        //A command that allows you to browse the files contained in a folder and returns the size of the files in that folder in a tuple.
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
                    Console.WriteLine("The size of {0} is {1} bytes.", f.Name, f.Length);
					totalLength += f.Length;
                }
					
					
			}
			return (fiArr, totalLength);
        }
		public void CopyFiles(string sourpath, string despath) {

			(FileInfo[],long)result = this.BrowseSourceDirectory(sourpath);

            foreach (FileInfo f in result.Item1)
            {
                // Remove path from the file name.
                string fName = f.FullName.Substring(sourpath.Length + 1);

                // Use the Path.Combine method to safely append the file name to the path.
                // Will overwrite if the destination file already exists.
                File.Copy(Path.Combine(sourpath, fName), Path.Combine(despath, fName), true);
            }

        }


	}
}