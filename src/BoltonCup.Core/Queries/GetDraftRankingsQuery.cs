using BoltonCup.Core.Queries.Base;
using BoltonCup.Core.Values;
using FluentValidation;

namespace BoltonCup.Core;

public sealed record GetDraftRankingsQuery : QueryBase
{
    public string? Position { get; set; }
    
    public int? TeamId { get; set; }
    
    public bool? IsDrafted { get; set; }
}


public class GetDraftRankingsQueryValidator : PaginationQueryValidator<GetDraftRankingsQuery>
{
    public GetDraftRankingsQueryValidator()
    {
        var positions = new List<string> { Position.Forward, Position.Defense, Position.Goalie };
        RuleFor(x => x.Position)
            .Must(x => positions.Contains(x!))
            .When(x => !string.IsNullOrEmpty(x.Position))
            .WithMessage("Position must be one of: " + string.Join(", ", positions));
    }
}