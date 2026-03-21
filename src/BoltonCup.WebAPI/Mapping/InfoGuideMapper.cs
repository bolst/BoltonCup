using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface IInfoGuideMapper
{
    IPagedList<InfoGuideDto> ToDtoList(IPagedList<InfoGuide> guides);
    InfoGuideSingleDto? ToDto(InfoGuide? guide);
}

public class InfoGuideMapper(IAssetUrlResolver _urlResolver, IBriefMapper _briefMapper) : IInfoGuideMapper
{
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
