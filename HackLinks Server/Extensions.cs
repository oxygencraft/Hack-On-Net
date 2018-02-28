using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server
{
    static class Extensions
    {
        public static string StripEscaped(this string str)
        {
            while(str.IndexOf("\\") != -1)
            {
                str = str.Remove(str.IndexOf("\\"), 2);
            }
            return str;
        }
    }
}
