using System.Collections.ObjectModel;
using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class FindServiceViewModel : BaseViewModel
{
    private string _searchText = string.Empty;
    private string _selectedCategory = "Tout";

    public ObservableCollection<ServiceListing> Services { get; } = new();
    public ObservableCollection<ServiceCategoryFilter> Categories { get; } = new();
    public string ResultsLabel => Services.Count == 1 ? "1 offre disponible" : $"{Services.Count} offres disponibles";
    public bool HasResults => Services.Count > 0;
    public bool HasNoResults => !HasResults;
    public ICommand SelectCategoryCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand OpenOfferCommand { get; }
    public ICommand BackCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value)) FilterServices();
        }
    }

    public FindServiceViewModel()
    {
        SelectCategoryCommand = new Command<string>(SelectCategory);
        ResetCommand = new Command(() =>
        {
            SelectedCategory = "Tout";
            SearchText = string.Empty;
            ServiceSearchState.InitialQuery = string.Empty;
        });
        OpenOfferCommand = new Command<ServiceListing>(async offer =>
        {
            if (offer is null) return;
            RequestFlowStore.SelectOffer(offer);
            await Shell.Current.GoToAsync(nameof(Pages.ServiceDetailPage));
        });
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

        foreach (var name in new[] { "Tout", "Impression", "Cours", "Dépannage", "Livraison", "Autre" })
        {
            var filter = new ServiceCategoryFilter(name) { SelectCommand = SelectCategoryCommand };
            Categories.Add(filter);
        }
        Categories[0].IsSelected = true;
        SearchText = ServiceSearchState.InitialQuery;
        FilterServices();
    }

    private string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory == value) return;
            _selectedCategory = value;
            foreach (var category in Categories) category.IsSelected = category.Name == value;
            FilterServices();
        }
    }

    private void SelectCategory(string? category)
    {
        if (!string.IsNullOrWhiteSpace(category)) SelectedCategory = category;
    }

    private void FilterServices()
    {
        var allServices = ServiceCatalog.Listings.Concat(OfferManagementStore.MyOffers
            .Where(offer => offer.IsActive)
            .Select(offer => offer.Listing));
        var query = SearchText.Trim();
        var matches = allServices.Where(offer =>
            (string.IsNullOrWhiteSpace(query) ||
             offer.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
             offer.Category.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
             offer.Provider.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
             offer.Description.Contains(query, StringComparison.CurrentCultureIgnoreCase)) &&
            MatchesCategory(offer, SelectedCategory));

        Services.Clear();
        foreach (var offer in matches)
        {
            offer.OpenCommand = OpenOfferCommand;
            Services.Add(offer);
        }
        OnPropertyChanged(nameof(ResultsLabel));
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(HasNoResults));
    }

    private static bool MatchesCategory(ServiceListing offer, string category) => category switch
    {
        "Tout" => true,
        "Impression" => offer.Category.Contains("IMPRESSION", StringComparison.OrdinalIgnoreCase),
        "Cours" => offer.Category.Contains("COURS", StringComparison.OrdinalIgnoreCase) || offer.Category.Contains("SCOLAIRE", StringComparison.OrdinalIgnoreCase),
        "Dépannage" => offer.Category.Contains("TECH", StringComparison.OrdinalIgnoreCase) || offer.Category.Contains("DÉPANNAGE", StringComparison.OrdinalIgnoreCase),
        "Livraison" => offer.Category.Contains("LIVRAISON", StringComparison.OrdinalIgnoreCase),
        _ => !new[] { "IMPRESSION", "COURS", "SCOLAIRE", "TECH", "DÉPANNAGE", "LIVRAISON" }
            .Any(known => offer.Category.Contains(known, StringComparison.OrdinalIgnoreCase))
    };
}
