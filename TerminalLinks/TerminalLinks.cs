using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TerminalLinks.Net;

namespace TerminalLinks
{
    static class TerminalLinks
    {
        public enum LoginState { MENU, LOGGED_IN, INVALID, UNAVAILABLE, LOGGED, LOGGING_IN, SERVER_REJECTED }

        public static LoginState loginState = LoginState.MENU;
        public static string serverRejectReason = "";

        async public static void StartGame(string server, int port, string username, string password)
        {
            NetManager netManager = new NetManager();
            netManager.Init(server, port);

            if (loginState == LoginState.UNAVAILABLE)
            {
                Console.WriteLine("The server is unavailable.");
                loginState = LoginState.MENU;
                await Task.Delay(5000);
                return;
            }

            string hashedPassword = Hash(password);
            netManager.Login(username, hashedPassword);

            Console.WriteLine("Logging in...");

            loginState = LoginState.LOGGING_IN;

            for (int i = 0; i < 1000; i++)
            {
                await Task.Delay(10);
                if (loginState != LoginState.LOGGING_IN)
                    break;
            }
            if (loginState == LoginState.LOGGING_IN)
            {
                loginState = LoginState.MENU;
                Console.WriteLine("Login Timeout");
                await Task.Delay(5000);
                return;
            }
            else if (loginState == LoginState.LOGGED)
            {
                Console.WriteLine("Logged in");
                Console.WriteLine("------------------------------------------------------------------------------------------------");
            }
            else if (loginState == LoginState.INVALID)
            {
                loginMessage = "Invalid Username or Password.";
            }
            else if (loginState == LoginState.UNAVAILABLE)
            {
                loginMessage = "The server is unavailable.";
            }
            else if (loginState == LoginState.SERVER_REJECTED)
            {
                string reason = string.IsNullOrWhiteSpace(serverRejectReason) ? "Server did not provide reason for rejection" : serverRejectReason;
                loginMessage = "The server rejected the connection: " + reason;
                serverRejectReason = "";
            }

            loginState = LoginState.MENU;
        }

        public void HandleKernel(string[] command)
        {
            if (command[0] == "connect")
            {
                if (command[1] == "fail")
                {
                    Write("Connection failed.");
                    return;
                }
                int privilege;
                int.TryParse(command[3], out privilege);
                this.netMap.DiscoverNode(command[2], true);
                string currentIP;
                currentIP = command[2];
                display.state = DisplayState.SSH_SESSION;
                Write("Connected to: " + command[2]);
                activeSession = new Session(currentIP, privilege);
            }
            else if (command[0] == "disconnect")
            {
                activeSession = null;
                display.state = DisplayState.NONE;
            }
            else if (command[0] == "ls")
            {
                display.state = DisplayState.NONE;
                activeSession.workingPath = command[1];
                if (activeSession.GetState().GetStateType() != SessionState.StateType.LS)
                {
                    activeSession.SetState(new LsState(activeSession));
                }
                var sessionState = (LsState)activeSession.GetState();
                sessionState.files.Clear();
                for (int i = 2; i < command.Length; i += 3)
                {
                    if (command[i] == "")
                        continue;
                    sessionState.files.Add(new LsFileEntry(command.Skip(i).Take(3).ToArray()));
                }
                display.state = DisplayState.LS;
            }
            else if (command[0] == "cd")
            {
                activeSession.workingPath = command[1];
            }
            else if (command[0] == "login")
            {
                activeSession.privilege = int.Parse(command[1]);
                activeSession.accountName = command[2];
            }
            else if (command[0] == "state")
            {
                if (command[1] == "irc")
                {
                    if (command[2] == "join")
                    {
                        activeSession.SetState(new IrcState(activeSession));
                        display.state = DisplayState.IRC;
                    }
                    else if (command[2] == "messg")
                    {
                        for (int i = 3; i < command.Length; i += 2)
                        {
                            string author = command[i];
                            string message = command[i + 1];

                            ((IrcState)activeSession.GetState()).AddMessage(author, message);
                        }

                    }
                }
                else if (command[1] == "view")
                {
                    string fileName = command[2];
                    string fileContent = command[3];

                    activeSession.SetState(new ViewState(activeSession, fileName, fileContent));
                    display.state = DisplayState.VIEW;
                }
                else if (command[1] == "http")
                {

                }
            }
            else if (command[0] == "node")
            {
                // Display stuff and daemon button stuff which the console can't use
            }
            else if (command[0] == "changetheme")
            {
                // It isn't possible to have themes in console but if you find another way to implement themes in console then cool
                Console.WriteLine("Sorry, but themes aren't supported in TerminalLinks");
            }
            else if (command[0] == "nmap")
            {
                // TODO: Add nmap support
                Console.WriteLine("Sorry, nmap isn't supported yet");
            }
        }

        private static string Hash(string input)
        {
            var hash = (new SHA256Managed()).ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }
    }
}
