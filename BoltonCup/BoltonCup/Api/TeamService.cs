using System.Text.Json;

namespace BoltonCup.Api
{
    public class TeamService
    {
        private static TeamService? instance = null;
        public TeamService()
        {
            TeamDataList = new();
            Fetch();
        }
        public static TeamService Instance()
        {
            if (instance == null) { instance = new TeamService(); }
            return instance;
        }

        public void Fetch()
        {
            try
            {
                for (int id = 1; id <= 4; id++)
                {
                    using (var stream = new StreamReader($"wwwroot/team{id}.json"))
                    {
                        string fileContent = stream.ReadToEnd();
                        TeamData? data = JsonSerializer.Deserialize<TeamData>(fileContent);
                        if (data is not null) { TeamDataList.Add(data); }
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("TeamService:");
                Console.WriteLine(exc.ToString());
            }
        }

        private List<TeamData> TeamDataList { get; set; }

        public TeamData? GetTeamByID(string id)
        {
            int intId = int.Parse(id);
            return TeamDataList[intId - 1];
        }

        public List<TeamData> GetTeams()
        {
            return TeamDataList;
        }

        public void AddPlayerToTeam(int id, TeamPlayer player)
        {
            if (id <= TeamDataList.Count) TeamDataList[id - 1].Players!.Add(player);
        }

        public void Save()
        {
            foreach (var team in TeamDataList)
            {
                string json = JsonSerializer.Serialize(team);
                File.WriteAllText($"wwwroot/team{team.Id}.json", json);
            }
        }

        public TeamData? GetTeamByName(string name)
        {
            return TeamDataList.Where(t => t.Name == name).First();
        }

        public TeamPlayer? GetPlayerByNumber(string num, string teamName)
        {
            TeamData? team = GetTeamByName(teamName);
            if (team is null) return null;
            var players = team.Players!.Where(p => p.Number == num);
            if (players.Count() == 0) return new TeamPlayer();
            return players.First();
        }

        public List<TeamPlayer> GetPlayers()
        {
            List<TeamPlayer> res = new();
            foreach (var team in TeamDataList)
            {
                var players = team.Players;
                res.AddRange(players!);
            }
            return res;
        }

        public TeamPlayer? GetPlayerByName(string name)
        {
            var players = GetPlayers().Where(p => p.Name == name);
            if (players.Count() == 0) return new TeamPlayer();
            return players.First();
        }

        public TeamData GetPlayerTeam(TeamPlayer player)
        {
            return TeamDataList.Where(t => t.Players!.Any(p => p.Name == player.Name)).FirstOrDefault()!;
        }

        public static string TeamAbbrev(string teamName)
        {
            return string.Join("", teamName.Split(" ").Select(o => o.ToUpper().First()).ToList());
        }
    }
}