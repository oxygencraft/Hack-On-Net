using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Permissions
{
    static class PermissionHelper
    {
        /// <summary>
        /// Returns the <see cref="Group"/> for the given string or <see cref="Group.INVALID"/> if no match was found.
        /// </summary>
        /// <param name="groupString"></param>
        /// <returns>The matching Group or INVALID if no match</returns>
        public static Group GetGroupFromString(string groupString)
        {
            if (!Enum.TryParse(groupString.ToUpper(), out Group group) || !Enum.IsDefined(typeof(Group), group))
            {
                return Group.INVALID;
            }
            return group;
        }
    }
}
