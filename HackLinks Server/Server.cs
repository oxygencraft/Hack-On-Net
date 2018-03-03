using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HackLinks_Server.Computers;
using System.Text.RegularExpressions;

namespace HackLinks_Server
{
    class Server
    {
        public static readonly Server Instance = new Server();

        public List<GameClient> clients;

        private MySqlConnection conn;


        private ComputerManager computerManager;


        private Server()
        {

            clients = new List<GameClient>();
            conn = new MySqlConnection();
            conn.ConnectionString =
            "Data Source=127.0.0.1;" +
            "Initial Catalog=hacklinks;" +
            "User id=root;" + //"User id=hacklinksServer;" +
            "Password="; //"Password=cx4IDcbAOUhY16Ab;";
            Console.WriteLine("Opening SQL connection");
            conn.Open();
            Console.WriteLine("SQL Running");

            computerManager = new ComputerManager(this);
            Console.WriteLine("Downloading Computer data...");
            computerManager.DownloadDatabase();
            Console.WriteLine("Computer data loaded");
        }

        public void AddClient(Socket client)
        {
            var gameClient = new GameClient(client, this);
            clients.Add(gameClient);
            gameClient.Start();
        }

        public MySqlConnection GetConnection()
        {
            return conn;
        }

        public ComputerManager GetComputerManager()
        {
            return this.computerManager;
        }

        public string GetConnectionString()
        {
            return "Data Source=127.0.0.1;" +
            "Initial Catalog=hacklinks;" +
            "User id=root;" + //"User id=hacklinksServer;" +
            "Password="; //"Password=cx4IDcbAOUhY16Ab;";
        }

        public void TreatMessage(GameClient client, string message)
        {
            //var messages = Regex.Split(message, "(?< !\\\\):");
            var messages = message.Split(new char[] { ':' });

            if (client.username == "")
            {
                if(messages.Length >= 2)
                {
                    if(messages[0] == "LOGIN") // LOGIN:[username]:[password]
                    {
                        if (messages.Length < 3)
                            return;

                        string tempUsername = messages[1];
                        string tempPass = messages[2];

                        MySqlCommand command = new MySqlCommand("SELECT pass, homeComputer FROM accounts WHERE username = @0", conn);
                        command.Parameters.Add(new MySqlParameter("0", tempUsername));
                        bool correctUser = false;
                        int homeId = -1;
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    if(reader.GetString("pass") == tempPass)
                                    {
                                        correctUser = true;
                                        homeId = reader.GetInt32("homeComputer");
                                        break;
                                    }
                                }
                                catch(Exception ex)
                                {

                                }
                            }
                        }
                        if(correctUser)
                        {
                            client.username = messages[1];
                            client.Send("LOGRE:0"); // Good account*/
                            var homeNode = computerManager.GetNodeById(homeId);
                            var ip = "none";
                            if (homeNode != null)
                                ip = homeNode.ip;
                            client.Send("START:" + ip);
                        }
                        else
                        {
                            client.Send("LOGRE:1");
                            client.Disconnect();
                        }
                    }
                }
            }
            else
            {
                if(messages.Length >= 2)
                {
                    if(messages[0] == "COMND")
                    {
                        if (!CommandHandler.TreatCommand(messages[1], client))
                            client.Send("OSMSG:ERR:0"); // OSMSG:ERR:0 = La commande est introuvable
                    }
                }
            }
        }

        public void RemoveClient(GameClient client)
        {
            if(client.activeSession != null)
                client.activeSession.DisconnectSession();
            clients.Remove(client);
        }


        public void Broadcast(string message)
        {
            foreach(GameClient client in clients)
            {
                client.Send(message);
            }
        }

        public void MainLoop()
        {
            Thread.Sleep(10);
            foreach(GameClient client in clients)
            {
            }
        }
    }
}
