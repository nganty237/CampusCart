using CampusCart.Api.Modules.Listings;
using CampusCart.Api.Modules.Messaging;

namespace CampusCart.Api.Modules.Requests;

public enum ServiceRequestStatus
{
    Pending,
    Accepted,
    Declined,
    Cancelled,
    CompletionPending,
    Completed
}

public sealed class ServiceRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public ServiceListing Listing { get; set; } = null!;
    public Conversation? Conversation { get; set; }
    public Guid RequesterId { get; set; }
    public Guid ProviderId { get; set; }
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
    public DateTimeOffset RequestedForUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public int PriceXaf { get; set; }
    public string HandoverLocation { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset? RequesterConfirmedCompletionAtUtc { get; set; }
    public DateTimeOffset? ProviderConfirmedCompletionAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();
}
