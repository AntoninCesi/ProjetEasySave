using System;
using System.IO;
using System.Text.Json;

namespace EasySave.Models
{
    /// <summary>
    /// Application settings model using Singleton pattern.
    /// Handles loading and saving application configuration to JSON file.
    /// </summary>
    public class AppSettings
    {
        private static AppSettings? _instance;
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave",
            "settings.json"
        );

        /// <summary>
        /// Gets the singleton instance of AppSettings
        /// </summary>
        public static AppSettings Instance => _instance ??= Load();

        // Settings properties with default values
        public string LogFormat { get; set; } = "JSON";
        public string EncryptionExtensions { get; set; } = ".docx,.xlsx,.pptx";
        public string BusinessSoftware { get; set; } = "CalculatorApp";
        public string Language { get; set; } = "en-US";

        /// <summary>
        /// Private constructor to enforce Singleton pattern
        /// </summary>
        private AppSettings() { }

        /// <summary>
        /// Loads settings from JSON file or creates default if file doesn't exist
        /// </summary>
        /// <returns>AppSettings instance with loaded or default values</returns>
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
                // Log error but continue with default settings
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new AppSettings();
        }

        /// <summary>
        /// Saves current settings to JSON file
        /// </summary>
        public void Save()
        {
            try
            {
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Serialize and save with indentation for readability
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
        }
    }
}
