using System;
using System.IO;
using System.Text;

namespace EasyLog
{
    public sealed class LocalFileSink : ILogSink
    {
        private static readonly object _lock = new();
        private readonly string _baseFolder;

        public LocalFileSink(string baseFolder)
        {
            _baseFolder = baseFolder;
            Directory.CreateDirectory(_baseFolder);
        }

        public void Write(string payload, string fileExtension)
        {
            string filePath = Path.Combine(
                _baseFolder,
                $"{DateTime.Now:yyyy-MM-dd}.{fileExtension}"
            );

            lock (_lock)
            {
                if (fileExtension == "xml")
                    AppendXmlEntry(filePath, payload);
                else
                    File.AppendAllText(filePath, payload + Environment.NewLine, Encoding.UTF8);
            }
        }

        private static void AppendXmlEntry(string filePath, string xmlEntry)
        {
            if (!File.Exists(filePath))
                File.WriteAllText(filePath,
                    $"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<Logs>{Environment.NewLine}</Logs>",
                    Encoding.UTF8);

            string content = File.ReadAllText(filePath, Encoding.UTF8);
            int idx = content.LastIndexOf("</Logs>", StringComparison.Ordinal);
            if (idx < 0)
            {
                File.WriteAllText(filePath,
                $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Logs>\n  {xmlEntry}\n</Logs>"); return;
            }

            File.WriteAllText(filePath,
                content[..idx] + "  " + xmlEntry + Environment.NewLine + content[idx..],
                Encoding.UTF8);
        }

        public void Dispose() { }
    }
}