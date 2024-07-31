namespace BoltonCup.Api
{
    public class ScheduleService
    {
        #region singleton
        private static ScheduleService? instance = null;
        public static ScheduleService Instance()
        {
            if (instance == null) instance = new ScheduleService();
            return instance;
        }
        #endregion

        private List<Matchup> matchups;

        private ScheduleService()
        {
            matchups = GetMatchupSchedule();
        }

        private List<Matchup> GetMatchupSchedule()
        {
            var teamInst = TeamService.Instance();
            var placeholder = (string n) => new TeamData
            {
                Id = 0,
                Name = n,
                Logo = "logo.png",
                Players = new(),
            };
            var jtt = teamInst.GetTeamByID("1") ?? placeholder(""); // Just the Tip
            var nt = teamInst.GetTeamByID("2") ?? placeholder(""); // Nipple Ticklers
            var ttt = teamInst.GetTeamByID("3") ?? placeholder(""); // Tecumseh Titty Twisters
            var sws = teamInst.GetTeamByID("4") ?? placeholder(""); // South West Sausages

            var res = new List<Matchup>
            {
                new Matchup("Game 1", jtt, sws, new DateTime(2024,08,02,05 + 12 ,30,00), "Rink C"),
                new Matchup("Game 2", nt, ttt, new DateTime(2024,08,02,06 + 12 ,30,00), "Rink C"),
                new Matchup("Game 3", sws, ttt, new DateTime(2024,08,03,03 + 12 ,30,00), "Shuren"),
                new Matchup("Game 4", jtt, nt, new DateTime(2024,08,03,04 + 12 ,30,00), "Shuren"),
                new Matchup("Game 5", sws, nt, new DateTime(2024,08,03,05 + 12 ,30,00), "Shuren"),
                new Matchup("Game 6", ttt, jtt, new DateTime(2024,08,03,06 + 12 ,30,00), "Shuren"),
                new Matchup("Finals (Bronze)", placeholder("Finals (3rd)"), placeholder("Finals (4th)"), new DateTime(2024,08,04,01 + 12 ,00,00), "Shuren"),
                new Matchup("Finals (Gold)", placeholder("Finals (1st)"), placeholder("Finals (2nd)"), new DateTime(2024,08,04,02 + 12 ,15,00), "Shuren"),
            };
            return res;
        }

        public List<Matchup> Matchups()
        {
            return matchups;
        }

    }
}
