using System.Windows.Input;

namespace CampusCart.ViewModels;

public sealed class LandingViewModel
{
    public ICommand OpenSignInCommand { get; }
    public ICommand OpenRegistrationCommand { get; }

    public LandingViewModel()
    {
        OpenSignInCommand = new Command(async () => await Shell.Current.GoToAsync("//login"));
        OpenRegistrationCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(CampusCart.Pages.InscriptionPage)));
    }
}
