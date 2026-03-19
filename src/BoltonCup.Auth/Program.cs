using BoltonCup.Common;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BoltonCup.Auth.Components;
using BoltonCup.Auth.Services;
using BoltonCup.SessionStorage;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBoltonCupCommonServices(builder.Configuration);

builder.Services.AddScoped<AuthSessionStateService>();

builder.Services.AddMudServices();
builder.Services.AddBoltonCupSessionStorage();

await builder.Build().RunAsync();