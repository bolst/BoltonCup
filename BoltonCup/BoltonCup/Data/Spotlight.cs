namespace BoltonCup.Data
{
    public class Spotlight
    {
        public string Image {get;}
        public string Name {get;}
        public string Hometown {get;}
        public string Age {get;}
        public string FavBeer {get;}

        public Spotlight(string image, string name, string ht, string age, string beer)
        {
            Image = image;
            Name = name;
            Hometown = ht;
            Age = age;
            FavBeer = beer;
        }
    }
}