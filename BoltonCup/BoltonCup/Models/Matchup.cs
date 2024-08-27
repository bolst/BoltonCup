namespace BoltonCup.Models
{
    public class Matchup
    {
        public string Title { get; set; }
        public TeamData? HomeTeam { get; set; }
        public TeamData? AwayTeam { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }

        public Matchup(string title, TeamData homeTeam, TeamData awayTeam, DateTime date, string location)
        {
            Title = title;
            HomeTeam = homeTeam;
            AwayTeam = awayTeam;
            Date = date;
            Location = location;
        }
    }
}