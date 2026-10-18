using CampusCart.ViewModels;

namespace CampusCart.Controls;

public partial class BottomNavigationBar : ContentView
{
    public static readonly BindableProperty SelectedTabProperty = BindableProperty.Create(
        nameof(SelectedTab), typeof(string), typeof(BottomNavigationBar), "home",
        propertyChanged: (bindable, _, value) => ((BottomNavigationBar)bindable)._viewModel?.SelectTab((string)value));

    private readonly BottomNavigationViewModel _viewModel;

    public BottomNavigationBar()
    {
        InitializeComponent();
        _viewModel = new BottomNavigationViewModel(SelectedTab);
        BindingContext = _viewModel;
    }

    public string SelectedTab
    {
        get => (string)GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }
}
