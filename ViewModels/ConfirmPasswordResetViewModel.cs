using System.Windows.Input;

namespace CampusCart.ViewModels;

public sealed class ConfirmPasswordResetViewModel : BaseViewModel, IQueryAttributable
{
    private string _email = string.Empty;
    private string _code = string.Empty;
    private string _codeError = string.Empty;
    public string Email { get => _email; private set => SetProperty(ref _email, value); }
    public string Code { get => _code; set => SetProperty(ref _code, value); }
    public string CodeError { get => _codeError; private set => SetProperty(ref _codeError, value); }
    public bool IsConfirmed => false;
    public bool IsFormVisible => true;
    public string DemoCodeHint => "La réinitialisation du mot de passe n’est pas disponible dans ce parcours.";
    public ICommand ConfirmCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand OpenSignInCommand { get; }
    public ConfirmPasswordResetViewModel()
    {
        ConfirmCommand = new Command(() => CodeError = "Le backend ne fournit pas le parcours de confirmation par code utilisé par cet écran.");
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        OpenSignInCommand = new Command(async () => await Shell.Current.GoToAsync("//login"));
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Email", out var email) && email is string value) Email = value;
    }
}
