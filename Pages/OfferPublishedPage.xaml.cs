using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class OfferPublishedPage : ContentPage
{
    public OfferPublishedPage()
    {
        InitializeComponent();
        BindingContext = new OfferPublishedViewModel();
    }
}
