using System.Diagnostics;
using System.Linq;
using EasyLog;
using EasySave.Models;
using EasySave.Services;
using static System.Reflection.Metadata.BlobBuilder;

namespace EasySave.ViewModels;


/*public class MainViewModel
{
	private readonly List<BackupState> _states = new();
	private readonly List<BackupJob> _jobs = new();
	private readonly StateService _stateService = new();
	public MainViewModel()
	{
		_jobs.Add(new BackupJob
		{
*			SourcePath = @"C:\Temp\SourceTest",
			TargetPath = @"C:\Temp\TargetTest",
			Type = BackupType.Full
		});
	}
	public void ExecuteBackup(int jobIndex)
	{
		if (jobIndex < 0 || jobIndex >= _jobs.Count) return;

		var job = _jobs[jobIndex];

		// 1) Récupérer ou créer l'état du job
		var state = _states.FirstOrDefault(s => s.JobName == job.Name);
		if (state == null)
		{
			state = new BackupState { JobName = job.Name };
			_states.Add(state);
		}

		state.Status = BackupStatus.ACTIF;
		state.LastActionTimestamp = DateTime.Now;
		state.CurrentSourceUNC = job.SourcePath;
		state.CurrentDestinationUNC = job.TargetPath;

		try
		{
			// 2) Lister les fichiers + calculer totaux
			var files = Directory.GetFiles(job.SourcePath, "*.*", SearchOption.AllDirectories);

			state.TotalFiles = files.Length;
			state.TotalSizeBytes = files.Sum(f => new FileInfo(f).Length);

			state.RemainingFiles = state.TotalFiles;
			state.RemainingSizeBytes = state.TotalSizeBytes;
			state.Progress = 0f;

			_stateService.SaveStates(_states);

			// 3) Copier + logs + update state temps réel
			foreach (var file in files)
			{
				var fileInfo = new FileInfo(file);

				string destFile = file.Replace(job.SourcePath, job.TargetPath);
				string destDir = Path.GetDirectoryName(destFile)!;

				// Différentiel : ne copie que si le fichier a changé
				if (job.Type == BackupType.Differential && File.Exists(destFile))
				{
					var targetInfo = new FileInfo(destFile);
					if (fileInfo.Length == targetInfo.Length && fileInfo.LastWriteTime == targetInfo.LastWriteTime)
					{
						// Si on skip, on considère quand même ce fichier comme "traité"
						state.RemainingFiles -= 1;
						state.RemainingSizeBytes -= fileInfo.Length;

						state.Progress = state.TotalFiles > 0
							? (float)(state.TotalFiles - state.RemainingFiles) / state.TotalFiles
							: 0f;

						state.LastActionTimestamp = DateTime.Now;
						_stateService.SaveStates(_states);
						continue;
					}
				}

				if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

				// Update current file (avant copie)
				state.CurrentSourceUNC = file;
				state.CurrentDestinationUNC = destFile;
				state.LastActionTimestamp = DateTime.Now;
				_stateService.SaveStates(_states);

				// Timer + copie
				var stopWatch = Stopwatch.StartNew();
				File.Copy(file, destFile, true);
				stopWatch.Stop();

				// Log via la DLL EasyLog (1 ligne JSON par événement)
				EasyLog.EasyLog.Instance.WriteLog(
					DateTime.Now,
					job.Name,
					file,
					destFile,
					fileInfo.Length,
					stopWatch.ElapsedMilliseconds
				);

				// Update progress après traitement du fichier
				state.RemainingFiles -= 1;
				state.RemainingSizeBytes -= fileInfo.Length;

				state.Progress = state.TotalFiles > 0
					? (float)(state.TotalFiles - state.RemainingFiles) / state.TotalFiles
					: 0f;

				state.LastActionTimestamp = DateTime.Now;
				_stateService.SaveStates(_states);
			}

			// 4) Fin OK
			state.Status = BackupStatus.TERMINE;
			state.Progress = 1f;
			state.RemainingFiles = 0;
			state.RemainingSizeBytes = 0;
			state.CurrentSourceUNC = "";
			state.CurrentDestinationUNC = "";
			state.LastActionTimestamp = DateTime.Now;
			_stateService.SaveStates(_states);
		}
		catch (Exception ex)
		{
			// 5) Erreur
			Console.WriteLine($"Error during backup: {ex.Message}");

			state.Status = BackupStatus.EN_ERREUR;
			state.LastActionTimestamp = DateTime.Now;
			_stateService.SaveStates(_states);
		}
	}
}
*/