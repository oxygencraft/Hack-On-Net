using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Files;
using MySql.Data.MySqlClient;

namespace HackLinks_Server.Files
{
    class File
    {

        public enum FileType
        {
            NORMAL,
            DAEMON,
            LOG,
            EXE,
            CONFIG
        }

        public readonly int id;

        private string name;
        private int writePriv = 0;
        private int readPriv = 0;
        private string content = "";

        private File parent;
        private int parentId;
        public int computerId;

        private FileType type = FileType.NORMAL;

        public bool Dirty { get; set; }
        public string Name { get => name; set { name = value; Dirty = true; } }
        public int WritePriv { get => writePriv; set { writePriv = value; Dirty = true; } }
        public int ReadPriv { get => readPriv; set { readPriv = value; Dirty = true; } }
        public string Content { get => content; set { content = value; Dirty = true;  } }

        public int ParentId { get => parentId; set { parentId = value; Dirty = true; } }
        public int ComputerId { get => computerId; set { computerId = value; Dirty = true; } }

        public FileType Type { get => type; set { type = value; Dirty = true; } }

        internal File Parent { get => parent;
            set
            {
                if (parent != null)
                {
                    parent.children.RemoveAll(child => child.id == id);
                }

                parent = value;
                if(parent != null)
                {
                    ParentId = parent.id;
                }
            }
        }

        public List<File> children = new List<File>();
        internal bool isFolder;

        protected File(int id, Node computer, File parent, string name)
        {
            this.id = id;
            this.computerId = computer.id;
            this.Name = name;
            this.Parent = parent;
            if(parent != null)
            {
                this.Parent.children.Add(this);
            }
        }

        /// <summary>
        /// Create a new file and register it a new file id with the given <see cref="FileSystemManager"/>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="computer"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static File CreateNewFile(FileSystemManager manager, Node computer, File parent, string name)
        {
            File newFile = new File(manager.GetNewFileId(), computer, parent, name);
            manager.RegisterNewFile(newFile);
            return newFile;
        }

        /// <summary>
        /// Attempt to create a new file with the given id and register it with the given <see cref="FileSystemManager"/>
        /// It's usually better to use <see cref="CreateNewFile(FileSystemManager, Node, File, string)"/> unless you need to explicitly specify the file id.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="computer"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Thrown when the id is already registered</exception>
        public static File CreateNewFile(int id, FileSystemManager manager, Node computer, File parent, string name)
        {
            if (manager.IsIdInUse(id))
            {
                throw new ArgumentException($"File id \"{id}\" is already in use");
            }

            File newFile = new File(id, computer, parent, name);
            manager.RegisterNewFile(newFile);
            return newFile;
        }

        public bool HasWritePermission(Session session)
        {
            return HasWritePermission(session.privilege);
        }

        public bool HasWritePermission(int priv)
        {
            return priv <= WritePriv;
        }

        public bool HasReadPermission(int priv)
        {
            return priv <= ReadPriv;
        }

        virtual public bool IsFolder()
        {
            return isFolder;
        }

        virtual public void RemoveFile()
        {
            Parent.children.Remove(this);
            ParentId = 0;
        }

        public void SetType(int specType)
        {
            Type = (FileType)specType;
        }

        public string[] GetLines()
        {
            return this.Content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
        }

        public File GetFile(string name)
        {
            foreach (File file in children)
            {
                if (file.Name == name)
                    return file;
            }
            return null;
        }

        public File GetFileAtPath(string path)
        {
            string[] pathSteps = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            File activeFolder = this;
            for (int i = 0; i < pathSteps.Length - 1; i++)
            {
                var folder = activeFolder.GetFile(pathSteps[i]);
                if (folder == null || !folder.IsFolder())
                    return null;
                activeFolder = folder;
            }
            return activeFolder.GetFile(pathSteps[pathSteps.Length - 1]);
        }

        public void PrintFolderRecursive(int depth)
        {
            string tabs = new String(' ', depth);
            Console.WriteLine(tabs + id + "  d- " + Name);
            foreach (var item in children)
            {
                if (item.IsFolder())
                {
                    item.PrintFolderRecursive(depth + 1);
                }
                else
                {
                    Console.WriteLine(tabs + " " + item.id + "  f- " + item.Name);
                }
            }
        }

    }
}
