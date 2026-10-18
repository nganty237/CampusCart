using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class MyOffersPage : ContentPage
{
    private MyOffersViewModel ViewModel => (MyOffersViewModel)BindingContext;

    public MyOffersPage()
    {
        InitializeComponent();
        BindingContext = new MyOffersViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel.Refresh();
    }
}
