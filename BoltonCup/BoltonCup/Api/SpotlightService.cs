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

        public SpotlightService()
        {
            Spotlights = GenerateSpotlights();
            RandomizeSpotlights();
        }

        public List<Spotlight> GenerateSpotlights()
        {
            return new()
        {
            new Spotlight("spotlight/ConnorKelly_Profile.webp", "Connor Kelly", "Tecumseh, ON", "21", "OV"),
            new Spotlight("https://lscluster.hockeytech.com/download.php?client_code=pjhlon&file_path=media/ee1f503644bd34dc0f96f4690375e78c.png", "Landon Prince", "Tecumseh, ON", "19", "Bud light"),
            new Spotlight("spotlight/LiamPrince_Profile.webp", "Liam Prince", "Tecumseh, ON", "20", "Bud light"),
            new Spotlight("spotlight/TylerFuhr_Profile.webp", "Tyler Fuhr", "Tecumseh, ON", "20", "Russian Stouts"),
            new Spotlight("spotlight/ChrisBolton_Profile.webp", "Chris Bolton", "Tecumseh, ON", "20", "Coors Banquet"),
            new Spotlight("spotlight/NicBolton_Profile.webp", "Nic Bolton", "Tecumseh, ON", "22", "Miller Lite"),
            new Spotlight("spotlight/ColtonKrz_Profile.webp", "Colton Krzeminski", "Tecumseh, ON", "23", "Water"),
            new Spotlight("spotlight/RyanSterling_Profile.webp", "Ryan Sterling", "Windsor, ON", "23", "Coors"),
            new Spotlight("spotlight/MatteoFrat_Profile.webp", "Matteo Frattaroli", "Tecumseh, ON", "21", "Bud light"),
            new Spotlight("spotlight/BrandonLeblanc_Profile.webp", "Brandon Leblanc", "Lasalle, ON","20" , "OV"),
            new Spotlight("spotlight/LiamGodwin_Profile.webp", "Liam Godwin", "Tecumseh, ON", "19", "Miller Lite")
        };
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

