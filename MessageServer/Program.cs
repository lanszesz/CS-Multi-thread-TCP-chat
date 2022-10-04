using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace MessageServer
{
    public class MessageServer
    {
        private int listeningPort;
        private string welcomeMessage;

        private TcpListener server;
        private TcpClient client;
        private NetworkStream stream;
        private IPAddress listeningIP;

        private string name;
        private string partnerName;

        IntPtr handle;

        public MessageServer(int listeningPort, string name, string welcomeMessage)
        {
            this.listeningPort = listeningPort;
            this.welcomeMessage = welcomeMessage;
            this.name = name;

            server = new TcpListener(IPAddress.Any, listeningPort);
            handle = Process.GetCurrentProcess().MainWindowHandle;

            Console.Title = "tcpChat - Server&Client";
        }

        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

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

            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Server started");
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Your name: " + name + " Listening for: " + listeningIP + " Listening Port: " + listeningPort);
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

            try
            {
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

                    string receivedMessage = DateTime.Now.ToString("[HH:mm:ss] ") + partnerName + ": " + ReceiveMessage() + "\n";

                    // for cool text effect
                    typeWriter(receivedMessage);

                    // orange light on taskbar if window is not in focus
                    FlashWindow(handle, true);
                }
            }
            catch (Exception)
            {
                setTextColor(1);
                Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Lost connection to client");
                Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Press any key to exit...");
                Console.ReadLine();
            }
        }

        public void typeWriter(string outputText)
        {
            for (int i = 0; i < outputText.Length; i++)
            {
                Console.Write(outputText[i]);
                Thread.Sleep(10);
            }
        }

        public void LoadOrSetup()
        {
            Console.Write("Load last configuration? (y/n): ");

            string answer = Console.ReadLine();
            switch (answer)
            {
                case "y":
                    Load();
                    break;
                case "Y":
                    Load();
                    break;
                case "n":
                    Setup();
                    break;
                case "N":
                    Setup();
                    break;
                default:
                    Console.Clear();
                    Logo();
                    LoadOrSetup();
                    break;
            }
        }

        public void Load()
        {
            Console.WriteLine("load");
        }

        public void Setup()
        {
            bool ok = false;

            Console.Clear();
            Logo();

            Console.WriteLine("Hi! This program works as a Server and a Client at the same time.");
            Console.Write("Your name: ");
            name = Console.ReadLine();

            while (!ok)
            {
                Console.Write("Listening PORT: ");
                try
                {
                    listeningPort = int.Parse(Console.ReadLine());
                    ok = true;
                }
                catch (Exception)
                {
                    Console.Clear();
                    Logo();
                }
            }

            ok = false;

            while (!ok)
            {
                Console.WriteLine("Listen for a single client IP address or 'any'?");
                Console.Write("Type in an IP or the keyword 'any': ");
                string ip = Console.ReadLine();

                switch (ip)
                {
                    case "any":
                        try
                        {
                            listeningIP = IPAddress.Any;
                            server = new TcpListener(listeningIP, listeningPort);
                            ok = true;
                        }
                        catch (Exception) { Console.Clear(); Logo(); }
                        break;
                    case "localhost":
                        try
                        {
                            listeningIP = IPAddress.Parse("127.0.0.1");
                            server = new TcpListener(listeningIP, listeningPort);
                            ok = true;
                        }
                        catch (Exception) { Console.Clear(); Logo(); }
                        break;
                    default:
                        try
                        {
                            listeningIP = IPAddress.Parse(ip);
                            server = new TcpListener(listeningIP, listeningPort);
                            ok = true;
                        }
                        catch (Exception) { Console.Clear(); Logo(); }
                        break;
                }
            }


            Console.Clear();
            Logo();
        }

        public void Logo()
        {
            Console.WriteLine("  /  |/  /__ ___ ___ ___ ____ ____ / __/__ _____  _____ ____");
            Console.WriteLine(" / /|_/ / -_|_-<(_-</ _ `/ _ `/ -_)\\ \\/ -_) __/ |/ / -_) __/");
            Console.WriteLine("/_/  /_/\\__/___/___/\\_,_/\\_, /\\__/___/\\__/_/  |___/\\__/_/   ");
            Console.WriteLine("                        /___/                              ");
        }

        public void Run()
        {
            Logo();

            LoadOrSetup();

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
            MessageServer messageServer = new MessageServer(7676, "ErwinServer", "Hi! You are connected!");
            messageServer.Run();
        }
    }
}
