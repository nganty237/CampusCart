using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class FindServicePage : ContentPage
{
    public FindServicePage()
    {
        InitializeComponent();
        BindingContext = new FindServiceViewModel();
    }
}
