using Microsoft.AspNetCore.Identity;

namespace CampusCart.Api.Modules.Auth;

public sealed class CampusCartUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
}
