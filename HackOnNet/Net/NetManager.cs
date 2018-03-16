using HackOnNet.GUI;
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

namespace HackOnNet.Net
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

        public UserScreen userScreen;

        public NetManager(UserScreen screen)
        {
            userScreen = screen;
        }

        public void Disconnect(Exception e, bool isInGame)
        {
            if (e.GetType() == typeof(SocketException))
            {
                var sockExcp = (SocketException)e;
                Console.WriteLine(sockExcp.ErrorCode);
                if(sockExcp.ErrorCode == 10061)
                {
                    MainMenu.loginState = MainMenu.LoginState.UNAVAILABLE;
                    response = "Server is unavailable.";
                }
            }
            connectDone.Set();
            Disconnect(isInGame);
        }

        public void Disconnect(bool isInGame)
        {
            clientSocket.Close();
            if(isInGame)
                userScreen.quitGame(this, "Connection Lost");
        }

        public void Init()
        {
            connectDone.Reset();
            sendDone.Reset();
            receiveDone.Reset();
            try
            {

                //Create config with defaults
                ConfigData conf = new ConfigData();
                conf.ServerIP = "127.0.0.1";
                conf.Port = 27015;

                //Populate config from file or create config file if it's not present
                if(!ConfigUtil.LoadConfig(configFile, conf))
                {
                    ConfigUtil.SaveConfig(configFile, conf);
                }

                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(conf.ServerIP), conf.Port);

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
            Send(NetUtil.PacketType.LOGIN, username + ":" + password);
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
                    var messages = content.Split(new string[] { "!!!" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var message in messages)
                        TreatMessage(message);
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

        private void TreatMessage(string message)
        {
            if (message.StartsWith("LOGRE:")) // LOGRE:[token(byte)]
            {
                var messages = message.Split(new char[] { ':' }, 2);
                if (messages.Length >= 2)
                {
                    if (messages[1] == "0") // LOGRE:0 = You're logged in
                        MainMenu.loginState = MainMenu.LoginState.LOGGED;
                    else if (messages[1] == "1") // LOGRE:1 = Invalid account
                        MainMenu.loginState = MainMenu.LoginState.INVALID;
                }
            }
            else if (message.StartsWith("MESSG:"))
            {
                var messages = message.Split(new char[] { ':' }, 2);
                if (messages.Length >= 2)
                {
                    string printMessage = messages[1];
                    userScreen.Write(printMessage);
                }
            }
            else if (message.StartsWith("KERNL:"))
            {
                var messages = message.Split(new char[] { ':' }, 2);
                if (messages.Length >= 2)
                {
                    userScreen.HandleKernel(messages[1]);
                }
            }
            else if (message.StartsWith("START:"))
            {
                var messages = message.Split(new char[] { ':' }, 2);
                userScreen.homeIP = messages[1];
            }
        }

        public void Send(NetUtil.PacketType type, String data)
        {
            data = $"{type.ToString()}:{data}!!!";
            byte[] byteData = Encoding.ASCII.GetBytes(data);

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
