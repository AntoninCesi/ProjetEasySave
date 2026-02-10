using System.Text;
using System.Text.Json;
using EasySave.Models;

namespace EasySave.Services;

/// <summary>
/// Gère l'ensemble des états de sauvegarde et leur persistance.
/// Responsable de l'écriture du fichier state.json en temps réel.
/// </summary>
public class BackupStateManager
{
	/// <summary>
	/// Verrou pour garantir la thread-safety
	/// (plusieurs sauvegardes peuvent écrire en même temps).
	/// </summary>
	private readonly object _lock = new();

	/// <summary>
	/// Chemin complet vers le fichier state.json.
	/// </summary>
	private readonly string _stateFilePath;

	/// <summary>
	/// Liste des états de tous les jobs de sauvegarde.
	/// </summary>
	private readonly List<BackupState> _listState = new();

	public BackupStateManager()
	{
		// Dossier conforme au CDC : ProgramData\ProSoft\EasySave\State
		string folderPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
			"ProSoft", "EasySave", "State"
		);

		// Création du dossier s'il n'existe pas
		Directory.CreateDirectory(folderPath);

		_stateFilePath = Path.Combine(folderPath, "state.json");
	}

	/// <summary>
	/// Crée un nouvel état de sauvegarde pour un job.
	/// Si le job existe déjà, son état est remplacé.
	/// </summary>
	public BackupState CreateState(int id, string jobName)
	{
		var state = new BackupState
		{
			Id = id,
			JobName = jobName,
			LastActionTimestamp = DateTime.Now,
			Status = BackupStatus.NON_ACTIF,
			Progress = 0f,

			// Valeurs initiales cohérentes
			TotalFiles = 0,
			TotalSizeBytes = 0,
			RemainingFiles = 0,
			RemainingSizeBytes = 0,
			CurrentSourceUNC = string.Empty,
			CurrentDestinationUNC = string.Empty
		};

		lock (_lock)
		{
			_listState.RemoveAll(s => s.Id == id || s.JobName == jobName);
			_listState.Add(state);

			SaveStateToFile_NoThrow();
		}

		return state;
	}

	/// <summary>
	/// Met à jour le fichier actuellement traité par le job.
	/// Appelé à chaque début de copie de fichier.
	/// </summary>
	public void UpdateCurrentFile(
		string jobName,
		string sourceFullPath,
		string destFullPath,
		long fileSize,
		DateTime startDate)
	{
		lock (_lock)
		{
			var state = FindByJobName(jobName);
			if (state == null) return;

			state.Status = BackupStatus.ACTIF;
			state.LastActionTimestamp = DateTime.Now;

			// ✅ NOMS CORRIGÉS (alignés avec BackupState.cs)
			state.CurrentSourceUNC = sourceFullPath;
			state.CurrentDestinationUNC = destFullPath;

			SaveStateToFile_NoThrow();
		}
	}

	/// <summary>
	/// Met à jour la progression globale du job.
	/// </summary>
	public void UpdateProgress(
		string jobName,
		int totalFiles,
		long totalSizeBytes,
		int remainingFiles,
		long remainingSizeBytes)
	{
		lock (_lock)
		{
			var state = FindByJobName(jobName);
			if (state == null) return;

			state.TotalFiles = totalFiles;
			state.TotalSizeBytes = totalSizeBytes;
			state.RemainingFiles = remainingFiles;
			state.RemainingSizeBytes = remainingSizeBytes;

			// ✅ Progress en 0..1 (comme ton MainViewModel)
			state.Progress = totalFiles > 0
				? (float)(totalFiles - remainingFiles) / totalFiles
				: 0f;

			state.LastActionTimestamp = DateTime.Now;

			SaveStateToFile_NoThrow();
		}
	}

	/// <summary>
	/// Marque un job comme terminé.
	/// </summary>
	public void MarkCompleted(string jobName)
	{
		lock (_lock)
		{
			var state = FindByJobName(jobName);
			if (state == null) return;

			state.Status = BackupStatus.TERMINE;
			state.Progress = 1f;
			state.RemainingFiles = 0;
			state.RemainingSizeBytes = 0;
			state.CurrentSourceUNC = string.Empty;
			state.CurrentDestinationUNC = string.Empty;
			state.LastActionTimestamp = DateTime.Now;

			SaveStateToFile_NoThrow();
		}
	}

	/// <summary>
	/// Marque un job comme étant en erreur.
	/// </summary>
	public void MarkError(string jobName)
	{
		lock (_lock)
		{
			var state = FindByJobName(jobName);
			if (state == null) return;

			state.Status = BackupStatus.EN_ERREUR;
			state.LastActionTimestamp = DateTime.Now;

			SaveStateToFile_NoThrow();
		}
	}

	/// <summary>
	/// Retourne le chemin du fichier state.json.
	/// </summary>
	public string GetStateFilePath() => _stateFilePath;

	/// <summary>
	/// Recherche un état par nom de job.
	/// </summary>
	private BackupState? FindByJobName(string jobName)
		=> _listState.FirstOrDefault(
			s => s.JobName.Equals(jobName, StringComparison.OrdinalIgnoreCase)
		);

	/// <summary>
	/// Sauvegarde la liste des états dans state.json.
	/// Écriture atomique pour éviter un fichier corrompu en cas de crash.
	/// </summary>
	private void SaveStateToFile_NoThrow()
	{
		try
		{
			var options = new JsonSerializerOptions { WriteIndented = true };
			string json = JsonSerializer.Serialize(_listState, options);

			string tmpFile = _stateFilePath + ".tmp";

			File.WriteAllText(tmpFile, json, Encoding.UTF8);
			File.Copy(tmpFile, _stateFilePath, true);
			File.Delete(tmpFile);
		}
		catch
		{
			// Ne jamais interrompre la sauvegarde principale
			// à cause d'un problème d'écriture de l'état
		}
	}
}
