namespace CampusCart.Pages;

public partial class MessagesPage : ContentPage
{
    public MessagesPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.MessagesViewModel();
    }
}
