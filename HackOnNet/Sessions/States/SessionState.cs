using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Sessions.States
{
    abstract class SessionState
    {
        Session session;

        public enum StateType
        {
            DEFAULT,
            LS,
            VIEW,
            IRC
        };

        public SessionState(Session session)
        {
            this.session = session;
        }

        public abstract StateType GetStateType();
    }
}
