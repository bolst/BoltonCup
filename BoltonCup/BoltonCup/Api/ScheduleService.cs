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
            var ti = TeamService.Instance();
            var ft = (string n) => new TeamData
            {
                Id = 0,
                Name = n,
                Logo = "logo.png",
                Players = new(),
            };
            var res = new List<Matchup>
            {
                new Matchup("Game 1", ti.GetTeamByID("1"), ti.GetTeamByID("4"), new DateTime(2024,08,02,05 + 12 ,30,00)),
                new Matchup("Game 2", ti.GetTeamByID("2"), ti.GetTeamByID("3"), new DateTime(2024,08,02,06 + 12 ,30,00)),
                new Matchup("Game 3", ti.GetTeamByID("1"), ti.GetTeamByID("2"), new DateTime(2024,08,03,03 + 12 ,30,00)),
                new Matchup("Game 4", ti.GetTeamByID("3"), ti.GetTeamByID("4"), new DateTime(2024,08,03,04 + 12 ,30,00)),
                new Matchup("Game 5", ti.GetTeamByID("1"), ti.GetTeamByID("3"), new DateTime(2024,08,03,05 + 12 ,30,00)),
                new Matchup("Game 6", ti.GetTeamByID("4"), ti.GetTeamByID("2"), new DateTime(2024,08,03,06 + 12 ,30,00)),
                new Matchup("Finals (Bronze)", ft("Finals (3rd)"), ft("Finals (4th)"), new DateTime(2024,08,04,01 + 12 ,00,00)),
                new Matchup("Finals (Gold)", ft("Finals (1st)"), ft("Finals (2nd)"), new DateTime(2024,08,04,02 + 12 ,30,00)),
            };
            return res;
        }

        public List<Matchup> Matchups()
        {
            return matchups;
        }

    }
}