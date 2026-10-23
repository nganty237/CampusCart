using System.Windows.Input;

namespace CampusCart.Models;

public sealed record ServiceRequest(
    string Schedule,
    string Status,
    string Title,
    string ContactName,
    string ContactInitials,
    string ContactDetail,
    Color AvatarColor,
    Color StatusColor,
    string RequestedTime = "",
    string HandoverLocation = "",
    string Message = "")
{
    public Guid Id { get; init; }
    public Guid ListingId { get; init; }
    public Guid RequesterId { get; init; }
    public Guid ProviderId { get; init; }
    public int PriceXaf { get; init; }
    public bool IsSent { get; init; }
    public Guid ConversationId { get; init; }
    public ICommand? ReviewCommand { get; set; }
    public ICommand? AcceptCommand { get; set; }
    public ICommand? DeclineCommand { get; set; }
    public ICommand? CancelCommand { get; set; }
    public ICommand? CompleteCommand { get; set; }
    public bool CanReview => Status.StartsWith("Termin", StringComparison.OrdinalIgnoreCase);
    public bool CanAcceptOrDecline => !IsSent && Status == "En attente";
    public bool CanCancel => Status is "En attente" or "Acceptée";
    public bool CanConfirmCompletion => Status is "Acceptée" or "À confirmer";
}
