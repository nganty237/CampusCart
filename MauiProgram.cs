using Microsoft.Extensions.Logging;

using IconFont.Maui.MaterialDesignIcons;

namespace CampusCart;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMaterialDesignIcons()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<ViewModels.HomeViewModel>();
		builder.Services.AddSingleton<ViewModels.AuthViewModel>();
		builder.Services.AddSingleton<Pages.ConnexionPage>();
		builder.Services.AddTransient<Pages.InscriptionPage>();
		builder.Services.AddSingleton<Pages.MainPage>();
		builder.Services.AddSingleton<AppShell>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
