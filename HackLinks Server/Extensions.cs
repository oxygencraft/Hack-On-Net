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

        public static string JoinWords(this string[] str, string separator, int start=0, int end=-1)
        {
            if (end == -1)
                end = str.Length;
            string result = "";
            for(int i = start; i < end; i++)
            {
                result += str[i] + (i == end-1 ? "" : separator);
            }
            return result;
        }

        public static int GetNthOccurence(this string str, int n, char occ)
        {
            return str.TakeWhile(c => (n -= (c == occ ? 1 : 0)) > 0).Count();
        }
    }
}
