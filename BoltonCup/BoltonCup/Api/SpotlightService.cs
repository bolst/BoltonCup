namespace BoltonCup.Api{
    public class SpotlightService{
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
            new Spotlight("ConnorKelly_Profile.jpg", "Connor Kelly", "Tecumseh, ON", "21", "OV"),
            new Spotlight("LandonPrince_Profile.jpg", "Landon Prince", "Tecumseh, ON", "19", "Bud light"),
            new Spotlight("LiamPrince_Profile.jpg", "Liam Prince", "Tecumseh, ON", "20", "Bud light"),
            new Spotlight("TylerFuhr_Profile.jpg", "Tyler Fuhr", "Tecumseh, ON", "20", "Russian Stouts"),
            new Spotlight("ChrisBolton_Profile.jpg", "Chris Bolton", "Tecumseh, ON", "20", "Coors Banquet"),
            new Spotlight("NicBolton_Profile.jpg", "Nic Bolton", "Tecumseh, ON", "22", "Miller Lite"),
            new Spotlight("ColtonKrz_Profile.jpg", "Colton Krzeminski", "Tecumseh, ON", "23", "Water"),
            new Spotlight("RyanSterling_Profile.jpg", "Ryan Sterling", "Windsor, ON", "23", "Coors"),
            new Spotlight("MatteoFrat_Profile.jpg", "Matteo Fratteroli", "Tecumseh, ON", "21", "Bud light"),
            new Spotlight("BrandonLeblanc_Profile.jpg", "Brandon Leblanc", "Lasalle, ON","20" , "OV"),
            new Spotlight("LiamGodwin_Profile.jpg", "Liam Godwin", "Tecumseh, ON", "19", "Miller Lite")



            
        };
    }

    public List<Spotlight> Spotlights {get;}

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

