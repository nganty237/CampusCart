using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class CreateOfferPage : ContentPage
{
    public CreateOfferPage()
    {
        InitializeComponent();
        BindingContext = new CreateOfferViewModel();
    }
}
