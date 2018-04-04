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
                " (id, name, parentFile, type, specialType, content, computerId, groupId, permissions)" +
                " VALUES" +
                " (@id, @name, @parentFile, @type, @specialType, @content, @computerId, @groupId, @permissions)" +
                " ON DUPLICATE KEY UPDATE" +
                " name = @name," +
                " parentFile = @parentFile," +
                " specialType = @specialType," +
                " content = @content," +
                " groupId = @groupId," +
                " permissions = @permissions"
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
