using HackLinks_Server.Daemons;
using HackLinks_Server.Daemons.Types;
using HackLinks_Server.FileSystem;
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
        public int id;
        public string ip;

        public Folder rootFolder = new Folder(null, "/");

        public List<Session> sessions = new List<Session>();
        public List<Daemon> daemons = new List<Daemon>();

        public void LaunchDaemon(File daemonLauncher)
        {
            var lines = daemonLauncher.content.Split(new string[]{ "\r\n" }, StringSplitOptions.None);
            if(lines[0] == "IRC")
            {
                var newDaemon = new IrcDaemon(this);
                daemons.Add(newDaemon);
            }
        }

        public void Login(GameClient client, string username, string password)
        {
            var configFolder = (Folder)rootFolder.GetFile("cfg");
            if (configFolder == null)
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
            var accounts = usersFile.content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var account in accounts)
            {
                var accountData = account.Split(new char[] { ',', '=' });
                if (accountData[1] == username && accountData[2] == password)
                {
                    client.activeSession.Login(accountData[0], username);
                    client.Send(NetUtil.PacketType.MESSG, "Logged as : " + username);
                    return;
                }
            }
            client.Send(NetUtil.PacketType.MESSG, "Wrong identificants.");
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
