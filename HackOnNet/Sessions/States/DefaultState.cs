using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Sessions.States
{
    internal class DefaultState : SessionState
    {
        public DefaultState(Session session) : base(session)
        {

        }

        public override StateType GetStateType()
        {
            return SessionState.StateType.DEFAULT;
        }
    }
}
