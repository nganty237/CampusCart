using CampusCart.Api.Infrastructure.Persistence;
using CampusCart.Api.Modules.Auth;
using CampusCart.Api.Modules.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CampusCart.Api.Modules.Messaging;

[ApiController]
[Authorize]
[Route("api/v1/conversations")]
public sealed class ConversationsController(
    CampusCartDbContext db,
    UserManager<CampusCartUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var conversations = await db.Conversations.AsNoTracking()
            .Where(conversation =>
                conversation.Request.RequesterId == user.Id ||
                conversation.Request.ProviderId == user.Id)
            .OrderByDescending(conversation => conversation.UpdatedAtUtc)
            .Select(conversation => new ConversationSummary(
                conversation.Id,
                conversation.RequestId,
                conversation.Request.Listing.Title,
                conversation.Request.RequesterId == user.Id
                    ? conversation.Request.ProviderId
                    : conversation.Request.RequesterId,
                conversation.UpdatedAtUtc,
                conversation.Messages
                    .OrderByDescending(message => message.CreatedAtUtc)
                    .Select(message => message.Body)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Ok(conversations);
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (page < 1 || pageSize is < 1 or > 100)
            return BadRequest(new ProblemDetails { Title = "Pagination parameters are invalid." });

        var conversation = await db.Conversations.AsNoTracking()
            .Where(item => item.Id == id &&
                (item.Request.RequesterId == user.Id || item.Request.ProviderId == user.Id))
            .Select(item => new { item.Id })
            .SingleOrDefaultAsync(cancellationToken);
        if (conversation is null) return NotFound();

        var messages = await db.ConversationMessages.AsNoTracking()
            .Where(message => message.ConversationId == id)
            .OrderByDescending(message => message.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(message => new ConversationMessageResponse(
                message.Id,
                message.SenderId,
                message.Body,
                message.CreatedAtUtc,
                message.ReadAtUtc))
            .ToListAsync(cancellationToken);

        messages.Reverse();
        return Ok(messages);
    }

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> SendMessage(
        Guid id,
        SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.Body))
        {
            ModelState.AddModelError(nameof(request.Body), "Le message ne peut pas être vide.");
            return ValidationProblem(ModelState);
        }

        var conversation = await db.Conversations
            .Include(item => item.Request)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (conversation is null) return NotFound();
        if (conversation.Request.RequesterId != user.Id &&
            conversation.Request.ProviderId != user.Id)
            return Forbid();

        if (conversation.Request.Status is ServiceRequestStatus.Declined
            or ServiceRequestStatus.Cancelled
            or ServiceRequestStatus.Completed)
        {
            return Conflict(new ProblemDetails { Title = "Cette conversation est en lecture seule." });
        }

        var message = new ConversationMessage
        {
            ConversationId = conversation.Id,
            SenderId = user.Id,
            Body = request.Body.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        conversation.UpdatedAtUtc = message.CreatedAtUtc;
        db.ConversationMessages.Add(message);
        await db.SaveChangesAsync(cancellationToken);

        return Created(
            $"/api/v1/conversations/{conversation.Id}/messages",
            new ConversationMessageResponse(
                message.Id,
                message.SenderId,
                message.Body,
                message.CreatedAtUtc,
                message.ReadAtUtc));
    }
}

public sealed record SendMessageRequest(
    [param: Required, StringLength(2000, MinimumLength = 1)] string Body);

public sealed record ConversationSummary(
    Guid Id,
    Guid RequestId,
    string ListingTitle,
    Guid OtherParticipantId,
    DateTimeOffset UpdatedAtUtc,
    string? LastMessage);

public sealed record ConversationMessageResponse(
    Guid Id,
    Guid SenderId,
    string Body,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReadAtUtc);
