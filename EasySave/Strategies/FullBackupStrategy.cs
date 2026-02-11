using System;
using EasySave.Models;
using Tool.Utils;
using SysFileInfo = System.IO.FileInfo;
using SysDirInfo = System.IO.DirectoryInfo;

namespace EasySave.Strategies
{
    
    /// Stratégie de sauvegarde complète : copie tous les fichiers et dossiers
    /// de la source vers la destination de manière récursive
    
    public class FullBackupStrategy 
    {
        public void Execute(BackupJob job)// ProgressCallback? callback)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            if (string.IsNullOrWhiteSpace(job.sourcePath))
                throw new ArgumentException("Le chemin source ne peut pas être vide");

            if (string.IsNullOrWhiteSpace(job.destinationPath))
                throw new ArgumentException("Le chemin de destination ne peut pas être vide");

            if (!System.IO.Directory.Exists(job.sourcePath))
                throw new System.IO.DirectoryNotFoundException($"Le répertoire source n'existe pas : {job.sourcePath}");

            // Création du répertoire de destination s'il n'existe pas
            if (!System.IO.Directory.Exists(job.destinationPath))
            {
                System.IO.Directory.CreateDirectory(job.destinationPath);
            }

            // Calcul du nombre total de fichiers pour le suivi de progression
            int totalFiles = CountFilesRecursive(job.sourcePath);
            int remainingFiles = totalFiles;

            // Calcul de la taille totale
            long totalSize = CalculateTotalSize(job.sourcePath);

           
            try
            {
                // Copie récursive de tous les fichiers et dossiers
                CopyDirectoryRecursive(
                    job.sourcePath,
                    job.destinationPath,
                    job,
                    ref remainingFiles);
                

            
            }
            catch (Exception ex)
            {
                job.status.Status = BackupStateResum.ERROR; 
                job.status.LastActionTimestamp = DateTime.Now;
                throw new Exception($"Erreur lors de la sauvegarde complète : {ex.Message}", ex);
            }
        }


        /// Copie récursivement un répertoire et tout son contenu

        private void CopyDirectoryRecursive(
            string sourcePath,
            string destinationPath,
            BackupJob job,
            ref int remainingFiles)
           
        {
            SysDirInfo sourceDir = new SysDirInfo(sourcePath);

            // S'assurer que le répertoire de destination existe
            if (!System.IO.Directory.Exists(destinationPath))
            {
                System.IO.Directory.CreateDirectory(destinationPath);
            }

            // Copier tous les fichiers du répertoire courant
            SysFileInfo[] files = sourceDir.GetFiles();
            foreach (SysFileInfo file in files)
            {
                try
                {
                    // Créer l'objet FileInfo personnalisé pour le callback
                    var fileData = new EasySave.Models.FileInfo
                    {
                        fileName = file.Name,
                        filePath = file.FullName,
                        fileSize = file.Length,
                        isDirectory = false,
                        lastModified = file.LastWriteTime
                    };

                    string destFilePath = System.IO.Path.Combine(destinationPath, file.Name);

                    // Copie physique du fichier
                    file.CopyTo(destFilePath, true);

             
                    
                    
                 

                    // Notification de progression au Manager
                    
                }
                catch (Exception ex)
                {
                    // Log de l'erreur mais continue avec les autres fichiers
                    Console.WriteLine($"Erreur lors de la copie de {file.FullName} : {ex.Message}");
                    // Optionnel : vous pouvez choisir de throw ici selon votre gestion d'erreur
                }
            }

            // Copier récursivement tous les sous-répertoires
            SysDirInfo[] subDirs = sourceDir.GetDirectories();
            foreach (SysDirInfo subDir in subDirs)
            {
                try
                {
                    // Créer l'objet FileInfo pour le répertoire (optionnel, pour le tracking)
                    var dirData = new EasySave.Models.FileInfo
                    {
                        fileName = subDir.Name,
                        filePath = subDir.FullName,
                        fileSize = 0,
                        isDirectory = true,
                        lastModified = subDir.LastWriteTime
                    };

                    string destSubDirPath = System.IO.Path.Combine(destinationPath, subDir.Name);

                    // Appel récursif pour copier le sous-répertoire
                    CopyDirectoryRecursive(
                        subDir.FullName, 
                        destSubDirPath, 
                        job,
                        ref remainingFiles
                       );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la copie du répertoire {subDir.FullName} : {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Compte récursivement le nombre total de fichiers dans un répertoire
        /// </summary>
        private int CountFilesRecursive(string path)
        {
            int count = 0;

            try
            {
                SysDirInfo dir = new SysDirInfo(path);
                
                // Compter les fichiers du répertoire courant
                count += dir.GetFiles().Length;

                // Compter récursivement dans les sous-répertoires
                foreach (SysDirInfo subDir in dir.GetDirectories())
                {
                    count += CountFilesRecursive(subDir.FullName);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignorer les répertoires auxquels on n'a pas accès
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du comptage des fichiers dans {path} : {ex.Message}");
            }

            return count;
        }

        /// <summary>
        /// Calcule récursivement la taille totale des fichiers dans un répertoire
        /// </summary>
        private long CalculateTotalSize(string path)
        {
            long totalSize = 0;

            try
            {
                SysDirInfo dir = new SysDirInfo(path);
                
                // Additionner la taille des fichiers du répertoire courant
                foreach (SysFileInfo file in dir.GetFiles())
                {
                    totalSize += file.Length;
                }

                // Calculer récursivement dans les sous-répertoires
                foreach (SysDirInfo subDir in dir.GetDirectories())
                {
                    totalSize += CalculateTotalSize(subDir.FullName);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignorer les répertoires auxquels on n'a pas accès
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du calcul de la taille dans {path} : {ex.Message}");
            }

            return totalSize;
        }
    }
}
