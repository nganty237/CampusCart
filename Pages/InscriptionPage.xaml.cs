using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class InscriptionPage : ContentPage
{
    public InscriptionPage()
    {
        InitializeComponent();
        BindingContext = new AuthViewModel();
    }
}
