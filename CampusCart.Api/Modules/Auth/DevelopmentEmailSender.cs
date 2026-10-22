using Microsoft.AspNetCore.Identity;

namespace CampusCart.Api.Modules.Auth;

/// <summary>
/// Development-only sink. A real provider must replace this before production deployment.
/// </summary>
public sealed class DevelopmentEmailSender(
    ILogger<DevelopmentEmailSender> logger) : IEmailSender<CampusCartUser>
{
    public Task SendConfirmationLinkAsync(
        CampusCartUser user,
        string email,
        string confirmationLink)
    {
        logger.LogInformation(
            "Development email confirmation for {Email}: {ConfirmationLink}",
            email,
            confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(
        CampusCartUser user,
        string email,
        string resetLink)
    {
        logger.LogInformation(
            "Development password reset for {Email}: {ResetLink}",
            email,
            resetLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(
        CampusCartUser user,
        string email,
        string resetCode)
    {
        logger.LogInformation(
            "Development password reset code for {Email}: {ResetCode}",
            email,
            resetCode);
        return Task.CompletedTask;
    }
}
