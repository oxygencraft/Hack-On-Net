using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Sessions.States
{
    class ViewState : SessionState
    {
        public string name;
        public string content;

        public ViewState(Session session, string name, string content) : base(session)
        {
            this.name = name;
            this.content = content;
        }

        public override StateType GetStateType()
        {
            return StateType.VIEW;
        }
    }
}
