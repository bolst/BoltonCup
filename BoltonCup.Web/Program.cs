using Blazored.LocalStorage;
using MudBlazor.Services;
using BoltonCup.Web.Components;
using BoltonCup.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add local storage
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ICustomLocalStorageProvider, BoltonCup.Web.Data.CustomLocalStorageProvider>();

// Core services
builder.Services.AddBoltonCupServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(BoltonCup.Shared.Components.Pages.Info).Assembly);

app.Run();
