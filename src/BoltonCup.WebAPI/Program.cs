using BoltonCup.Core.Queries;
using BoltonCup.Infrastructure;
using BoltonCup.WebAPI.Authentication;
using BoltonCup.WebAPI.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBoltonCupInfrastructure(builder.Configuration);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<DefaultPaginationQuery>();

builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyConstants.Scheme, null);

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(IdentityConstants.BearerScheme, ApiKeyConstants.Scheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilterAttribute>());
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// add CORS
// TODO: configure further
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


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
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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

app.UseCors();

app.MapIdentityApi<IdentityUser>();

app.UseExceptionHandler("/error");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers()
    .RequireAuthorization();

app.Run();