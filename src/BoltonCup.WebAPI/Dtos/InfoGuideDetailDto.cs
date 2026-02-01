using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record InfoGuideDetailDto : IMappable<InfoGuide, InfoGuideDetailDto>
{
    public required Guid Id { get; init; }
    public string? Title { get; set; }
    public int? TournamentId { get; set; }
    public TournamentSummary? Tournament { get; init; }


    static Expression<Func<InfoGuide, InfoGuideDetailDto>> IMappable<InfoGuide, InfoGuideDetailDto>.Projection =>
        infoGuide => new InfoGuideDetailDto
        {
            Id = infoGuide.Id,
            Title = infoGuide.Title,
            TournamentId = infoGuide.TournamentId,
            Tournament = infoGuide.Tournament == null ? null : new TournamentSummary(infoGuide.Tournament),
        };
}

