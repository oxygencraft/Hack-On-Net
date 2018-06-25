namespace HackLinks_Server.Daemons.Types.Mission {
    class MissionAccount
    {
        public string accountName;
        public int ranking;
        public int currentMission;
        public string password;
        public string clientUsername;
        public string email;

        public MissionAccount(string accountName, int ranking, int currentMission, string password, string clientUsername, string email)
        {
            this.accountName = accountName;
            this.ranking = ranking;
            this.currentMission = currentMission;
            this.password = password;
            this.clientUsername = clientUsername;
            this.email = email;
        }
    }
}
