using CampusCart.Pages;

namespace CampusCart;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(Pages.InscriptionPage), typeof(Pages.InscriptionPage));
        Routing.RegisterRoute(nameof(Pages.ServiceDetailPage), typeof(Pages.ServiceDetailPage));
        Routing.RegisterRoute(nameof(Pages.FindServicePage), typeof(Pages.FindServicePage));
        Routing.RegisterRoute(nameof(Pages.CreateRequestPage), typeof(Pages.CreateRequestPage));
        Routing.RegisterRoute(nameof(Pages.RequestConfirmationPage), typeof(Pages.RequestConfirmationPage));
        Routing.RegisterRoute(nameof(Pages.CreateOfferPage), typeof(Pages.CreateOfferPage));
        Routing.RegisterRoute(nameof(Pages.OfferPublishedPage), typeof(Pages.OfferPublishedPage));
        Routing.RegisterRoute(nameof(Pages.ChatPage), typeof(Pages.ChatPage));
    }
}
