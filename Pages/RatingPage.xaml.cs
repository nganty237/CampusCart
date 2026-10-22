using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class RatingPage : ContentPage
{
    public RatingPage()
    {
        InitializeComponent();
        BindingContext = new RatingViewModel();
    }
}
