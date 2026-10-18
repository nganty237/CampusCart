using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class CreateRequestPage : ContentPage
{
    public CreateRequestPage()
    {
        InitializeComponent();
        BindingContext = new CreateRequestViewModel();
    }
}
