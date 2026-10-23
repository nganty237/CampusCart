using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class ChatPage : ContentPage
{
    public ChatPage()
    {
        InitializeComponent();
        BindingContext = new ChatViewModel();
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is ChatViewModel viewModel)
            viewModel.StopPolling();
        base.OnDisappearing();
    }
}
