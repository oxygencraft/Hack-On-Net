using System;
using System.Collections.Generic;

namespace HackLinks_Server.Computers.Processes
{
    /// <summary>
    /// Helper class for Processes that parse commands
    /// </summary>
    public abstract class CommandProcess : Process
    {
        public delegate bool Command(CommandProcess process, string[] command);

        public abstract SortedDictionary<string, Tuple<string, Command>> Commands { get; }

        public CommandProcess(int pid, Printer printer, Node computer, Credentials credentials) : base(pid,  printer, computer, credentials)
        {
        }

        public bool RunCommand(string command)
        {
            string[] commandParts = command.Split(new char[] { ' ' }, 2);
            if (Commands.ContainsKey(commandParts[0]))
                return Commands[commandParts[0]].Item2(this, commandParts);
            return false;
        }

        public override void WriteInput(string inputData)
        {
            RunCommand(inputData);
        }

        /// <summary>
        /// Helper function, Run the given command and exit immediately
        /// </summary>
        /// <param name="command"></param>
        public override void Run(string command)
        {
            exitCode = (byte) (RunCommand(command) ? 0 : 1);
            CurrentState = State.Dead;
        }
    }
}