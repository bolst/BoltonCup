using System.Text.Json;

namespace BoltonCup.Api
{
    public static class TeamService
    {
        public static TeamData? GetTeamByID(string id)
        {
            try
            {
                using (var stream = new StreamReader($"wwwroot/team{id}.json"))
                {
                    string fileContent = stream.ReadToEnd();
                    TeamData? data = JsonSerializer.Deserialize<TeamData>(fileContent);
                    return data;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("GetTeamByID:");
                Console.WriteLine(exc.ToString());
                return null;
            }
        }
    }
}