using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace MessageClient
{
    public class MessageClient
    {
        // Welcome Message that will be received from the server
        private string welcomeMessage;

        // ipEndPoint = new IPEndPoint(ipAddress, serverPort);
        private IPAddress ipAddress;
        private int serverPort;

        // client.Connect(ipEndPoint);
        private IPEndPoint ipEndPoint;

        // The client that will connect to us. In this case it's the serveer and the client at the same time
        private TcpClient client;

        // For writing and reading messages
        private NetworkStream stream;

        private string name;
        private string partnerName;

        // For the log filename
        private string timestamp;

        // For output and log formatting purposes
        private int lineWidth;

        // For the orange flash on the taskbar icon
        IntPtr handle;

        public MessageClient()
        {
            Console.Title = "tcpChat - Client";
            handle = Process.GetCurrentProcess().MainWindowHandle;
            timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            lineWidth = 100;
        }

        // Goes through everything until the conversation can start
        public void Run()
        {
            // These methods all handle validation by themselves
            // You can only switch their order here, which is not recommended
            Logo();

            // Sets name, ipAddress, serverPort
            LoadOrSetup();

            CreateClient();

            Console.Title += "@" + name;

            // Connecting to server
            bool ok = false;
            while(!ok)
            {
                try
                {
                    client.Connect(ipEndPoint);
                    ok = true;
                }
                catch (Exception)
                {
                    Console.WriteLine("Couldn't connect to server");
                    Console.WriteLine("Press enter to retry...");
                    Console.ReadLine();
                }
            }

            // Magenta
            setTextColor(1);

            if (Handshaking())
            {
                // Green
                setTextColor(2);
                MessageLoop();
            }
        }

        // Ask the user to load previous settings (settings_client.txt) or run through the setup process again
        public void LoadOrSetup()
        {
            // If settings_client.txt doesn't exists jump to Setup();
            if (!File.Exists(@"settings_client.txt"))
            {
                Setup();

                // After Setup(); continue Run();
                return;
            }

            Console.Write("Load last configuration? (y/n): ");
            string answer = Console.ReadLine();

            // Input handling + validation
            switch (answer)
            {
                case "":
                    LoadSetup();
                    break;
                case "y":
                    LoadSetup();
                    break;
                case "Y":
                    LoadSetup();
                    break;
                case "n":
                    Setup();
                    break;
                case "N":
                    Setup();
                    break;
                default:
                    Logo();
                    LoadOrSetup();
                    break;
            }
        }

        // Listening for connection then exchanging names and sending the welcome message + some status text
        public bool Handshaking()
        {
            // Status text
            Log(DateTime.Now.ToString("[HH:mm:ss] ") + "Connecting...");

            // After successfull connection get the stream ready to read and write
            stream = client.GetStream();

            // Sending our name
            SendMessage(name);

            // Receiving partnerName and Welcome Message (successfull connection)
            partnerName = ReceiveMessage();
            welcomeMessage = ReceiveMessage();

            // Status text
            Log(DateTime.Now.ToString("[HH:mm:ss] ") + "Connection established with: " + client.Client.RemoteEndPoint + " (" + partnerName + ")");
            Log(DateTime.Now.ToString("[HH:mm:ss] ") + "Welcome message: '" + welcomeMessage + "' received");
            Log(DateTime.Now.ToString("[HH:mm:ss] ") + "Conversation started: " + DateTime.Now.ToString());
            Log(DateTime.Now.ToString("[HH:mm:ss] ") + "Send your first message");

            // This means the connection has been established, we can continue, see Run(); method
            return true;
        }

        // Handles when we are receiving or sending a message
        private void MessageLoop()
        {
            bool status = true; // false - receiving || true - sending 

            try
            {
                // This will throw an exception when connection is lost to server
                while (client.Connected)
                {
                    if (status)
                    {
                        // We are the senders now!

                        // We didn't send the timestamp to the client
                        // So we have a finalMessage for e.g: [17:42:57] lanses: HI! <- we log this
                        int cursorX = Console.CursorLeft;
                        int cursorY = Console.CursorTop;

                        string finalMessage = DateTime.Now.ToString("[HH:mm:ss] ") + name + ": ";

                        Console.Write(finalMessage);
                        string message = Console.ReadLine();

                        // And an actual message we send, in this example: "HI!"
                        SendMessage(message);

                        finalMessage += message;

                        PrettifyText(cursorX, cursorY, finalMessage);

                        status = false;
                        continue;
                    }

                    // We are the receivers now!

                    // Formatting the received message
                    string receivedMessage = DateTime.Now.ToString("[HH:mm:ss] ") + partnerName + ": " + ReceiveMessage() + "\n";

                    // Output the received message with a retro terminal effect
                    typeWriter(receivedMessage);

                    // Orange flash effect on taskbar, to notify a new message has arrived
                    FlashWindow(handle, true);

                    status = true;
                }
            }
            catch (Exception)
            {
                // Connection lost to server, prepare to exit
                setTextColor(1);
                Log(DateTime.Now.ToString("[HH:mm:ss] ") + "Lost connection to server");
                Log(DateTime.Now.ToString("[HH:mm:ss] ") + "Press enter to exit...");
                Console.ReadLine();
            }
        }

        // name, ipAddress, serverPort can be loaded from settings_client.txt
        public void LoadSetup()
        {
            // Stores the settings_client.txt
            List<string> settingsList = new List<string>();

            // Reading settings_client.txt line by line
            // 1.name, 2.ipAddress, 3.serverPort
            foreach (string line in File.ReadLines(@"settings_client.txt"))
            {
                settingsList.Add(line);
            }

            // Parsing the ipAddress or the serverPort could fail here
            try
            {
                // Storing what has been read
                name = settingsList[0];
                ipAddress = IPAddress.Parse(settingsList[1]);
                serverPort = int.Parse(settingsList[2]);
            }
            catch (Exception)
            {
                Console.WriteLine("Someone modified the settings_client.txt incorrectly!");
                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();

                Console.Clear();
                Setup();
            }
        }

        // Creates the client, client.connect() is in Run(); !!
        public void CreateClient()
        {
            try
            {
                ipEndPoint = new IPEndPoint(ipAddress, serverPort);
                client = new TcpClient();
            }
            catch (Exception)
            {
                Console.WriteLine("Client couldn't be created");
                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();

                Console.Clear();
                Setup();
            }
        }

        // For setting up name, ipAddress, serverPort -> these can be saved in a .txt
        public void Setup()
        {
            Logo();

            Console.WriteLine("Hi! This program works as a Client.");
            Console.Write("Your name: ");
            name = Console.ReadLine();

            // Until ipAddress is correct
            bool ok = false;
            while (!ok)
            {
                Console.Write("Server IP address: ");
                string ip = Console.ReadLine();

                if (ip == "localhost")
                {
                    ipAddress = IPAddress.Parse("127.0.0.1");
                    ok = true;
                    continue;
                }

                try
                {
                    ipAddress = IPAddress.Parse(ip);
                    ok = true;
                }
                catch (Exception)
                {
                    Logo();
                }
            }

            // Until listeningPort is correct (at least it's an int...)
            ok = false;
            while (!ok)
            {
                Console.Write("Server PORT: ");
                try
                {
                    serverPort = int.Parse(Console.ReadLine());
                    ok = true;
                }
                catch (Exception)
                {
                    Logo();
                }
            }

            SaveSetup();

            Logo();
        }

        // name, ipAddress, serverPort can be saved in settings_client.txt
        public void SaveSetup()
        {
            string[] lines = { name, ipAddress.ToString(), serverPort.ToString() };

            try
            {
                File.WriteAllLines("settings_client.txt", lines);
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't save settings");
                Console.WriteLine("Press enter to continue without saving...");
            }
        }

        // Sends a message via stream
        public void SendMessage(string message)
        {
            byte[] buffer = Encoding.Default.GetBytes(message);

            stream.Write(buffer, 0, buffer.Length);
        }

        // Receives a message from stream, then returns the message string
        public string ReceiveMessage()
        {
            byte[] buffer = new byte[512];

            stream.Read(buffer, 0, buffer.Length);

            string receivedMessage = Encoding.Default.GetString(buffer);

            // Buffer size is larger than the actual message
            // The rest is filled with '\0' (' '), we trim it
            receivedMessage = receivedMessage.TrimEnd('\0');

            return receivedMessage;
        }

        // Logs everything in a .txt then WriteLine
        public void Log(string message)
        {
            Console.WriteLine(message);

            string filename = "log_" + timestamp + ".txt";
            File.AppendAllText(filename, message + '\n');
        }

        // Logs everything but without WriteLine
        public void Log(string message, bool noWriteLine)
        {
            string filename = "log_" + timestamp + ".txt";
            File.AppendAllText(filename, message);
        }

        // Retro terminal effect for outputting received messages
        public void typeWriter(string outputText)
        {
            // We log the formatted string, see PrettifyText()'s comment
            string log = "";

            for (int i = 0, j = 1; i < outputText.Length; i++, j++)
            {
                Console.Write(outputText[i]);
                log += outputText[i];

                Thread.Sleep(10);

                // New line and left padding for long texts
                if (j % lineWidth == 0)
                {
                    Console.Write("\n           ");
                    log += "\n           ";
                }
            }

            Log(log, true);
        }

        // Console.ReadLine() can jump to the next row if our message is long enough
        // But we need padding so it looks aesthetic
        // [date] name: text bla bla
        //              bla bla bla
        // Also we log the prettified string 
        private void PrettifyText(int cursorX, int cursorY, string finalMessage)
        {
            Console.SetCursorPosition(cursorX, cursorY);

            string log = "";

            for (int i = 0, j = 1; i < finalMessage.Length; i++, j++)
            {
                Console.Write(finalMessage[i]);
                log += finalMessage[i];

                // New line and left padding for long texts
                if (j % lineWidth == 0)
                {
                    Console.Write("\n           ");
                    log += "\n           ";
                }
            }

            Log(log + '\n', true);

            Console.WriteLine();
        }

        public void Logo()
        {
            // Small Slant
            Console.Clear();
            Log("   __  ___                          ________          __ ");
            Log("  /  |/  /__ ___ ___ ___ ____ ____ / ___/ (_)__ ___  / /_");
            Log(" / /|_/ / -_|_-<(_-</ _ `/ _ `/ -_) /__/ / / -_) _ \\/ __/");
            Log("/_/  /_/\\__/___/___/\\_,_/\\_, /\\__/\\___/_/_/\\__/_//_/\\__/ ");
            Log("                        /___/                           ");
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

        // For the orange flash on the taskbar icon
        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hwnd, bool bInvert);
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            MessageClient messageServer = new MessageClient();
            messageServer.Run();
        }
    }
}
