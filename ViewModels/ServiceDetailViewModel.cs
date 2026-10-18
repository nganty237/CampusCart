using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class ServiceDetailViewModel : BaseViewModel
{
    public ServiceListing? Offer => RequestFlowStore.SelectedOffer;
    public ICommand RequestCommand { get; }
    public ICommand BackCommand { get; }

    public ServiceDetailViewModel()
    {
        RequestCommand = new Command(async () =>
        {
            if (Offer is not null)
                await Shell.Current.GoToAsync(nameof(Pages.CreateRequestPage));
        });
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }
}
