using System.Text.Json;

namespace EasyLog
{
    public sealed class JsonLogSerializer : ILogSerializer
    {
        public string FileExtension => "json";

        public string Serialize(LogEntry entry)
            => JsonSerializer.Serialize(entry, JsonDefaults.Options);
    }
}