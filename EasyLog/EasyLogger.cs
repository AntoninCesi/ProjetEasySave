using System;

namespace EasyLog
{
    public sealed class EasyLogger : IDisposable
    {
        private readonly ILogSink[] _sinks;
        private readonly ILogSerializer _serializer;
        private readonly LogFormatter _formatter = new();

        public EasyLogger(LogFormat format, ILogSink[] sinks)
        {
            _serializer = format switch
            {
                LogFormat.Xml => new XmlLogSerializer(),
                _ => new JsonLogSerializer()
            };
            _sinks = sinks;
        }

        public void Write(LogEvent ev)
        {
            var entry = _formatter.Format(ev);
            string payload = _serializer.Serialize(entry);

            // Chaque sink est thread-safe indépendamment
            foreach (var sink in _sinks)
                sink.Write(payload, _serializer.FileExtension);
        }

        public void Dispose()
        {
            foreach (var sink in _sinks)
                sink.Dispose();
        }
    }
}