using BoltonCup.Core.Queries;
using BoltonCup.Infrastructure;
using BoltonCup.WebAPI.Middleware;
using BoltonCup.WebAPI.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBoltonCupInfrastructure(builder.Configuration);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<DefaultPaginationQuery>();

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter access token below."
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapSwagger("/openapi/{documentName}.json");
    app.MapScalarApiReference("/docs", options => options
        .WithTitle("Bolton Cup API Documentation")
        .WithTheme(ScalarTheme.Purple)
        .AddPreferredSecuritySchemes("Bearer")
    );
}

app.UseHttpsRedirection();

app.MapIdentityApi<IdentityUser>();

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<DevAuthMiddleware>();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers()
    .RequireAuthorization();

app.Run();