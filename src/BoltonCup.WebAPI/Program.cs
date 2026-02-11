using BoltonCup.Infrastructure;
using BoltonCup.WebAPI;
using BoltonCup.WebAPI.Authentication;
using BoltonCup.WebAPI.Controllers;
using BoltonCup.WebAPI.Filters;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

var keyDirectory = builder.Configuration["DataProtection:KeyDirectory"];
if (!string.IsNullOrEmpty(keyDirectory))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory))
        .SetApplicationName("BoltonCup.SharedAuth");
}

builder.Services.AddBoltonCupInfrastructure(builder.Configuration);

builder.Services.AddBoltonCupWebAPIServices(builder.Environment);

// https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bolton Cup",
        Version = "v1",
        Description = "Bolton Cup API"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter access token below."
    });
    options.AddSecurityDefinition(ApiKeyConstants.Scheme, new OpenApiSecurityScheme
    {
        Name = ApiKeyConstants.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = ApiKeyConstants.Scheme,
        In = ParameterLocation.Header,
        Description = "Enter your API key below."
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
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
    // add docstrings
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

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

app.UseForwardedHeaders();
app.UseRateLimiter();
app.UseCors();

app.MapGroup("/api/auth")
    .MapIdentityApi<IdentityUser>()
    .WithTags("Auth");

app.UseExceptionHandler("/error");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers()
    .RequireAuthorization();

// for now
app.MapGet("/", async context =>
{
    context.Response.Redirect("/docs");
    await Task.CompletedTask;
});

app.Run();