namespace CampusCart.Pages;

public partial class RequestsPage : ContentPage
{
    private ViewModels.RequestsViewModel ViewModel => (ViewModels.RequestsViewModel)BindingContext;

    public RequestsPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.RequestsViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel.RefreshRequests();
    }
}
