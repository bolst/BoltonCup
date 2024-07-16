using System.Text.Json.Serialization;

namespace BoltonCup.Data
{
    public class Matchup
    {
        public string Title { get; set; }
        public TeamData? HomeTeam { get; set; }
        public TeamData? AwayTeam { get; set; }
        public DateTime Date { get; set; }

        public Matchup(string title, TeamData homeTeam, TeamData awayTeam, DateTime date)
        {
            Title = title;
            HomeTeam = homeTeam;
            AwayTeam = awayTeam;
            Date = date;
        }
    }
}