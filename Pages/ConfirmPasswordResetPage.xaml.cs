using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class ConfirmPasswordResetPage : ContentPage
{
    public ConfirmPasswordResetPage()
    {
        InitializeComponent();
        BindingContext = new ConfirmPasswordResetViewModel();
    }
}
