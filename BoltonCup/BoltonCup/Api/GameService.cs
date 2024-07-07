using System.Text.Json;

namespace BoltonCup.Api
{
    public class GameService
    {
        private static GameService? instance = null;
        public GameService() { FetchData(); }
        private void FetchData()
        {
            Scores = new();
            try
            {
                for (int id = 1; id <= 4; id++)
                {
                    using (var stream = new StreamReader($"wwwroot/scores.json"))
                    {
                        string fileContent = stream.ReadToEnd();
                        Scores = JsonSerializer.Deserialize<ScoresData>(fileContent) ?? new();
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("GameService:");
                Console.WriteLine(exc.ToString());
            }
        }
        public static GameService Instance()
        {
            if (instance == null) { instance = new GameService(); }
            return instance;
        }

        private ScoresData Scores { get; set; }

        public string GetScore(int index)
        {
            FetchData();
            return Scores.Scores!.ElementAt(index) ?? "";
        }

    }
}