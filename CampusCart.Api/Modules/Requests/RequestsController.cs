using CampusCart.Api.Infrastructure.Persistence;
using CampusCart.Api.Modules.Auth;
using CampusCart.Api.Modules.Messaging;
using CampusCart.Api.Modules.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CampusCart.Api.Modules.Requests;

[ApiController]
[Authorize]
[Route("api/v1/requests")]
public sealed class RequestsController(
    CampusCartDbContext db,
    UserManager<CampusCartUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine(
        [FromQuery] string direction = "sent",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (direction is not ("sent" or "received"))
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid direction.",
                Detail = "Direction must be 'sent' or 'received'."
            });
        if (page < 1 || pageSize is < 1 or > 50)
            return BadRequest(new ProblemDetails { Title = "Pagination parameters are invalid." });

        var query = db.Requests.AsNoTracking();
        query = direction == "sent"
            ? query.Where(request => request.RequesterId == user.Id)
            : query.Where(request => request.ProviderId == user.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var requests = await query
            .OrderByDescending(request => request.CreatedAtUtc)
            .ThenBy(request => request.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(request => new RequestSummary(
                request.Id,
                request.ListingId,
                request.Listing.Title,
                request.RequesterId,
                request.ProviderId,
                request.Status,
                request.PriceXaf,
                request.RequestedForUtc,
                request.CreatedAtUtc,
                request.HandoverLocation,
                request.Message))
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<RequestSummary>(requests, page, pageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var request = await db.Requests.AsNoTracking()
            .Where(item => item.Id == id &&
                (item.RequesterId == user.Id || item.ProviderId == user.Id))
            .Select(item => new RequestSummary(
                item.Id,
                item.ListingId,
                item.Listing.Title,
                item.RequesterId,
                item.ProviderId,
                item.Status,
                item.PriceXaf,
                item.RequestedForUtc,
                item.CreatedAtUtc,
                item.HandoverLocation,
                item.Message))
            .SingleOrDefaultAsync(cancellationToken);

        return request is null ? NotFound() : Ok(request);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (request.RequestedForUtc <= DateTimeOffset.UtcNow)
        {
            ModelState.AddModelError(nameof(request.RequestedForUtc), "La date demandée doit être dans le futur.");
            return ValidationProblem(ModelState);
        }

        var listing = await db.Listings.SingleOrDefaultAsync(
            item => item.Id == request.ListingId && item.IsActive,
            cancellationToken);
        if (listing is null) return NotFound();
        if (listing.ProviderId == user.Id)
            return BadRequest(new ProblemDetails { Title = "Tu ne peux pas demander ton propre service." });

        var serviceRequest = new ServiceRequest
        {
            ListingId = listing.Id,
            Listing = listing,
            RequesterId = user.Id,
            ProviderId = listing.ProviderId,
            Status = ServiceRequestStatus.Pending,
            PriceXaf = listing.PriceXaf,
            RequestedForUtc = request.RequestedForUtc,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            HandoverLocation = request.HandoverLocation.Trim(),
            Message = request.Message.Trim()
        };
        serviceRequest.Conversation = new Conversation { Request = serviceRequest };

        db.Requests.Add(serviceRequest);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = serviceRequest.Id },
            new RequestSummary(
                serviceRequest.Id,
                serviceRequest.ListingId,
                listing.Title,
                serviceRequest.RequesterId,
                serviceRequest.ProviderId,
                serviceRequest.Status,
                serviceRequest.PriceXaf,
                serviceRequest.RequestedForUtc,
                serviceRequest.CreatedAtUtc,
                serviceRequest.HandoverLocation,
                serviceRequest.Message));
    }

    [HttpPost("{id:guid}/accept")]
    public Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken) =>
        TransitionAsync(id, ServiceRequestStatus.Accepted, cancellationToken);

    [HttpPost("{id:guid}/decline")]
    public Task<IActionResult> Decline(Guid id, CancellationToken cancellationToken) =>
        TransitionAsync(id, ServiceRequestStatus.Declined, cancellationToken);

    [HttpPost("{id:guid}/cancel")]
    public Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken) =>
        TransitionAsync(id, ServiceRequestStatus.Cancelled, cancellationToken);

    [HttpPost("{id:guid}/confirm-completion")]
    public async Task<IActionResult> ConfirmCompletion(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var request = await db.Requests.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (request is null) return NotFound();
        if (user.Id != request.RequesterId && user.Id != request.ProviderId) return Forbid();
        if (request.Status is not (ServiceRequestStatus.Accepted or ServiceRequestStatus.CompletionPending))
            return Conflict(new ProblemDetails { Title = "Cette demande ne peut pas être terminée à cette étape." });

        var now = DateTimeOffset.UtcNow;
        if (user.Id == request.RequesterId)
            request.RequesterConfirmedCompletionAtUtc ??= now;
        else
            request.ProviderConfirmedCompletionAtUtc ??= now;

        if (request.RequesterConfirmedCompletionAtUtc is not null &&
            request.ProviderConfirmedCompletionAtUtc is not null)
        {
            request.Status = ServiceRequestStatus.Completed;
            request.CompletedAtUtc = now;
        }
        else
        {
            request.Status = ServiceRequestStatus.CompletionPending;
        }

        request.ConcurrencyToken = Guid.NewGuid();
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new ProblemDetails { Title = "La demande vient d'être mise à jour. Recharge-la et réessaie." });
        }

        return Ok(new { request.Id, request.Status, request.CompletedAtUtc });
    }

    private async Task<IActionResult> TransitionAsync(
        Guid id,
        ServiceRequestStatus targetStatus,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var request = await db.Requests.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (request is null) return NotFound();

        var isProviderAction = targetStatus is ServiceRequestStatus.Accepted or ServiceRequestStatus.Declined;
        var isParticipant = request.ProviderId == user.Id || request.RequesterId == user.Id;
        if (isProviderAction && request.ProviderId != user.Id) return Forbid();
        if (targetStatus == ServiceRequestStatus.Cancelled && !isParticipant) return Forbid();
        if (request.Status != ServiceRequestStatus.Pending &&
            !(targetStatus == ServiceRequestStatus.Cancelled && request.Status == ServiceRequestStatus.Accepted))
            return Conflict(new ProblemDetails { Title = "Cette transition de statut n'est pas autorisée." });

        request.Status = targetStatus;
        request.ConcurrencyToken = Guid.NewGuid();
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new ProblemDetails { Title = "La demande vient d'être mise à jour. Recharge-la et réessaie." });
        }
        return Ok(new { request.Id, request.Status });
    }
}

public sealed record CreateServiceRequest(
    [param: Required] Guid ListingId,
    [param: Required] DateTimeOffset RequestedForUtc,
    [param: Required, StringLength(160, MinimumLength = 2)] string HandoverLocation,
    [param: Required, StringLength(1000, MinimumLength = 2)] string Message);

public sealed record RequestSummary(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    Guid RequesterId,
    Guid ProviderId,
    ServiceRequestStatus Status,
    int PriceXaf,
    DateTimeOffset RequestedForUtc,
    DateTimeOffset CreatedAtUtc,
    string HandoverLocation,
    string Message);
