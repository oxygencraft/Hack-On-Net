using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.FileSystem
{
    class File
    {
        public int id;

        public Folder parent;
        public int parentId;

        public string name;
        public int writePriv = 0;
        public int readPriv = 0;

        public string content;

        public enum FileType
        {
            NORMAL,
            DAEMON,
            LOG,
            EXE,
            CONFIG
        }

        public FileType type = FileType.NORMAL;

        public List<File> children = new List<File>();

        public File(Folder parent, string name)
        {
            this.name = name;
            this.parent = parent;
            if(parent != null)
            {
                this.parent.children.Add(this);
            }
        }

        public bool HasWritePermission(int priv)
        {
            return priv <= writePriv;
        }

        public bool HasReadPermission(int priv)
        {
            return priv <= readPriv;
        }

        virtual public bool IsFolder()
        {
            return false;
        }

        virtual public void RemoveFile()
        {
            parent.children.Remove(this);
            parentId = 0;
        }

        public void SetType(int specType)
        {
            type = (FileType)specType;
        }
    }
}
