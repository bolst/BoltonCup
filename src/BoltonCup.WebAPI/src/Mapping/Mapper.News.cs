using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

#pragma warning disable CS1591 // Disable warning for missing XML comments

public partial class Mapper
{
    // ---------- News ----------

    public GetNewsPostsQuery ToQuery(GetNewsPostsRequest request) => new GetNewsPostsQuery
    {
        Label = request.Label,
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending,
    };

    public IPagedList<NewsPostDto> ToDtoList(IPagedList<NewsPost> posts) => posts.ProjectTo(post => new NewsPostDto
    {
        Id = post.Id,
        Title = post.Title,
        Slug = post.Slug,
        Summary = post.Summary,
        CoverImageUrl = _urlResolver.GetFullUrl(post.CoverImage),
        PublishedAt = post.PublishedAt,
        Tags = ToTagDtos(post.Tags),
    });

    public NewsPostSingleDto? ToDto(NewsPost? post) => post is null
            ? null
            : new NewsPostSingleDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Summary = post.Summary,
                CoverImageUrl = _urlResolver.GetFullUrl(post.CoverImage),
                PublishedAt = post.PublishedAt,
                Tags = ToTagDtos(post.Tags),
                MarkdownContent = post.MarkdownContent,
            };

    /// <summary>Resolves a tag to its target type and display name. Null when the row carries no single target.</summary>
    public TagDto? ToTagDto(EntityTag tag)
    {
        if (TagTargets.GetTargetType(tag) is not { } type)
        {
            return null;
        }

        var targetId = TagTargets.GetTargetId(tag, type) ?? 0;
        var name = type switch
        {
            TagTargetType.Label => tag.Label?.Name,
            TagTargetType.Team => tag.Team?.Name,
            TagTargetType.Tournament => tag.Tournament?.Name,
            TagTargetType.Account => tag.Account is null ? null : AccountName(tag.Account),
            TagTargetType.Game => tag.Game is { HomeTeam: not null, AwayTeam: not null } game ? $"{game.HomeTeam.Name} vs {game.AwayTeam.Name}" : null,
            _ => null,
        };

        return new TagDto
        {
            Id = tag.Id,
            Type = type,
            TargetId = targetId,
            // A navigation the query did not load still yields a stable, recognizable name.
            Name = string.IsNullOrWhiteSpace(name) ? $"{type} {targetId}" : name,
        };
    }

    IReadOnlyList<TagDto> ToTagDtos(IEnumerable<EntityTag> tags) => tags
        .Select(ToTagDto)
        .OfType<TagDto>()
        .OrderBy(t => t.Type)
        .ThenBy(t => t.Name)
        .ToList();
}
