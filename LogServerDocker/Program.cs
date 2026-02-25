using System;
using System.Net;
using System.Net.Sockets;
using System.IO;


namespace ServerApp
{
    class Server
    {
        static IPEndPoint clientEndPoint;
        static string logFile;

        static void Main(string[] args)
        {
            bool IsInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

            if (IsInContainer)
            {
                Console.WriteLine("Running in a container");
                logFile = "/app/data/server.log";
                Directory.CreateDirectory(Path.GetDirectoryName(logFile));
            }
            else
            {
                logFile = "server.log";
            }

            Socket serverSocket = StartServer();
            Socket clientSocket = AcceptConnection(serverSocket);
            ListenToClient(clientSocket);
            DisconnectClient(clientSocket);
            serverSocket.Close();


        }
        private static Socket StartServer()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 1051);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(serverEndPoint);
            serverSocket.Listen(1);
            Console.WriteLine("Server is listening on port 1051...");
            return serverSocket;
        }

        private static Socket AcceptConnection(Socket socket)
        {
            Socket clientSocket = socket.Accept();
            clientEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;
            Console.WriteLine("Client connected to address {0} on port {1}", clientEndPoint.Address, clientEndPoint.Port);
            return clientSocket;
        }

        private static void ListenToClient(Socket client)
        {
            byte[] buffer = new byte[1024];
            int receivedDataLenght = 0;
            string WelcomeMessage = "Welcome to the server !";
            client.Send(System.Text.Encoding.UTF8.GetBytes(WelcomeMessage));
            while (true)
            {
                try
                {
                    receivedDataLenght = client.Receive(buffer);
                    if (receivedDataLenght == 0)
                    {
                        Console.WriteLine("Client disconnected.");
                        break;
                    }

                    string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, receivedDataLenght);
                    Console.WriteLine("Received message from client: {0}", receivedMessage);
                    string responseMessage = "Server received: " + receivedMessage;
                    Console.WriteLine("Sending response to client: {0}", responseMessage);
                    log(receivedMessage);
                    client.Send(System.Text.Encoding.UTF8.GetBytes(responseMessage));
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Socket exception: {0}", ex.Message);
                    break;
                }
            }
        }
        private static void log(string message)
        {
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM--dd HH:mm:ss}: {message}{Environment.NewLine}";
                File.AppendAllText(logFile, logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to log file: {0}", ex.Message);
            }
        }

        private static void DisconnectClient(Socket client)
        {
            Console.WriteLine("Client disconnected from {0}.", clientEndPoint.Address);
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            Console.ReadLine();
        }

    }
}