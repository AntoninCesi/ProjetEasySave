namespace EasyLog
{
    /// <summary>
    /// Destination d'un log (fichier local, Docker, les deux).
    /// Thread-safe : chaque implémentation gère son propre verrou.
    /// </summary>
    public interface ILogSink : IDisposable
    {
        void Write(string payload, string fileExtension);
    }
}