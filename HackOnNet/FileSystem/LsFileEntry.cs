using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.FileSystem
{
    class LsFileEntry
    {
        string name;
        public bool hasWritePermission;

        string displayName;

        public bool isFolder = false;

        public LsFileEntry(string[] fileData, int permission=0)
        {
            name = fileData[0];
            isFolder = fileData[1] == "d";
            hasWritePermission = fileData[2] == "w";

            displayName = name + (isFolder ? "/" : "");
        }

        public string GetDisplayName()
        {
            return displayName;
        }
        public string GetActualName()
        {
            return name;
        }
        public bool IsFolder()
        {
            return isFolder;
        }
    }
}
