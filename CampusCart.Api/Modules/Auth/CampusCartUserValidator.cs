using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CampusCart.Api.Modules.Auth;

public sealed class CampusCartUserValidator(
    IOptions<AllowedEmailDomainsOptions> options) : IUserValidator<CampusCartUser>
{
    private readonly HashSet<string> _allowedDomains = options.Value.Domains
        .Select(domain => domain.Trim().TrimStart('@'))
        .Where(domain => !string.IsNullOrWhiteSpace(domain))
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public Task<IdentityResult> ValidateAsync(
        UserManager<CampusCartUser> manager,
        CampusCartUser user)
    {
        var email = user.Email?.Trim();
        var separator = email?.LastIndexOf('@') ?? -1;
        var domain = separator >= 0 ? email![(separator + 1)..] : string.Empty;

        if (_allowedDomains.Contains(domain))
            return Task.FromResult(IdentityResult.Success);

        var error = new IdentityError
        {
            Code = "EmailDomainNotAllowed",
            Description = "L'inscription est réservée aux adresses de l'université."
        };
        return Task.FromResult(IdentityResult.Failed(error));
    }
}
