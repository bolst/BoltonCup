using Blazorise.Extensions;

namespace BoltonCup.Api
{
    public class DraftService
    {
        public readonly static string DRAFT_KEY = Environment.GetEnvironmentVariable("BC_DRAFT_KEY") ?? "";

        public List<DraftPlayer> draftPlayers { get; }

        private int round;
        private int pick;

        public List<DraftPick> picks { get; }

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

            // adding empty player
            DraftPlayer emptyPlayer = new()
            {
                Team = "",
                Name = "",
                DOB = "0000-00-00",
                Position = "",
                HighestLvl = "",
                CanPlayGame1 = "",
                CanPlayGame2 = "",
                CanPlayGame3 = "",
                CanPlayGame4 = "",
                PrefBeer = "",
            };
            draftPlayers.Add(emptyPlayer);

            round = 0;
            pick = 1;
            picks = new();
        }

        public int GetCurrentRound() { return round; }
        public int GetCurrentPick() { return pick; }
        public TeamData GetCurrentTeam()
        {
            return TeamService.Instance().GetTeamByID(pick.ToString())!;
        }

        public TeamData GetCurrentTeamSnake()
        {
            if (round % 2 == 0 && round != 0)
            {
                int p = 5 - pick;
                return TeamService.Instance().GetTeamByID(p.ToString())!;
            }
            else
            {
                return GetCurrentTeam();
            }
        }

        public List<DraftPlayer> GetAvailablePlayers() { return draftPlayers.Where(x => x.Team.IsNullOrEmpty()).ToList(); }

        public void MakePick(TeamData team, string playerName)
        {
            int index = draftPlayers.FindIndex(x => x.Name == playerName);

            // if player was not found
            if (index == -1)
            {
                Console.WriteLine($"Player not found (index = {index})");
                return;
            }
            // if player was somehow already selected
            if (!draftPlayers[index].Team.IsNullOrEmpty())
            {
                Console.WriteLine($"Player {draftPlayers[index].Name} is already on {draftPlayers[index].Team}");
                return;
            }

            draftPlayers[index].Team = team.Name!;

            TeamService.Instance().AddPlayerToTeam(team.Id ?? 0, draftPlayers[index].ToTeamPlayer());

            Console.WriteLine(draftPlayers[index].Name + " is now on " + draftPlayers[index].Team);

            if (round != 0)
            {
                picks.Add(new DraftPick(draftPlayers[index], team, round, pick));
            }
            NextPick();
        }

        private void NextPick()
        {
            if (pick >= 4)
            {
                pick = 1;
                round += 1;
            }
            else
            {
                pick += 1;
            }
        }


    }

}