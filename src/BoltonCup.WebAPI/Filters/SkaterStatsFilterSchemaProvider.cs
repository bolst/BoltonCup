using BoltonCup.WebAPI.Controllers;
using BoltonCup.WebAPI.Mapping;

namespace BoltonCup.WebAPI.Filters;

public sealed class SkaterStatsFilterSchemaProvider(LinkGenerator _links, IHttpContextAccessor _http)
{
    public FilterSchemaDto GetSchema()
    {
        return new FilterSchemaDto
        {
            PrimaryFields =
            [
                new FilterFieldDto
                {
                    FieldName = nameof(GetSkaterStatsRequest.TournamentId),
                    Label = "Tournament",
                    Type = FilterFieldType.SingleSelect,
                    Required = true,
                    OptionsSource = new FilterOptionsSourceDto
                    {
                        SourceType = FilterOptionsSourceType.Endpoint,
                        EndpointPath = ResolveEndpoint<TournamentsController>(nameof(TournamentsController.GetTournaments))
                    }
                },
                new FilterFieldDto
                {
                    FieldName = nameof(GetSkaterStatsRequest.Position),
                    Label = "Position",
                    Type = FilterFieldType.MultiSelect,
                    Required = false,
                    OptionsSource = new FilterOptionsSourceDto
                    {
                        SourceType = FilterOptionsSourceType.Static,
                        StaticOptions =
                        [
                            new FilterOptionDto(Core.Values.Position.Forward,"Forward"),
                            new FilterOptionDto(Core.Values.Position.Defense, "Defense"),
                        ]
                    }
                }
            ],
            SecondaryFields =
            [
                new FilterFieldDto
                {
                    FieldName = nameof(GetSkaterStatsRequest.TeamIds),
                    Label = "Team",
                    Type = FilterFieldType.MultiSelect,
                    Required = false,
                    OptionsSource = new FilterOptionsSourceDto
                    {
                        SourceType = FilterOptionsSourceType.Endpoint,
                        EndpointPath = ResolveEndpoint<TeamsController>(nameof(TeamsController.GetTeams))
                    }
                }
            ]
        };
    }

    private string ResolveEndpoint<T>(string action)
        where T : BoltonCupControllerBase
    {
        var context = _http.HttpContext!;
        return _links.GetPathByAction(context, action, GetControllerName<T>())
               ?? throw new ArgumentException(action);
    }

    private static string GetControllerName<T>() 
        where T : BoltonCupControllerBase
    {
        return typeof(T).Name.Replace("Controller", string.Empty);
    }
}