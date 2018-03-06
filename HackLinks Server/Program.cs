using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;

namespace HackLinks_Server
{
    class Program
    {
        public class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 2048;
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }

        public static bool recieving = false;

        static void Main(string[] args)
        {

            bool showHelp = false;

            //Set Defaults
            Server.Instance.MySQLServer = "127.0.0.1";
            Server.Instance.Database = "hacklinks";
            Server.Instance.UserID = "root";
            Server.Instance.Password = "";

            bool passSet = true;

            //Handle Args
            OptionSet options = new OptionSet() {
                { "s|server=", "the MySQL {SERVER} to use (default: \"127.0.0.1\").", v => Server.Instance.MySQLServer = v},
                { "d|database=", "the {DATABASE} to use (default: \"hacklinks\").", v => Server.Instance.Database = v},
                { "u|user=", "the {USERNAME} to connect with (default: \"root\").", v => Server.Instance.UserID = v},
                { "p|password:", "set the {PASSWORD} to connect with (default: None) or prompt for a password.", v => {passSet = v != null; Server.Instance.Password = v;} },
                { "h|help",  "show this message and exit.", v => showHelp = v != null },
            };

            try
            {
                options.Parse(args);

                if (showHelp) // If help requested then show help and exit
                {
                    Console.WriteLine("Usage: HackLinks Server.exe [OPTIONS]");
                    Console.WriteLine("Starts the HackLinks Server.");
                    Console.WriteLine();

                    // output the options
                    Console.WriteLine("Options:");
                    options.WriteOptionDescriptions(Console.Out);

                    return;
                }

                //Check if password is null and prompt if it is
                //Done here to avoid asking for password when other options are going to fail anyway or help should be displayed.
                if (!passSet)
                    Server.Instance.Password = GetPassword();

            } catch(OptionException e)
            {
                //One of our options failed. Output the message
                Console.WriteLine(e.Message);
                return;
            }

            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 27015);

            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Server.Instance.StartServer();
            } catch(MySql.Data.MySqlClient.MySqlException e)
            {
                Console.WriteLine(e.Message);

                Console.WriteLine("\nHit enter to continue...");
                Console.Read();
                return;
            }

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    if(recieving == false)
                    {
                        Console.WriteLine("Waiting for a connection...");
                        listener.BeginAccept(
                            new AsyncCallback(AcceptCallback),
                            listener);
                        recieving = true;
                    }
                    Server.Instance.MainLoop();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        private static string GetPassword()
        {
            Console.Write("Please Enter Password:");

            string password = string.Empty;

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            while (keyInfo.Key != ConsoleKey.Enter)
            {
                password += keyInfo.KeyChar;
                keyInfo = Console.ReadKey(true);
            }

            Console.WriteLine();

            return password;
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            recieving = false;

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            Server.Instance.AddClient(handler);
        }

        

    }
}
