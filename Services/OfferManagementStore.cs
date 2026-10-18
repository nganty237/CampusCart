using System.Collections.ObjectModel;
using CampusCart.Models;
using IconFont.Maui.MaterialDesignIcons;

namespace CampusCart.Services;

/// <summary>In-memory offer catalogue and draft state for the frontend prototype.</summary>
public static class OfferManagementStore
{
    public static OfferDraft Draft { get; } = new();
    public static ObservableCollection<OfferedService> MyOffers { get; } = new();
    public static OfferedService? LatestPublishedOffer { get; private set; }
    public static int CurrentStep { get; set; } = 1;

    public static OfferedService PublishDraft()
    {
        var draft = Draft;
        var (glyph, color) = CategoryStyle(draft.Category);
        var studentName = AppSession.FullName;
        var initials = AppSession.GetInitials(studentName);
        var listing = new ServiceListing
        {
            Title = draft.Title.Trim(),
            Category = draft.Category.ToUpperInvariant(),
            Provider = $"{studentName} · Étudiant",
            ProviderInitials = initials,
            ProviderLevel = "Étudiant du campus",
            Location = draft.Availability.Trim(),
            Price = $"{draft.Price.Trim()} FCFA / {draft.Unit.Trim()}",
            PriceNote = $"Par {draft.Unit.Trim()}",
            Description = draft.Description.Trim(),
            Availability = draft.Availability.Trim(),
            Rating = "Nouveau",
            IconGlyph = glyph,
            ImageBackground = color
        };

        var published = new OfferedService
        {
            Title = listing.Title,
            Category = draft.Category,
            Price = $"{draft.Price.Trim()} FCFA",
            Unit = draft.Unit.Trim(),
            Description = draft.Description.Trim(),
            Availability = draft.Availability.Trim(),
            Listing = listing
        };

        MyOffers.Insert(0, published);
        LatestPublishedOffer = published;
        Draft.Clear();
        CurrentStep = 1;
        return published;
    }

    private static (string Glyph, Color Color) CategoryStyle(string category) => category switch
    {
        "Impression" => (MaterialDesignIconsOutlinedRegular.Print, Color.FromArgb("#EDF3FF")),
        "Cours" => (MaterialDesignIconsOutlinedRegular.School, Color.FromArgb("#F2F1FF")),
        "Dépannage" => (MaterialDesignIconsOutlinedRegular.Computer, Color.FromArgb("#EEF3F8")),
        "Livraison" => (MaterialDesignIconsOutlinedRegular.LocalShipping, Color.FromArgb("#EEF7F3")),
        _ => (MaterialDesignIconsOutlinedRegular.MoreHoriz, Color.FromArgb("#F5F1EA"))
    };
}
