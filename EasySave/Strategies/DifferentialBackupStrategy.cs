using System;
using EasySave.Models;
using Tool.Utils;
using SysFileInfo = System.IO.FileInfo;
using SysDirInfo = System.IO.DirectoryInfo;

namespace EasySave.Strategies
{
    /// <summary>
    /// Stratégie de sauvegarde différentielle : copie uniquement les fichiers
    /// nouveaux ou modifiés depuis la dernière sauvegarde
    /// </summary>
    public class DifferentialBackupStrategy 
    {
        public void Execute(BackupJob job)
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
            int filesToBackup = 0;
            int remainingFiles = totalFiles;

            // Calcul de la taille totale et de la taille à sauvegarder
            long totalSize = CalculateTotalSize(job.sourcePath);
            long sizeToBackup = 0;

            // Analyse préalable pour déterminer quels fichiers doivent être sauvegardés
            AnalyzeBackupNeeds(job.sourcePath, job.destinationPath, ref filesToBackup, ref sizeToBackup);

            // Mise à jour de l'état initial
            /*job.status.Status = BackupStateResum.ACTIVE;
            job.status.TotalFiles = filesToBackup;
            job.status.TotalSize = sizeToBackup;
            job.status.RemainingFiles = filesToBackup;
            job.status.RemainingSize = sizeToBackup;
            job.status.Progress = 0;
            job.status.SourcePath = job.sourcePath;
            job.status.DestinationPath = job.destinationPath;
            job.status.LastActionTimestamp = DateTime.Now;

            // Mise à jour du job
            job.totalFiles = filesToBackup;
            job.totalSize = sizeToBackup;
            */

            try
            {
                // Copie différentielle de tous les fichiers et dossiers
                CopyDirectoryDifferential(
                    job.sourcePath,
                    job.destinationPath,
                    job,
                    ref remainingFiles);
                
                // Mise à jour de l'état final
                /*job.status.Status = BackupStateResum.FINISHED;
                job.status.RemainingFiles = 0;
                job.status.RemainingSize = 0;
                job.status.Progress = 100;
                job.status.LastActionTimestamp = DateTime.Now;*/
            }
            catch (Exception ex)
            {
                job.status.Status = BackupStateResum.ERROR; 
                job.status.LastActionTimestamp = DateTime.Now;
                throw new Exception($"Erreur lors de la sauvegarde différentielle : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Analyse les besoins de sauvegarde pour calculer le nombre de fichiers et la taille à sauvegarder
        /// </summary>
        private void AnalyzeBackupNeeds(string sourcePath, string destinationPath, ref int filesToBackup, ref long sizeToBackup)
        {
            try
            {
                SysDirInfo sourceDir = new SysDirInfo(sourcePath);

                // Analyser tous les fichiers du répertoire courant
                SysFileInfo[] files = sourceDir.GetFiles();
                foreach (SysFileInfo file in files)
                {
                    if (NeedsBackup(file, destinationPath))
                    {
                        filesToBackup++;
                        sizeToBackup += file.Length;
                    }
                }

                // Analyser récursivement tous les sous-répertoires
                SysDirInfo[] subDirs = sourceDir.GetDirectories();
                foreach (SysDirInfo subDir in subDirs)
                {
                    string destSubDirPath = System.IO.Path.Combine(destinationPath, subDir.Name);
                    AnalyzeBackupNeeds(subDir.FullName, destSubDirPath, ref filesToBackup, ref sizeToBackup);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignorer les répertoires auxquels on n'a pas accès
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'analyse dans {sourcePath} : {ex.Message}");
            }
        }

        /// <summary>
        /// Copie différentiellement un répertoire et tout son contenu
        /// </summary>
        private void CopyDirectoryDifferential(
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

            // Copier tous les fichiers du répertoire courant qui nécessitent une sauvegarde
            SysFileInfo[] files = sourceDir.GetFiles();
            foreach (SysFileInfo file in files)
            {
                try
                {
                    string destFilePath = System.IO.Path.Combine(destinationPath, file.Name);

                    // Vérifier si le fichier nécessite une sauvegarde
                    if (NeedsBackup(file, destinationPath))
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

                        // Copie physique du fichier
                        file.CopyTo(destFilePath, true);

                        // Décrémenter le nombre de fichiers restants
                        remainingFiles--;

                        // Notification de progression au Manager
                    }
                    else
                    {
                        // Fichier déjà à jour, on le compte quand même comme traité
                        remainingFiles--;
                    }
                }
                catch (Exception ex)
                {
                    // Log de l'erreur mais continue avec les autres fichiers
                    Console.WriteLine($"Erreur lors de la copie de {file.FullName} : {ex.Message}");
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
                    CopyDirectoryDifferential(
                        subDir.FullName, 
                        destSubDirPath, 
                        job,
                        ref remainingFiles);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la copie du répertoire {subDir.FullName} : {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Détermine si un fichier nécessite une sauvegarde
        /// Un fichier nécessite une sauvegarde si :
        /// - Il n'existe pas dans la destination
        /// - Il existe mais a été modifié (date de modification différente)
        /// - Il existe mais a une taille différente
        /// </summary>
        private bool NeedsBackup(SysFileInfo sourceFile, string destinationPath)
        {
            try
            {
                string destFilePath = System.IO.Path.Combine(destinationPath, sourceFile.Name);

                // Si le fichier n'existe pas dans la destination, il faut le sauvegarder
                if (!System.IO.File.Exists(destFilePath))
                {
                    return true;
                }

                // Le fichier existe, vérifier s'il a été modifié
                SysFileInfo destFile = new SysFileInfo(destFilePath);

                // Comparer la date de dernière modification
                // On utilise une tolérance de 1 seconde pour éviter les problèmes de précision
                TimeSpan timeDifference = sourceFile.LastWriteTime - destFile.LastWriteTime;
                if (Math.Abs(timeDifference.TotalSeconds) > 1)
                {
                    return true;
                }

                // Comparer la taille des fichiers
                if (sourceFile.Length != destFile.Length)
                {
                    return true;
                }

                // Le fichier est identique, pas besoin de le sauvegarder
                return false;
            }
            catch (Exception ex)
            {
                // En cas d'erreur, on préfère sauvegarder le fichier par sécurité
                Console.WriteLine($"Erreur lors de la vérification de {sourceFile.Name} : {ex.Message}");
                return true;
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
