using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons
{
    public abstract class DaemonClient : CommandProcess
    {
        Session session;
        public Session Session => session;

        Daemon daemon;
        public Daemon Daemon => daemon;

        public DaemonClient(Session session, Daemon daemon, int pid, Printer printer, Node computer, Credentials credentials) : base(pid, printer, computer, credentials)
        {
            this.session = session;
            this.daemon = daemon;
        }

        public override void WriteInput(string inputData)
        {
            if (inputData.Equals("daemon exit"))
            {
                CurrentState = State.Dead;
                return;
            }
            base.WriteInput(inputData);
        }
    }
}
