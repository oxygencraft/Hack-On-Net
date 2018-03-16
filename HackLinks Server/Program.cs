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

        public static bool recieving = false;

        static void Main(string[] args)
        {

            bool showHelp = false;
            bool writeConfig = false;
            bool overwriteConfig = false;
            string writeConfigPath = null;

            //Set Defaults and create config object
            ConfigUtil.ConfigData configData = new ConfigUtil.ConfigData();
            configData.MySQLServer = "127.0.0.1";
            configData.Port = 27015;
            configData.Database = "hacklinks";
            configData.UserID = "root";
            configData.Password = "";

            bool passSet = true;

            //Handle Args
            OptionSet options = new OptionSet() {
                { "s|server=", "the MySQL {SERVER} to use (default: \"127.0.0.1\").", v => configData.MySQLServer = v},
                { "d|database=", "the {DATABASE} to use (default: \"hacklinks\").", v => configData.Database = v},
                { "u|user=", "the {USERNAME} to connect with (default: \"root\").", v => configData.UserID = v},
                { "p|password:", "set the {PASSWORD} to connect with (default: None) or prompt for a password.", v => {passSet = v != null;  configData.Password = v;} },
                { "P|port=",
                    "set the {PORT} to open the server on (default: 27015).",
                    v =>
                    {
                        int result;
                        if(int.TryParse(v, out result))
                        {
                            configData.Port = result;
                        }
                        else
                        {
                            Console.WriteLine("Invalid Port Specified: \"{0}\". Using Default Port.", v);
                        }
                    }
                },
                { "c|config:",
                    "load settings from {CONFIG} file (default: No) or from default config file \"serverconfig.conf\".\n" +
                    "If the file doesn't exist it will be created with the the final values when the server runs unless the {-o/--overwrite-config} flag specifies a file instead.",
                    v =>
                    {
                        //Use given path, Existing path if it exists, or default if not.
                        string readConfigPath = v ?? "serverconfig.conf";

                        //If we aren't overwritting or if we are but the path is unset
                        if(!overwriteConfig || overwriteConfig && writeConfigPath == null)
                            writeConfigPath = readConfigPath;

                        //Loadconfig returns true if the file was loaded. So we should write the config if it's not
                        writeConfig = ! ConfigUtil.LoadConfig(readConfigPath, configData);
                    }
                },
                { "o|overwrite-config:",
                    "force the {CONFIG} file to be overwritten with the final values when the server runs.\n" +
                    "You can optionally specify the config file to be written here.",
                    v => 
                    {
                        //If config path is specified use that instead
                        writeConfigPath = v ?? writeConfigPath ?? "serverconfig.conf";

                        overwriteConfig = true;
                    }
                },
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

                //We write our config here as we likely don't want to save the prompted password
                if (overwriteConfig || writeConfig)
                    ConfigUtil.SaveConfig(writeConfigPath, configData);

                //Check if password is null and prompt if it is
                //Done here to avoid asking for password when other options are going to fail anyway or help should be displayed.
                if (!passSet)
                    configData.Password = GetPassword();

            } catch(OptionException e)
            {
                //One of our options failed. Output the message
                Console.WriteLine(e.Message);
                return;
            }

            //Apply Config to Server
            Server.Instance.MySQLServer = configData.MySQLServer;
            Server.Instance.Database = configData.Database;
            Server.Instance.UserID = configData.UserID;
            Server.Instance.Password = configData.Password;

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, configData.Port);
          
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
