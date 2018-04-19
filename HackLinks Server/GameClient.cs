using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static HackLinksCommon.NetUtil;

namespace HackLinks_Server
{
    public class GameClient
    {

        public Socket client;
        public Server server;

        public string username = "";

        public Session activeSession;
        public List<Permissions> permissions =  new List<Permissions>();
        public Node homeComputer;

        public string buffer = "";

        public enum PlayerStatus
        {
            ONLINE,
            TERMINATED
        }

        public PlayerStatus status = PlayerStatus.ONLINE;

        public int UserId { get; internal set; }

        public GameClient(Socket client, Server server)
        {
            this.client = client;
            this.server = server;
        }

        public void ConnectTo(Node node)
        {
            Send(PacketType.KERNL, "connect", "succ", node.ip, "3");
            if (node == homeComputer)
            {
                activeSession = new Session(this, node);

                activeSession.Login(Group.ROOT, username);
            }
            else
            {
                activeSession = new Session(this, node);
            }
        }

        public void Disconnect()
        {
            if(activeSession != null)
            {
                activeSession.DisconnectSession();
                activeSession = null;
                Send(PacketType.KERNL, "disconnect");
            }
        }

        public void Start()
        {
            try
            {
                StateObject state = new StateObject();

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                netDisconnect();
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            try
            {
                String content = String.Empty;

                StateObject state = (StateObject)ar.AsyncState;

                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    content = state.sb.ToString();

                    Console.WriteLine($"Received Data: \"{content}\"");

                    List<Packet> packets = ParsePackets(content);

                    foreach (Packet packet in packets)
                    {
                        server.TreatMessage(this, packet.Type, packet.Data);
                    }

                    state.sb.Clear();
                    
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                netDisconnect();
            }
        }

        public void netDisconnect()
        {
            //client.Disconnect(false);
            client.Dispose();
            server.RemoveClient(this);
        }

        public void Send(PacketType type, params string[] data)
        {
            try
            {
                JObject packet = new JObject
                {
                    {"type", type.ToString()},
                    {"data", new JArray(data)},
                };

                // Convert the string data to byte data using ASCII encoding.
                byte[] byteData = Encoding.ASCII.GetBytes(packet.ToString());

                // Begin sending the data to the remote device.
                client.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), client);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                netDisconnect();
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                netDisconnect();
            }
        }

        public void TraceTermination()
        {
            if(this.activeSession != null)
            {
                activeSession.traceSpd = 0;
                activeSession.trace = 0;
            }
            

            Send(PacketType.FX, "traceOver");
            Disconnect();
            status = PlayerStatus.TERMINATED;
        }
    }
}
