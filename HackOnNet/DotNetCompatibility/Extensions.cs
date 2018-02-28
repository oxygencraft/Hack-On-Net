using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HackOnNet.DotNetCompatibility
{
    static class Extensions
    {
        public static Task Delay(int milliseconds)
        {
            var tcs = new TaskCompletionSource<object>();
            new Timer(_ => tcs.SetResult(null)).Change(milliseconds, -1);
            return tcs.Task;
        }

        public static string EscapeChar(this string str)
        {
            string res = "";
            string forbiddenChar = ";:,^\\";
            for (int i = 0; i < str.Length; i++)
            {
                if (forbiddenChar.Contains(str[1]))
                {
                    res += "\\" + str[1];
                }
                else
                    res += str[1];
            }
            return res;
        }

    }
}
