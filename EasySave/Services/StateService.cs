using System.Text;
using System.Text.Json;
using EasySave.Models;

namespace EasySave.Services;

public class StateService
{
	private readonly object _lock = new();
	private readonly string _stateFilePath;

	public StateService()
	{
		string folderPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
			"ProSoft", "EasySave", "State"
		);

		Directory.CreateDirectory(folderPath);
		_stateFilePath = Path.Combine(folderPath, "state.json");
	}

	/// <summary>
	/// Ecrit l'état COMPLET (tous les jobs) dans state.json (temps réel).
	/// </summary>
	public void SaveStates(List<BackupState> states)
	{
		var options = new JsonSerializerOptions { WriteIndented = true };
		string json = JsonSerializer.Serialize(states, options);

		lock (_lock)
		{
			// écriture atomique = pas de JSON corrompu si crash
			string tmp = _stateFilePath + ".tmp";
			File.WriteAllText(tmp, json, Encoding.UTF8);
			File.Copy(tmp, _stateFilePath, true);
			File.Delete(tmp);
		}
	}
}
