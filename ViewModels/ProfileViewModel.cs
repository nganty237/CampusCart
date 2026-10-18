using System.Windows.Input;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class ProfileViewModel : BaseViewModel
{
    public string FullName => AppSession.FullName;
    public string Email => AppSession.Email;
    public string Initials => AppSession.Initials;
    public ICommand LogoutCommand { get; }
    public ICommand OpenOffersCommand { get; }
    public ICommand CreateOfferCommand { get; }

    public ProfileViewModel()
    {
        LogoutCommand = new Command(async () => await LogoutAsync());
        OpenOffersCommand = new Command(async () => await Shell.Current.GoToAsync("//myoffers"));
        CreateOfferCommand = new Command(async () =>
        {
            OfferManagementStore.CurrentStep = 1;
            await Shell.Current.GoToAsync(nameof(Pages.CreateOfferPage));
        });
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(FullName));
        OnPropertyChanged(nameof(Email));
        OnPropertyChanged(nameof(Initials));
    }

    private static async Task LogoutAsync()
    {
        AppSession.SignOut();
        await Shell.Current.GoToAsync("//login");
    }
}
