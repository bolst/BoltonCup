using Blazored.LocalStorage;
using BoltonCup.Shared.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Supabase;

namespace BoltonCup.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		builder.Services.AddBlazoredLocalStorage();
		builder.Services.AddScoped<ICustomLocalStorageProvider, BoltonCup.App.Data.CustomLocalStorageProvider>();

		builder.Services.AddBoltonCupServices();
		
		builder.Services.AddScoped(provider =>
		{
			var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
			var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");
            
			return new Supabase.Client(url, key, new SupabaseOptions
			{
				AutoConnectRealtime = true,
			});
		});
		
		// Authorization
		builder.Services.AddAuthorizationCore();
		//builder.Services.AddScoped<CustomUserService>();
		builder.Services.AddScoped<CustomUserService>();
		// builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
		builder.Services.AddScoped<CustomAuthenticationStateProvider>();
		builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());

		builder.Services.AddMudServices();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
