using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CampusCart.Models;

public sealed class OfferDraft : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private string _category = "Cours";
    private string _price = string.Empty;
    private string _unit = string.Empty;
    private string _description = string.Empty;
    private string _availability = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title { get => _title; set => SetProperty(ref _title, value); }
    public string Category { get => _category; set => SetProperty(ref _category, value); }
    public string Price { get => _price; set => SetProperty(ref _price, value); }
    public string Unit { get => _unit; set => SetProperty(ref _unit, value); }
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public string Availability { get => _availability; set => SetProperty(ref _availability, value); }

    public void Clear()
    {
        Title = string.Empty;
        Category = "Cours";
        Price = string.Empty;
        Unit = string.Empty;
        Description = string.Empty;
        Availability = string.Empty;
    }

    private void SetProperty(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (field == value) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
