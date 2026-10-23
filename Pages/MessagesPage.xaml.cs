namespace CampusCart.Pages;

public partial class MessagesPage : ContentPage
{
    private ViewModels.MessagesViewModel ViewModel => (ViewModels.MessagesViewModel)BindingContext;
    public MessagesPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.MessagesViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadConversationsAsync();
        ViewModel.StartAutoRefresh();
    }

    protected override void OnDisappearing()
    {
        ViewModel.StopAutoRefresh();
        base.OnDisappearing();
    }
}
