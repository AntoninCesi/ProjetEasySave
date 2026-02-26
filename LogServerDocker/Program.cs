using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerApp
{
    class Server
    {
        static string logFolder = "/app/data";

        static void Main()
        {
            bool inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            if (!inContainer) logFolder = "DockerLogs";
            Directory.CreateDirectory(logFolder);

            var listener = new TcpListener(IPAddress.Any, 1051);
            listener.Start();
            Console.WriteLine("Log server listening on :1051");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                // Un thread par client — respecte votre pattern existant
                var t = new Thread(() => HandleClient(client)) { IsBackground = true };
                t.Start();
            }
        }

        static void HandleClient(TcpClient client)
        {
            Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Format attendu : "yyyy-MM-dd|ext|payload"
                    var parts = line.Split('|', 3);
                    if (parts.Length == 3)
                    {
                        string date = parts[0];   
                        string ext = parts[1];   // json ou xml
                        string payload = parts[2];

                        string filePath = Path.Combine(logFolder, $"{date}.{ext}");

                        lock (logFolder) // verrou global simple (un seul dossier)
                        {
                            File.AppendAllText(filePath, payload + Environment.NewLine, Encoding.UTF8);
                        }

                        Console.WriteLine($"[{date}] Log written ({ext})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client disconnected.");
            }
        }
    }
}