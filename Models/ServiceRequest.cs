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
    public ICommand? ReviewCommand { get; set; }
    public bool CanReview => Status.Equals("Terminée", StringComparison.OrdinalIgnoreCase);
}
