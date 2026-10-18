using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class MainPage : ContentPage
{
    private HomeViewModel ViewModel => (HomeViewModel)BindingContext;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = new HomeViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel.RefreshServices();
    }
}
