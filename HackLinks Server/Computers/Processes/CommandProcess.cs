using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

        public virtual bool RunCommand(string command)
        {
            string[] commandParts = command.Split(new char[] { ' ' }, 2);
            if (commandParts[0] == "help")
                return Help(commandParts);
            if (Commands.ContainsKey(commandParts[0]))
                return Commands[commandParts[0]].Item2(this, commandParts);
            return false;
        }

        public override void WriteInput(string inputData)
        {
            bool responsed = RunCommand(inputData);
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


        public bool Help(string[] command)
        {
            const int ITEMS_PER_PAGE = 10;
            int totalPages = Commands.Count / ITEMS_PER_PAGE + 1;

            int pageNum = 0;

            bool inputValid = command.Length == 1 || int.TryParse(command[1], out pageNum) && pageNum <= totalPages;

            if (pageNum == 0 || !inputValid)
                pageNum = 1;

            string header = $"---------------------------------\nCommand List - Page {pageNum} of {totalPages}:\n";
            string footer = "\n---------------------------------\n";

            StringBuilder builder = new StringBuilder();

            if (!inputValid)
                builder.AppendLine("Invalid Page Number");

            builder.AppendLine(header);

            if (pageNum <= totalPages)
            {
                builder.AppendLine("------- Commands -------\n");
                foreach (string key in Commands.Keys.Skip((pageNum - 1) * 10).Take(10))
                {
                    builder.AppendLine(Commands[key].Item1);
                    builder.AppendLine();
                }
            }

            builder.Append("help [page]\n    Displays the specified page of commands.");

            builder.Append(footer);

            Print(builder.ToString());

            return true;
        }

    }
}