using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EasyLog
{
    /// <summary>
    /// Envoie les logs vers le serveur Docker via TCP sur un thread dédié.
    /// Thread-safe : la queue ConcurrentQueue + ManualResetEvent gère la synchro.
    /// </summary>
    public sealed class DockerSocketSink : ILogSink
    {
        private readonly string _host;
        private readonly int _port;

        // File d'attente thread-safe : les threads de backup y poussent,
        // le thread d'envoi consomme.
        private readonly ConcurrentQueue<string> _queue = new();
        private readonly ManualResetEventSlim _signal = new(false);
        private readonly CancellationTokenSource _cts = new();
        private readonly Thread _senderThread;

        private TcpClient? _client;
        private NetworkStream? _stream;

        public DockerSocketSink(string host, int port)
        {
            _host = host;
            _port = port;

            // Thread dédié à l'envoi — ne bloque PAS les threads de backup
            _senderThread = new Thread(SendLoop)
            {
                IsBackground = true,
                Name = "DockerLogSender"
            };
            _senderThread.Start();
        }

        /// <summary>
        /// Appelé depuis n'importe quel thread de backup — non bloquant.
        /// </summary>
        public void Write(string payload, string fileExtension)
        {
            // On préfixe la ligne avec la date pour que le serveur puisse
            // créer le bon fichier journalier de son côté.
            string line = $"{DateTime.Now:yyyy-MM-dd}|{fileExtension}|{payload}";
            _queue.Enqueue(line);
            _signal.Set(); // Réveille le thread d'envoi
        }

        // ─── Thread dédié à l'envoi ───────────────────────────────────────
        private void SendLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                _signal.Wait(_cts.Token); // Dort jusqu'à ce qu'il y ait quelque chose
                _signal.Reset();

                while (_queue.TryDequeue(out string? line))
                {
                    TrySend(line);
                }
            }
        }

        private void TrySend(string line)
        {
            try
            {
                EnsureConnected();
                byte[] data = Encoding.UTF8.GetBytes(line + "\n");
                _stream!.Write(data, 0, data.Length);
            }
            catch
            {
                // Connexion perdue : on remet dans la queue et on attend
                _queue.Enqueue(line);
                Reconnect();
                Thread.Sleep(2000);
            }
        }

        private void EnsureConnected()
        {
            if (_client?.Connected == true) return;
            Reconnect();
        }

        private void Reconnect()
        {
            try { _client?.Close(); } catch { }
            _client = new TcpClient();
            _client.Connect(_host, _port);
            _stream = _client.GetStream();
        }

        public void Dispose()
        {
            _cts.Cancel();
            _signal.Set();
            _senderThread.Join(3000);
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}