using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class FindServicePage : ContentPage
{
    private ViewModels.FindServiceViewModel ViewModel => (ViewModels.FindServiceViewModel)BindingContext;
    public FindServicePage()
    {
        InitializeComponent();
        BindingContext = new FindServiceViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadServicesAsync();
    }
}
