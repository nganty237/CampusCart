using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class RequestConfirmationViewModel : BaseViewModel
{
    public ServiceRequest? Request => RequestFlowStore.LatestRequest;
    public ICommand ViewRequestsCommand { get; }
    public ICommand HomeCommand { get; }

    public RequestConfirmationViewModel()
    {
        ViewRequestsCommand = new Command(async () => await Shell.Current.GoToAsync("//requests"));
        HomeCommand = new Command(async () => await Shell.Current.GoToAsync("//home"));
    }
}
