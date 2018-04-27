using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Processes
{
    public abstract class Process
    {
        public delegate void Printer(string text);

        enum State
        {
            New, // New and not running
            Running, // Running
            Stopped, // Stopped and not running (no longer receiving Update ticks)
            Dead // Dead and waiting for it's exit code to be retrieived so that it may be cleared from the process table
        }

        public readonly int ProcessId;
        public Node computer;

        public File ActiveDirectory { get; set; }
        
        public Credentials Credentials { get; }

        //TODO checks for invalid state on state change
        State CurrentState { get; set; }

        byte exitCode = 0;

        private readonly Printer printer;

        public Printer Print => printer;

        public Process(int pid, Printer printer, Node computer, Credentials credentials)
        {
            ProcessId = pid;
            this.printer = printer ?? delegate { };
            this.computer = computer;
            ActiveDirectory = computer.fileSystem.rootFile;
            Credentials = credentials;

            CurrentState = State.New;

            computer.processes.Add(this);
        }

        /// <summary>
        /// Overridden to provide inital application startup
        /// </summary>
        /// <param name="command">The full command used to launch the application</param>
        public virtual void Run(string command)
        {
            CurrentState = State.Running;
        }

        /// <summary>
        /// Overridden to provide regular periodic updates to long running processes.
        /// It is the responsibility of the implementor to ensure this returns in a timely manner.
        /// </summary>
        public virtual void Update()
        {

        }

        /// <summary>
        /// Overridden to provide input to a running command (effectively STDIN for the process)
        /// </summary>
        /// <param name="inputData"></param>
        public virtual void WriteInput(string inputData)
        {

        }
    }
}
