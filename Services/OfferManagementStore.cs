using System.Collections.ObjectModel;
using CampusCart.Models;

namespace CampusCart.Services;

/// <summary>UI draft and current listing selection; saved listings are loaded from the API.</summary>
public static class OfferManagementStore
{
    public static OfferDraft Draft { get; } = new();
    public static ObservableCollection<OfferedService> MyOffers { get; } = new();
    public static OfferedService? LatestPublishedOffer { get; private set; }
    public static int CurrentStep { get; set; } = 1;
    public static Guid? EditingListingId { get; set; }

    public static OfferedService AddPublished(ServiceListing listing)
    {
        var offer = FromListing(listing);
        MyOffers.Insert(0, offer); LatestPublishedOffer = offer; ResetDraft();
        return offer;
    }
    public static void BeginEdit(OfferedService offer)
    {
        EditingListingId = offer.Listing.Id;
        Draft.Title = offer.Title; Draft.Category = CampusCartApiService.CategoryForUi(offer.Category); Draft.Description = offer.Description;
        Draft.Price = offer.Listing.PriceXaf.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Draft.Unit = offer.Unit; Draft.Availability = offer.Availability; CurrentStep = 1;
    }
    public static void ReplacePublished(ServiceListing listing)
    {
        var index = MyOffers.ToList().FindIndex(item => item.Listing.Id == listing.Id);
        if (index < 0) { AddPublished(listing); return; }
        var previous = MyOffers[index];
        var replacement = FromListing(listing); replacement.IsActive = previous.IsActive;
        MyOffers[index] = replacement; LatestPublishedOffer = replacement; ResetDraft();
    }
    private static OfferedService FromListing(ServiceListing listing) => new()
    {
        Title = listing.Title, Category = listing.Category, Price = listing.Price, Unit = listing.Unit,
        Description = listing.Description, Availability = listing.Location, Listing = listing
    };
    private static void ResetDraft() { EditingListingId = null; Draft.Clear(); CurrentStep = 1; }
}
