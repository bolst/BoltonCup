using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BoltonCup.WebAPI.Swagger;

// Scalar uses the OpenAPI `summary` field as the operation title, which replaces the endpoint path.
// Moving the summary to `description` lets Scalar fall back to showing the HTTP method + path as
// the title while still surfacing the text as the operation description.
public sealed class SummaryToDescriptionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (string.IsNullOrEmpty(operation.Summary))
            return;

        operation.Description = string.IsNullOrEmpty(operation.Description)
            ? operation.Summary
            : $"{operation.Summary}\n\n{operation.Description}";

        operation.Summary = null;
    }
}
