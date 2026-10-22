using System.Windows.Input;

namespace CampusCart.ViewModels;

public sealed class ForgotPasswordViewModel : BaseViewModel
{
    private string _email = string.Empty;
    private string _emailError = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string EmailError { get => _emailError; private set => SetProperty(ref _emailError, value); }
    public ICommand SubmitCommand { get; }
    public ICommand OpenSignInCommand { get; }
    public ForgotPasswordViewModel()
    {
        SubmitCommand = new Command(() =>
        {
            EmailError = !string.IsNullOrWhiteSpace(Email) && Email.Contains('@') && Email.Contains('.')
                ? "La réinitialisation n’est pas encore intégrée à cette application."
                : "Saisis une adresse e-mail universitaire valide.";
        });
        OpenSignInCommand = new Command(async () => await Shell.Current.GoToAsync("//login"));
    }
}
