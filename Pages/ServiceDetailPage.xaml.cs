using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class ServiceDetailPage : ContentPage
{
    public ServiceDetailPage()
    {
        InitializeComponent();
        BindingContext = new ServiceDetailViewModel();
    }
}
