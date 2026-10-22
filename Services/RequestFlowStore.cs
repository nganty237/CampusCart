using CampusCart.Models;

namespace CampusCart.Services;

/// <summary>Navigation context for the request flow; durable data is loaded from the API.</summary>
public static class RequestFlowStore
{
    public static ServiceListing? SelectedOffer { get; private set; }
    public static ServiceRequest? LatestRequest { get; private set; }
    public static ServiceRequest? RequestToReview { get; private set; }
    public static void SelectOffer(ServiceListing offer) { SelectedOffer = offer; LatestRequest = null; }
    public static void SetLatestRequest(ServiceRequest request) => LatestRequest = request;
    public static void SelectRequestForReview(ServiceRequest request)
    {
        if (!request.CanReview) throw new InvalidOperationException("Seules les demandes terminées peuvent être évaluées.");
        RequestToReview = request;
    }
}
