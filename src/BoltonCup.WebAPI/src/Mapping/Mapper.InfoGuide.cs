using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- InfoGuide ----------

    public GetInfoGuidesQuery ToQuery(GetInfoGuidesRequest request) => new GetInfoGuidesQuery
    {
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending,
    };

    public IPagedList<InfoGuideDto> ToDtoList(IPagedList<InfoGuide> guides) => guides.ProjectTo(guide => new InfoGuideDto
    {
        Id = guide.Id,
        Title = guide.Title,
        TournamentId = guide.TournamentId,
        Tournament = guide.Tournament == null ? null : ToTournamentBriefDto(guide.Tournament),
    });

    public InfoGuideSingleDto? ToDto(InfoGuide? guide) => guide is null
            ? null
            : new InfoGuideSingleDto
            {
                Id = guide.Id,
                Title = guide.Title,
                TournamentId = guide.TournamentId,
                Tournament = guide.Tournament == null ? null : ToTournamentBriefDto(guide.Tournament),
                MarkdownContent = guide.MarkdownContent
            };
}
