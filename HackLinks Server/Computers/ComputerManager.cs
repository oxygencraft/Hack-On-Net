using HackLinks_Server.FileSystem;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers
{
    class ComputerManager
    {
        List<Node> nodeList = new List<Node>();
        Server server;

        public ComputerManager(Server server)
        {
            this.server = server;
        }

        public Node GetNodeByIp(string ip)
        {
            foreach(Node node in nodeList)
            {
                if (node.ip == ip)
                    return node;
            }
            return null;
        }

        public void DownloadDatabase()
        {
            var conn = server.GetConnection();

            MySqlCommand sqlCommand = new MySqlCommand("SELECT * FROM computers", conn);
            using (MySqlConnection cn1 = new MySqlConnection(server.GetConnectionString()))
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

                            MySqlCommand fileCommand = new MySqlCommand("SELECT * FROM files WHERE computerId = @0", cn1);
                            fileCommand.Parameters.Add(new MySqlParameter("0", newNode.id));
                            Folder fileSystem = new Folder(null, "/");
                            List<File> computerFiles = new List<File>();
                            using (MySqlDataReader fileReader = fileCommand.ExecuteReader())
                            {
                                if (fileReader.HasRows)
                                {
                                    while (fileReader.Read())
                                    {
                                        File newFile = null;
                                        int fileType = fileReader.GetByte(3);
                                        string fileName = fileReader.GetString(1);

                                        if (fileType == 1)
                                        {
                                            newFile = new Folder(null, fileReader.GetString(1));
                                        }
                                        else
                                        {
                                            newFile = new File(null, fileReader.GetString(1));
                                        }
                                        newFile.id = fileReader.GetInt32(0);
                                        newFile.parentId = fileReader.GetInt32(2);
                                        newFile.readPriv = fileReader.GetInt32(8);
                                        newFile.writePriv = fileReader.GetInt32(7);
                                        newFile.content = fileReader.GetString(5);

                                        newFile.SetType(fileReader.GetInt32(4));
                                        computerFiles.Add(newFile);
                                    }
                                }
                            }
                            fileSystem.children = FixFolder(computerFiles, 0, fileSystem);
                            newNode.rootFolder = fileSystem;
                            fileSystem.readPriv = 1;
                            fileSystem.writePriv = 1;

                            nodeList.Add(newNode);
                        }
                    }

                }
            }

            Console.WriteLine("Initializing daemons");
            foreach(Node node in nodeList)
            {
                var daemonsFolder = (Folder)node.rootFolder.GetFile("daemons");
                if (daemonsFolder == null)
                    continue;
                var autorunFile = daemonsFolder.GetFile("autorun");
                if (autorunFile == null)
                    continue;
                foreach(string line in autorunFile.content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var daemonFile = daemonsFolder.GetFile(line);
                    if (daemonFile == null)
                        continue;
                    if (daemonFile.type != File.FileType.DAEMON)
                        continue;
                    node.LaunchDaemon(daemonFile);
                }
            }
        }

        public Node GetNodeById(int homeId)
        {
            foreach (Node node in nodeList)
                if (node.id == homeId)
                    return node;
            return null;
        }

        public static List<File> FixFolder(List<File> files, int parentId, Folder father=null)
        {
            List<File> fixedFiles = new List<File>();

            foreach (var item in files.Where(x => x.parentId.Equals(parentId)))
            {
                item.parent = father;
                fixedFiles.Add(item);
                if(item.IsFolder())
                {
                    item.children = FixFolder(files, item.id, (Folder)item);
                }
            }

            return fixedFiles;
        }
    }
}
