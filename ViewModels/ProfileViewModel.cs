using System.Windows.Input;
using System.Globalization;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class ProfileViewModel : BaseViewModel
{
    public string FullName => AppSession.FullName;
    public string Email => AppSession.Email;
    public string Initials => AppSession.Initials;
    private string _completedServicesLabel = "—";
    private string _averageRatingLabel = "—";
    private string _reviewCountLabel = "";
    private string _statsMessage = "Chargement des statistiques…";
    private bool _isLoadingStats;

    public string CompletedServicesLabel { get => _completedServicesLabel; private set => SetProperty(ref _completedServicesLabel, value); }
    public string AverageRatingLabel { get => _averageRatingLabel; private set => SetProperty(ref _averageRatingLabel, value); }
    public string ReviewCountLabel { get => _reviewCountLabel; private set => SetProperty(ref _reviewCountLabel, value); }
    public string StatsMessage { get => _statsMessage; private set => SetProperty(ref _statsMessage, value); }
    public bool IsLoadingStats { get => _isLoadingStats; private set => SetProperty(ref _isLoadingStats, value); }
    public ICommand LogoutCommand { get; }
    public ICommand EditProfileCommand { get; }
    public ICommand OpenOffersCommand { get; }
    public ICommand CreateOfferCommand { get; }

    public ProfileViewModel()
    {
        LogoutCommand = new Command(async () => await LogoutAsync());
        EditProfileCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Pages.EditProfilePage)));
        OpenOffersCommand = new Command(async () => await Shell.Current.GoToAsync("//myoffers"));
        CreateOfferCommand = new Command(async () =>
        {
            OfferManagementStore.CurrentStep = 1;
            await Shell.Current.GoToAsync(nameof(Pages.CreateOfferPage));
        });
    }

    public async void Refresh()
    {
        if (IsLoadingStats) return;
        IsLoadingStats = true;
        StatsMessage = "Chargement des statistiques…";
        try
        {
            var profile = await App.Api.GetProfileAsync();
            AppSession.UpdateProfile(profile.FullName, profile.Email);
            CompletedServicesLabel = profile.CompletedServicesCount.ToString(CultureInfo.InvariantCulture);
            AverageRatingLabel = profile.AverageRating?.ToString("0.0", CultureInfo.GetCultureInfo("fr-FR")) ?? "—";
            ReviewCountLabel = profile.ReviewCount == 1 ? "1 avis" : $"{profile.ReviewCount} avis";
            StatsMessage = profile.ReviewCount == 0 ? "Aucun avis reçu pour le moment." : string.Empty;
        }
        catch (ApiException ex) { StatsMessage = ex.Message; }
        catch (Exception) { StatsMessage = "Impossible de charger les statistiques. Vérifie ta connexion et réessaie."; }
        finally { IsLoadingStats = false; }
        OnPropertyChanged(nameof(FullName));
        OnPropertyChanged(nameof(Email));
        OnPropertyChanged(nameof(Initials));
    }

    private static async Task LogoutAsync()
    {
        await App.Api.LogoutAsync();
        AppSession.SignOut();
        await Shell.Current.GoToAsync("//login");
    }
}
