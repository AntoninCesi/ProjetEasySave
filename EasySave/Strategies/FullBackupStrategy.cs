using System;
using System.IO;
using EasySave.Models;
using EasySave.Services;
using Tool.Utils;
using SysFileInfo = System.IO.FileInfo;
using SysDirInfo = System.IO.DirectoryInfo;

namespace EasySave.Strategies
{
<<<<<<< HEAD
    /// <summary>
    /// Full backup strategy with encryption and integrated logging
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));
=======
	public class FullBackupStrategy : IBackupStrategy
	{
		public void Execute(BackupJob job)
		{
			if (job == null)
				throw new ArgumentNullException(nameof(job));
>>>>>>> feature/dlltype2

			if (string.IsNullOrWhiteSpace(job.sourcePath))
				throw new ArgumentException("Le chemin source ne peut pas être vide");

			if (string.IsNullOrWhiteSpace(job.destinationPath))
				throw new ArgumentException("Le chemin de destination ne peut pas être vide");

			if (!Directory.Exists(job.sourcePath))
				throw new DirectoryNotFoundException($"Le répertoire source n'existe pas : {job.sourcePath}");

<<<<<<< HEAD
            if (!Directory.Exists(job.destinationPath))
                Directory.CreateDirectory(job.destinationPath);

            if (job.progressObserver == null)
                job.progressObserver = new BackupProgressObserver();
=======
			if (!Directory.Exists(job.destinationPath))
				Directory.CreateDirectory(job.destinationPath);

			if (job.progressObserver == null)
				job.progressObserver = new BackupProgressObserver();
>>>>>>> feature/dlltype2

			job.progressObserver.Reset();

<<<<<<< HEAD
            try
            {
                job.status.Status = BackupStateResum.ON;
                job.progressObserver.UpdateStatus(BackupStateResum.ON);
                job.status.LastActionTimestamp = DateTime.Now;
=======
			try
			{
				job.status.Status = BackupStateResum.ACTIVE;
				job.status.LastActionTimestamp = DateTime.Now;
>>>>>>> feature/dlltype2

				CopyDirectoryRecursive(job.sourcePath, job.destinationPath, job);

<<<<<<< HEAD
                job.status.Status = BackupStateResum.END;
                job.progressObserver.UpdateStatus(BackupStateResum.END);
                job.status.Progress = 100;
                job.status.LastActionTimestamp = DateTime.Now;
            }
            catch (OperationCanceledException)
            {
                job.status.Status = BackupStateResum.ERROR;
                job.status.LastActionTimestamp = DateTime.Now;
                throw;
            }
            catch (Exception ex)
            {
                job.status.Status = BackupStateResum.ERROR;
                job.progressObserver.UpdateStatus(BackupStateResum.ERROR);
                job.status.LastActionTimestamp = DateTime.Now;
                job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                throw new Exception($"Erreur lors de la sauvegarde complète : {ex.Message}", ex);
            }
        }

        private void CopyDirectoryRecursive(string sourcePath, string destinationPath, BackupJob job)
        {
            SysDirInfo sourceDir = new SysDirInfo(sourcePath);
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            foreach (SysFileInfo file in sourceDir.GetFiles())
            {
                // Vérification du logiciel métier
                if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
                {
                    DateTime startTime = DateTime.Now;
                    string destFilePath = Path.Combine(destinationPath, file.Name);

                    try
                    {
                        CopyAndEncryptFile(file, destFilePath);
                        TimeSpan duration = DateTime.Now - startTime;
                        job.progressObserver.NotifyFileSaved(file.Length, duration);

                        // Log événement logiciel métier
                        LogService.Instance.LogBusinessSoftwareEvent(job.name,
                            "Backup stopped - Business software detected during execution");
                    }
                    catch
                    {
                        job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                    }

                    throw new OperationCanceledException("Business software detected during backup");
                }

                // Copie normale du fichier
                DateTime startTime2 = DateTime.Now;
                string destFilePath2 = Path.Combine(destinationPath, file.Name);

                try
                {
                    CopyAndEncryptFile(file, destFilePath2);

                    TimeSpan duration = DateTime.Now - startTime2;
                    job.progressObserver.NotifyFileSaved(file.Length, duration);

                    // Logger chaque fichier copié
                    LogService.Instance.WriteLog(job.name, file.FullName, destFilePath2, file.Length, (long)duration.TotalMilliseconds);
                }
                catch
                {
                    job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                }
            }

            foreach (SysDirInfo subDir in sourceDir.GetDirectories())
            {
                string destSubDirPath = Path.Combine(destinationPath, subDir.Name);
                CopyDirectoryRecursive(subDir.FullName, destSubDirPath, job);
            }
        }

        private void CopyAndEncryptFile(SysFileInfo file, string destFilePath)
        {
            bool shouldEncrypt = file.Extension.ToLower() == ".txt";

            if (shouldEncrypt)
            {
                string tempEncrypted = destFilePath + ".temp";
                var encryptResult = CryptoSoftService.EncryptFile(file.FullName, tempEncrypted, "DefaultEncryptionKey2025");

                if (encryptResult.Success)
                {
                    if (File.Exists(destFilePath))
                        File.Delete(destFilePath);

                    File.Move(tempEncrypted, destFilePath);
                }
                else
                {
                    file.CopyTo(destFilePath, true);
                }
            }
            else
            {
                file.CopyTo(destFilePath, true);
            }
        }
    }
}
=======
				job.status.Status = BackupStateResum.FINISHED;
				job.status.Progress = 100;
				job.status.LastActionTimestamp = DateTime.Now;
			}
			catch (OperationCanceledException)
			{
				job.status.Status = BackupStateResum.ERROR;
				job.status.LastActionTimestamp = DateTime.Now;
				throw;
			}
			catch (Exception ex)
			{
				job.status.Status = BackupStateResum.ERROR;
				job.status.LastActionTimestamp = DateTime.Now;
				job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);

				LogService.Instance.LogBusinessSoftwareEvent(job.name, $"Full backup error: {ex.Message}");
				throw new Exception($"Erreur lors de la sauvegarde complète : {ex.Message}", ex);
			}
		}

		private void CopyDirectoryRecursive(string sourcePath, string destinationPath, BackupJob job)
		{
			SysDirInfo sourceDir = new SysDirInfo(sourcePath);

			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);

			foreach (SysFileInfo file in sourceDir.GetFiles())
			{
				// ⚠️ Vérification logiciel métier PENDANT l'exécution
				if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
				{
					Console.WriteLine($"⚠️  Logiciel métier détecté pendant la sauvegarde de {job.name}");
					Console.WriteLine($"    Fin du transfert du fichier en cours puis arrêt...");

					DateTime startTime = DateTime.Now;
					string destFilePath = Path.Combine(destinationPath, file.Name);

					try
					{
						bool shouldEncrypt = file.Extension.ToLower() == ".txt";

						if (shouldEncrypt)
						{
							string tempEncrypted = destFilePath + ".temp";
							var encryptResult = CryptoSoftService.EncryptFile(
								file.FullName,
								tempEncrypted,
								"DefaultEncryptionKey2025"
							);

							if (encryptResult.Success)
							{
								if (File.Exists(destFilePath))
									File.Delete(destFilePath);

								File.Move(tempEncrypted, destFilePath);
							}
							else
							{
								file.CopyTo(destFilePath, true);
							}
						}
						else
						{
							file.CopyTo(destFilePath, true);
						}

						TimeSpan duration = DateTime.Now - startTime;

						// ✅ LOG fichier copié (ms)
						long transferMs = (long)duration.TotalMilliseconds;
						LogService.Instance.WriteLog(job.name, file.FullName, destFilePath, file.Length, transferMs);

						job.progressObserver.NotifyFileSaved(file.Length, duration);
					}
					catch (Exception ex)
					{
						job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
						LogService.Instance.LogBusinessSoftwareEvent(job.name, $"Copy error: {file.FullName} - {ex.Message}");
					}

					LogService.Instance.LogBusinessSoftwareEvent(
						job.name,
						"Backup stopped - Business software detected during execution"
					);

					throw new OperationCanceledException("Business software detected during backup");
				}

				// COPIE NORMALE
				DateTime startTime2 = DateTime.Now;
				string destFilePath2 = Path.Combine(destinationPath, file.Name);

				try
				{
					bool shouldEncrypt = file.Extension.ToLower() == ".txt";

					if (shouldEncrypt)
					{
						string tempEncrypted = destFilePath2 + ".temp";
						var encryptResult = CryptoSoftService.EncryptFile(
							file.FullName,
							tempEncrypted,
							"DefaultEncryptionKey2025"
						);

						if (encryptResult.Success)
						{
							if (File.Exists(destFilePath2))
								File.Delete(destFilePath2);

							File.Move(tempEncrypted, destFilePath2);
						}
						else
						{
							file.CopyTo(destFilePath2, true);
						}
					}
					else
					{
						file.CopyTo(destFilePath2, true);
					}

					TimeSpan duration = DateTime.Now - startTime2;

					// ✅ LOG fichier copié (ms)
					long transferMs = (long)duration.TotalMilliseconds;
					LogService.Instance.WriteLog(job.name, file.FullName, destFilePath2, file.Length, transferMs);

					job.progressObserver.NotifyFileSaved(file.Length, duration);
				}
				catch (Exception ex)
				{
					job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
					LogService.Instance.LogBusinessSoftwareEvent(job.name, $"Copy error: {file.FullName} - {ex.Message}");
				}
			}

			foreach (SysDirInfo subDir in sourceDir.GetDirectories())
			{
				string destSubDirPath = Path.Combine(destinationPath, subDir.Name);
				CopyDirectoryRecursive(subDir.FullName, destSubDirPath, job);
			}
		}
	}
}
>>>>>>> feature/dlltype2
