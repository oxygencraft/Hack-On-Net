using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Daemons.Types.Bank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HackLinksCommon.NetUtil;

namespace HackLinks_Server.Daemons.Types
{
    class BankDaemon : Daemon
    {

        public SortedDictionary<string, Tuple<string, CommandHandler.Command>> daemonCommands = new SortedDictionary<string, Tuple<string, CommandHandler.Command>>()
        {
            { "account", new Tuple<string, CommandHandler.Command>("account [create/balance/transfer/loan/transactions]\n    Performs an account operation.", Account) },
        };

        public override SortedDictionary<string, Tuple<string, CommandHandler.Command>> Commands
        {
            get => daemonCommands;
        }

        public override string StrType => "bank";

        public override DaemonType GetDaemonType()
        {
            return DaemonType.BANK;
        }

        public BankDaemon(Node node) : base(node)
        {
            
            this.accessLevel = Group.GUEST;
        }

        public List<Account> accounts = new List<Account>();

        public static bool Account(GameClient client, string[] command)
        {
            Session session = client.activeSession;

            BankDaemon daemon = (BankDaemon)client.activeSession.activeDaemon;

            if (command[0] == "account")
            {
                if (command.Length < 2)
                {
                    session.owner.Send(PacketType.MESSG, "Usage : account [create/balance/transfer/loan/transactions]");
                    return true;
                }
                var cmdArgs = command[1].Split(' ');
                if (command[1] == "create")
                {
                    
                }
                return true;
            }
            return false;
        }

        public override void OnStartUp()
        {
            
        }

        public override bool HandleDaemonCommand(GameClient client, string[] command)
        {
            if (Commands.ContainsKey(command[0]))
                return Commands[command[0]].Item2(client, command);

            return false;
        }

        public override string GetSSHDisplayName()
        {
            return null;
        }
    }
}
