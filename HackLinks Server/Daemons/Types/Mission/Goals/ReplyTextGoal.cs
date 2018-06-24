using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types.Mission.Goals
{
    class ReplyTextGoal : MissionGoal
    {
        public string requiredText;
        public override bool AdditionalInformationRequired { get { return true; } }

        public ReplyTextGoal(int id, string requiredText)
        {
            Id = id;
            this.requiredText = requiredText;
        }

        public override bool IsComplete(string additionalInformation)
        {
            if (additionalInformation == requiredText)
                return true;
            return false;
        }
    }
}
