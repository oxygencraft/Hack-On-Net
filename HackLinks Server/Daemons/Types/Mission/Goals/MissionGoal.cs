using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types.Mission.Goals
{
    abstract class MissionGoal
    {
        public int Id { get; protected set; }
        virtual public bool AdditionalInformationRequired { get { return false; } }

        abstract public bool IsComplete(string additionalInfo);
    }
}
