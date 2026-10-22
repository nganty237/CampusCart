using CampusCart.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CampusCart.Api.Modules.Auth;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public sealed class RegistrationController(
    CampusCartDbContext db,
    UserManager<CampusCartUser> userManager,
    IAccountEmailSender emailSender,
    EmailVerificationCodeService codeService) : ControllerBase
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromMinutes(1);
    private const int MaxFailedAttempts = 5;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            ModelState.AddModelError(nameof(request.FullName), "Saisis ton nom complet.");
            return ValidationProblem(ModelState);
        }

        var email = request.Email.Trim();
        var user = new CampusCartUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(error =>
                    error.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                var existingUser = await userManager.FindByEmailAsync(email);
                if (existingUser is not null && !existingUser.EmailConfirmed)
                {
                    var lastChallenge = await db.EmailVerificationChallenges
                        .Where(challenge => challenge.UserId == existingUser.Id)
                        .OrderByDescending(challenge => challenge.CreatedAtUtc)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (lastChallenge is null ||
                        DateTimeOffset.UtcNow - lastChallenge.CreatedAtUtc >= ResendCooldown)
                    {
                        await SendCodeAsync(existingUser, cancellationToken);
                    }
                }

                return Accepted(new { message = "Si l'adresse peut être inscrite, un code sera envoyé." });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        await SendCodeAsync(user, cancellationToken);
        return Accepted(new { message = "Un code de vérification a été envoyé à cette adresse." });
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || user.EmailConfirmed)
            return BadRequest(new ProblemDetails { Title = "Le code est invalide ou expiré." });

        var challenge = await db.EmailVerificationChallenges
            .Where(item => item.UserId == user.Id && item.ConsumedAtUtc == null)
            .OrderByDescending(item => item.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (challenge is null || challenge.ExpiresAtUtc <= DateTimeOffset.UtcNow ||
            challenge.FailedAttempts >= MaxFailedAttempts)
        {
            return BadRequest(new ProblemDetails { Title = "Le code est invalide ou expiré." });
        }

        if (!codeService.Matches(request.Code.Trim(), challenge.CodeHash))
        {
            challenge.FailedAttempts++;
            await db.SaveChangesAsync(cancellationToken);
            return BadRequest(new ProblemDetails { Title = "Le code est invalide ou expiré." });
        }

        var identityToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await userManager.ConfirmEmailAsync(user, identityToken);
        if (!result.Succeeded)
        {
            return Problem(
                title: "La confirmation de l'adresse e-mail a échoué.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        challenge.ConsumedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Adresse e-mail confirmée. Tu peux maintenant te connecter." });
    }

    [HttpPost("resend-code")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendCode(
        ResendVerificationCodeRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is not null && !user.EmailConfirmed)
        {
            var lastChallenge = await db.EmailVerificationChallenges
                .Where(item => item.UserId == user.Id)
                .OrderByDescending(item => item.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastChallenge is null ||
                DateTimeOffset.UtcNow - lastChallenge.CreatedAtUtc >= ResendCooldown)
            {
                await SendCodeAsync(user, cancellationToken);
            }
        }

        return Accepted(new { message = "Si le compte existe, un nouveau code sera envoyé." });
    }

    private async Task SendCodeAsync(CampusCartUser user, CancellationToken cancellationToken)
    {
        var code = codeService.CreateCode();
        var now = DateTimeOffset.UtcNow;
        var activeChallenges = await db.EmailVerificationChallenges
            .Where(challenge => challenge.UserId == user.Id && challenge.ConsumedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var activeChallenge in activeChallenges)
            activeChallenge.ConsumedAtUtc = now;

        db.EmailVerificationChallenges.Add(new EmailVerificationChallenge
        {
            UserId = user.Id,
            CodeHash = codeService.Hash(code),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(CodeLifetime)
        });
        await db.SaveChangesAsync(cancellationToken);
        await emailSender.SendVerificationCodeAsync(user.Email!, code, cancellationToken);
    }
}

public sealed record RegisterRequest(
    [property: Required, StringLength(120, MinimumLength = 2)] string FullName,
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Required, StringLength(100, MinimumLength = 8)] string Password);
public sealed record ConfirmEmailRequest(
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Required, RegularExpression(@"^\d{6}$")] string Code);
public sealed record ResendVerificationCodeRequest(
    [property: Required, EmailAddress, StringLength(254)] string Email);
