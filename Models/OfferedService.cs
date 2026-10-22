using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CampusCart.Models;

public sealed class OfferedService : INotifyPropertyChanged
{
    private bool _isActive = true;

    public event PropertyChangedEventHandler? PropertyChanged;
    public required string Title { get; init; }
    public required string Category { get; init; }
    public required string Price { get; init; }
    public required string Unit { get; init; }
    public required string Description { get; init; }
    public required string Availability { get; init; }
    public required ServiceListing Listing { get; init; }
    public ICommand? ToggleCommand { get; set; }
    public ICommand? DeleteCommand { get; set; }
    public ICommand? EditCommand { get; set; }
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value) return;
            _isActive = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(ActionLabel));
        }
    }

    public string Status => IsActive ? "En ligne" : "En pause";
    public string ActionLabel => IsActive ? "Mettre en pause" : "Réactiver";
    public Color StatusColor => IsActive ? Color.FromArgb("#2A9C6A") : Color.FromArgb("#B47A19");

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
