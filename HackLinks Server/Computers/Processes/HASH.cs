using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Processes
{
    class HASH : Process
    {
        public HASH(long pid, long ppid, Printer printer, Node computer, Credentials credentials) : base(pid, ppid, printer, computer, credentials)
        {
            // left empty because we don't do anything special to initalize this Process
        }

        public override void WriteInput(string inputData)
        {
            if(inputData != null)
            {
                Process child = computer.Kernel.StartProcess(this, "Hackybox");
                child.Run(inputData);
            }
        }
    }
}
