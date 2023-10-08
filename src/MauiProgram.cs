using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Logging;
using UtilsCore.Cors;

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
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });
        Environment.SetEnvironmentVariable(
"WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS",
"--disable-web-security");
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

