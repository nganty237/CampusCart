using System.Collections.ObjectModel;
using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;
using IconFont.Maui.MaterialDesignIcons;

namespace CampusCart.ViewModels;

public sealed class HomeViewModel : BaseViewModel
{
    private string _searchText = string.Empty;
    private readonly List<ServiceListing> _seedServices = [];
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    public ObservableCollection<ServiceCategory> Categories { get; } = new();
    public ObservableCollection<ServiceListing> Services { get; } = new();
    public string Greeting => $"Bonjour, {AppSession.FullName}";
    public string UserInitials => AppSession.Initials;
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public ICommand PublishCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand OpenServiceCommand { get; }
    public ICommand BrowseServicesCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value)) FilterServices();
        }
    }

    public HomeViewModel()
    {
        AppSession.Changed += OnSessionChanged;

        Categories.Add(new("Impression", MaterialDesignIconsOutlinedRegular.Print, Color.FromArgb("#EEF3FF"), "Documents, rapports..."));
        Categories.Add(new("Aide tech", MaterialDesignIconsOutlinedRegular.Computer, Color.FromArgb("#EEF7FA"), "Installation, d\u00E9pannage..."));
        Categories.Add(new("Soutien scolaire", MaterialDesignIconsOutlinedRegular.School, Color.FromArgb("#F2F1FF"), "Cours, exercices..."));
        Categories.Add(new("Autres services", MaterialDesignIconsOutlinedRegular.MoreHoriz, Color.FromArgb("#F5F1EA"), "Et plus encore"));

        ToggleFavoriteCommand = new Command<ServiceListing>(service =>
        {
            if (service is not null) service.IsFavorite = !service.IsFavorite;
        });
        OpenServiceCommand = new Command<ServiceListing>(async service =>
        {
            if (service is null) return;
            ServiceSearchState.InitialQuery = service.Title;
            await Shell.Current.GoToAsync(nameof(Pages.FindServicePage));
        });
        BrowseServicesCommand = new Command(async () =>
        {
            ServiceSearchState.InitialQuery = string.Empty;
            await Shell.Current.GoToAsync(nameof(Pages.FindServicePage));
        });
        RefreshServices();

        PublishCommand = new Command(async () =>
        {
            OfferManagementStore.CurrentStep = 1;
            await Shell.Current.GoToAsync(nameof(Pages.CreateOfferPage));
        });
    }

    public void RefreshUser()
    {
        OnPropertyChanged(nameof(Greeting));
        OnPropertyChanged(nameof(UserInitials));
    }

    public void RefreshServices()
    {
        var allServices = OfferManagementStore.MyOffers
            .Where(offer => offer.IsActive)
            .Select(offer => offer.Listing)
            .Concat(_seedServices)
            .GroupBy(service => service.Id)
            .Select(group => group.First());
        var query = SearchText.Trim();
        var matches = string.IsNullOrWhiteSpace(query)
            ? allServices
            : allServices.Where(service =>
                service.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                service.Category.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                service.Provider.Contains(query, StringComparison.CurrentCultureIgnoreCase));

        Services.Clear();
        foreach (var service in matches)
        {
            service.FavoriteCommand = ToggleFavoriteCommand;
            service.OpenCommand = OpenServiceCommand;
            Services.Add(service);
        }
    }

    public async Task LoadServicesAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var result = await App.Api.GetListingsAsync();
            _seedServices.Clear();
            _seedServices.AddRange(result.Items.Select(ApiUiMapper.ToListing));
            StatusMessage = string.Empty;
            RefreshServices();
        }
        catch (ApiException ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private void FilterServices()
    {
        RefreshServices();
    }

    private void OnSessionChanged(object? sender, EventArgs e) => RefreshUser();
}
