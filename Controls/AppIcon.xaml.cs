namespace CampusCart.Controls;

public partial class AppIcon : ContentView
{
    public static readonly BindableProperty GlyphProperty = BindableProperty.Create(
        nameof(Glyph), typeof(string), typeof(AppIcon), string.Empty,
        propertyChanged: (bindable, _, value) => ((AppIcon)bindable).GlyphLabel.Text = (string)value);

    public static readonly BindableProperty IconSizeProperty = BindableProperty.Create(
        nameof(IconSize), typeof(double), typeof(AppIcon), 22d,
        propertyChanged: (bindable, _, value) => ((AppIcon)bindable).GlyphLabel.FontSize = (double)value);

    public static readonly BindableProperty IconColorProperty = BindableProperty.Create(
        nameof(IconColor), typeof(Color), typeof(AppIcon), Color.FromArgb("#315CEB"),
        propertyChanged: (bindable, _, value) => ((AppIcon)bindable).GlyphLabel.TextColor = (Color)value);

    public AppIcon()
    {
        InitializeComponent();
        GlyphLabel.FontSize = IconSize;
        GlyphLabel.TextColor = IconColor;
        GlyphLabel.Text = Glyph;
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }
}
