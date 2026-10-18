using System.Windows.Input;

namespace CampusCart.ViewModels;

public sealed class ForgotPasswordViewModel : BaseViewModel
{
    private string _email = string.Empty;
    private string _emailError = string.Empty;

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string EmailError
    {
        get => _emailError;
        private set => SetProperty(ref _emailError, value);
    }

    public ICommand SubmitCommand { get; }
    public ICommand OpenSignInCommand { get; }

    public ForgotPasswordViewModel()
    {
        SubmitCommand = new Command(async () => await SubmitRequestAsync());
        OpenSignInCommand = new Command(async () => await Shell.Current.GoToAsync("//login"));
    }

    private async Task SubmitRequestAsync()
    {
        EmailError = IsCampusEmail(Email)
            ? string.Empty
            : "Saisis une adresse e-mail universitaire valide.";

        if (!string.IsNullOrEmpty(EmailError)) return;

        await Shell.Current.GoToAsync(nameof(Pages.ConfirmPasswordResetPage), new Dictionary<string, object>
        {
            ["Email"] = Email.Trim()
        });
    }

    private static bool IsCampusEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Contains('.');
}
