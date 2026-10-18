using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class ConnexionPage : ContentPage
{
    public ConnexionPage()
    {
        InitializeComponent();
        BindingContext = new AuthViewModel();
    }
}
