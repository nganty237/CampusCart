using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class RequestConfirmationPage : ContentPage
{
    public RequestConfirmationPage()
    {
        InitializeComponent();
        BindingContext = new RequestConfirmationViewModel();
    }
}
