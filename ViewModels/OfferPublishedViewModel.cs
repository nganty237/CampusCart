using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class OfferPublishedViewModel : BaseViewModel
{
    public OfferedService? Offer => OfferManagementStore.LatestPublishedOffer;
    public ICommand ManageOffersCommand { get; }
    public ICommand HomeCommand { get; }

    public OfferPublishedViewModel()
    {
        ManageOffersCommand = new Command(async () => await Shell.Current.GoToAsync("//myoffers"));
        HomeCommand = new Command(async () => await Shell.Current.GoToAsync("//home"));
    }
}
