using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Logging;


using PveAdmin.Data;

namespace PveAdmin;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        Options.Load_config();
		Options.GetResAsync();
        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif
		
		builder.Services.AddSingleton<WeatherForecastService>();
        return builder.Build();
	}
}

