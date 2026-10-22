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
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
    public string EmailError { get => _emailError; private set => SetProperty(ref _emailError, value); }
    public string PasswordError { get => _passwordError; private set => SetProperty(ref _passwordError, value); }
    public string NameError { get => _nameError; private set => SetProperty(ref _nameError, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

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
        ForgotPasswordCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(CampusCart.Pages.ForgotPasswordPage)));
    }

    private async Task SignInAsync()
    {
        EmailError = IsCampusEmail(Email) ? string.Empty : "Utilise ton adresse étudiante se terminant par @enspd.cm.";
        PasswordError = Password.Length >= 8 ? string.Empty : "8 caract\u00E8res minimum.";
        if (!string.IsNullOrEmpty(EmailError) || !string.IsNullOrEmpty(PasswordError)) return;

        await RunApiAsync(async () =>
        {
            var profile = await App.Api.LoginAsync(Email.Trim(), Password);
            AppSession.SignIn(profile.Email, profile.FullName, profile.Id);
            await Shell.Current.GoToAsync("//home");
        });
    }

    private async Task RegisterAsync()
    {
        NameError = string.IsNullOrWhiteSpace(FullName) ? "Saisis ton nom complet." : string.Empty;
        EmailError = IsCampusEmail(Email) ? string.Empty : "Utilise ton adresse étudiante se terminant par @enspd.cm.";
        PasswordError = Password.Length >= 8 && Password.Any(char.IsDigit)
            ? string.Empty
            : "8 caract\u00E8res minimum, dont un chiffre.";
        if (!string.IsNullOrEmpty(NameError) || !string.IsNullOrEmpty(EmailError) || !string.IsNullOrEmpty(PasswordError)) return;

        await RunApiAsync(async () =>
        {
            await App.Api.RegisterAsync(FullName.Trim(), Email.Trim(), Password);
            StatusMessage = "Un code de confirmation a été envoyé. Saisis-le pour activer ton compte.";
            var code = await Shell.Current.DisplayPromptAsync("Confirmer l’e-mail", "Code à 6 chiffres", "Confirmer", "Plus tard", "000000", 6, Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(code))
            {
                var action = await Shell.Current.DisplayActionSheetAsync("Confirmation reportée", "Fermer", null, "Renvoyer le code");
                if (action == "Renvoyer le code")
                {
                    await App.Api.ResendCodeAsync(Email.Trim());
                    StatusMessage = "Si le compte attend une confirmation, un nouveau code a été envoyé.";
                }
                return;
            }
            await App.Api.ConfirmEmailAsync(Email.Trim(), code.Trim());
            var profile = await App.Api.LoginAsync(Email.Trim(), Password);
            AppSession.SignIn(profile.Email, profile.FullName, profile.Id);
            await Shell.Current.GoToAsync("//home");
        });
    }

    private async Task RunApiAsync(Func<Task> operation)
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusMessage = string.Empty;
        try { await operation(); }
        catch (CampusCart.Services.ApiException ex)
        {
            StatusMessage = ex.Message;
            if (ex.IsUnauthorized) AppSession.SignOut();
        }
        catch (Exception ex) { StatusMessage = $"Une erreur est survenue : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private static bool IsCampusEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && email.Trim().EndsWith("@enspd.cm", StringComparison.OrdinalIgnoreCase);
}
