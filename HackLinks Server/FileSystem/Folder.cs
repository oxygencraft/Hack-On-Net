using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.FileSystem
{
    class Folder : File
    {
        public Folder(Folder parent, string name) : base(parent, name)
        {

        }

        override public bool IsFolder()
        {
            return true;
        }

        public File GetFile(string name)
        {
            foreach(File file in children)
            {
                if (file.name == name)
                    return file;
            }
            return null;
        }

        public void PrintFolderRecursive(int depth)
        {
            string tabs = new String(' ', depth);
            Console.WriteLine(tabs + id + "  d- " + name);
            foreach(var item in children)
            {
                if(item.IsFolder())
                {
                    ((Folder)item).PrintFolderRecursive(depth + 1);
                }
                else
                {
                    Console.WriteLine(tabs + " " + item.id + "  f- " + item.name);
                }
            }
        }

    }
}
