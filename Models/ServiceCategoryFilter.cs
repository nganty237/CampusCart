using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CampusCart.Models;

public sealed class ServiceCategoryFilter(string name) : INotifyPropertyChanged
{
    private bool _isSelected;
    public event PropertyChangedEventHandler? PropertyChanged;
    public string Name { get; } = name;
    public ICommand? SelectCommand { get; set; }
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(TextColor));
            OnPropertyChanged(nameof(BorderColor));
        }
    }
    public Color BackgroundColor => IsSelected ? Color.FromArgb("#315CEB") : Colors.White;
    public Color TextColor => IsSelected ? Colors.White : Color.FromArgb("#70798A");
    public Color BorderColor => IsSelected ? Color.FromArgb("#315CEB") : Color.FromArgb("#E1E5EC");
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}
