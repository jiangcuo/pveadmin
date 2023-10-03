using Microsoft.AspNetCore.Components.WebView.Maui;
using PveAdmin.Data;

namespace PveAdmin;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        Options.Load_config();
        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif
		
		builder.Services.AddSingleton<WeatherForecastService>();
        return builder.Build();
	}
}

