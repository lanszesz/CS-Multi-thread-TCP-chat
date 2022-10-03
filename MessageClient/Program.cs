using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace MessageServer
{
    public class MessageClient
    {
        private string serverIP;
        private int serverPort;
        private string welcomeMessage;

        IPAddress ipAddress;
        IPEndPoint ipEndPoint;
        TcpClient client;
        private NetworkStream stream;

        private string name;
        private string partnerName;

        public MessageClient(string serverIP, int serverPort, string name, string welcomeMessage)
        {
            this.serverIP = serverIP;
            this.serverPort = serverPort;
            this.welcomeMessage = welcomeMessage;
            this.name = name;

            ipAddress = IPAddress.Parse("127.0.0.1");
            ipEndPoint = new IPEndPoint(ipAddress, 7676);
            client = new TcpClient();
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
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Connecting...");

            client.Connect(ipEndPoint);

            stream = client.GetStream();

            // Sending our name
            SendMessage(name);

            // Receiving partnerName and Welcome Message (successfull connection)
            partnerName = ReceiveMessage();
            welcomeMessage = ReceiveMessage();

            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Connection established with: " + client.Client.RemoteEndPoint + " (" + partnerName + ")");
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Welcome message: '" + welcomeMessage + "' received");
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Conversation started: " + DateTime.Now.ToString());
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Send your first message");

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
            bool status = true; // false - receiving || true - sending 

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
            Console.WriteLine("  /  |/  /__ ___ ___ ___ ____ ____ / ___/ (_)__ ___  / /_");
            Console.WriteLine(" / /|_/ / -_|_-<(_-</ _ `/ _ `/ -_) /__/ / / -_) _ \\/ __/");
            Console.WriteLine("/_/  /_/\\__/___/___/\\_,_/\\_, /\\__/\\___/_/_/\\__/_//_/\\__/ ");
            Console.WriteLine("                        /___/                            ");

            Console.Write("Server IP address: ");
            ipAddress = IPAddress.Parse(Console.ReadLine());
            Console.Write("Server PORT: ");
            serverPort = int.Parse(Console.ReadLine());
            ipEndPoint = new IPEndPoint(ipAddress, serverPort);

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
            MessageClient messageServer = new MessageClient("asd", 7676, "Erwin", "Hi! You are connected!");
            messageServer.Run();
        }
    }
}
