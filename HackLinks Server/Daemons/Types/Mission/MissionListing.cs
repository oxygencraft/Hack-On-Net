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
        public Difficulty difficulty;
        public Status status;
        public string from;
        public string accepted;

        public MissionListing(int id, string missionName, int requiredRanking, Difficulty difficulty, Status status, string from, string accepted)
        {
            this.id = id;
            this.missionName = missionName;
            this.requiredRanking = requiredRanking;
            this.difficulty = difficulty;
            this.status = status;
            this.from = from;
            this.accepted = accepted;
        }

        public enum Status
        {
            Unclaimed,
            InProgress,
            Complete,
            Failed
        }

        public enum Difficulty
        {
            NoobFriendly,
            BeginnerFriendly,
            Easy,
            Medium,
            Hard,
            Extreme,
            Impossible
        }
    }
}
