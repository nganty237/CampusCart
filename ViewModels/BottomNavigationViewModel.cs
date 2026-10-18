using System.Windows.Input;

namespace CampusCart.ViewModels;

public sealed class BottomNavigationViewModel : BaseViewModel
{
    private string _selectedTab;

    public Color HomeColor => GetTabColor("home");
    public Color RequestsColor => GetTabColor("requests");
    public Color MessagesColor => GetTabColor("messages");
    public Color ProfileColor => GetTabColor("profile");
    public ICommand NavigateCommand { get; }

    public BottomNavigationViewModel(string selectedTab)
    {
        _selectedTab = selectedTab;
        NavigateCommand = new Command<string>(async route => await NavigateAsync(route));
    }

    public void SelectTab(string selectedTab)
    {
        if (string.Equals(_selectedTab, selectedTab, StringComparison.Ordinal)) return;
        _selectedTab = selectedTab;
        OnPropertyChanged(nameof(HomeColor));
        OnPropertyChanged(nameof(RequestsColor));
        OnPropertyChanged(nameof(MessagesColor));
        OnPropertyChanged(nameof(ProfileColor));
    }

    private Color GetTabColor(string tab) =>
        string.Equals(_selectedTab, tab, StringComparison.Ordinal)
            ? Color.FromArgb("#315CEB")
            : Color.FromArgb("#8590A4");

    private static async Task NavigateAsync(string? route)
    {
        if (string.IsNullOrWhiteSpace(route)) return;
        await Shell.Current.GoToAsync($"//{route}");
    }
}
