namespace CampusCart;

public partial class App : Application
{
    public static CampusCart.Services.CampusCartApiService Api =>
        Current?.Handler?.MauiContext?.Services.GetService<CampusCart.Services.CampusCartApiService>()
        ?? throw new InvalidOperationException("Le service API n'est pas initialisé.");

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        window.Created += async (_, _) => await RestoreSessionAsync();
        return window;
    }

    private static async Task RestoreSessionAsync()
    {
        try
        {
            var store = Current?.Handler?.MauiContext?.Services.GetService<CampusCart.Services.SecureTokenStore>();
            if (store is null || string.IsNullOrWhiteSpace(await store.GetAccessTokenAsync())) return;
            var profile = await Api.GetProfileAsync();
            CampusCart.Services.AppSession.SignIn(profile.Email, profile.FullName, profile.Id);
            await Shell.Current.GoToAsync("//home");
        }
        catch (Exception) { }
    }
}
