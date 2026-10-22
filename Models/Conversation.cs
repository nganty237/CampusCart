namespace CampusCart.Models;

public sealed record Conversation(
    string ContactName,
    string ContactInitials,
    string ServiceTitle,
    string Preview,
    string TimeLabel,
    int UnreadCount,
    Color AvatarColor,
    string RequestSummary = "20 pages",
    string Price = "500 FCFA",
    string Status = "Acceptée",
    string Schedule = "Aujourd'hui · 16:00",
    string Location = "Bibliothèque centrale")
{
    public Guid Id { get; init; }
    public Guid OtherParticipantId { get; init; }
    public bool HasRequestContext => !string.IsNullOrWhiteSpace(Status) || !string.IsNullOrWhiteSpace(Schedule) || !string.IsNullOrWhiteSpace(Location);
    public bool HasUnread => UnreadCount > 0;
}
