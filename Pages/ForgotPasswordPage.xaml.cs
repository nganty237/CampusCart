using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage()
    {
        InitializeComponent();
        BindingContext = new ForgotPasswordViewModel();
    }
}
