using System.Text.Json;

namespace BoltonCup.Api
{
    public class StatService
    {
        #region singleton
        private static StatService? instance = null;
        public static StatService Instance()
        {
            if (instance == null) { instance = new StatService(); }
            return instance;
        }
        #endregion
        private StatService()
        {
            stats = SavedStats();
        }
        private const string FILE_PATH = "wwwroot/game-stats.json";

        private Dictionary<string, ScoresheetGame?> stats;

        public void AddScoresheet(string gameTitle, ScoresheetGame stat)
        {
            stats[gameTitle] = stat;
            SaveData();
        }

        public Dictionary<string, ScoresheetGame?> Stats()
        {
            return stats;
        }
        public ScoresheetGame? Stats(string key)
        {
            if (!stats.ContainsKey(key)) return null;
            return stats[key];
        }

        private void SaveData()
        {
            string json = JsonSerializer.Serialize(stats);
            File.WriteAllText(FILE_PATH, json);
        }

        private Dictionary<string, ScoresheetGame?> SavedStats()
        {
            try
            {
                using (StreamReader reader = new StreamReader(FILE_PATH))
                {
                    string json = reader.ReadToEnd();
                    var res = JsonSerializer.Deserialize<Dictionary<string, ScoresheetGame?>>(json) ?? new();
                    return res;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("SavedStats:\nCouldn't parse data");
                return new();
            }
        }

        public List<ScoresheetGame?> TeamGames(string teamName)
        {
            return stats.Where(s => (s.Value.HomeTeam == teamName) || (s.Value.AwayTeam == teamName)).ToDictionary(s => s.Key, s => s.Value).Values.ToList();
        }

        public PlayerStat PlayerTotals(TeamPlayer player, TeamData team)
        {
            var games = TeamGames(team.Name);
            PlayerStat res = new(0, 0, 0);
            foreach (var game in games)
            {
                if (game.HomeTeam == team.Name)
                {
                    res.Goals += game.HomeGoals.Where(hg => hg.PlayerNumber == player.Number).Count();
                    res.Assists += game.HomeGoals.Where(hg => hg.Assist1 == player.Number || hg.Assist2 == player.Number).Count();
                    res.PIMs += game.HomePenalties.Where(hp => hp.PlayerNumber == player.Number).ToList().Sum(p =>
                    Convert.ToInt32(p.Minutes.Split(":")[0]));
                }
                else if (game.AwayTeam == team.Name)
                {
                    res.Goals += game.AwayGoals.Where(ag => ag.PlayerNumber == player.Number).Count();
                    res.Assists += game.AwayGoals.Where(ag => ag.Assist1 == player.Number || ag.Assist2 == player.Number).Count();
                    res.PIMs += game.AwayPenalties.Where(ap => ap.PlayerNumber == player.Number).ToList().Sum(p =>
                    Convert.ToInt32(p.Minutes.Split(":")[0]));
                }
            }
            return res;
        }

        public List<(TeamPlayer, PlayerStat)> UnorderedPlayerTotals()
        {
            List<(TeamPlayer, PlayerStat)> res = new(64);
            foreach (var team in Api.TeamService.Instance().GetTeams())
            {
                foreach (var player in team.Players)
                {
                    res.Add((player, PlayerTotals(player, team)));
                }
            }
            return res;
        }

        public List<(TeamPlayer, PlayerStat)> OrderedPlayerTotals(string? sortRule = null)
        {
            var unordered = UnorderedPlayerTotals();
            if (sortRule is null || sortRule == "points")
            {
                var ordered = unordered.OrderBy(s => s.Item2.Goals + s.Item2.Assists).ToList();
                ordered.Reverse();
                return ordered;
            }
            else if (sortRule == "goals")
            {
                var ordered = unordered.OrderBy(s => s.Item2.Goals).ToList();
                ordered.Reverse();
                return ordered;
            }
            else if (sortRule == "assists")
            {
                var ordered = unordered.OrderBy(s => s.Item2.Assists).ToList();
                ordered.Reverse();
                return ordered;
            }
            else if (sortRule == "pims")
            {
                var ordered = unordered.OrderBy(s => s.Item2.PIMs).ToList();
                ordered.Reverse();
                return ordered;
            }
            else
            {
                return unordered;
            }
        }

        public (int, int, int) GetTeamWLT(TeamData team)
        {
            int wins = 0;
            int losses = 0;
            int ties = 0;
            foreach (var stat in TeamGames(team.Name!))
            {
                int homeGoals = stat.HomeGoals.Count();
                int awayGoals = stat.AwayGoals.Count();

                if (stat.HomeTeam == team.Name)
                {
                    if (homeGoals > awayGoals) wins++;
                    else if (homeGoals < awayGoals) losses++;
                    else ties++;
                }
                else
                {
                    if (homeGoals > awayGoals) losses++;
                    else if (homeGoals < awayGoals) wins++;
                    else ties++;
                }
            }
            return (wins, losses, ties);
        }
    }
}