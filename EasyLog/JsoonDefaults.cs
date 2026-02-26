using System.Text.Encodings.Web;
using System.Text.Json;

namespace EasyLog
{
    internal static class JsonDefaults
    {
        // JSON compact (1 ligne par entrée) : idéal pour append en temps réel.
        internal static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
}
