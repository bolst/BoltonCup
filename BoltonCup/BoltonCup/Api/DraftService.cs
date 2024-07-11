using Blazorise.Extensions;

namespace BoltonCup.Api
{
    public class DraftService
    {
        public readonly static string DRAFT_KEY = Environment.GetEnvironmentVariable("BC_DRAFT_KEY") ?? "";

        private List<DraftPlayer> draftPlayers;

        int round;
        int pick;

        public DraftService(string strCsvData)
        {
            var lines = strCsvData.Split(Environment.NewLine).ToList();
            lines.RemoveAt(0); // headers
            lines.RemoveAt(lines.Count - 1); // last empty entry

            draftPlayers = new List<DraftPlayer>();
            foreach (var line in lines)
            {
                var columns = line.Split(',');
                DraftPlayer player = new()
                {
                    Team = "",
                    Name = columns[6],
                    DOB = columns[7],
                    Position = columns[8],
                    HighestLvl = columns[9],
                    CanPlayGame1 = columns[10],
                    CanPlayGame2 = columns[11],
                    CanPlayGame3 = columns[12],
                    CanPlayGame4 = columns[13],
                    PrefBeer = columns[14],
                };
                draftPlayers.Add(player);
            }

            round = 1;
            pick = 1;
        }

        public int GetCurrentRound() { return round; }
        public int GetCurrentPick() { return pick; }
        public TeamData GetCurrentTeam()
        {
            return TeamService.Instance().GetTeamByID(pick.ToString())!;
        }

        public TeamData GetCurrentTeamSnake()
        {
            if (round % 2 == 0)
            {
                int p = 5 - pick;
                return TeamService.Instance().GetTeamByID(p.ToString())!;
            }
            else
            {
                return GetCurrentTeam();
            }
        }

        public List<DraftPlayer> GetPlayers() { return draftPlayers; }

        public List<DraftPlayer> GetAvailablePlayers()
        {
            return draftPlayers.Where(x => x.Team.IsNullOrEmpty()).ToList();
        }

        public void MakePick(string playerName)
        {
            // TODO:
        }

    }

}