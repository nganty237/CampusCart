using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class ChatPage : ContentPage
{
    public ChatPage()
    {
        InitializeComponent();
        BindingContext = new ChatViewModel();
    }
}
