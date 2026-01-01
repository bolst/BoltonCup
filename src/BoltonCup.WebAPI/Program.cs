using BoltonCup.WebAPI;
using BoltonCup.WebAPI.Data;
using BoltonCup.WebAPI.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBoltonCupWebAPIServices(builder.Configuration);

// Add identity auth
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AuthDbContext>();

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
    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc), []
        }
    });
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