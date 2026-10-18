using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using IconFont.Maui.MaterialDesignIcons;

namespace CampusCart.Models;

public sealed class ServiceListing : INotifyPropertyChanged
{
    private bool _isFavorite;

    public event PropertyChangedEventHandler? PropertyChanged;
    public required string Title { get; init; }
    public required string Category { get; init; }
    public required string Provider { get; init; }
    public required string Location { get; init; }
    public required string Price { get; init; }
    public required string Rating { get; init; }
    public required string IconGlyph { get; init; }
    public required Color ImageBackground { get; init; }
    public string Description { get; init; } = "Un étudiant du campus propose ce service. Contacte-le pour convenir des détails.";
    public string ProviderInitials { get; init; } = "ET";
    public string ProviderLevel { get; init; } = "Étudiant du campus";
    public string Availability { get; init; } = "Disponible cette semaine · Répond rapidement";
    public string PriceNote { get; init; } = "Tarif proposé par l'étudiant";
    public ICommand? FavoriteCommand { get; set; }
    public ICommand? OpenCommand { get; set; }

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            if (_isFavorite == value) return;
            _isFavorite = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FavoriteGlyph));
        }
    }

    public string FavoriteGlyph => IsFavorite
        ? MaterialDesignIconsOutlinedRegular.Favorite
        : MaterialDesignIconsOutlinedRegular.FavoriteBorder;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
