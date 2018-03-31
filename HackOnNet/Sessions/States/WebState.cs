using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Sessions.States
{
    class WebState : SessionState
    {
        public string title = "";
        public string content = "";

        public WebState(Session session, string title, string content) : base(session)
        {
            this.title = title;
            this.content = content;
        }
        
        public override StateType GetStateType()
        {
            return StateType.WEB;
        }
    }
}
