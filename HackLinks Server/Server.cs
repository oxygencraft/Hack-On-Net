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
using static HackLinksCommon.NetUtil;

namespace HackLinks_Server
{
    class Server
    {
        public static readonly Server Instance = new Server();

        public List<GameClient> clients;

        private MySqlConnection conn;


        private ComputerManager computerManager;

        //Connection String args
        private MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder();
        public string MySQLServer { get => connectionStringBuilder.Server; internal set => connectionStringBuilder.Server = value; }
        public string Database { get => connectionStringBuilder.Database; internal set => connectionStringBuilder.Database = value; }
        public string UserID { get => connectionStringBuilder.UserID; internal set => connectionStringBuilder.UserID = value; }
        public string Password { get => connectionStringBuilder.Password; internal set => connectionStringBuilder.Password = value; }

        private Server()
        {
            clients = new List<GameClient>();
        }

        public void StartServer()
        {
            conn = new MySqlConnection(GetConnectionString());

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
            return connectionStringBuilder.GetConnectionString(true);
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
                            client.Send(PacketType.LOGRE, "0"); // Good account*/
                            var homeNode = computerManager.GetNodeById(homeId);
                            client.homeComputer = homeNode;
                            var ip = "none";
                            if (homeNode != null)
                                ip = homeNode.ip;
                            client.Send(PacketType.START, ip);
                        }
                        else
                        {
                            client.Send(PacketType.LOGRE, "1");
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
                            client.Send(PacketType.OSMSG, "ERR:0"); // OSMSG:ERR:0 = La commande est introuvable
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


        public void Broadcast(PacketType type, string message)
        {
            foreach(GameClient client in clients)
            {
                client.Send(type, message);
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
