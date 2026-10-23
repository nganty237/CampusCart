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
        ToggleOfferCommand = new Command<OfferedService>(async offer =>
        {
            if (offer is null) return;
            try
            {
                await App.Api.SetListingActiveAsync(offer.Listing.Id, !offer.IsActive);
                offer.IsActive = !offer.IsActive;
            }
            catch (ApiException ex) { await Shell.Current.DisplayAlertAsync("Erreur", ex.Message, "OK"); }
        });
        DeleteOfferCommand = new Command<OfferedService>(async offer =>
        {
            if (offer is null) return;

            var confirmed = await Shell.Current.DisplayAlertAsync(
                "Supprimer l’offre ?",
                $"« {offer.Title} » sera retirée de tes services publiés.",
                "Supprimer",
                "Annuler");

            if (!confirmed) return;

            try
            {
                await App.Api.DeleteListingAsync(offer.Listing.Id);
                Offers.Remove(offer);
            }
            catch (ApiException ex)
            {
                await Shell.Current.DisplayAlertAsync("Suppression impossible", ex.Message, "OK");
            }
        });
        EditOfferCommand = new Command<OfferedService>(async offer =>
        {
            if (offer is null) return;
            OfferManagementStore.BeginEdit(offer);
            await Shell.Current.GoToAsync(nameof(Pages.CreateOfferPage));
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

    public async void Refresh()
    {
        try
        {
            var remote = await App.Api.GetMyListingsAsync();
            Offers.Clear();
            foreach (var item in remote)
            {
                var listing = ApiUiMapper.ToListing(item);
                var offer = new OfferedService
                {
                    Title = item.Title, Category = item.Category, Price = listing.Price, Unit = item.Unit,
                    Description = item.Description, Availability = item.Location, Listing = listing,
                    IsActive = item.IsActive
                };
                Offers.Add(offer);
            }
        }
        catch (ApiException ex) { await Shell.Current.DisplayAlertAsync("Mes annonces", ex.Message, "OK"); }
        OnPropertyChanged(nameof(HasOffers));
        OnPropertyChanged(nameof(HasNoOffers));
        OnPropertyChanged(nameof(OfferCountLabel));
    }

    private void SetOfferCommands(OfferedService offer)
    {
        offer.ToggleCommand = ToggleOfferCommand;
        offer.DeleteCommand = DeleteOfferCommand;
        offer.EditCommand = EditOfferCommand;
    }

    public ICommand EditOfferCommand { get; }
}
