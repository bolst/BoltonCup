namespace BoltonCup.Shared.Data;

public class BCRefreshToken
{
    public int id { get; set; }
    public DateTime created_at { get; set; }
    public required string refresh { get; set; }
    public required Guid local_id { get; set; }
    public required string provider { get; set; }
    public string? access { get; set; }
}