using System.Text.Json;

namespace BoltonCup.Api
{
    public class SpotlightService
    {
        private static SpotlightService? instance = null;
        public static SpotlightService Instance()
        {
            if (instance is null)
            {
                instance = new SpotlightService();
            }
            return instance;
        }

        private const string FILE_PATH = "wwwroot/spotlight.json";

        public SpotlightService()
        {
            Spotlights = GetSavedSpotlights();
            RandomizeSpotlights();
        }

        private List<Spotlight> GetSavedSpotlights()
        {
            List<Spotlight> res = new();
            try
            {
                using (var stream = new StreamReader(FILE_PATH))
                {
                    string fileContent = stream.ReadToEnd();
                    res = JsonSerializer.Deserialize<List<Spotlight>>(fileContent);
                    if (res is null)
                    {
                        res = new();
                    }
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine("GetSavedSpotlights:");
                Console.WriteLine(exc.ToString());
            }

            return res;
        }

        public List<Spotlight> Spotlights { get; }

        private static Random rng = new Random();
        private void RandomizeSpotlights()
        {
            int n = Spotlights.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Spotlight val = Spotlights[k];
                Spotlights[k] = Spotlights[n];
                Spotlights[n] = val;
            }
        }



    }



}

