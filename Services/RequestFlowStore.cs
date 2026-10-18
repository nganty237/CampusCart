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
            "Patrick", "P", "Campus ENSPD", Color.FromArgb("#DCE6FF"), Color.FromArgb("#E15B64"))
    ];

    public static IReadOnlyList<ServiceRequest> SentRequests => _sentRequests;
    public static int ReceivedRequestCount => 2;
    public static ServiceListing? SelectedOffer { get; private set; }
    public static ServiceRequest? LatestRequest { get; private set; }

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
