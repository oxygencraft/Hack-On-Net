using HackLinks_Server.Computers.Files;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Daemons;
using HackLinks_Server.Daemons.Types;
using HackLinks_Server.Files;
using HackLinksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers
{
    class Node
    {
        public static string SERVER_CONFIG_PATH = "/cfg/server.cfg";

        public int id;
        public string ip;

        public int ownerId;

        public readonly FileSystem fileSystem = new FileSystem(Server.Instance.FileSystemManager);

        public List<Session> sessions = new List<Session>();
        public List<Daemon> daemons = new List<Daemon>();

        public string GetDisplayName()
        {
            var cfgFile = fileSystem.rootFile.GetFileAtPath(SERVER_CONFIG_PATH);
            if (cfgFile == null)
                return ip;
            var lines = cfgFile.GetLines();
            foreach(var line in lines)
            {
                if (line.StartsWith("name="))
                    return line.Substring(5);
            }
            return ip;
        }

        public void LaunchDaemon(File daemonLauncher)
        {
            var lines = daemonLauncher.Content.Split(new string[]{ "\r\n" }, StringSplitOptions.None);
            if(lines[0] == "IRC")
            {
                var newDaemon = new IrcDaemon(this);
                daemons.Add(newDaemon);
            }
            else if(lines[0] == "DNS")
            {
                var newDaemon = new DNSDaemon(this);
                daemons.Add(newDaemon);
            }
            else if(lines[0] == "HTTP")
            {
                var newDaemon = new HTTPDaemon(this);
                daemons.Add(newDaemon);
            }
        }

        public Daemon GetDaemon(string type)
        {
            foreach(Daemon daemon in daemons)
                if (daemon.IsOfType(type))
                    return daemon;
            return null;
        }

        public bool HasUser(string username)
        {
            var configFolder = fileSystem.rootFile.GetFile("cfg");
            if (configFolder == null || !configFolder.IsFolder())
            {
                return false;
            }
            var usersFile = configFolder.GetFile("users.cfg");
            if (usersFile == null)
            {
                return false;
            }

            var accounts = usersFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var account in accounts)
            {
                var accountData = account.Split(new char[] { ',', '=' });
                if (accountData[1] == username)
                {
                    return true;
                }
            }
            return false;
        }


        public void Login(GameClient client, string username, string password)
        {
            var configFolder = fileSystem.rootFile.GetFile("cfg");
            if (configFolder == null || !configFolder.IsFolder())
            {
                client.Send(NetUtil.PacketType.MESSG, "No config folder was found !");
                return;
            }
            var usersFile = configFolder.GetFile("users.cfg");
            if (usersFile == null)
            {
                client.Send(NetUtil.PacketType.MESSG, "No config file was found !");
                return;
            }
            var accounts = usersFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var account in accounts)
            {
                var accountData = account.Split(new char[] { ',', '=' });
                if (accountData[1] == username && accountData[2] == password)
                {
                    Group loginGroup = PermissionHelper.GetGroupFromString(accountData[0]);
                    if(loginGroup != Group.INVALID)
                    {
                        client.activeSession.Login(loginGroup, username);
                        client.Send(NetUtil.PacketType.MESSG, "Logged as : " + username);
                    }
                    else
                    {
                        client.Send(NetUtil.PacketType.MESSG, $"Can't login as {username} {accountData[0]} is not a valid group");
                    }

                    return;
                }
            }
            client.Send(NetUtil.PacketType.MESSG, "Wrong identificants.");
        }

        internal void SetRoot(File newFile)
        {
            if(fileSystem.rootFile != null)
                throw new ArgumentException("Root file for this computer is already set.");
            fileSystem.rootFile = newFile;
        }

        /*public Folder getFolderFromPath(string path, bool createFoldersThatDontExist = false)
        {
            Folder result;
            if (string.IsNullOrWhiteSpace(path))
            {
                result = rootFolder;
            }
            else
            {
                System.Collections.Generic.List<int> folderPath = this.getFolderPath(path, createFoldersThatDontExist);
                result = Computer.getFolderAtDepth(this, folderPath.Count, folderPath);
            }
            return result;
        }

        public System.Collections.Generic.List<int> getFolderPath(string path)
        {
            System.Collections.Generic.List<int> list = new System.Collections.Generic.List<int>();
            char[] separator = new char[]
            {
                '/',
                '\\'
            };
            string[] array = path.Split(separator);
            Folder folder = rootFolder;
            for (int i = 0; i < array.Length; i++)
            {
                bool flag = false;
                for (int j = 0; j < folder.children.Count; j++)
                {
                    if (folder.children[j].IsFolder() && folder.children[j].name.Equals(array[i]))
                    {
                        list.Add(j);
                        folder = (Folder)folder.children[j];
                        flag = true;
                        break;
                    }
                }
            }
            return list;
        }*/
    }
}
