using System.Collections.ObjectModel;
using CampusCart.Models;

namespace CampusCart.Services;

/// <summary>Temporary in-memory state for the service request flow, until persistence is connected.</summary>
public static class RequestFlowStore
{
    private static readonly ObservableCollection<ServiceRequest> _sentRequests =
    [
        new("Aujourd'hui · 16:00 · 500 FCFA", "Acceptée", "Impression documents A4",
            "Messina Richard", "MR", "Bibliothèque centrale · Entrée", Color.FromArgb("#DCE6FF"), Color.FromArgb("#2DA875")),
        new("Demain · 14:00 · 2 000 FCFA", "En attente", "Cours de maths · L1",
            "Bikele Frank", "BF", "Campus ENSPD", Color.FromArgb("#DCE6FF"), Color.FromArgb("#E69A20")),
        new("4 oct. · 1 500 FCFA", "Refusée", "Réparation PC",
            "Patrick", "P", "Campus ENSPD", Color.FromArgb("#DCE6FF"), Color.FromArgb("#E15B64")),
        new("12 oct. · 2 000 FCFA", "Terminée", "Mise en page de mémoire",
            "Nadine Tchana", "NT", "Bibliothèque centrale", Color.FromArgb("#E8E5FF"), Color.FromArgb("#2DA875"))
    ];

    public static IReadOnlyList<ServiceRequest> SentRequests => _sentRequests;
    public static int ReceivedRequestCount => 2;
    public static ServiceListing? SelectedOffer { get; private set; }
    public static ServiceRequest? LatestRequest { get; private set; }
    public static ServiceRequest? RequestToReview { get; private set; }
    private static readonly Dictionary<ServiceRequest, (int Rating, string Comment)> _reviews =
        new(ReferenceEqualityComparer.Instance);

    public static (int Rating, string Comment)? GetReview(ServiceRequest request) =>
        _reviews.TryGetValue(request, out var review) ? review : null;

    public static void SelectRequestForReview(ServiceRequest request)
    {
        if (!request.CanReview) throw new InvalidOperationException("Seules les demandes terminées peuvent être évaluées.");
        RequestToReview = request;
    }

    public static void SubmitReview(ServiceRequest request, int rating, string comment)
    {
        if (!request.CanReview) throw new InvalidOperationException("Seules les demandes terminées peuvent être évaluées.");
        if (rating is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(rating));
        _reviews[request] = (rating, comment.Trim());
    }

    public static void SelectOffer(ServiceListing offer)
    {
        SelectedOffer = offer;
        LatestRequest = null;
    }

    public static ServiceRequest SubmitRequest(string requestedTime, string location, string message)
    {
        var offer = SelectedOffer ?? throw new InvalidOperationException("Aucune offre n'a été sélectionnée.");
        var request = new ServiceRequest(
            $"{requestedTime} · {offer.Price}",
            "Envoyée",
            offer.Title,
            offer.Provider,
            offer.ProviderInitials,
            location,
            offer.ImageBackground,
            Color.FromArgb("#315CEB"),
            requestedTime,
            location,
            message);

        LatestRequest = request;
        _sentRequests.Insert(0, request);
        return request;
    }
}
