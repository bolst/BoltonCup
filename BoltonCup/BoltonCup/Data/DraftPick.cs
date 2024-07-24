namespace BoltonCup.Data
{
    public class DraftPick
    {

        public DraftPlayer Player { get; }
        public TeamData Team { get; }
        private int Round;
        private int Pick;
        public string BgColor { get; }
        public Background Bg { get; }
        public DraftPick(DraftPlayer player, TeamData team, int round, int pick)
        {
            Player = player;
            Team = team;
            Round = round;
            Pick = pick;
            switch (team.Name)
            {
                case "Just the Tip":
                    BgColor = "#294a7d";
                    Bg = Background.Primary;
                    break;
                case "Nipple Ticklers":
                    BgColor = "#7d3232";
                    Bg = Background.Secondary;
                    break;
                case "Tecumseh Titty Twisters":
                    BgColor = "#a16037";
                    Bg = Background.Warning;
                    break;
                case "South West Sausages":
                    BgColor = "#998937";
                    Bg = Background.Info;
                    break;
                default:
                    BgColor = "#f6f6f6";
                    Bg = Background.Light;
                    break;
            }
        }

        public int GetOverallPick()
        {
            return (Round - 1) * 4 + Pick;
        }
    }
}