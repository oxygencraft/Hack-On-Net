using HackOnNet.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Sessions.States
{
    internal class LsState : SessionState
    {
        public List<LsFileEntry> files = new List<LsFileEntry>();

        public LsState(Session session) : base(session)
        {
            
        }

        public override StateType GetStateType()
        {
            return StateType.LS;
        }
    }
}
