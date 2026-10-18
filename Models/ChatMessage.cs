namespace CampusCart.Models;

public sealed record ChatMessage(
    string Text,
    string TimeLabel,
    bool IsOutgoing,
    string? AttachmentName = null)
{
    public bool HasAttachment => !string.IsNullOrWhiteSpace(AttachmentName);
    public bool IsIncoming => !IsOutgoing;
}
