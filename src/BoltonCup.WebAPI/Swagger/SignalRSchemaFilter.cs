using BoltonCup.WebAPI.Mapping;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BoltonCup.WebAPI.Swagger;

// let there be SignalR models in the OpenAPI spec
/// <summary>Registers SignalR hub event DTOs in the OpenAPI schema so they appear in the generated spec.</summary>
public sealed class SignalRSchemaFilter : IDocumentFilter
{
    /// <summary>Applies the filter, generating schemas for SignalR event types.</summary>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        context.SchemaGenerator.GenerateSchema(typeof(DraftUpdateEventDto), context.SchemaRepository);
        context.SchemaGenerator.GenerateSchema(typeof(DraftPickMadeEventDto), context.SchemaRepository);
    }
}