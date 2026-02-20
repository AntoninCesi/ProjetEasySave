using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLog
{
    public interface ILogSerializer
    {
        string FileExtension { get; } // "json" ou "xml"
        string Serialize(LogEntry entry); // 1 entry -> 1 ligne
    }
}