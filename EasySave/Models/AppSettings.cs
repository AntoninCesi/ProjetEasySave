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

<<<<<<< HEAD
		// ✅ Back-end : liste d'extensions prioritaires
		// (sert aux règles de priorité)
		public List<string> PriorityExtensions { get; set; } = new();

		// ✅ Back-end : seuil (Ko) utilisé pour limiter le parallélisme
		// On met long pour éviter ton erreur: "long -> int"
		public long MaxParallelFileSizeKo { get; set; } = 0;
=======
        /// <summary>
        /// Extensions de fichiers considérées comme prioritaires lors des sauvegardes.
        /// Les fichiers avec ces extensions seront copiés en premier.
        /// Format : ".ext1,.ext2,.ext3"
        /// </summary>
        public string PriorityExtensions { get; set; } = "";

        /// <summary>
        /// Taille maximale en Ko au-delà de laquelle un fichier est considéré "gros".
        /// Un seul fichier "gros" peut être transféré à la fois en parallèle.
        /// 0 = pas de limite (comportement par défaut).
        /// </summary>
        public long MaxParallelFileSizeKo { get; set; } = 0;

        /// <summary>
        /// Private constructor to enforce Singleton pattern
        /// </summary>
        private AppSettings() { }

        /// <summary>
        /// Retourne la liste des extensions prioritaires sous forme de tableau.
        /// </summary>
        public string[] GetPriorityExtensionsArray()
        {
            if (string.IsNullOrWhiteSpace(PriorityExtensions))
                return Array.Empty<string>();

            return PriorityExtensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        /// <summary>
        /// Loads settings from JSON file or creates default if file doesn't exist
        /// </summary>
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
>>>>>>> 29742782d5f8c515a37c523f4200e5c09c305aa9

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

<<<<<<< HEAD
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
=======
        /// <summary>
        /// Saves current settings to JSON file
        /// </summary>
        public void Save()
        {
            try
            {
                string? directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

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

        /// <summary>
        /// Resets all settings to default values
        /// </summary>
        public void ResetToDefaults()
        {
            LogFormat = "JSON";
            EncryptionExtensions = ".docx,.xlsx,.pptx";
            BusinessSoftware = "calc";
            Language = "en-US";
            PriorityExtensions = "";
            MaxParallelFileSizeKo = 0;
        }
    }
}
>>>>>>> 29742782d5f8c515a37c523f4200e5c09c305aa9
