using CampusCart.Api.Infrastructure.Persistence;
using CampusCart.Api.Modules.Auth;
using CampusCart.Api.Modules.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace CampusCart.Api.Modules.Reviews;

[ApiController]
[Authorize]
[Route("api/v1/reviews")]
public sealed class ReviewsController(
    CampusCartDbContext db,
    UserManager<CampusCartUser> userManager) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var serviceRequest = await db.Requests.SingleOrDefaultAsync(
            item => item.Id == request.RequestId,
            cancellationToken);
        if (serviceRequest is null) return NotFound();
        if (serviceRequest.RequesterId != user.Id) return Forbid();
        if (serviceRequest.Status != ServiceRequestStatus.Completed)
            return Conflict(new ProblemDetails { Title = "Seule une demande terminée peut être évaluée." });

        var review = new ServiceReview
        {
            RequestId = serviceRequest.Id,
            Request = serviceRequest,
            ReviewerId = user.Id,
            ProviderId = serviceRequest.ProviderId,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        db.Reviews.Add(review);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new ProblemDetails { Title = "Un avis a déjà été déposé pour cette demande." });
        }

        return Created($"/api/v1/reviews/{review.Id}", new ReviewResponse(
            review.Id,
            review.RequestId,
            review.ProviderId,
            review.Rating,
            review.Comment,
            review.CreatedAtUtc));
    }

    [HttpGet("providers/{providerId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetForProvider(
        Guid providerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 50)
            return BadRequest(new ProblemDetails
            {
                Title = "Pagination parameters are invalid.",
                Detail = "Page must be at least 1 and pageSize must be between 1 and 50."
            });

        var query = db.Reviews.AsNoTracking().Where(review => review.ProviderId == providerId);
        var totalCount = await query.CountAsync(cancellationToken);
        var reviews = await query
            .OrderByDescending(review => review.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(review => new ReviewResponse(
                review.Id,
                review.RequestId,
                review.ProviderId,
                review.Rating,
                review.Comment,
                review.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(new ReviewsPage(reviews, page, pageSize, totalCount));
    }
}

public sealed record CreateReviewRequest(
    [property: Required] Guid RequestId,
    [property: Range(1, 5)] int Rating,
    [property: StringLength(1000)] string? Comment);

public sealed record ReviewResponse(
    Guid Id,
    Guid RequestId,
    Guid ProviderId,
    int Rating,
    string? Comment,
    DateTimeOffset CreatedAtUtc);

public sealed record ReviewsPage(
    IReadOnlyList<ReviewResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
