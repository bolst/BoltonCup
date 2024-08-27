using System.Text;

namespace BoltonCup.Models
{
    public class DraftPlayer
    {
        public string? Team { get; set; }
        public string? Name { get; set; }
        public string? DOB { get; set; }
        public string? Position { get; set; }
        public string? HighestLvl { get; set; }
        public string? CanPlayGame1 { get; set; }
        public string? CanPlayGame2 { get; set; }
        public string? CanPlayGame3 { get; set; }
        public string? CanPlayGame4 { get; set; }
        public string? PrefBeer { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Name: {Name}");
            sb.AppendLine($"D.O.B: {DOB}");
            sb.AppendLine($"Position: {Position}");
            sb.AppendLine($"Highest Level Played: {HighestLvl}");
            sb.AppendLine($"Can Play Game 1: {CanPlayGame1}");
            sb.AppendLine($"Can Play Game 2: {CanPlayGame2}");
            sb.AppendLine($"Can Play Game 3: {CanPlayGame3}");
            sb.AppendLine($"Can Play Game 4: {CanPlayGame4}");
            sb.AppendLine($"Preferred Beer: {PrefBeer}");
            return sb.ToString();
        }

        public TeamPlayer ToTeamPlayer()
        {
            return new TeamPlayer()
            {
                Name = this.Name,
                BirthYear = this.DOB!.Substring(0, 4),
                Position = this.Position,
            };
        }

        public string PosAbbrev()
        {
            return Position!.Contains("/") ? "F/D" : Position.First().ToString();
        }

    }
}