namespace CampusCart.Api.Modules.Auth;

/// <summary>
/// Development-only sink. Verification codes are never written to logs in production.
/// </summary>
public sealed class DevelopmentAccountEmailSender(
    ILogger<DevelopmentAccountEmailSender> logger) : IAccountEmailSender
{
    public Task SendVerificationCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Development verification code for {Email}: {Code}",
            email,
            code);
        return Task.CompletedTask;
    }
}
