using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BoltonCup.Api
{
    using StatType = ScoresheetGame;
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

        private Dictionary<string, StatType?> stats;

        public void AddScoresheet(string gameTitle, StatType stat)
        {
            stats[gameTitle] = stat;
            SaveData();
        }

        public Dictionary<string, StatType?> Stats()
        {
            return stats;
        }
        public StatType? Stats(string key)
        {
            if (!stats.ContainsKey(key)) return null;
            return stats[key];
        }

        private void SaveData()
        {
            string json = JsonSerializer.Serialize(stats);
            File.WriteAllText(FILE_PATH, json);
        }

        private Dictionary<string, StatType?> SavedStats()
        {

            try
            {
                using (StreamReader reader = new StreamReader(FILE_PATH))
                {
                    string json = reader.ReadToEnd();
                    var res = JsonSerializer.Deserialize<Dictionary<string, StatType?>>(json) ?? new();
                    return res;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("SavedStats:\nCouldn't parse data");
                return new();
            }
        }


    }
}