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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ViewModel.RefreshUser();
        ViewModel.RefreshServices();
        await ViewModel.LoadServicesAsync();
    }
}
