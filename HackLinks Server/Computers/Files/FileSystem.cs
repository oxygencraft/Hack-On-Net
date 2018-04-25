using HackLinks_Server.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Files
{
    /// <summary>
    /// Contains Files for a computer
    /// </summary>
    class FileSystem
    {
        public readonly FileSystemManager fileSystemManager;

        public File rootFile;

        public FileSystem(FileSystemManager fileSystemManager)
        {
            this.fileSystemManager = fileSystemManager;
        }

        public File CreateFile(Node computer, File parent, string fileName)
        {
            return File.CreateNewFile(fileSystemManager, computer, parent, fileName);
        }

        public File CreateFile(int id, Node computer, File parent, string fileName)
        {
            return File.CreateNewFile(id, fileSystemManager, computer, parent, fileName);
        }

        public File CreateFolder(Node computer, File parent, string fileName)
        {
            return File.CreateNewFolder(fileSystemManager, computer, parent, fileName);
        }
    }
}
