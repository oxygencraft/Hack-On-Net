using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Processes
{
    public class False : Process
    {
        public False(int pid, Printer printer, Node computer, Credentials credentials) : base(pid, printer, computer, credentials)
        {
        }

        public override void Run(string command)
        {
            base.Run(command);
            CurrentState = State.Dead;
            exitCode = 1; // non-zero exitcode
        }
    }
}
