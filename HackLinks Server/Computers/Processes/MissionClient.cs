using HackLinks_Server.Daemons;
using HackLinks_Server.Daemons.Types;
using HackLinks_Server.Daemons.Types.Mission;
using HackLinks_Server.Daemons.Types.Mission.Goals;
using HackLinks_Server.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Processes
{
    class MissionClient : DaemonClient
    {
        public SortedDictionary<string, Tuple<string, Command>> commands = new SortedDictionary<string, Tuple<string, Command>>()
        {
            { "account", new Tuple<string, Command>("account [create/login/resetpass/delete]\n    Performs an account operation.", Account) },
            { "mission", new Tuple<string, Command>("mission [browse/accept/complete/create/abandon]\n    Performs a mission board operation.", Mission) }
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;
        private MissionAccount loggedInAccount = null;

        public MissionClient(Session session, Daemon daemon, int pid, Printer printer, Node computer, Credentials credentials) : base(session, daemon, pid, printer, computer, credentials)
        {
            
        }

        public override bool RunCommand(string command)
        {
            // We hide the old runCommand function to perform this check on startup
            if (!((MissionDaemon)Daemon).CheckFolders(this))
            {
                return true;
            }
            return base.RunCommand(command);
        }

        public static bool Mission(CommandProcess process, string[] command)
        {
            MissionClient client = (MissionClient)process;
            MissionDaemon daemon = (MissionDaemon)client.Daemon;

            var missionFolder = process.computer.fileSystem.rootFile.GetFile("mission");
            var accountFile = missionFolder.GetFile("accounts.db");
            var missionFile = missionFolder.GetFile("missions.db");

            if (command[0] == "mission")
            {
                if (client.loggedInAccount == null)
                {
                    process.Print("You are not logged in");
                    return true;
                }
                if (command.Length < 2)
                {
                    process.Print("Usage : mission [browse/accept/complete/create/abandon]");
                    return true;
                }
                var cmdArgs = command[1].Split(' ');
                if (cmdArgs[0] == "browse")
                {
                    if (daemon.missions.Count == 0)
                    {
                        process.Print("There are currently no missions available on this board");
                        return true;
                    }
                    string missionsForClient = "ID  MISSION NAME                                REQUIRED RANKING  DIFFICULTY  STATUS  EMPLOYER\n";
                    foreach (var missionPair in daemon.missions)
                    {
                        var mission = missionPair.Value;
                        if (mission.status == MissionListing.Status.Unclaimed)
                            missionsForClient += mission.id + " " + mission.missionName + " " + mission.requiredRanking + " " + mission.difficulty + " " + mission.status + " " + mission.employer + "\n";
                    }
                    File missionFileForClient = client.Session.owner.homeComputer.fileSystem.rootFile.GetFile("Missions_On_" + client.computer.ip);
                    if (missionFileForClient == null)
                    {
                        if (missionsForClient == "ID  MISSION NAME                                REQUIRED RANKING  DIFFICULTY  STATUS  EMPLOYER\n")
                        {
                            process.Print("There are currently no missions visible to you");
                            return true;
                        }
                        missionFileForClient = client.Session.owner.homeComputer.fileSystem.CreateFile(client.Session.owner.homeComputer, client.Session.owner.homeComputer.fileSystem.rootFile, "Missions_On_" + client.computer.ip);
                        missionFileForClient.Content = missionsForClient;
                        missionFileForClient.OwnerId = 0;
                        missionFileForClient.Permissions.SetPermission(FilePermissions.PermissionType.User, true, true, true);
                        missionFileForClient.Permissions.SetPermission(FilePermissions.PermissionType.Group, true, true, true);
                        missionFileForClient.Group = missionFileForClient.Parent.Group;
                        process.Print("A file containing the missions on this baord has been uploaded to your computer");
                        return true;
                    }
                    if (missionsForClient == "ID  MISSION NAME                                REQUIRED RANKING  DIFFICULTY  STATUS  EMPLOYER\n")
                    {
                        process.Print("There are currently no missions visible to you");
                        return true;
                    }
                    missionFileForClient.Content = missionsForClient;
                    process.Print("A file containing the missions on this board has been uploaded to your computer");
                }
                if (cmdArgs[0] == "browsecreated")
                {
                    if (daemon.missions.Count == 0)
                    {
                        process.Print("There are currently no missions available on this board");
                        return true;
                    }
                    string missionsForClient = "ID  MISSION NAME                                REQUIRED RANKING  DIFFICULTY  STATUS  EMPLOYER\n";
                    foreach (var missionPair in daemon.missions)
                    {
                        var mission = missionPair.Value;
                        if (mission.employer == client.loggedInAccount.accountName)
                            missionsForClient += mission.id + " " + mission.missionName + " " + mission.requiredRanking + " " + mission.difficulty + " " + mission.status + " " + mission.employer + "\n";
                    }
                    File missionFileForClient = client.Session.owner.homeComputer.fileSystem.rootFile.GetFile("Your_Missions_On_" + client.computer.ip);
                    if (missionFileForClient == null)
                    {
                        if (missionsForClient == "ID  MISSION NAME                                REQUIRED RANKING  DIFFICULTY  STATUS  EMPLOYER\n")
                        {
                            process.Print("There are currently no missions visible to you");
                            return true;
                        }
                        missionFileForClient = client.Session.owner.homeComputer.fileSystem.CreateFile(client.Session.owner.homeComputer, client.Session.owner.homeComputer.fileSystem.rootFile, "Your_Missions_On_" + client.computer.ip);
                        missionFileForClient.Content = missionsForClient;
                        missionFileForClient.OwnerId = 0;
                        missionFileForClient.Permissions.SetPermission(FilePermissions.PermissionType.User, true, true, true);
                        missionFileForClient.Permissions.SetPermission(FilePermissions.PermissionType.Group, true, true, true);
                        missionFileForClient.Group = missionFileForClient.Parent.Group;
                        process.Print("A file containing the missions you created on this baord has been uploaded to your computer");
                        return true;
                    }
                    if (missionsForClient == "ID  MISSION NAME                                REQUIRED RANKING  DIFFICULTY  STATUS  EMPLOYER\n")
                    {
                        process.Print("There are currently no missions visible to you");
                        return true;
                    }
                    missionFileForClient.Content = missionsForClient;
                    process.Print("A file containing the missions on this board has been uploaded to your computer");
                }
                if (cmdArgs[0] == "description")
                {
                    if (cmdArgs.Length < 2)
                    {
                        process.Print("Usage : mission description [missionid]");
                        return true;
                    }
                    if (CheckMissionId(cmdArgs[1], out MissionListing mission, client, process, daemon, false))
                        return true;
                    if (string.IsNullOrWhiteSpace(mission.description))
                    {
                        process.Print("No description for mission ID " + mission.id);
                        return true;
                    }
                    process.Print("Description for mission ID " + mission.id + "\n\n" + mission.description);
                }
                if (cmdArgs[0] == "accept")
                {
                    if (cmdArgs.Length < 2)
                    {
                        process.Print("Usage : mission accept [missionid]");
                        return true;
                    }
                    if (CheckMissionId(cmdArgs[1], out MissionListing mission, client, process, daemon, false))
                        return true;
                    mission.status = MissionListing.Status.InProgress;
                    mission.claimedBy = client.loggedInAccount.accountName;
                    client.loggedInAccount.currentMission = mission.id;
                    daemon.UpdateMissionDatabase();
                }
                if (cmdArgs[0] == "completegoal")
                {
                    if (cmdArgs.Length < 2)
                    {
                        process.Print("Usage : mission completegoal [additionalinfo]");
                        return true;
                    }
                    if (client.loggedInAccount.currentMission == 0)
                    {
                        process.Print("You don't have a mission in progress");
                        return true;
                    }
                    MissionListing mission = null;
                    if (CheckMissionId(client.loggedInAccount.currentMission.ToString(), out mission, client, process, daemon, false))
                        return true;
                    if (mission.completedGoals == null)
                        mission.completedGoals = new List<int>();

                    int doneWords = 0;
                    string additionalInfo = "";
                    foreach (var word in cmdArgs)
                    {
                        if (doneWords < 1)
                        {
                            doneWords++;
                            continue;
                        }
                        if (additionalInfo == "")
                        {
                            additionalInfo = word;
                            continue;
                        }
                        additionalInfo += " " + word;
                    }

                    foreach (var goal in mission.goals)
                    {
                        if (goal.IsComplete(additionalInfo))
                        {
                            mission.completedGoals.Add(goal.Id);
                        }
                    }
                    daemon.UpdateMissionDatabase();
                }
                if (cmdArgs[0] == "complete")
                {
                    if (client.loggedInAccount.currentMission == 0)
                    {
                        process.Print("You don't have a mission in progress");
                        return true;
                    }
                    MissionListing mission = null;
                    if (CheckMissionId(client.loggedInAccount.currentMission.ToString(), out mission, client, process, daemon, false))
                        return true;
                    if (mission.completedGoals == null)
                        mission.completedGoals = new List<int>();

                    if (mission.goals.Count != mission.completedGoals.Count)
                    {
                        foreach (var goal in mission.goals)
                        {
                            if (goal.IsComplete("") && !goal.AdditionalInformationRequired)
                            {
                                mission.completedGoals.Add(goal.Id);
                            }
                        }
                    }
                    if (mission.goals.Count != mission.completedGoals.Count)
                    {
                        process.Print("Mission Incomplete.\nUse mission completegoal to provide additional information.");
                        return true;
                    }
                    mission.status = MissionListing.Status.Complete;
                    client.loggedInAccount.currentMission = 0;
                    daemon.UpdateMissionDatabase();
                }
                if (cmdArgs[0] == "abandon")
                {
                    MissionListing mission = null;
                    if (CheckMissionId(client.loggedInAccount.currentMission.ToString(), out mission, client, process, daemon, false))
                        return true;
                    mission.status = MissionListing.Status.Unclaimed;
                    mission.claimedBy = null;
                    client.loggedInAccount.currentMission = 0;
                    daemon.UpdateMissionDatabase();
                }
                if (cmdArgs[0] == "addgoal")
                {
                    if (cmdArgs.Length < 3)
                    {
                        process.Print("Usage : mission addgoal [missionid] [getpass/replytext] [additionalinformationifrequired]");
                        return true;
                    }
                    MissionListing mission = null;
                    if (CheckMissionId(cmdArgs[1], out mission, client, process, daemon, true))
                        return true;
                    if (mission.goals == null)
                        mission.goals = new List<MissionGoal>();
                    if (cmdArgs[2] == "getpass")
                    {
                        if (cmdArgs.Length < 5)
                        {
                            process.Print("Usage : mission addgoal [missionid] [getpass] [ip] [accountname]");
                            return true;
                        }
                        mission.goals.Add(new GetNodePasswordGoal(mission.goals.Count + 1, cmdArgs[3], cmdArgs[4]));
                    }
                    if (cmdArgs[2] == "replytext")
                    {
                        if (cmdArgs.Length < 4)
                        {
                            process.Print("Usage : mission addgoal [missionid] [replytext] [texttoreply]");
                            return true;
                        }
                        int doneWords = 0;
                        string text = "";
                        foreach (var word in cmdArgs)
                        {
                            if (doneWords < 3)
                            {
                                doneWords++;
                                continue;
                            }
                            if (text == "")
                            {
                                text = word;
                                continue;
                            }
                            text += " " + word;
                        }
                        mission.goals.Add(new ReplyTextGoal(mission.goals.Count + 1, text));
                    }
                    daemon.UpdateMissionDatabase();
                }
                if (cmdArgs[0] == "setdescription")
                {
                    if (cmdArgs.Length < 3)
                    {
                        process.Print("Usage : mission setdescription [missionid] [description]");
                        return true;
                    }
                    MissionListing mission = null;
                    if (CheckMissionId(cmdArgs[1], out mission, client, process, daemon, true))
                        return true;
                    int doneWords = 0;
                    string description = "";
                    foreach (var word in cmdArgs)
                    {
                        if (doneWords < 2)
                        {
                            doneWords++;
                            continue;
                        }
                        if (description == "")
                        {
                            description = word;
                            continue;
                        }
                        description += " " + word;
                    }
                    mission.description = description;
                    daemon.UpdateMissionDatabase();
                }
                if (cmdArgs[0] == "setstartdescription")
                {
                    if (cmdArgs.Length < 3)
                    {
                        process.Print("Usage : mission setstartdescription [missionid] [description]");
                        return true;
                    }
                    MissionListing mission = null;
                    if (CheckMissionId(cmdArgs[1], out mission, client, process, daemon, true))
                        return true;
                    int doneWords = 0;
                    string description = "";
                    foreach (var word in cmdArgs)
                    {
                        if (doneWords < 2)
                        {
                            doneWords++;
                            continue;
                        }
                        if (description == "")
                        {
                            description = word;
                            continue;
                        }
                        description += " " + word;
                    }
                    mission.startDescription = description;
                    daemon.UpdateMissionDatabase();
                }
                if (cmdArgs[0] == "setdifficulty")
                {
                    if (cmdArgs.Length < 3)
                    {
                        process.Print("Usage : mission setdifficulty [missionid] [difficulty]");
                        return true;
                    }
                    MissionListing mission = null;
                    if (CheckMissionId(cmdArgs[1], out mission, client, process, daemon, true))
                        return true;
                    int difficultyInt;
                    if (!int.TryParse(cmdArgs[3], out difficultyInt))
                    {
                        process.Print("Difficulty must be a number and be one of the options\nValid Options: 0 = Beginner\n1 = Basic\n2 = Intermediate\n3 = Advanced\n4 = Expert\n5 = Extreme\n6 = Impossible");
                        return true;
                    }
                    else if (difficultyInt > 6 || difficultyInt < 0)
                    {
                        process.Print("Not a valid difficulty option\nValid Options: 0 = Beginner\n1 = Basic\n2 = Intermediate\n3 = Advanced\n4 = Expert\n5 = Extreme\n6 = Impossible");
                        return true;
                    }
                    mission.difficulty = (MissionListing.Difficulty)difficultyInt;
                    daemon.UpdateMissionDatabase();
                }
                if (cmdArgs[0] == "publish")
                {
                    if (cmdArgs.Length < 2)
                    {
                        process.Print("Usage : mission publish [missionid]");
                        return true;
                    }
                    MissionListing mission = null;
                    if (CheckMissionId(cmdArgs[1], out mission, client, process, daemon, true))
                        return true;
                    if (mission.goals == null || mission.goals.Count == 0)
                    {
                        process.Print("You must set goals for the mission before you can publish the mission");
                        return true;
                    }
                    if (string.IsNullOrWhiteSpace(mission.startDescription))
                    {
                        process.Print("You must set a start description for the mission before you can publish the mission. A start description should contain everything the user needs to know (including the goals as the user does not get a list of goals, they have to get the goals from the start description)");
                        return true;
                    }
                    mission.status = MissionListing.Status.Unclaimed;
                    daemon.UpdateMissionDatabase();
                }
                if (cmdArgs[0] == "unpublish")
                {
                    if (cmdArgs.Length < 2)
                    {
                        process.Print("Usage : mission publish [missionid]");
                        return true;
                    }
                    MissionListing mission = null;
                    if (CheckMissionId(cmdArgs[1], out mission, client, process, daemon, true))
                        return true;
                    if (mission.status == MissionListing.Status.Complete)
                    {
                        process.Print("You cannot unpublish a complete mission");
                        return true;
                    }
                    if (mission.status == MissionListing.Status.InProgress)
                    {
                        for (int i = 0; i < daemon.accounts.Count; i++)
                        {
                            if (daemon.accounts[i].accountName == mission.claimedBy)
                            {
                                // TODO: Tell the user that the employer has unpublished the mission
                                daemon.accounts[i].currentMission = 0;
                                break;
                            }
                        }
                    }
                    mission.status = MissionListing.Status.Unpublished;
                    daemon.UpdateMissionDatabase();
                }
                if (cmdArgs[0] == "create")
                {
                    if (cmdArgs.Length < 2)
                    {
                        process.Print("Usage : mission create [missionname] [requiredranking] [difficulty]\nValid Options for Difficulty: 0 = Beginner\n1 = Basic\n2 = Intermediate\n3 = Advanced\n4 = Expert\n5 = Extreme\n6 = Impossible");
                        return true;
                    }
                    if (!int.TryParse(cmdArgs[2], out int requiredRanking)) {
                        process.Print("Required ranking must be a number");
                        return true;
                    } else if (requiredRanking < 0) {
                        process.Print("Required ranking cannot be a negative number");
                        return true;
                    }
                    if (!int.TryParse(cmdArgs[3], out int difficultyInt))
                    {
                        process.Print("Difficulty must be a number and be one of the options\nValid Options: 0 = Beginner\n1 = Basic\n2 = Intermediate\n3 = Advanced\n4 = Expert\n5 = Extreme\n6 = Impossible");
                        return true;
                    }
                    else if (difficultyInt > 6 || difficultyInt < 0)
                    {
                        process.Print("Not a valid difficulty option\nValid Options: 0 = Beginner\n1 = Basic\n2 = Intermediate\n3 = Advanced\n4 = Expert\n5 = Extreme\n6 = Impossible");
                        return true;
                    }
                    MissionListing.Difficulty difficulty = (MissionListing.Difficulty)difficultyInt;
                    daemon.missions.Add(daemon.missions.Count + 1, new MissionListing(daemon.missions.Count + 1, cmdArgs[1], null, requiredRanking, difficulty, MissionListing.Status.Unpublished, client.loggedInAccount.accountName, null, null, null, null));
                    daemon.UpdateMissionDatabase();
                }
            }
            return false;
        }

        public static bool Account(CommandProcess process, string[] command)
        {
            MissionClient client = (MissionClient)process;
            MissionDaemon daemon = (MissionDaemon)client.Daemon;

            var missionFolder = process.computer.fileSystem.rootFile.GetFile("mission");
            var accountFile = missionFolder.GetFile("accounts.db");

            if (command[0] == "account")
            {
                if (command.Length < 2)
                {
                    process.Print("Usage : account [create/login/resetpass/delete]");
                    return true;
                }
                var cmdArgs = command[1].Split(' ');
                if (cmdArgs[0] == "create")
                {
                    // TODO: When mail daemon is implemented, require an email address for password reset
                    if (cmdArgs.Length < 3)
                    {
                        process.Print("Usage : account create [accountname] [email] [password]");
                        return true;
                    }
                    string[] emailArgs = cmdArgs[2].Split('@');
                    if (emailArgs.Length != 2) {
                        process.Print("Not a valid email address");
                        return true;
                    }
                    Node emailServer = Server.Instance.GetComputerManager().GetNodeByIp(emailArgs[1]);
                    if (emailServer == null) {
                        process.Print("The email server does not exist!\nMake sure you use the IP of the mail server, not the domain.");
                    }
                    if (!(new MailDaemon(emailServer.NextPID, null, emailServer, new Credentials(emailServer.GetUserId("guest"), Permissions.Group.GUEST)).accounts.Any(x => x.accountName == emailArgs[0]))) {
                        process.Print("The email account on that email server does not exist!");
                        return true;
                    }
                    if (daemon.accounts.Count != 0)
                    {
                        foreach (var account in daemon.accounts)
                        {
                            if (account.clientUsername == client.Session.owner.username)
                            {
                                process.Print("You already have an account on this mission board.\nTo reset your password, use account resetpass");
                                return true;
                            }
                        }
                    }
                    List<string> accounts = new List<string>();
                    var accountsFile = accountFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (accountsFile.Length != 0)
                    {
                        foreach (string line in accountFile.Content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var data = line.Split(',');
                            if (data.Length < 5)
                                continue;
                            accounts.Add(data[0]);
                        }
                    }
                    if (accounts.Contains(cmdArgs[1]))
                    {
                        process.Print("This account name is not available");
                        return true;
                    }
                    daemon.accounts.Add(new MissionAccount(cmdArgs[1], 0, 0, cmdArgs[3], client.Session.owner.username, cmdArgs[2]));
                    daemon.UpdateAccountDatabase();
                    process.Print("Your account has been opened. Use account login [accountname] [password] to login.");
                }
                if (cmdArgs[0] == "login")
                {
                    if (cmdArgs.Length < 3)
                    {
                        process.Print("Usage : account login [accountname] [password]");
                        return true;
                    }
                    foreach (var account in daemon.accounts)
                    {
                        if (account.accountName == cmdArgs[1] && account.password == cmdArgs[2])
                        {
                            client.loggedInAccount = account;
                            daemon.computer.Log(Log.LogEvents.Login, daemon.computer.logs.Count + 1 + " " + client.Session.owner.homeComputer.ip + " logged in as bank account " + account.accountName, client.Session.sessionId, client.Session.owner.homeComputer.ip);
                            process.Print($"Logged into mission board account {account.accountName} successfully");
                            return true;
                        }
                    }
                    process.Print("Invalid account name or password");
                }
                if (cmdArgs[0] == "resetpass") {
                    if (cmdArgs.Length == 2) {
                        MissionAccount account = daemon.accounts.Where(x => x.accountName == cmdArgs[1]).DefaultIfEmpty(null).First();
                        if (account == null) {
                            process.Print("That account does not exist!");
                            return true;
                        }
                        if (!MailDaemon.SendPasswordResetEmail(process.computer, account.email, account.accountName)) {
                            process.Print("The email failed to send! Either the account no longer exists, or some other error occured.");
                            return true;
                        }
                        process.Print("Password reset email sent to the email associated with this account!");
                        return true;
                    }
                    if (cmdArgs.Length < 4) {
                        process.Print("Usage : account resetpass [accountname] [authentication code] [newpassword]");
                        return true;
                    }
                    if (!int.TryParse(cmdArgs[2], out int authCode)) {
                        process.Print("Please use a valid authentication code!");
                        return true;
                    }
                    if (!MailDaemon.CheckIfAuthRequestIsValid(process.computer, cmdArgs[1], authCode) || !daemon.accounts.Any(x => x.accountName == cmdArgs[1])) {
                        process.Print("Something went wrong when trying to authenticate!\nEither the username does not exist, or the authentication code is invalid!");
                        return true;
                    }

                    foreach (MissionAccount account in daemon.accounts) {
                        if (account.accountName == cmdArgs[1]) {
                            account.password = cmdArgs[3];
                            daemon.UpdateAccountDatabase();
                        }
                    }
                    process.Print("Password reset successful!");
                    return true;
                }
                if (cmdArgs[0] == "delete") {
                    if (client.loggedInAccount == null) {
                        process.Print("You are not logged in");
                        return true;
                    }
                    if (client.loggedInAccount.currentMission != 0) {
                        process.Print("You cannot delete your account while you have a mission in progress\nYou must complete or abandon the mission before you can delete your account");
                        return true;
                    }
                    if (cmdArgs.Length >= 2) {
                        if (cmdArgs[1] != "y") {
                            process.Print("Are you sure you want to delete your account?\nRun account delete y if you are sure you want to delete your account");
                            return true;
                        }
                    } else {
                        process.Print("Are you sure you want to delete your account?\nRun account delete y if you are sure you want to delete your account");
                        return true;
                    }
                    daemon.accounts.Remove(client.loggedInAccount);
                    daemon.UpdateAccountDatabase();
                    client.loggedInAccount = null;
                    process.Print("Your account has been deleted");
                }
                return true;
            }
            return false;
        }

        private static bool CheckMissionId(string missionIdString, out MissionListing mission, MissionClient client, CommandProcess process, MissionDaemon daemon, bool employerCheck = false)
        {
            mission = null;
            if (daemon.missions.Count == 0)
            {
                process.Print("There are currently no missions available on this board");
                return true;
            }
            int missionId;
            if (!int.TryParse(missionIdString, out missionId))
            {
                process.Print("The mission ID must be a number");
                return true;
            }
            mission = daemon.missions[missionId];
            if (mission == null)
            {
                process.Print("Mission ID not found");
                return true;
            }
            if (employerCheck)
            {
                if (mission.employer != client.loggedInAccount.accountName)
                {
                    process.Print("Only the employer can edit the mission");
                    return true;
                }
            }
            return false;
        }
    }
}
