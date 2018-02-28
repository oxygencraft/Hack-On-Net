using HackOnNet.Sessions.States.Irc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Sessions.States
{
    internal class IrcState : SessionState
    {
        private List<IrcMessage> messages = new List<IrcMessage>();

        public IrcState(Session session) : base(session)
        {

        }

        public override StateType GetStateType()
        {
            return StateType.IRC;
        }

        public void AddMessage(string author, string content)
        {
            messages.Add(new IrcMessage(author, content));
            if(messages.Count > 60)
            {
                messages.RemoveAt(0);
            }
        }

        public List<IrcMessage> GetMessages()
        {
            return messages;
        }
    }
}
