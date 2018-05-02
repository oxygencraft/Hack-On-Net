using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Files;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers
{
    public class ComputerManager
    {
        Server server;

        private List<Node> nodeList = new List<Node>();
        private List<File> toDelete = new List<File>();

        public List<Node> NodeList => nodeList;
        public List<File> ToDelete => toDelete;

        public ComputerManager(Server server, List<Node> nodeList)
        {
            this.server = server;
            this.nodeList = nodeList;
        }

        public void Init()
        {
            Console.WriteLine("Initializing daemons");
            foreach (Node node in nodeList)
            {
                var daemonsFolder = node.fileSystem.rootFile.GetFile("daemons");
                if (daemonsFolder == null)
                    continue;
                var autorunFile = daemonsFolder.GetFile("autorun");
                if (autorunFile == null)
                    continue;
                foreach (string line in autorunFile.Content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var daemonFile = daemonsFolder.GetFile(line);
                    if (daemonFile == null)
                        continue;
                    if (daemonFile.OwnerId != 0 || daemonFile.Group != Group.ROOT)
                        continue;
                    if (!daemonFile.HasExecutePermission(0, Group.ROOT))
                        continue;
                    node.LaunchDaemon(daemonFile);
                }
            }
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

        public Node GetNodeById(int homeId)
        {
            foreach (Node node in nodeList)
                if (node.id == homeId)
                    return node;
            return null;
        }

        public static void FixFolder(List<File> files, File rootFile)
        {
            List<File> fixedFiles = new List<File>();
            Queue<File> fileQueue = new Queue<File>();

            fileQueue.Enqueue(rootFile);

            while(fileQueue.Any())
            {
                File parent = fileQueue.Dequeue();
                Console.WriteLine($"Processing File {parent.Name} {parent.id}");

                foreach (File child in files.Where(x => x.ParentId.Equals(parent.id)))
                {
                    Console.WriteLine($"Processing Child File {child.Name} {child.id} of {parent.Name} {parent.id}");

                    child.Parent = parent;
                    parent.children.Add(child);

                    fixedFiles.Add(child);
                    if(child.IsFolder())
                    {
                        fileQueue.Enqueue(child);
                    }
                }
            }
        }

        public void AddToDelete(File file)
        {
            toDelete.Add(file);
        }
    }
}
