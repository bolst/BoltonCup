using System.Text.Json;

namespace BoltonCup.Shared.Data;

public class PlayerProfile : IEquatable<PlayerProfile>
{
    public int id { get; set; }
    public required string name { get; set; }
    public DateTime dob { get; set; }
    public string position { get; set; } = "";
    public int? jersey_number { get; set; }
    public int? account_id { get; set; }
    public int? team_id { get; set; }
    public bool champion { get; set; }
    public int tournament_id { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? Birthday { get; set; }
    public string? HighestLevel { get; set; }
    public string? ProfilePicture { get; set; }
    
    public string? availabilities { get; set; }

    private IEnumerable<KeyValuePair<DateTime, string>>? _availabilities;
    public IEnumerable<KeyValuePair<DateTime, string>> Availabilities
    {
        get
        {
            if (_availabilities is not null)
            {
                return _availabilities;
            }
            
            if (string.IsNullOrEmpty(availabilities))
            {
                _availabilities = [];
                return [];
            }
            
            var pairs = JsonSerializer.Deserialize<Dictionary<string, string>>(availabilities);
            if (pairs?.Count == 1)
            {
                _availabilities = [];
                return [];
            }

            var retval = pairs
                .Where(x => DateTime.TryParse(x.Key, out _))
                .Select(pair =>
                {
                    var date = DateTime.Parse(pair.Key).AddHours(4); // for EST cause Im lazy
                    return new KeyValuePair<DateTime, string>(date, pair.Value);
                });

            _availabilities = retval.OrderBy(x => x.Key);
            return _availabilities;
        }
    }
    public bool IsForward => position == "forward";
    public bool IsDefense => position == "defense";
    public bool IsGoalie => position == "goalie";
    
    
    #region IEquatable
    
    public bool Equals(PlayerProfile? other) => other is not null && other.id == id;
    public override bool Equals(object? obj) => Equals(obj as PlayerProfile);
    public override int GetHashCode() => id.GetHashCode();
    public static bool operator == (PlayerProfile? left, PlayerProfile? right) => left is null ? right is null : left.Equals(right);
    
    public static bool operator != (PlayerProfile? left, PlayerProfile? right) => left is null ? right is not null : !left.Equals(right);

    #endregion
}