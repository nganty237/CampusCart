using System.Windows.Input;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class ProfileViewModel : BaseViewModel
{
    public string FullName => AppSession.FullName;
    public string Email => AppSession.Email;
    public string Initials => AppSession.Initials;
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
        try
        {
            var profile = await App.Api.GetProfileAsync();
            AppSession.UpdateProfile(profile.FullName, profile.Email);
        }
        catch (ApiException ex) { await Shell.Current.DisplayAlertAsync("Profil", ex.Message, "OK"); }
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
