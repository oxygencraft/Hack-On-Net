using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackLinks_Server.Files;

namespace HackLinks_Server.Computers.Files
{
    /// <summary>
    /// Contains the global list of all files and alocates ids to new files.
    /// </summary>
    public class FileSystemManager
    {
        private Dictionary<int, File> fileMap = new Dictionary<int, File>();

        private int fileCounter = 1;

        /// <summary>
        /// Return a new globally unique file id
        /// </summary>
        /// <returns>globally unique file id</returns>
        public int GetNewFileId()
        {
            while(fileMap.ContainsKey(fileCounter))
            {
                fileCounter++;
            }
            return fileCounter;
        }

        /// <summary>
        /// Register the given file. Existing files with the same id will be overriden.
        /// </summary>
        /// <param name="newFile">File to register</param>
        public void RegisterNewFile(File newFile)
        {
            Console.WriteLine($"{newFile.Name} Registered with id {newFile.id}");
            fileMap[newFile.id] = newFile;
        }

        /// <summary>
        /// Check if a file id is already in use
        /// </summary>
        /// <param name="id">The file id to check</param>
        /// <returns>Will return true if the file id is in use else returns false</returns>
        public bool IsIdInUse(int id)
        {
            return fileMap.ContainsKey(id);
        }
    }
}
