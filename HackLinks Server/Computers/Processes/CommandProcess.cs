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

        public CommandProcess(long pid, long ppid, Printer printer, Node computer, Credentials credentials) : base(pid, ppid, printer, computer, credentials)
        {
        }

        public bool RunCommand(string command)
        {
            string[] commandParts = command.Split(new char[] { ' ' }, 2);
            if (Commands.ContainsKey(commandParts[0]))
                return Commands[commandParts[0]].Item2(this, commandParts);
            return false;
        }

        public override void Run(string command)
        {
            RunCommand(command);

            // TODO set return code
        }
    }
}