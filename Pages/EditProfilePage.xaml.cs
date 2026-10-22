using CampusCart.ViewModels;

namespace CampusCart.Pages;

public partial class EditProfilePage : ContentPage
{
    public EditProfilePage()
    {
        InitializeComponent();
        BindingContext = new EditProfileViewModel();
    }
}
