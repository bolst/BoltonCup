using System.Text.Json.Serialization;

namespace BoltonCup.Models
{
    public class Spotlight
    {
        [JsonPropertyName("image")] public string Image { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("hometown")] public string Hometown { get; set; }
        [JsonPropertyName("age")] public string Age { get; set; }
        [JsonPropertyName("favourite beer")] public string FavBeer { get; set; }

        [JsonConstructor] public Spotlight() { }

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