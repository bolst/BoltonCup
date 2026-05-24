using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps <see cref="InfoGuide"/> entities to DTOs and queries.</summary>
public interface IInfoGuideMapper
{
    /// <summary>Maps a <see cref="GetInfoGuidesRequest"/> to a <see cref="GetInfoGuidesQuery"/>.</summary>
    GetInfoGuidesQuery ToQuery(GetInfoGuidesRequest request);

    /// <summary>Maps a paged list of <see cref="InfoGuide"/> entities to a paged list of <see cref="InfoGuideDto"/>.</summary>
    IPagedList<InfoGuideDto> ToDtoList(IPagedList<InfoGuide> guides);

    /// <summary>Maps an <see cref="InfoGuide"/> to an <see cref="InfoGuideSingleDto"/>, or returns <see langword="null"/> if the guide is null.</summary>
    InfoGuideSingleDto? ToDto(InfoGuide? guide);
}

/// <summary>Maps <see cref="InfoGuide"/> entities to DTOs and queries.</summary>
public class InfoGuideMapper(IBriefMapper _briefMapper) : IInfoGuideMapper
{
    /// <inheritdoc/>
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
    
    /// <inheritdoc/>
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
    
    
    /// <inheritdoc/>
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
