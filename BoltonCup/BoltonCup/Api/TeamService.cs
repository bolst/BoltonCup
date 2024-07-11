using System.Text.Json;

namespace BoltonCup.Api
{
    public class TeamService
    {
        private static TeamService? instance = null;
        public TeamService() { Fetch(); }
        public static TeamService Instance()
        {
            if (instance == null) { instance = new TeamService(); }
            return instance;
        }

        public void Fetch()
        {
            TeamDataList = new();
            try
            {
                for (int id = 1; id <= 4; id++)
                {
                    using (var stream = new StreamReader($"wwwroot/team{id}.json"))
                    {
                        string fileContent = stream.ReadToEnd();
                        TeamData? data = JsonSerializer.Deserialize<TeamData>(fileContent);
                        if (data != null) { TeamDataList.Add(data); }
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
            if (id <= TeamDataList.Count) TeamDataList[id - 1].Players.Add(player);
        }

        public void Save()
        {
            foreach (var team in TeamDataList)
            {
                string json = JsonSerializer.Serialize(team);
                File.WriteAllText($"wwwroot/team{team.Id}.json", json);
            }
        }
    }
}