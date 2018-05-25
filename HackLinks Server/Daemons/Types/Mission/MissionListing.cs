using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types.Mission
{
    class MissionListing
    {
        public int id;
        public string missionName;
        public int requiredRanking;
        public int status;
        public string from;
        public string accepted;

        public MissionListing(int id, string missionName, int requiredRanking, int status, string from, string accepted)
        {
            this.id = id;
            this.missionName = missionName;
            this.requiredRanking = requiredRanking;
            this.status = status;
            this.from = from;
            this.accepted = accepted;
        }
    }
}
