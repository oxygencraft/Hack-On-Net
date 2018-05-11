using System;
using System.Collections.Generic;

namespace HackLinks_Server
{
    public class CompiledFileManager
    {
        private Dictionary<int, string> types = new Dictionary<int, string>();

        public void AddType(int checksum, string type)
        {
            types.Add(checksum, type);
        }

        public string GetType(int checksum)
        {
            return types.ContainsKey(checksum) ? types[checksum] : "False";
        }

        public Dictionary<int, string> GetMap()
        {
            // We return a shallow clone here to prevent unauthorized manipulation of the map
            return new Dictionary<int, string>(types);
        }
    }
}