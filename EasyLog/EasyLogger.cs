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

            string line = _serializer.Serialize(entry);

            lock (_globalLock)
            {
                File.AppendAllText(filePath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
