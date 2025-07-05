namespace BoltonCup.Shared.Data;

public class BCSponsor
{
    public int id { get; set; }
    public required string name { get; set; }
    public bool is_active { get; set; }
    public string site_url { get; set; }
    public string logo_url { get; set; }
}