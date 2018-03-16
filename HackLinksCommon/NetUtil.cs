using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinksCommon
{
    public class NetUtil
    {
        public enum PacketType
        {
            COMND,
            KERNL,
            MESSG,
            LOGRE,
            START,
            OSMSG,
            LOGIN,
        }

        public class StateObject
        {
            public const int BufferSize = 2048;
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }

        public class Packet
        {
            public PacketType Type { get; set; }
            public dynamic Data { get; set; }
        }
    }
}
