using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetTournamentsQuery : QueryBase
{
    public bool? RegistrationOpen { get; set; }
}