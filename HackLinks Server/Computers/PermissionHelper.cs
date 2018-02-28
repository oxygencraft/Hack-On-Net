using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers
{
    static class PermissionHelper
    {
        public static int GetPermissionLevelFromString(string level)
        {
            return level == "root" ? 0 : level == "admin" ? 1 : level == "user" ? 2 : level == "guest" ? 3 : -1;
        }

        public static int ParsePermissionLevel(string level)
        {
            int res = GetPermissionLevelFromString(level);
            if (res == -1)
                if (!int.TryParse(level, out res))
                    return -1;
            return res;
        }
    }
}
