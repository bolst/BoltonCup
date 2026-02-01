using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record InfoGuideSingleDetailDto : InfoGuideDetailDto, IMappable<InfoGuide, InfoGuideSingleDetailDto>
{
    public string? MarkdownContent { get; set; }


    static Expression<Func<InfoGuide, InfoGuideSingleDetailDto>> IMappable<InfoGuide, InfoGuideSingleDetailDto>.Projection =>
        infoGuide => new InfoGuideSingleDetailDto
        {
            Id = infoGuide.Id,
            Title = infoGuide.Title,
            TournamentId = infoGuide.TournamentId,
            Tournament = infoGuide.Tournament == null ? null : new TournamentSummary(infoGuide.Tournament),
            MarkdownContent = infoGuide.MarkdownContent,
        };
}

