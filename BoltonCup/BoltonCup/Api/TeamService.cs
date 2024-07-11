using System.Text.Json;

namespace BoltonCup.Api
{
    public class TeamService
    {
        private static TeamService? instance = null;
        public TeamService()
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
        public static TeamService Instance()
        {
            if (instance == null) { instance = new TeamService(); }
            return instance;
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

        public void AddPlayerToTeam(int id, PlayerData player)
        {
            if (id < TeamDataList.Count) TeamDataList[id].Players.Add(player);
        }
    }
}