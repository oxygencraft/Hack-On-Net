using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackLinks_Server.Computers;
using MySql.Data.MySqlClient;

namespace HackLinks_Server.FileSystem
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

        public int id;

        private string name;
        private int writePriv = 0;
        private int readPriv = 0;
        private string content = "";

        private Folder parent;
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

        internal Folder Parent { get => parent;
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

        public File(Node computer, Folder parent, string name)
        {
            this.computerId = computer.id;
            this.Name = name;
            this.Parent = parent;
            if(parent != null)
            {
                this.Parent.children.Add(this);
            }
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
            return false;
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

    }
}
