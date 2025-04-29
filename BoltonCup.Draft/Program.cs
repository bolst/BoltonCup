using Blazored.LocalStorage;
using BoltonCup.Draft.Components;
using BoltonCup.Draft.Data;
using BoltonCup.Shared.Data;
using BoltonCup.Draft.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddBoltonCupServices(builder.Configuration);

builder.Services.AddScoped<DraftServiceProvider>();

builder.Services.AddScoped(sp =>
{
    var Navigation = sp.GetRequiredService<NavigationManager>();
    var hubConnection = new HubConnectionBuilder()
        .WithUrl(Navigation.ToAbsoluteUri(DraftHub.HubUrl))
        .WithAutomaticReconnect()
        .Build();

    return new HubConnectionProvider(hubConnection);
});

builder.Services.AddSignalR();

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
    .AddInteractiveServerRenderMode();

app.MapHub<DraftHub>(DraftHub.HubUrl);

app.Run();