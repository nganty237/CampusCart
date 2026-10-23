using CampusCart.Api.Infrastructure.Persistence;
using CampusCart.Api.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CampusCart.Api.Modules.Listings;

[ApiController]
[Route("api/v1/listings")]
public sealed class ListingsController(
    CampusCartDbContext db,
    UserManager<CampusCartUser> userManager) : ControllerBase
{
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var listings = await db.Listings.AsNoTracking()
            .Where(listing => listing.ProviderId == user.Id)
            .OrderByDescending(listing => listing.CreatedAtUtc)
            .Select(listing => new MyListingSummary(
                listing.Id,
                listing.Title,
                listing.Category,
                listing.Description,
                listing.PriceXaf,
                listing.Unit,
                listing.Location,
                listing.IsActive,
                listing.CreatedAtUtc,
                listing.Provider.FullName,
                db.Reviews.Where(review => review.ProviderId == listing.ProviderId)
                    .Select(review => (double?)review.Rating).Average(),
                db.Reviews.Count(review => review.ProviderId == listing.ProviderId)))
            .ToListAsync(cancellationToken);

        return Ok(listings);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PagedResult<ListingSummary>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ListingSummary>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? category,
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

        var query = db.Listings.AsNoTracking().Where(listing => listing.IsActive);
        var currentUser = User.Identity?.IsAuthenticated == true
            ? await userManager.GetUserAsync(User)
            : null;
        if (currentUser is not null)
            query = query.Where(listing => listing.ProviderId != currentUser.Id);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(listing =>
                EF.Functions.ILike(listing.Title, $"%{term}%") ||
                EF.Functions.ILike(listing.Description, $"%{term}%") ||
                EF.Functions.ILike(listing.Category, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim();
            query = query.Where(listing =>
                EF.Functions.ILike(listing.Category, normalizedCategory));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(listing => listing.CreatedAtUtc)
            .ThenBy(listing => listing.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(listing => new ListingSummary(
                listing.Id,
                listing.Title,
                listing.Category,
                listing.Description,
                listing.PriceXaf,
                listing.Unit,
                listing.Location,
                listing.CreatedAtUtc,
                listing.Provider.FullName,
                db.Reviews
                    .Where(review => review.ProviderId == listing.ProviderId)
                    .Select(review => (double?)review.Rating)
                    .Average(),
                db.Reviews.Count(review => review.ProviderId == listing.ProviderId)))
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<ListingSummary>(items, page, pageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ListingSummary>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var currentUser = User.Identity?.IsAuthenticated == true
            ? await userManager.GetUserAsync(User)
            : null;
        var listing = await db.Listings.AsNoTracking()
            .Where(item => item.Id == id && item.IsActive)
            .Where(item => currentUser == null || item.ProviderId != currentUser.Id)
            .Select(item => new ListingSummary(
                item.Id,
                item.Title,
                item.Category,
                item.Description,
                item.PriceXaf,
                item.Unit,
                item.Location,
                item.CreatedAtUtc,
                item.Provider.FullName,
                db.Reviews.Where(review => review.ProviderId == item.ProviderId)
                    .Select(review => (double?)review.Rating).Average(),
                db.Reviews.Count(review => review.ProviderId == item.ProviderId)))
            .SingleOrDefaultAsync(cancellationToken);

        return listing is null ? NotFound() : Ok(listing);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ListingSummary>> Create(
        CreateListingRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var listing = new ServiceListing
        {
            ProviderId = user.Id,
            Provider = user,
            Title = request.Title.Trim(),
            Category = request.Category.Trim(),
            Description = request.Description.Trim(),
            PriceXaf = request.PriceXaf,
            Unit = request.Unit.Trim(),
            Location = request.Location.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        db.Listings.Add(listing);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = listing.Id }, await ToSummaryAsync(listing, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateListingRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var listing = await db.Listings.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken);
        if (listing is null) return NotFound();
        if (listing.ProviderId != user.Id) return Forbid();

        listing.Title = request.Title.Trim();
        listing.Category = request.Category.Trim();
        listing.Description = request.Description.Trim();
        listing.PriceXaf = request.PriceXaf;
        listing.Unit = request.Unit.Trim();
        listing.Location = request.Location.Trim();
        await db.SaveChangesAsync(cancellationToken);

        return Ok(await ToSummaryAsync(listing, cancellationToken));
    }

    [HttpPatch("{id:guid}/active")]
    [Authorize]
    public async Task<IActionResult> SetActive(
        Guid id,
        SetListingActiveRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var listing = await db.Listings.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken);
        if (listing is null) return NotFound();
        if (listing.ProviderId != user.Id) return Forbid();

        listing.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<ListingSummary> ToSummaryAsync(
        ServiceListing listing,
        CancellationToken cancellationToken)
    {
        var ratings = db.Reviews.Where(review => review.ProviderId == listing.ProviderId);
        return new ListingSummary(
            listing.Id,
            listing.Title,
            listing.Category,
            listing.Description,
            listing.PriceXaf,
            listing.Unit,
            listing.Location,
            listing.CreatedAtUtc,
            (await userManager.FindByIdAsync(listing.ProviderId.ToString()))?.FullName ?? string.Empty,
            await ratings.Select(review => (double?)review.Rating).AverageAsync(cancellationToken),
            await ratings.CountAsync(cancellationToken));
    }
}

public sealed record ListingSummary(
    Guid Id,
    string Title,
    string Category,
    string Description,
    int PriceXaf,
    string Unit,
    string Location,
    DateTimeOffset CreatedAtUtc,
    string ProviderFullName,
    double? AverageRating,
    int ReviewCount);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record CreateListingRequest(
    [param: Required, StringLength(120, MinimumLength = 3)] string Title,
    [param: Required, StringLength(60, MinimumLength = 2)] string Category,
    [param: Required, StringLength(2000, MinimumLength = 10)] string Description,
    [param: Range(1, 100_000_000)] int PriceXaf,
    [param: Required, StringLength(80, MinimumLength = 2)] string Unit,
    [param: Required, StringLength(120, MinimumLength = 2)] string Location);

public sealed record UpdateListingRequest(
    [param: Required, StringLength(120, MinimumLength = 3)] string Title,
    [param: Required, StringLength(60, MinimumLength = 2)] string Category,
    [param: Required, StringLength(2000, MinimumLength = 10)] string Description,
    [param: Range(1, 100_000_000)] int PriceXaf,
    [param: Required, StringLength(80, MinimumLength = 2)] string Unit,
    [param: Required, StringLength(120, MinimumLength = 2)] string Location);

public sealed record SetListingActiveRequest(bool IsActive);

public sealed record MyListingSummary(
    Guid Id,
    string Title,
    string Category,
    string Description,
    int PriceXaf,
    string Unit,
    string Location,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    string ProviderFullName,
    double? AverageRating,
    int ReviewCount);
