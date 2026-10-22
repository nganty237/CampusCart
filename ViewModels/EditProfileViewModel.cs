using System.Windows.Input;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class EditProfileViewModel : BaseViewModel
{
    private string _fullName = AppSession.FullName;
    private string _email = AppSession.Email;
    private string _validationMessage = string.Empty;

    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public EditProfileViewModel()
    {
        SaveCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private async Task SaveAsync()
    {
        var name = FullName.Trim();
        var email = Email.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ValidationMessage = "Saisis ton nom complet.";
            return;
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1 ||
            !email[(atIndex + 1)..].Contains('.', StringComparison.Ordinal))
        {
            ValidationMessage = "Saisis une adresse e-mail valide.";
            return;
        }

        AppSession.UpdateProfile(name, email);
        await Shell.Current.GoToAsync("..");
    }
}
