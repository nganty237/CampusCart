using System.Collections.ObjectModel;
using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class MyOffersViewModel : BaseViewModel
{
    public ObservableCollection<OfferedService> Offers => OfferManagementStore.MyOffers;
    public bool HasOffers => Offers.Count > 0;
    public bool HasNoOffers => !HasOffers;
    public string OfferCountLabel => Offers.Count == 1 ? "1 offre publiée" : $"{Offers.Count} offres publiées";
    public ICommand CreateOfferCommand { get; }
    public ICommand ToggleOfferCommand { get; }
    public ICommand DeleteOfferCommand { get; }
    public ICommand BackToProfileCommand { get; }

    public MyOffersViewModel()
    {
        CreateOfferCommand = new Command(async () =>
        {
            OfferManagementStore.CurrentStep = 1;
            await Shell.Current.GoToAsync(nameof(Pages.CreateOfferPage));
        });
        ToggleOfferCommand = new Command<OfferedService>(offer =>
        {
            if (offer is not null) offer.IsActive = !offer.IsActive;
        });
        DeleteOfferCommand = new Command<OfferedService>(async offer =>
        {
            if (offer is null) return;

            var confirmed = await Shell.Current.DisplayAlertAsync(
                "Supprimer l’offre ?",
                $"« {offer.Title} » sera retirée de tes services publiés.",
                "Supprimer",
                "Annuler");

            if (confirmed) Offers.Remove(offer);
        });
        BackToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("//profile"));
        foreach (var offer in Offers) SetOfferCommands(offer);
        Offers.CollectionChanged += (_, _) =>
        {
            foreach (var offer in Offers) SetOfferCommands(offer);
            OnPropertyChanged(nameof(HasOffers));
            OnPropertyChanged(nameof(HasNoOffers));
            OnPropertyChanged(nameof(OfferCountLabel));
        };
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(HasOffers));
        OnPropertyChanged(nameof(HasNoOffers));
        OnPropertyChanged(nameof(OfferCountLabel));
    }

    private void SetOfferCommands(OfferedService offer)
    {
        offer.ToggleCommand = ToggleOfferCommand;
        offer.DeleteCommand = DeleteOfferCommand;
    }
}
