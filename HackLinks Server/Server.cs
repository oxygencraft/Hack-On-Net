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
using HackLinks_Server.Computers.Files;
using HackLinks_Server.Database;

namespace HackLinks_Server
{
    class Server
    {
        public static readonly Server Instance = new Server();

        public List<GameClient> clients;

        private ComputerManager computerManager;
        private FileSystemManager fileSystemManager = new FileSystemManager();

        public FileSystemManager FileSystemManager => fileSystemManager;

        public DatabaseLink DatabaseLink { get; private set; }

        private Server()
        {
            clients = new List<GameClient>();
        }

        public void Initalize(ConfigUtil.ConfigData config)
        {
            DatabaseLink = new DatabaseLink(config);
        }

        public void StartServer()
        {
            Console.WriteLine("Downloading Computer data...");
            computerManager = new ComputerManager(this, DatabaseLink.DownloadDatabase());
            computerManager.Init();
            Console.WriteLine("Computer data loaded");
        }

        public void AddClient(Socket client)
        {
            var gameClient = new GameClient(client, this);
            clients.Add(gameClient);
            gameClient.Start();
        }

        public ComputerManager GetComputerManager()
        {
            return this.computerManager;
        }

        public void TreatMessage(GameClient client, PacketType type, string[] messages)
        {
            switch (type)
            {
                case PacketType.COMND:
                    if (client.status == GameClient.PlayerStatus.TERMINATED)
                        break;
                    if (!CommandHandler.TreatCommand(client, messages[0]))
                        client.Send(PacketType.OSMSG, "ERR:0"); // OSMSG:ERR:0 = La commande est introuvable
                    break;
                case PacketType.LOGIN:
                    if (messages.Length < 2)
                        return;

                    string tempUsername = messages[0];
                    string tempPass = messages[1];
                    int banExpiry;

                    if (DatabaseLink.TryLogin(client, tempUsername, tempPass, out int homeId))
                    {
                        client.username = tempUsername;
                        if (/*DatabaseLink.CheckUserBanStatus(client.username, out banExpiry)*/false)
                        {
                            if (banExpiry == 0)
                            {
                                client.Send(PacketType.LOGRE, "2", "You have been banned permanently");
                                client.Disconnect();
                                break;
                            }
                            client.Send(PacketType.LOGRE, "2", $"You have been banned until {DateTimeOffset.FromUnixTimeSeconds(banExpiry).ToString()} UTC");
                            client.Disconnect();
                            break;
                        }
                        client.Send(PacketType.LOGRE, "0"); // Good account*/
                        var homeNode = computerManager.GetNodeById(homeId);
                        var ip = "none";
                        if (homeNode != null)
                        {
                            ip = homeNode.ip;
                            client.homeComputer = homeNode;
                        }
                        client.permissions = DatabaseLink.GetUserPermissions()[client.username];
                        client.Send(PacketType.START, ip, DatabaseLink.GetUserNodes(client.username));
                    }
                    else
                    {
                        client.Send(PacketType.LOGRE, "1");
                        client.Disconnect();
                    }
                    break;
                case PacketType.DSCON:
                    client.netDisconnect();
                    break;
            }
        }

        public void RemoveClient(GameClient client)
        {
            try
            {
                if (client.activeSession != null)
                    client.activeSession.DisconnectSession();
            }
            catch (Exception e) { Console.WriteLine("Failed to disconnect session from disconnected client"); }
            Console.WriteLine(client.username + " disconnected from server.");
            clients.Remove(client);
        }


        public void Broadcast(PacketType type, params string[] data)
        {
            foreach(GameClient client in clients)
            {
                client.Send(type, data);
            }
        }

        public void MainLoop(double dT)
        {
            Thread.Sleep(10);
            foreach(GameClient client in clients)
            {
                if(client.activeSession != null)
                {
                    client.activeSession.UpdateTrace(dT);
                }
            }
        }

        internal void SaveDatabase()
        {
            DatabaseLink.UploadDatabase(computerManager.NodeList, computerManager.ToDelete);
        }
    }
}
