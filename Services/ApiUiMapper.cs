using CampusCart.Models;
using IconFont.Maui.MaterialDesignIcons;

namespace CampusCart.Services;

public static class ApiUiMapper
{
    public static ServiceListing ToListing(ListingDto item) => new()
    {
        Id = item.Id, Title = item.Title, Category = item.Category.ToUpperInvariant(),
        Provider = item.ProviderFullName, ProviderInitials = AppSession.GetInitials(item.ProviderFullName),
        ProviderLevel = "Étudiant du campus", Location = item.Location,
        Price = CampusCartApiService.PriceLabel(item.PriceXaf, item.Unit), PriceXaf = item.PriceXaf, Unit = item.Unit,
        PriceNote = $"Par {item.Unit}", Description = item.Description,
        Rating = item.AverageRating is null ? "Nouveau" : $"{item.AverageRating:0.0} · {item.ReviewCount} avis",
        IconGlyph = Icon(item.Category), ImageBackground = Background(item.Category)
    };

    public static ServiceListing ToListing(MyListingDto item) => new()
    {
        Id = item.Id, Title = item.Title, Category = item.Category.ToUpperInvariant(),
        Provider = item.ProviderFullName, ProviderInitials = AppSession.GetInitials(item.ProviderFullName),
        ProviderLevel = "Étudiant du campus", Location = item.Location,
        Price = CampusCartApiService.PriceLabel(item.PriceXaf, item.Unit), PriceXaf = item.PriceXaf, Unit = item.Unit,
        PriceNote = $"Par {item.Unit}", Description = item.Description,
        Rating = item.AverageRating is null ? "Nouveau" : $"{item.AverageRating:0.0} · {item.ReviewCount} avis",
        IconGlyph = Icon(item.Category), ImageBackground = Background(item.Category)
    };

    public static ServiceRequest ToRequest(RequestDto item, bool sent) => new(
        $"{item.RequestedForUtc.ToLocalTime():dd MMM · HH:mm} · {item.PriceXaf:N0} FCFA",
        item.Status switch { 0 => "En attente", 1 => "Acceptée", 2 => "Refusée", 3 => "Annulée", 4 => "À confirmer", 5 => "Terminée", _ => "Inconnue" },
        item.ListingTitle, sent ? "Prestataire" : "Demandeur", "ET", item.HandoverLocation,
        Color.FromArgb("#DCE6FF"), Color.FromArgb("#315CEB"), item.RequestedForUtc.ToLocalTime().ToString("g"), item.HandoverLocation, item.Message)
    { Id = item.Id, ListingId = item.ListingId, RequesterId = item.RequesterId, ProviderId = item.ProviderId, PriceXaf = item.PriceXaf };

    public static Conversation ToConversation(ConversationDto item) => new(
        "Étudiant", "ET", item.ListingTitle,
        item.LastMessage ?? "Aucun message", item.UpdatedAtUtc.ToLocalTime().ToString("dd MMM HH:mm"), 0,
        Color.FromArgb("#DCE6FF"), "Demande liée au service", string.Empty, string.Empty, string.Empty, string.Empty)
        { Id = item.Id, OtherParticipantId = item.OtherParticipantId };

    private static string Icon(string category) => category.ToUpperInvariant() switch
    {
        var c when c.Contains("IMPRESSION") => MaterialDesignIconsOutlinedRegular.Print,
        var c when c.Contains("COURS") => MaterialDesignIconsOutlinedRegular.School,
        var c when c.Contains("TECH") => MaterialDesignIconsOutlinedRegular.Computer,
        var c when c.Contains("LIVRAISON") => MaterialDesignIconsOutlinedRegular.LocalShipping,
        _ => MaterialDesignIconsOutlinedRegular.MoreHoriz
    };
    private static Color Background(string category) => category.ToUpperInvariant() switch
    {
        var c when c.Contains("IMPRESSION") => Color.FromArgb("#EDF3FF"),
        var c when c.Contains("COURS") => Color.FromArgb("#F2F1FF"),
        var c when c.Contains("TECH") => Color.FromArgb("#EEF3F8"),
        _ => Color.FromArgb("#F5F1EA")
    };
}
