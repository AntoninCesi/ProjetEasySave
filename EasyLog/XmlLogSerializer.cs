using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace EasyLog
{
    public sealed class XmlLogSerializer : ILogSerializer
    {
        public string FileExtension => "xml";

        public string Serialize(LogEntry entry)
        {
            var serializer = new XmlSerializer(typeof(LogEntry));

            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = false,
                NewLineHandling = NewLineHandling.None
            };

            using var sw = new StringWriterUtf8();
            using (var xw = XmlWriter.Create(sw, settings))
            {
                serializer.Serialize(xw, entry);
            }

            // 1 ligne / entrée (évite de casser le JSONL-like)
            return sw.ToString().Replace("\r", "").Replace("\n", "");
        }

        private sealed class StringWriterUtf8 : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
