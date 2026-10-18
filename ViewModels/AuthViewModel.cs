using System.Windows.Input;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class AuthViewModel : BaseViewModel
{
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _fullName = string.Empty;
    private string _emailError = string.Empty;
    private string _passwordError = string.Empty;
    private string _nameError = string.Empty;

    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
    public string EmailError { get => _emailError; private set => SetProperty(ref _emailError, value); }
    public string PasswordError { get => _passwordError; private set => SetProperty(ref _passwordError, value); }
    public string NameError { get => _nameError; private set => SetProperty(ref _nameError, value); }

    public ICommand SignInCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand OpenRegistrationCommand { get; }
    public ICommand OpenSignInCommand { get; }
    public ICommand OpenLandingCommand { get; }
    public ICommand ForgotPasswordCommand { get; }

    public AuthViewModel()
    {
        SignInCommand = new Command(async () => await SignInAsync());
        RegisterCommand = new Command(async () => await RegisterAsync());
        OpenRegistrationCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(CampusCart.Pages.InscriptionPage)));
        OpenSignInCommand = new Command(async () => await Shell.Current.GoToAsync("//login"));
        OpenLandingCommand = new Command(async () => await Shell.Current.GoToAsync("//landing"));
        ForgotPasswordCommand = new Command(async () => await Shell.Current.DisplayAlertAsync(
            "Mot de passe oubli\u00E9 ?", "La r\u00E9initialisation du mot de passe sera disponible prochainement.", "OK"));
    }

    private async Task SignInAsync()
    {
        EmailError = IsCampusEmail(Email) ? string.Empty : "Saisis une adresse e-mail valide.";
        PasswordError = string.IsNullOrWhiteSpace(Password) ? "Saisis ton mot de passe." : string.Empty;
        if (!string.IsNullOrEmpty(EmailError) || !string.IsNullOrEmpty(PasswordError)) return;

        AppSession.SignIn(Email);
        await Shell.Current.GoToAsync("//home");
    }

    private async Task RegisterAsync()
    {
        NameError = string.IsNullOrWhiteSpace(FullName) ? "Saisis ton nom complet." : string.Empty;
        EmailError = IsCampusEmail(Email) ? string.Empty : "Utilise une adresse e-mail universitaire.";
        PasswordError = Password.Length >= 8 && Password.Any(char.IsDigit)
            ? string.Empty
            : "8 caract\u00E8res minimum, dont un chiffre.";
        if (!string.IsNullOrEmpty(NameError) || !string.IsNullOrEmpty(EmailError) || !string.IsNullOrEmpty(PasswordError)) return;

        AppSession.SignIn(Email, FullName);
        await Shell.Current.DisplayAlertAsync("Compte pr\u00EAt", "La cr\u00E9ation de compte sera reli\u00E9e au backend dans une prochaine \u00E9tape.", "Continuer");
        await Shell.Current.GoToAsync("//home");
    }

    private static bool IsCampusEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Contains('.');
}
