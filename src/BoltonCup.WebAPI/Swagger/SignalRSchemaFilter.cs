using BoltonCup.WebAPI.Mapping;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BoltonCup.WebAPI.Swagger;

// let there be SignalR models in the OpenAPI spec

public sealed class SignalRSchemaFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        context.SchemaGenerator.GenerateSchema(typeof(DraftUpdateEventDto), context.SchemaRepository);
        context.SchemaGenerator.GenerateSchema(typeof(DraftPickMadeEventDto), context.SchemaRepository);
    }
}