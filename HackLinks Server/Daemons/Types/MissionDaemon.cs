using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Processes;
using HackLinks_Server.Daemons.Types.Mission;
using HackLinks_Server.Files;
using HackLinksCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HackLinksCommon.NetUtil;

namespace HackLinks_Server.Daemons.Types
{
    class MissionDaemon : Daemon
    {
        public override string StrType => "mission";

        protected override Type ClientType => typeof(BankClient);

        public override DaemonType GetDaemonType()
        {
            return DaemonType.MISSION;
        }

        public MissionDaemon(int pid, Printer printer, Node computer, Credentials credentials) : base(pid, printer, computer, credentials)
        {

        }

        public List<MissionAccount> accounts = new List<MissionAccount>();
        public Dictionary<int, MissionListing> missions = new Dictionary<int, MissionListing>();

        public void LoadAccounts()
        {
            accounts.Clear();
            File accountFile = node.fileSystem.rootFile.GetFileAtPath("/mission/accounts.db");
            if (accountFile == null)
                return;
            foreach (string line in accountFile.Content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var data = line.Split(',');
                if (data.Length < 5)
                    continue;
                accounts.Add(new MissionAccount(data[0], Convert.ToInt32(data[1]), Convert.ToInt32(data[2]), data[3], data[5]));
            }
        }

        public void UpdateAccountDatabase()
        {
            File accountFile = node.fileSystem.rootFile.GetFileAtPath("/mission/accounts.db");
            if (accountFile == null)
                return;
            string newAccountsFile = "";
            foreach (var account in accounts)
            {
                newAccountsFile += account.accountName + "," + account.ranking + "," + account.currentMission + "," + account.password + "," + account.clientUsername + "\r\n";
            }
            accountFile.Content = newAccountsFile;
        }

        public void UpdateMissionDatabase()
        {
            File missionFile = node.fileSystem.rootFile.GetFileAtPath("/mission/missions.db");
            if (missionFile == null)
                return;
            string newMissionsFile = "";
            foreach (var missionKeyPair in missions)
            {
                var mission = missionKeyPair.Value;
                int missionId = missionKeyPair.Key;
                newMissionsFile += missionId + "," + mission.missionName + "," + mission.requiredRanking + "," + mission.status + "," + mission.from + "," + mission.accepted + "\r\n";
            }
            missionFile.Content = newMissionsFile;
        }

        public bool CheckFolders(CommandProcess process)
        {
            var missionFolder = process.computer.fileSystem.rootFile.GetFile("mission");
            if (missionFolder == null || !missionFolder.IsFolder())
            {
                process.Print("No mission daemon folder was found ! (Contact the admin of this node to create one as the mission board is useless without one)");
                return false;
            }
            var accountFile = missionFolder.GetFile("accounts.db");
            if (accountFile == null)
            {
                process.Print("No accounts file was found ! (Contact the admin of this node to create one as the mission board is useless without one)");
                return false;
            }
            var missionFile = missionFolder.GetFile("missions.db");
            if (accountFile == null)
            {
                process.Print("No missions file was found ! (Contact the admin of this node to create one as the mission board is useless without one)");
                return false;
            }
            return true;
        }

        public override void OnStartUp()
        {
            LoadAccounts();
        }

        public override string GetSSHDisplayName()
        {
            return null;
        }
    }
}
