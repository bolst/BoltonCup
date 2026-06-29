using BoltonCup.Infrastructure;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.Shared;
using BoltonCup.WebAPI;
using BoltonCup.WebAPI.Auth;
using BoltonCup.WebAPI.Controllers;
using BoltonCup.WebAPI.Hubs;
using BoltonCup.WebAPI.Swagger;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add Sentry
builder.WebHost.UseSentry();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AuthDbContext>()
    .SetApplicationName("BoltonCup.SharedAuth");

builder.Services.AddIdentityApiEndpoints<BoltonCupUser>();

builder
    .AddBoltonCupInfrastructure()
    .AddBoltonCupAssetUrlResolver();
builder.AddBoltonCupWebAPIServices();

builder.Services.AddResponseCaching();

// https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.UseAllOfToExtendReferenceSchemas();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bolton Cup", Version = "v1", Description = "Bolton Cup API"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "Bearer", BearerFormat = "JWT",
        In = ParameterLocation.Header, Description = "Enter access token below."
    });
    options.AddSecurityDefinition(ApiKeyConstants.Scheme, new OpenApiSecurityScheme
    {
        Name = ApiKeyConstants.Header, Type = SecuritySchemeType.ApiKey, Scheme = ApiKeyConstants.Scheme, In = ParameterLocation.Header,
        Description = "Enter your API key below."
    });

    options.OperationFilter<ProblemDetailsOperationFilter>();
    options.OperationFilter<SecurityRequirementsOperationFilter>();
    options.DocumentFilter<SignalRSchemaFilter>();

    // generate operation IDs based on method names
    options.CustomOperationIds(description =>
    {
        if (description.TryGetMethodInfo(out var methodInfo))
        {
            var type = methodInfo.DeclaringType;
            if (type != null && typeof(BoltonCupControllerBase).IsAssignableFrom(type))
                return methodInfo.Name;
        }
        return null;
    });

    // add docstrings — must be registered before SummaryToDescriptionOperationFilter
    // so XML comments are applied before that filter moves summary to description
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    options.OperationFilter<SummaryToDescriptionOperationFilter>();
});

var app = builder.Build();

await app.Services.InitializeDbAsync(app.Configuration);

// Configure the HTTP request pipeline.
if (true) //(app.Environment.IsDevelopment())
{
    var defaultApiKey = app.Configuration[ApiKeyConstants.AppSettingsPath] ?? string.Empty;
    app.MapSwagger("/openapi/{documentName}.json");
    app.MapScalarApiReference("/docs", options => options
        .WithTitle("Bolton Cup API Documentation")
        .WithTheme(ScalarTheme.Purple)
        .AddPreferredSecuritySchemes(ApiKeyConstants.Scheme)
        .AddApiKeyAuthentication(ApiKeyConstants.Scheme, scheme => scheme
            .WithName(ApiKeyConstants.Header)
            .WithValue(defaultApiKey)
        )
    );
}

app.UseHttpsRedirection();

// Capture the true socket peer before UseForwardedHeaders rewrites RemoteIpAddress, so the
// admin-API-key network check (ApiKeyAuthenticationHandler) can't be spoofed via X-Forwarded-For.
app.Use(async (context, next) =>
{
    context.Items[ApiKeyConstants.TrueRemoteIpItemKey] = context.Connection.RemoteIpAddress;
    await next();
});

app.UseForwardedHeaders();
app.UseCors();
app.UseRateLimiter();

app.UseExceptionHandler();

app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// for now
app.MapGet("/", context =>
{
    context.Response.Redirect("/docs");
    return Task.CompletedTask;
});

app.MapHub<DraftHub>(Hubs.Draft);

app.MapGet("/health", () => Results.Ok(new
    {
        status = "healthy"
    }))
    .AllowAnonymous();

// Sentry
app.UseSentryTracing();

app.Run();