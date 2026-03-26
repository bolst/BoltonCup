using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi;

namespace BoltonCup.WebAPI.Filters;

public class GlobalBadRequestOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var errorSchema = context.SchemaGenerator.GenerateSchema(typeof(ApiErrorResponse), context.SchemaRepository);

        var badRequestResponse = new OpenApiResponse
        {
            Description = "Bad Request (Validation Errors)",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new() { Schema = errorSchema }
            }
        };

        operation.Responses?["400"] = badRequestResponse;
    }
}

public class ApiErrorResponse
{
    public required string Message { get; set; }

    // field name -> list of errors
    public Dictionary<string, string[]> Errors { get; set; } = [];
}