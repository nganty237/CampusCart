using Microsoft.Extensions.Logging;

using IconFont.Maui.MaterialDesignIcons;
using CampusCart.Services;

#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

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

#if MAUI_DEVFLOW
		builder.AddMauiDevFlowAgent();
#endif

		builder.Services.AddSingleton<ViewModels.HomeViewModel>();
		builder.Services.AddSingleton<ViewModels.AuthViewModel>();
		builder.Services.AddSingleton<SecureTokenStore>();
		builder.Services.AddSingleton<ApiClient>(services =>
		{
			var client = new HttpClient();
#if ANDROID
			// Téléphone physique sur le même Wi-Fi que le PC qui exécute l'API.
			// Pour l'émulateur Android, remplacer par http://10.0.2.2:5258/.
			client.BaseAddress = new Uri("http://192.168.1.146:5258/");
#elif WINDOWS
			client.BaseAddress = new Uri("http://localhost:5258/");
#else
			client.BaseAddress = new Uri("http://localhost:5258/");
#endif
			client.Timeout = TimeSpan.FromSeconds(20);
			return new ApiClient(client, services.GetRequiredService<SecureTokenStore>());
		});
		builder.Services.AddSingleton<CampusCartApiService>();
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
