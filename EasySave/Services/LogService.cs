using EasyLog;

namespace EasySave.Services;

public class LogService
{
    private static LogService? _instance;
    public static LogService Instance => _instance ??= new LogService();

    private LogService() { }

    public void WriteLog(string jobName, string source, string target, long fileSize, long transferTime)
    {
        EasyLogger.Instance.WriteLog(
            DateTime.Now,
            jobName,
            source,
            target,
            fileSize,
            transferTime
        );
    }

    // Logs a business event instead of a file transfer
    // "N/A" is used for source since there is no file involved
    // 0 for fileSize and -1 for transferTime indicate that this is not a transfer operation

    public void LogBusinessSoftwareEvent(string jobName, string eventMessage)
    {
        EasyLogger.Instance.WriteLog(
            DateTime.Now,
            jobName,
            "N/A",
            eventMessage,
            0,
            -1
        );
    }
}
