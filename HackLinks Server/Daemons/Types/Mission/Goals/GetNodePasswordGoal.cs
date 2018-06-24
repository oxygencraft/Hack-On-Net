using HackLinks_Server.Computers;
using HackLinks_Server.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types.Mission.Goals
{
    class GetNodePasswordGoal : MissionGoal
    {
        public string ip;
        public string accountName;
        public override bool AdditionalInformationRequired { get { return true; } }

        public GetNodePasswordGoal(int id, string ip, string accountName)
        {
            Id = id;
            this.ip = ip;
            this.accountName = accountName;
        }

        public override bool IsComplete(string additionalInformation)
        {
            Node computer = Server.Instance.GetComputerManager().GetNodeByIp(ip);
            var configFolder = computer.fileSystem.rootFile.GetFile("etc");
            if (configFolder == null || !configFolder.IsFolder())
                return true;
            File usersFile = configFolder.GetFile("passwd");
            if (usersFile == null)
                return true;
            File groupFile = configFolder.GetFile("group");
            if (usersFile == null)
                return true;
            string[] accounts = usersFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var account in accounts)
            {
                string[] accountData = account.Split(':');
                string accountUsername = accountData[0];
                string accountPassword = accountData[1];
                string accountGroupId = accountData[3];

                if (accountUsername == accountName && accountPassword == additionalInformation)
                    return true;
            }

            return false;
        }
    }
}
