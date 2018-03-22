using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            public string[] Data { get; set; }
        }

        public static List<Packet> ParsePackets(string content)
        {
            List<Packet> packets = new List<Packet>();

            JsonTextReader reader = new JsonTextReader(new StringReader(content))
            {
                SupportMultipleContent = true // Allows us to parse contiguous JSON data 
            };

            while (true)
            {
                if (!reader.Read())
                {
                    break;
                }

                JsonSerializer serializer = new JsonSerializer();
                Packet role = serializer.Deserialize<Packet>(reader);

                packets.Add(role);
            }

            return packets;
        }
    }
}
