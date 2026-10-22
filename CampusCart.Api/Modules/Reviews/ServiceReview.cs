using CampusCart.Api.Modules.Requests;

namespace CampusCart.Api.Modules.Reviews;

public sealed class ServiceReview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public ServiceRequest Request { get; set; } = null!;
    public Guid ReviewerId { get; set; }
    public Guid ProviderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
