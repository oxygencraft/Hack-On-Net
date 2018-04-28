using HackLinks_Server.Files;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers
{
    class Log
    {
        [JsonIgnore()]
        public File file;

        public int sessionId;
        public string ip;
        public LogEvents logEvent;
        public string message;
        public string messageExtended;

        public Log(File file, int sessionId, string ip, LogEvents logEvent, string message)
        {
            this.file = file;
            this.sessionId = sessionId;
            this.ip = ip;
            this.logEvent = logEvent;
            this.message = message;
            file.Content = "####!!*MACHINE READABLE####!!*\n" + SerializeLog(this);
            file.Dirty = true;
        }

        public Log(File file, int sessionId, string ip, LogEvents logEvent, string message, string messageExtended)
        {
            this.file = file;
            this.sessionId = sessionId;
            this.ip = ip;
            this.logEvent = logEvent;
            this.message = message;
            this.messageExtended = messageExtended;
            file.Content = messageExtended + "\n\n####!!*MACHINE READABLE####!!*" + SerializeLog(this);
            file.Dirty = true;
        }
        
        [JsonConstructor()]
        private Log() { }

        public static string SerializeLog(Log log)
        {
            return JsonConvert.SerializeObject(log);
        }

        public static Log Deserialize(string log)
        {
            return JsonConvert.DeserializeObject<Log>(log);
        }

        public enum LogEvents
        {
            Login
        }
    }
}
