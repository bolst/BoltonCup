using BoltonCup.WebAPI.Errors;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BoltonCup.WebAPI.Filters;

// Let there be exception response schemas in the OpenAPI spec.

public sealed class ProblemDetailsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses ??= new OpenApiResponses();
        
        // Let there be a schema for validation errors
        var validationSchema = context.SchemaGenerator
            .GenerateSchema(typeof(BoltonCupValidationProblemDetails), context.SchemaRepository);
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses["400"] = new OpenApiResponse
            {
                Description = "Validation Error",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new() { Schema = validationSchema }
                }
            };
        }

        // Let there be a single schema for every defined exception AND rate limiting.
        var problemSchema = context.SchemaGenerator
            .GenerateSchema(typeof(BoltonCupProblemDetails), context.SchemaRepository);

        if (!operation.Responses.ContainsKey("429"))
        {
            operation.Responses["429"] = new OpenApiResponse
            {
                Description = "Too Many Requests",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new() { Schema = problemSchema }
                }
            };
        }
        
        BoltonCupExceptionMappings.Values
            .Select(m => m.StatusCode.ToString())
            .Distinct()
            .Where(statusCode => !operation.Responses.ContainsKey(statusCode))
            .ToList()
            .ForEach(statusCode =>
            {
                operation.Responses?[statusCode] = new OpenApiResponse
                {
                    Description = "Error",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/problem+json"] = new() { Schema = problemSchema }
                    }
                };
            });
    }
}