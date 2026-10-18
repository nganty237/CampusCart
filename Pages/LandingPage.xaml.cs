using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class LandingPage : ContentPage
{
    public LandingPage()
    {
        InitializeComponent();
        BindingContext = new LandingViewModel();
    }
}
