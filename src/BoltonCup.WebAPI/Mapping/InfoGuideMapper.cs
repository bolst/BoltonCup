using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface IInfoGuideMapper
{
    GetInfoGuidesQuery ToQuery(GetInfoGuidesRequest request);
    IPagedList<InfoGuideDto> ToDtoList(IPagedList<InfoGuide> guides);
    InfoGuideSingleDto? ToDto(InfoGuide? guide);
}

public class InfoGuideMapper(IAssetUrlResolver _urlResolver, IBriefMapper _briefMapper) : IInfoGuideMapper
{
    public GetInfoGuidesQuery ToQuery(GetInfoGuidesRequest request)
    {
        return new GetInfoGuidesQuery
        {
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }
    
    public IPagedList<InfoGuideDto> ToDtoList(IPagedList<InfoGuide> guides)
    {
        return guides.ProjectTo(guide => new InfoGuideDto
        {
            Id = guide.Id,
            Title = guide.Title,
            TournamentId = guide.TournamentId,
            Tournament = guide.Tournament == null ? null : _briefMapper.ToTournamentBriefDto(guide.Tournament),
        });
    }    
    
    
    public InfoGuideSingleDto? ToDto(InfoGuide? guide)
    {
        return guide is null
            ? null
            : new InfoGuideSingleDto 
            { 
                Id = guide.Id,
                Title = guide.Title,
                TournamentId = guide.TournamentId,
                Tournament = guide.Tournament == null ? null : _briefMapper.ToTournamentBriefDto(guide.Tournament),
                MarkdownContent = guide.MarkdownContent
            };
    }
}
