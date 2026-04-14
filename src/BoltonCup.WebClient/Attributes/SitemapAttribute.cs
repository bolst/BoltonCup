namespace BoltonCup.WebClient.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class SitemapAttribute : Attribute
{
    public string? Location { get; }
    public string? LastModified { get; init; }
    public SitemapChangeFrequency? ChangeFrequency { get; init; }

    private readonly float _priority;
    public float Priority
    {
        get => _priority;
        init
        {
            if (value is < 0.0f or > 1.0f)
                throw new ArgumentException("Sitemap priority must be between 0.0 and 1.0");
            _priority = value;
        }
    }

    public SitemapAttribute(string? location, SitemapChangeFrequency changeFrequency)
        : this(location)
    {
        ChangeFrequency = changeFrequency;
    }

    public SitemapAttribute(SitemapChangeFrequency changeFrequency) 
        : this(null, changeFrequency) { }

    public SitemapAttribute(string? location)
    {
        Location = location;
        Priority = 0.5f;
        ChangeFrequency = null;
        LastModified = null;
    }
}
