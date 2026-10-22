using CampusCart.Api.Modules.Auth;
using CampusCart.Api.Infrastructure.Persistence;
using CampusCart.Api.Modules.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CampusCart.Api.Modules.Profiles;

[ApiController]
[Authorize]
[Route("api/v1/profiles")]
public sealed class ProfilesController(
    CampusCartDbContext db,
    UserManager<CampusCartUser> userManager) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        return user is null
            ? Unauthorized()
            : Ok(await CreateProfileResponseAsync(user, cancellationToken));
    }

    [HttpPut("me")]
    public async Task<IActionResult> Update(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            ModelState.AddModelError(nameof(request.FullName), "Saisis ton nom complet.");
            return ValidationProblem(ModelState);
        }

        user.FullName = request.FullName.Trim();
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());
            foreach (var error in errors)
            {
                foreach (var description in error.Value)
                {
                    ModelState.AddModelError(error.Key, description);
                }
            }

            return ValidationProblem(ModelState);
        }

        return Ok(await CreateProfileResponseAsync(user, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<PublicProfileResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublic(Guid id, CancellationToken cancellationToken)
    {
        var profile = await db.Users.AsNoTracking()
            .Where(user => user.Id == id && user.EmailConfirmed)
            .Select(user => new PublicProfileResponse(
                user.Id,
                user.FullName,
                db.Reviews.Where(review => review.ProviderId == user.Id)
                    .Select(review => (double?)review.Rating)
                    .Average(),
                db.Reviews.Count(review => review.ProviderId == user.Id)))
            .SingleOrDefaultAsync(cancellationToken);

        return profile is null ? NotFound() : Ok(profile);
    }

    private async Task<ProfileResponse> CreateProfileResponseAsync(
        CampusCartUser user,
        CancellationToken cancellationToken)
    {
        var stats = await db.Users.AsNoTracking()
            .Where(profile => profile.Id == user.Id)
            .Select(profile => new
            {
                CompletedServicesCount = db.Requests.Count(request =>
                    request.ProviderId == profile.Id && request.Status == ServiceRequestStatus.Completed),
                AverageRating = db.Reviews
                    .Where(review => review.ProviderId == profile.Id)
                    .Select(review => (double?)review.Rating)
                    .Average(),
                ReviewCount = db.Reviews.Count(review => review.ProviderId == profile.Id)
            })
            .SingleAsync(cancellationToken);

        return new ProfileResponse(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            user.EmailConfirmed,
            stats.CompletedServicesCount,
            stats.AverageRating,
            stats.ReviewCount);
    }
}

public sealed record UpdateProfileRequest(
    [param: Required, StringLength(120, MinimumLength = 2)] string FullName);

public sealed record ProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    bool EmailConfirmed,
    int CompletedServicesCount,
    double? AverageRating,
    int ReviewCount);

public sealed record PublicProfileResponse(
    Guid Id,
    string FullName,
    double? AverageRating,
    int ReviewCount);
