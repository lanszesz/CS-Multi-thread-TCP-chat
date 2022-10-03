using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace MessageServer
{
    public class MessageServer
    {
        private readonly string serverIP;
        private readonly int serverPort;
        private string welcomeMessage;

        private TcpListener server;
        private TcpClient client;
        private NetworkStream stream;

        private string name;
        private string partnerName;

        public MessageServer(string serverIP, int serverPort, string name, string welcomeMessage)
        {
            this.serverIP = serverIP;
            this.serverPort = serverPort;
            this.welcomeMessage = welcomeMessage;
            this.name = name;

            server = new TcpListener(IPAddress.Any, serverPort);
        }

        public void SendMessage(string message)
        {
            byte[] buffer = Encoding.Default.GetBytes(message);

            stream.Write(buffer, 0, buffer.Length);
        }

        public string ReceiveMessage()
        {
            byte[] buffer = new byte[512];

            stream.Read(buffer, 0, buffer.Length);

            string receivedMessage = Encoding.Default.GetString(buffer);

            receivedMessage = receivedMessage.TrimEnd('\0');

            return receivedMessage;
        }

        public bool Handshaking()
        {
            server.Start();

            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Waiting for connection...");

            client = server.AcceptTcpClient();

            stream = client.GetStream();

            // Receiving partnerName
            partnerName = ReceiveMessage();

            // Sending our name and welcome message (successfull connection)
            SendMessage(name);
            SendMessage(welcomeMessage);

            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Connection established with: " + client.Client.RemoteEndPoint + " (" + partnerName + ")");
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Welcome message '" + welcomeMessage + "' sent");
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Conversation started: " + DateTime.Now.ToString());
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Awaiting first response...");

            return true;
        }

        public void setTextColor(byte color)
        {
            switch (color)
            {
                case 1:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }
        }

        private void MessageLoop()
        {
            bool status = false; // false - receiving || true - sending 

            while (client.Connected)
            {
                if (status)
                {
                    status = false;

                    Console.Write((DateTime.Now.ToString("[HH:mm:ss] ") + name + ": "));
                    string message = Console.ReadLine();

                    SendMessage(message);

                    continue;
                }

                status = true;

                Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + partnerName + ": " + ReceiveMessage());
            }
        }

        public void Run()
        {
            Console.WriteLine("  /  |/  /__ ___ ___ ___ ____ ____ / __/__ _____  _____ ____");
            Console.WriteLine(" / /|_/ / -_|_-<(_-</ _ `/ _ `/ -_)\\ \\/ -_) __/ |/ / -_) __/");
            Console.WriteLine("/_/  /_/\\__/___/___/\\_,_/\\_, /\\__/___/\\__/_/  |___/\\__/_/   ");
            Console.WriteLine("                        /___/                              ");

            setTextColor(1);

            if (Handshaking())
            {
                setTextColor(2);
                MessageLoop();
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            MessageServer messageServer = new MessageServer("asd", 7676, "ErwinServer", "Hi! You are connected!");
            messageServer.Run();
        }
    }
}
