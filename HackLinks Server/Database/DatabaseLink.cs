using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Files;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Database
{
    class DatabaseLink
    {
        private MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder();

        public DatabaseLink(ConfigUtil.ConfigData config)
        {
            SetConnectionStringParameters(config);
        }

        public void SetConnectionStringParameters(ConfigUtil.ConfigData config)
        {
            connectionStringBuilder.Server = config.MySQLServer;
            connectionStringBuilder.Database = config.Database;
            connectionStringBuilder.UserID = config.UserID;
            connectionStringBuilder.Password = config.Password;
        }

        public string GetConnectionString()
        {         
            return connectionStringBuilder.GetConnectionString(true);
        }

        public List<Node> DownloadDatabase()
        {
            List<Node> nodeList = new List<Node>();

            using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();

                MySqlCommand sqlCommand = new MySqlCommand("SELECT * FROM computers", conn);
                using (MySqlConnection cn1 = new MySqlConnection(GetConnectionString()))
                {
                    cn1.Open();
                    using (MySqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Node newNode = null;
                                int type = reader.GetInt32(3);
                                if (type == 4)
                                {
                                    newNode = new PlayerTerminal();
                                    ((PlayerTerminal)newNode).ownerId = reader.GetInt32(2);
                                }
                                else
                                {
                                    newNode = new Node();
                                }
                                newNode.id = reader.GetInt32(0);
                                newNode.ip = reader.GetString(1);
                                newNode.ownerId = reader.GetInt32(2);

                                MySqlCommand fileCommand = new MySqlCommand("SELECT * FROM files WHERE computerId = @0", cn1);
                                fileCommand.Parameters.Add(new MySqlParameter("0", newNode.id));
                                List<File> computerFiles = new List<File>();

                                using (MySqlDataReader fileReader = fileCommand.ExecuteReader())
                                {
                                    if (fileReader.HasRows)
                                    {
                                        while (fileReader.Read())
                                        {
                                            int fileType = fileReader.GetByte(3);
                                            string fileName = fileReader.GetString(1);

                                            Console.WriteLine($"Creating file {fileName} with id {fileReader.GetInt32(0)}");

                                            File newFile = newNode.fileSystem.CreateFile(fileReader.GetInt32(0), newNode, newNode.fileSystem.rootFile, fileName);

                                            newFile.isFolder = fileType == 1;

                                            newFile.ParentId = fileReader.GetInt32(2);
                                            newFile.OwnerUsername = fileReader.GetString(9);
                                            newFile.Group = (Group)fileReader.GetInt32(7);
                                            newFile.Permissions.PermissionValue = fileReader.GetInt32(8);
                                            newFile.Content = fileReader.GetString(5);
                                            newFile.SetType(fileReader.GetInt32(4));

                                            computerFiles.Add(newFile);

                                            if (newFile.ParentId == 0)
                                            {
                                                newNode.SetRoot(newFile);
                                            }
                                        }
                                    }
                                }

                                ComputerManager.FixFolder(computerFiles, newNode.fileSystem.rootFile);
                                nodeList.Add(newNode);
                            }
                        }
                    }
                }
            }

            return nodeList;
        }

        public bool TryLogin(GameClient client, string tempUsername, string tempPass, out int homeId)
        {
            bool correctUser = false;
            homeId = -1;

            using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand("SELECT pass, homeComputer FROM accounts WHERE username = @0", conn);
                command.Parameters.Add(new MySqlParameter("0", tempUsername));

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetString("pass") == tempPass)
                        {
                            correctUser = true;
                            homeId = reader.GetInt32("homeComputer");
                            break;
                        }
                    }
                }
            }

            return correctUser;
        }

        public Dictionary<int, string> GetUsersInDatabase()
        {
            Dictionary<int, string> users = new Dictionary<int, string>();

            using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand("SELECT id, username FROM accounts", conn);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(reader.GetInt32("id"), reader.GetString("username"));
                    }
                }
            }

            return users;
        }

        public bool SetUserBanStatus(string user, int banExpiry, bool unban, bool permBan)
        {
            List<string> users = GetUsersInDatabase().Values.ToList();
            int userIndex = users.Count + 1; // The list is in inverted order for some reason idk of which is why we're subtracting from element count

            if (users.Contains(user) == false)
                return false;
            foreach (var user2 in users)
            {
                userIndex--;
                if (user2 == user)
                    break;
            }
            GameClient client = null;
            foreach (var client2 in Server.Instance.clients)
            {
                if (client2.username == user)
                {
                    client = client2;
                    break;
                }
            }
            try
            {
                client.Send(HackLinksCommon.NetUtil.PacketType.DSCON, "You have been banned from the server");
                client.netDisconnect();
            }
            catch (Exception) { }

            using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand($"UPDATE accounts SET banned = {banExpiry} WHERE id = {userIndex}", conn);
                if (unban)
                {
                    command.CommandText = $"UPDATE accounts SET banned = NULL, permBan = 0 WHERE id = {userIndex}";
                    command.ExecuteNonQuery();
                    return true;
                }
                if (permBan)
                {
                    command.CommandText = $"UPDATE accounts SET permBan = 1 WHERE id = {userIndex}";
                    command.ExecuteNonQuery();
                    return true;
                }
                command.ExecuteNonQuery();
                return true;
            }
        }

        public bool CheckUserBanStatus(string user, out int banExpiry)
        {
            Dictionary<string, int> bans = new Dictionary<string, int>();
            Dictionary<string, bool> permBans = new Dictionary<string, bool>();

            using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand("SELECT username, banned, permBanned FROM accounts", conn);
                using (MySqlDataReader reader =  command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(1))
                        {
                            if (reader.GetBoolean("permBanned"))
                                permBans.Add(reader.GetString("username"), true);
                            continue;
                        }
                        bans.Add(reader.GetString("username"), reader.GetInt32("banned"));
                        permBans.Add(reader.GetString("username"), reader.GetBoolean("permBanned"));
                    }
                }
            }

            try
            {
                if (permBans[user])
                {
                    banExpiry = 0;
                    return true;
                }
            }
            catch (Exception) { }

            try
            {
                if (bans[user] > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    banExpiry = bans[user];
                    return true;
                }
                if (bans[user] <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    SetUserBanStatus(user, 0, true, false);
            }
            catch (Exception) { }

            banExpiry = 0;
            return false;
        }

        public string GetUserNodes(string user)
        {
            List<string> nodes = new List<string>();
            string nodesString = "";

            using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand("SELECT netmap FROM accounts", conn);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nodes.Add(reader.GetString("netmap"));
                    }
                }
            }

            foreach (var node in nodes)
            {
                if (nodesString == "")
                    nodesString = node;
                nodesString = nodesString + "," + node;
            }

            return nodesString;
        }

        public void AddUserNode(string user, string ip, string pos)
        {
            string nodes = GetUserNodes(user);

            if (nodes == "")
            {
                nodes = ip + ":" + pos;
            }
            else
            {
                nodes = nodes + "," + ip + ":" + pos;
            }

            using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand($"UPDATE accounts SET netmap = '{nodes}' WHERE '{user}' = `username`", conn);
                command.ExecuteNonQuery();
            }
        }

        public Dictionary<string, List<Permissions>> GetUserPermissions()
        {
            Dictionary<string, List<Permissions>> permissionsDictionary = new Dictionary<string, List<Permissions>>();

            using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand("SELECT username, permissions FROM accounts", conn);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        List<Permissions> permissions = new List<Permissions>();
                        string[] permissionsString = reader.GetString("permissions").Split(',');
                        if (permissionsString.Contains("admin"))
                        {
                            permissions.Add(Permissions.Admin);
                        }
                        if (permissionsString.Contains("kick"))
                        {
                            permissions.Add(Permissions.Kick);
                        }
                        if (permissionsString.Contains("ban"))
                        {
                            permissions.Add(Permissions.Ban);
                        }
                        if (permissionsString.Contains("giveperms"))
                        {
                            permissions.Add(Permissions.Ban);
                        }
                        permissionsDictionary.Add(reader.GetString("username"), permissions);
                    }
                }
            }

            return permissionsDictionary;
        }

        public void SetUserPermissions(string user, List<Permissions> permissions)
        {
            string permissionsString = "";
            bool firstItem = true;
            if (permissions.Contains(Permissions.Admin))
            {
                if (firstItem)
                {
                    permissionsString = "admin";
                    firstItem = false;
                }
                else
                    permissionsString = permissionsString + ",admin";
            }
            if (permissions.Contains(Permissions.Kick))
            {
                if (firstItem)
                {
                    permissionsString = "kick";
                    firstItem = false;
                }
                else
                    permissionsString = permissionsString + ",kick";
            }
            if (permissions.Contains(Permissions.Ban))
            {
                if (firstItem)
                {
                    permissionsString = "ban";
                    firstItem = false;
                }
                else
                    permissionsString = permissionsString + ",ban";
            }
            if (permissions.Contains(Permissions.GivePerms))
            {
                if (firstItem)
                {
                    permissionsString = "giveperms";
                    firstItem = false;
                }
                else
                    permissionsString = permissionsString + ",giveperms";
            }

            using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand($"UPDATE accounts SET permissions = '{permissionsString}' WHERE '{user}' = `username`", conn);
                command.ExecuteNonQuery();
            }
        }

        public static IEnumerable<T> Traverse<T>(IEnumerable<T> items,
        Func<T, IEnumerable<T>> childSelector)
        {
            var stack = new Stack<T>(items);
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                foreach (var child in childSelector(next))
                    stack.Push(child);
            }
        }

        public void UploadDatabase(List<Node> nodeList, List<File> toDelete)
        {
            Console.WriteLine("Uploading Database");

            using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();

                foreach (Node node in nodeList)
                {
                    foreach (File child in Traverse(node.fileSystem.rootFile.children, file => file.children))
                    {
                        if (!child.Dirty) // Our child is clean. Continue to the next
                        {
                            continue;
                        }

                        if (UpdateDbFile(child, conn))
                        {
                            Console.WriteLine($"    Updated {child.Name}");
                        }

                        child.Dirty = false;
                    }
                }

                //We iterate our list backwards to avoid our indices being clobbered by removals.
                for (int i = toDelete.Count - 1; i >= 0; i--)
                {
                    File file = toDelete[i];
                    if (DeleteDbFile(file, conn))
                    {
                        Console.WriteLine($"    Deleted {file.Name}");
                        toDelete.Remove(file);
                    }
                    else
                    {
                        Console.WriteLine($"    Can't Delete {file.Name} ID {file.id}");
                    }
                } 
            }
        }

        public void RebuildDatabase()
        {
            Console.WriteLine("Rebuilding Database");

            using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();

                foreach (string commandString in DatabaseDump.Commands)
                {
                    MySqlCommand command = new MySqlCommand(commandString, conn);
                    int res = command.ExecuteNonQuery();
                }
            }
        }

        private bool UpdateDbFile(File child, MySqlConnection conn)
        {
            MySqlCommand fileCommand = new MySqlCommand(
                "INSERT INTO files" +
                " (id, name, parentFile, type, specialType, content, computerId, owner, groupId, permissions)" +
                " VALUES" +
                " (@id, @name, @parentFile, @type, @specialType, @content, @computerId, @owner, @groupId, @permissions)" +
                " ON DUPLICATE KEY UPDATE" +
                " name = @name," +
                " parentFile = @parentFile," +
                " specialType = @specialType," +
                " content = @content," +
                " groupId = @groupId," +
                " permissions = @permissions," +
                " owner = @owner"
                , conn);
            fileCommand.Parameters.AddRange(new MySqlParameter[] {
                        new MySqlParameter("id", child.id),
                        new MySqlParameter("name", child.Name),
                        new MySqlParameter("parentFile", child.ParentId),
                        new MySqlParameter("type", child.isFolder ? 1 : 0),
                        new MySqlParameter("specialType", child.Type),
                        new MySqlParameter("content", child.Content),
                        new MySqlParameter("computerId", child.computerId),
                        new MySqlParameter("groupId", child.Group),
                        new MySqlParameter("permissions", child.Permissions.PermissionValue),
                        new MySqlParameter("owner", child.OwnerUsername),
                    });

            int res = fileCommand.ExecuteNonQuery();

            int insertedId = (int)fileCommand.LastInsertedId;

            return res > 0;
        }

        private bool DeleteDbFile(File file, MySqlConnection conn)
        {
            MySqlCommand fileCommand = new MySqlCommand(
            "DELETE FROM files" +
            " WHERE" +
            " id = @id"
            , conn);

            fileCommand.Parameters.AddRange(new MySqlParameter[] {
                        new MySqlParameter("id", file.id),
                    });

            return fileCommand.ExecuteNonQuery() > 0;
        }
    }
}
