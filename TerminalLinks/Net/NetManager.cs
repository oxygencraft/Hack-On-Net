﻿using HackOnNet.GUI;
using HackLinksCommon;
using HackOnNet.Screens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static HackOnNet.ConfigUtil;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Media;
using System.Security.Cryptography;
using HackOnNet.Modules;

namespace TerminalLinks.Net
{
    class NetManager
    {

        private const string configFile = "Mods/HNMP.conf";
        public Socket clientSocket;

        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        private static String response = String.Empty;

        private bool disconnectHandled = false;
        //public UserScreen userScreen;
        public string nodesToSync = "";
        public bool gotNodes = false;

        public NetManager()
        {
            
        }

        public void Disconnect(Exception e, bool isInGame)
        {
            if (e.GetType() == typeof(SocketException))
            {
                var sockExcp = (SocketException)e;
                Console.WriteLine(sockExcp.ErrorCode);
                if(sockExcp.ErrorCode == 10061)
                {
                    TerminalLinks.loginState = TerminalLinks.LoginState.UNAVAILABLE;
                    response = "Server is unavailable.";
                }
            }
            connectDone.Set();
            Disconnect(isInGame, "Connection Lost");
        }

        public void Disconnect(bool isInGame, string reason, bool leftGame = false)
        {
            if (!disconnectHandled)
            {
                reason = string.IsNullOrWhiteSpace(reason) ? "The server or client did not provide a reason for disconnection" : reason;
                if (leftGame)
                {
                    disconnectHandled = true;
                    reason = "";
                }
                clientSocket.Close();
                Console.WriteLine($"Disconnected from game server: {reason}");
            }
        }

        public void Init(string server, int port)
        {
            connectDone.Reset();
            sendDone.Reset();
            receiveDone.Reset();
            try
            {

                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(server), 31342);

                clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                clientSocket.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), clientSocket);
                connectDone.WaitOne();


                // Release the socket.
                //clientSocket.Shutdown(SocketShutdown.Both);
                //clientSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Disconnect(e, false);
            }
        }

        public void Login(string username, string password)
        {
            Send(NetUtil.PacketType.LOGIN, username, password);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                // Complete the connection.
                clientSocket.EndConnect(ar);

                Console.WriteLine("Socket connected");/* to {0}",
                    clientSocket.RemoteEndPoint.ToString());*/

                // Signal that the connection has been made.
                connectDone.Set();
                response = "";

                Receive();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Disconnect(e, false);
            }
        }

        public void Receive()
        {
            try
            {
                NetUtil.StateObject state = new NetUtil.StateObject();

                clientSocket.BeginReceive(state.buffer, 0, NetUtil.StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Disconnect(e, true);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                NetUtil.StateObject state = (NetUtil.StateObject)ar.AsyncState;

                int bytesRead = clientSocket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    var content = state.sb.ToString();

                    Console.WriteLine($"Received Data: \"{content}\"");

                    List<NetUtil.Packet> packets = NetUtil.ParsePackets(content);

                    foreach(NetUtil.Packet packet in packets)
                    {
                        TreatMessage(packet.Type, packet.Data);
                    }
                }

                state.sb.Clear();
                clientSocket.BeginReceive(state.buffer, 0, NetUtil.StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Disconnect(e, true);
            }
        }

        private void TreatMessage(NetUtil.PacketType type, string[] messages)
        {
            switch (type)
            {
                case NetUtil.PacketType.KERNL:
                    if (messages.Length > 0)
                    {
                        userScreen.HandleKernel(messages);
                    }
                    break;
                case NetUtil.PacketType.MESSG:
                    if (messages.Length > 0)
                    {
                        string printMessage = messages[0];
                        Console.WriteLine(printMessage);
                    }
                    break;
                case NetUtil.PacketType.LOGRE:
                    if (messages.Length > 0)
                    {
                        if (messages[0] == "0") // LOGRE:0 = You're logged in
                            MainMenu.loginState = MainMenu.LoginState.LOGGED;
                        else if (messages[0] == "1") // LOGRE:1 = Invalid account
                            MainMenu.loginState = MainMenu.LoginState.INVALID;
                        else if (messages[0] == "2") // LOGRE:2 = The server rejected your connection for some reason (ban?)
                        {
                            if (string.IsNullOrWhiteSpace(messages[1]) == false)
                                MainMenu.serverRejectReason = messages[1];
                            MainMenu.loginState = MainMenu.LoginState.SERVER_REJECTED;
                        } 
                    }
                    break;
                case NetUtil.PacketType.START:
                    userScreen.homeIP = messages[0];
                    nodesToSync = messages[1];
                    gotNodes = true;
                    break;
                case NetUtil.PacketType.OSMSG:
                    break;
                case NetUtil.PacketType.FX:
                    userScreen.HandleFX(messages);
                    break;
                case NetUtil.PacketType.MUSIC:
                    if (messages.Length > 0) {
                        string songLocation = $"Mods\\HNMPMusic\\{messages[0]}.wav";
                        if (messages[0] == "shuffle") {
                            if (Directory.Exists("Mods\\HNMPMusic")) {
                                string[] files = Directory.GetFiles("Mods\\HNMPMusic");
                                List<string> songFiles = new List<string>();

                                foreach (string file in files) {
                                    if (Path.GetExtension(file) == ".wav") {
                                        songFiles.Add(file);
                                    }
                                }
                                MusicManager.Shuffle(songFiles);
                            } else {
                                userScreen.Write("Mods\\HNMPMusic does not exist.");
                            }
                        } else if (messages[0] == "list") {
                            if (Directory.Exists("Mods\\HNMPMusic")) {
                                string[] files = Directory.GetFiles("Mods\\HNMPMusic");
                                List<string> songFiles = new List<string>();
                                foreach (string file in files) {
                                    if (Path.GetExtension(file) == ".wav") {
                                        songFiles.Add(file);
                                    }
                                }
                                int i = 0;
                                List<string> names = new List<string>();
                                foreach (string name in songFiles) {
                                    int index = name.LastIndexOf('\\');
                                    names.Add(name.Substring(index + 1).Replace(".wav", ""));
                                    i++;
                                }
                                StringBuilder builder = new StringBuilder();
                                builder.Append(" \nYour Music Library:\n \n");
                                foreach (string name in names) {
                                    builder.Append(($"{name}\n"));
                                }
                                userScreen.Write(builder.ToString());
                            } else {
                                userScreen.Write("Mods\\HNMPMusic does not exist.");
                            }
                        } else if (messages[0] == "stop") {
                            MusicManager.Stop();
                            userScreen.Write("The music has been stopped!");
                        } else {
                            if (File.Exists(songLocation)) {
                                MusicManager.Play(songLocation);
                            } else {
                                userScreen.Write($"\"{songLocation}\" does not exist.");
                            }
                        }
                    }
                    break;
                case NetUtil.PacketType.DSCON:
                    Disconnect(true, messages[0]);
                    disconnectHandled = true;
                    break;
                default:
                    throw new InvalidOperationException($"Netmanager attempted treat message with invalid type { type.ToString() }");
            }
        }

        public void Send(NetUtil.PacketType type, params string[] data)
        {
            JObject packet = new JObject
            {
                {"type", type.ToString()},
                {"data", new JArray(data)},
            };

            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(packet.ToString());

            // Begin sending the data to the remote device.
            clientSocket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), clientSocket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                int bytesSent = clientSocket.EndSend(ar);
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Disconnect(e, true);
            }
        }
    }
}
