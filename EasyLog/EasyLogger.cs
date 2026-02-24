using System;
using System.IO;
using System.Text;

namespace EasyLog
{
	public sealed class EasyLogger
	{
		private static readonly object _globalLock = new();

		private readonly string _baseFolder;
		private readonly ILogSerializer _serializer;
		private readonly LogFormatter _formatter = new();

		public EasyLogger(LogFormat format, string? baseFolder = null)
		{
			_baseFolder = string.IsNullOrWhiteSpace(baseFolder)
				? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
				: baseFolder;

			Directory.CreateDirectory(_baseFolder);

			_serializer = format switch
			{
				LogFormat.Xml => new XmlLogSerializer(),
				_ => new JsonLogSerializer()
			};
		}

		public void Write(LogEvent ev)
		{
			var entry = _formatter.Format(ev);

			string filePath = Path.Combine(
				_baseFolder,
				$"{DateTime.Now:yyyy-MM-dd}.{_serializer.FileExtension}"
			);

			string payload = _serializer.Serialize(entry);

			lock (_globalLock)
			{
				if (_serializer is XmlLogSerializer)
				{
					AppendXmlEntry(filePath, payload);
				}
				else
				{
					File.AppendAllText(filePath, payload + Environment.NewLine, Encoding.UTF8);
				}
			}
		}

		private static void AppendXmlEntry(string filePath, string xmlEntry)
		{
			// Crée un vrai document XML si le fichier n’existe pas encore
			if (!File.Exists(filePath))
			{
				var init =
					"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
					"<Logs>" + Environment.NewLine +
					"</Logs>" + Environment.NewLine;

				File.WriteAllText(filePath, init, Encoding.UTF8);
			}

			// On insère l’entrée juste avant </Logs>
			string content = File.ReadAllText(filePath, Encoding.UTF8);
			const string closing = "</Logs>";

			int idx = content.LastIndexOf(closing, StringComparison.Ordinal);
			if (idx < 0)
			{
				// Fichier corrompu -> on repart propre (safe)
				var reset =
					"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
					"<Logs>" + Environment.NewLine +
					"  " + xmlEntry + Environment.NewLine +
					"</Logs>" + Environment.NewLine;

				File.WriteAllText(filePath, reset, Encoding.UTF8);
				return;
			}

			string before = content.Substring(0, idx);
			string after = content.Substring(idx); // </Logs> + fin

			// Ajoute une indentation simple
			string insertion = "  " + xmlEntry + Environment.NewLine;

			File.WriteAllText(filePath, before + insertion + after, Encoding.UTF8);
		}
	}
}