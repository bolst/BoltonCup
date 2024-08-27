using System.Text.Json.Serialization;

namespace BoltonCup.Models
{
    public class PlayerStat
    {
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int PIMs { get; set; }
        public PlayerStat(int goals, int assists, int pims)
        {
            Goals = goals;
            Assists = assists;
            PIMs = pims;
        }
    }
}