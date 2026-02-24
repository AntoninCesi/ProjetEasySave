using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
	public class AppSettings
	{
		private static AppSettings? _instance;
		private static readonly string SettingsFilePath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"EasySave",
			"settings.json"
		);

		public static AppSettings Instance => _instance ??= Load();

		public string LogFormat { get; set; } = "JSON";
		public string EncryptionExtensions { get; set; } = ".docx,.xlsx,.pptx";
		public string BusinessSoftware { get; set; } = "CalculatorApp";
		public string Language { get; set; } = "en-US";

		// ✅ Back-end : liste d'extensions prioritaires
		// (sert aux règles de priorité)
		public List<string> PriorityExtensions { get; set; } = new();

		// ✅ Back-end : seuil (Ko) utilisé pour limiter le parallélisme
		// On met long pour éviter ton erreur: "long -> int"
		public long MaxParallelFileSizeKo { get; set; } = 0;

		// -------------------------
		// PROPRIÉTÉS "BRIDGE" UI
		// -------------------------
		// UI aime souvent binder une textbox sur un string => on expose une version CSV.
		// JsonIgnore pour éviter de doubler les champs dans settings.json
		[JsonIgnore]
		public string PriorityExtensionsCsv
		{
			get => string.Join(",", PriorityExtensions ?? new List<string>());
			set => PriorityExtensions = SplitExtensions(value);
		}

		// Si certains ViewModels attendent un int, on expose un int (sans casser le stockage long)
		[JsonIgnore]
		public int MaxParallelFileSizeKoInt
		{
			get => (MaxParallelFileSizeKo > int.MaxValue) ? int.MaxValue : (int)MaxParallelFileSizeKo;
			set => MaxParallelFileSizeKo = value;
		}

		private AppSettings() { }

		private static AppSettings Load()
		{
			try
			{
				if (File.Exists(SettingsFilePath))
				{
					string json = File.ReadAllText(SettingsFilePath);
					return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading settings: {ex.Message}");
			}

			return new AppSettings();
		}

		public void Save()
		{
			try
			{
				string? directory = Path.GetDirectoryName(SettingsFilePath);
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				var options = new JsonSerializerOptions { WriteIndented = true };
				string json = JsonSerializer.Serialize(this, options);
				File.WriteAllText(SettingsFilePath, json);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error saving settings: {ex.Message}");
				throw;
			}
		}

		public void ResetToDefaults()
		{
			LogFormat = "JSON";
			EncryptionExtensions = ".docx,.xlsx,.pptx";
			BusinessSoftware = "calc";
			Language = "en-US";

			PriorityExtensions = new List<string>();
			MaxParallelFileSizeKo = 0;
		}

		private static List<string> SplitExtensions(string? csv)
		{
			if (string.IsNullOrWhiteSpace(csv)) return new List<string>();

			return csv
				.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(x => x.StartsWith(".") ? x : "." + x) // optionnel: force le point
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
		}
	}
}