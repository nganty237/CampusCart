using CampusCart.Api.Modules.Requests;

namespace CampusCart.Api.Modules.Messaging;

public sealed class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public ServiceRequest Request { get; set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}
