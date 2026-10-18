using System.Windows.Input;

namespace CampusCart.ViewModels;

public sealed class ConfirmPasswordResetViewModel : BaseViewModel, IQueryAttributable
{
    private const string DemoCode = "123456";
    private string _email = string.Empty;
    private string _code = string.Empty;
    private string _codeError = string.Empty;
    private bool _isConfirmed;

    public string Email
    {
        get => _email;
        private set => SetProperty(ref _email, value);
    }

    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    public string CodeError
    {
        get => _codeError;
        private set => SetProperty(ref _codeError, value);
    }

    public bool IsConfirmed
    {
        get => _isConfirmed;
        private set
        {
            if (!SetProperty(ref _isConfirmed, value)) return;
            OnPropertyChanged(nameof(IsFormVisible));
        }
    }

    public bool IsFormVisible => !IsConfirmed;

    public string DemoCodeHint => $"Code frontend de démonstration : {DemoCode}";
    public ICommand ConfirmCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand OpenSignInCommand { get; }

    public ConfirmPasswordResetViewModel()
    {
        ConfirmCommand = new Command(Confirm);
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        OpenSignInCommand = new Command(async () => await Shell.Current.GoToAsync("//login"));
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Email", out var email) && email is string value)
            Email = value;
    }

    private void Confirm()
    {
        CodeError = string.Equals(Code.Trim(), DemoCode, StringComparison.Ordinal)
            ? string.Empty
            : "Code incorrect. Utilise le code de démonstration affiché.";

        if (!string.IsNullOrEmpty(CodeError)) return;

        IsConfirmed = true;
    }
}
