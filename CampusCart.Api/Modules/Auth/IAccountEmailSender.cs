namespace CampusCart.Api.Modules.Auth;

public interface IAccountEmailSender
{
    Task SendVerificationCodeAsync(string email, string code, CancellationToken cancellationToken);
}
