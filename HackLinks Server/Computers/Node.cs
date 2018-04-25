using HackLinks_Server.Computers.Files;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Computers.Processes;
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
    public class Node
    {
        public static string SERVER_CONFIG_PATH = "/cfg/server.cfg";

        public int id;
        public string ip;

        public int ownerId;

        public readonly FileSystem fileSystem = new FileSystem(Server.Instance.FileSystemManager);

        public List<Session> sessions = new List<Session>();
        public List<Daemon> daemons = new List<Daemon>();
        public List<Log> logs = new List<Log>();

        public List<Process> processes = new List<Process>();
        public Stack<int> freedPIDs = new Stack<int>();

        private int nextPID = 1;
        public int NextPID => freedPIDs.Count > 0 ? freedPIDs.Pop() : nextPID++;

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
                //TODO credentials
                var newDaemon = new IrcDaemon(NextPID, 0, null, this, null);
                daemons.Add(newDaemon);
            }
            else if(lines[0] == "DNS")
            {
                //TODO credentials
                var newDaemon = new DNSDaemon(NextPID, 0, null, this, null);
                daemons.Add(newDaemon);
            }
            else if(lines[0] == "HTTP")
            {
                //TODO credentials
                var newDaemon = new HTTPDaemon(NextPID, 0, null, this, null);
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
            return GetUserId(username) != -1;
        }

        public string GetUserShell(int userId)
        {
            File configFolder = fileSystem.rootFile.GetFile("etc");
            if (configFolder == null || !configFolder.IsFolder())
            {
                return "";
            }
            File usersFile = configFolder.GetFile("passwd");
            if (usersFile == null)
            {
                return "";
            }
            File groupFile = configFolder.GetFile("group");
            if (usersFile == null)
            {
                return "";
            }
            string[] accounts = usersFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] groups = groupFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string account in accounts)
            {
                string[] accountData = account.Split(':');
                string accountUserIdString = accountData[2];

                if (userId.ToString() == accountUserIdString)
                {
                    string accountUserShell = accountData[6];
                    return accountUserShell;
                }
            }
            return "";
        }

        public string GetUsername(int userId)
        {
            File configFolder = fileSystem.rootFile.GetFile("etc");
            if (configFolder == null || !configFolder.IsFolder())
            {
                return "";
            }
            File usersFile = configFolder.GetFile("passwd");
            if (usersFile == null)
            {
                return "";
            }
            File groupFile = configFolder.GetFile("group");
            if (usersFile == null)
            {
                return "";
            }
            string[] accounts = usersFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] groups = groupFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string account in accounts)
            {
                string[] accountData = account.Split(':');
                string accountUsername = accountData[0];
                string accountUserIdString = accountData[2];

                if (userId.ToString() == accountUserIdString)
                {
                    return accountUsername;
                }
            }
            return "";
        }

        public int GetUserId(string username)
        {
            File configFolder = fileSystem.rootFile.GetFile("etc");
            if (configFolder == null || !configFolder.IsFolder())
            {
                return -1;
            }
            File usersFile = configFolder.GetFile("passwd");
            if (usersFile == null)
            {
                return -1;
            }
            File groupFile = configFolder.GetFile("group");
            if (usersFile == null)
            {
                return -1;
            }
            string[] accounts = usersFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] groups = groupFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string account in accounts)
            {
                string[] accountData = account.Split(':');
                string accountUsername = accountData[0];
                string accountUserIdString = accountData[2];

                if (accountUsername == username)
                {
                    return int.TryParse(accountUserIdString, out int result) ? result : -1;
                }
            }
            return -1;
        }

        public void Login(GameClient client, string username, string password)
        {
            var configFolder = fileSystem.rootFile.GetFile("etc");
            if (configFolder == null || !configFolder.IsFolder())
            {
                client.Send(NetUtil.PacketType.MESSG, "No config folder was found!");
                return;
            }
            File usersFile = configFolder.GetFile("passwd");
            if (usersFile == null)
            {
                client.Send(NetUtil.PacketType.MESSG, "No passwd file was found!");
                return;
            }
            File groupFile = configFolder.GetFile("group");
            if (usersFile == null)
            {
                client.Send(NetUtil.PacketType.MESSG, "No group file was found!");
                return;
            }
            string[] accounts = usersFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] groups = groupFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string account in accounts)
            {
                string[] accountData = account.Split(':');
                string accountUsername = accountData[0];
                string accountPassword = accountData[1];
                string accountGroupId = accountData[3];

                if (accountUsername == username && accountPassword == password)
                {
                    List<Group> loginGroups = new List<Group>();
                    foreach(string group in groups)
                    {
                        string[] groupData = group.Split(':');
                        string groupName = groupData[0];
                        string groupId = groupData[2];
                        string[] groupUsers = groupData[3].Split(',');
                        if (groupUsers.Contains(username) || accountGroupId.Equals(groupId))
                        {
                            Group loginGroup = PermissionHelper.GetGroupFromString(groupId);
                            if (loginGroup != Group.INVALID)
                            {
                                loginGroups.Add(loginGroup);
                            }
                            else
                            {
                                client.Send(NetUtil.PacketType.MESSG, $"Can't login as {username} {groupName} is not a valid group");
                            }
                        }
                    }

                    client.activeSession.Login(loginGroups, username);
                    client.Send(NetUtil.PacketType.MESSG, "Logged as : " + username);
                    Log(Computers.Log.LogEvents.Login, logs.Count + 1 + " " + client.homeComputer.ip + " logged in as " + username, client.activeSession.sessionId, client.homeComputer.ip);

                    return;
                }
            }
            client.Send(NetUtil.PacketType.MESSG, "Wrong identificants.");
        }

        public void Log(Log.LogEvents logEvent, string message, int sessionId, string ip)
        {
            File logsFolder = null;
            foreach (var file in fileSystem.rootFile.children)
            {
                if (file.Name == "logs")
                {
                    logsFolder = file;
                    break;
                }
            }
            if (logsFolder == null)
            {
                logsFolder = File.CreateNewFolder(fileSystem.fileSystemManager, this, fileSystem.rootFile, "logs");
                logsFolder.OwnerId = 0;
                logsFolder.Permissions.SetPermission(FilePermissions.PermissionType.User, true, true, true);
                logsFolder.Permissions.SetPermission(FilePermissions.PermissionType.Group, true, true, true);
                logsFolder.Group = logsFolder.Parent.Group;
                logsFolder.Type = File.FileType.LOG;
            }
            message = message.Replace(' ', '_');
            File logFile = File.CreateNewFile(fileSystem.fileSystemManager, this, logsFolder, message);
            logFile.OwnerId = 0;
            logFile.Permissions.SetPermission(FilePermissions.PermissionType.User, true, true, true);
            logFile.Permissions.SetPermission(FilePermissions.PermissionType.Group, true, true, true);
            logFile.Group = logsFolder.Parent.Group;
            logFile.Type = File.FileType.LOG;
            logs.Add(new Log(logFile, sessionId, ip, logEvent, message));
        }

        public void Log(Log.LogEvents logEvent, string message, string messageExtended, int sessionId, string ip)
        {
            File logsFolder = null;
            foreach (var file in fileSystem.rootFile.children)
            {
                if (file.Name == "logs")
                {
                    logsFolder = file;
                    break;
                }
            }
            if (logsFolder == null)
            {
                logsFolder = File.CreateNewFolder(fileSystem.fileSystemManager, this, fileSystem.rootFile, "logs");
                logsFolder.OwnerId = 0;
                logsFolder.Permissions.SetPermission(FilePermissions.PermissionType.User, true, true, true);
                logsFolder.Permissions.SetPermission(FilePermissions.PermissionType.Group, true, true, true);
                logsFolder.Group = logsFolder.Parent.Group;
                logsFolder.Type = File.FileType.LOG;
            }
            File logFile = File.CreateNewFile(fileSystem.fileSystemManager, this, logsFolder, message);
            logFile.OwnerId = 0;
            logFile.Permissions.SetPermission(FilePermissions.PermissionType.User, true, true, true);
            logFile.Permissions.SetPermission(FilePermissions.PermissionType.Group, true, true, true);
            logFile.Group = logsFolder.Parent.Group;
            logFile.Type = File.FileType.LOG;
            logs.Add(new Log(logFile, sessionId, ip, logEvent, message, messageExtended));
        }

        internal void ParseLogs()
        {
            List<Log> logs = new List<Log>();
            File logsFolder = null;
            foreach (var file in fileSystem.rootFile.children)
            {
                if (file.Name == "logs")
                {
                    logsFolder = file;
                    break;
                }
            }
            if (logsFolder == null)
                return;

            foreach (var log in logsFolder.children)
            {
                string machineReadChars = "";
                int machineReadCharType = 0;
                int machineReadCharsFound = 0;
                int machineReadSplit = 0;
                foreach (var character in log.Content)
                {
                    if (character == '#' && machineReadCharType == 0 && machineReadCharsFound < 4)
                    {
                        machineReadChars = machineReadChars + "#";
                        machineReadCharsFound++;
                        if (machineReadCharsFound >= 4)
                        {
                            machineReadCharType++;
                            machineReadCharsFound = 0;
                        }
                    }
                    else if (character == '!' && machineReadCharType == 1 && machineReadCharsFound < 2)
                    {
                        machineReadChars = machineReadChars + "!";
                        machineReadCharsFound++;
                        if (machineReadCharsFound >= 2)
                        {
                            machineReadCharType++;
                            machineReadCharsFound = 0;
                        }
                    }
                    else if (character == '*' && machineReadCharType == 2 && machineReadCharsFound < 1)
                    {
                        machineReadChars = machineReadChars + "*";
                        machineReadCharsFound++;
                    }
                    else if (machineReadChars == "####!!*")
                        break;
                    else
                    {
                        machineReadChars = "";
                        machineReadCharType = 0;
                        machineReadCharsFound = 0;
                    }
                    machineReadSplit++;
                }

                machineReadSplit += 23;
                Log logAdd = Computers.Log.Deserialize(log.Content.Substring(machineReadSplit));
                logAdd.file = log;
                logs.Add(logAdd);
            }

            this.logs = logs;
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
