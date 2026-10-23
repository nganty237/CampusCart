using System.Net;

namespace CampusCart.Services;

// Client-owned contracts. These intentionally mirror the public JSON API without
// referencing the ASP.NET Core project or its EF entities.
public sealed record RegisterRequestDto(string FullName, string Email, string Password);
public sealed record EmailCodeDto(string Email, string Code);
public sealed record EmailDto(string Email);
public sealed record LoginRequestDto(string Email, string Password);
public sealed record TokenResponseDto(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);
public sealed record RefreshTokenRequestDto(string RefreshToken);
public sealed record ProfileDto(Guid Id, string FullName, string Email, bool EmailConfirmed,
    int CompletedServicesCount, double? AverageRating, int ReviewCount);
public sealed record UpdateProfileDto(string FullName);
public sealed record ListingDto(Guid Id, string Title, string Category, string Description, int PriceXaf,
    string Unit, string Location, DateTimeOffset CreatedAtUtc, string ProviderFullName,
    double? AverageRating, int ReviewCount);
public sealed record MyListingDto(Guid Id, string Title, string Category, string Description, int PriceXaf,
    string Unit, string Location, bool IsActive, DateTimeOffset CreatedAtUtc, string ProviderFullName,
    double? AverageRating, int ReviewCount);
public sealed record PagedDto<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
public sealed record ListingRequestDto(string Title, string Category, string Description, int PriceXaf, string Unit, string Location);
public sealed record SetListingActiveDto(bool IsActive);
public sealed record CreateRequestDto(Guid ListingId, DateTimeOffset RequestedForUtc, string HandoverLocation, string Message);
public sealed record RequestDto(Guid Id, Guid ListingId, string ListingTitle, Guid RequesterId, Guid ProviderId,
    string RequesterFullName, string ProviderFullName,
    int Status, int PriceXaf, DateTimeOffset RequestedForUtc, DateTimeOffset CreatedAtUtc,
    string HandoverLocation, string Message);
public sealed record ConversationDto(Guid Id, Guid RequestId, string ListingTitle, Guid OtherParticipantId,
    string OtherParticipantFullName, DateTimeOffset UpdatedAtUtc, string? LastMessage);
public sealed record MessageDto(Guid Id, Guid SenderId, string SenderFullName, string Body, DateTimeOffset CreatedAtUtc, DateTimeOffset? ReadAtUtc);
public sealed record SendMessageDto(string Body);
public sealed record CreateReviewDto(Guid RequestId, int Rating, string? Comment);
public sealed record ReviewDto(Guid Id, Guid RequestId, Guid ProviderId, int Rating, string? Comment, DateTimeOffset CreatedAtUtc);
public sealed record ReviewsPageDto(IReadOnlyList<ReviewDto> Items, int Page, int PageSize, int TotalCount);

public sealed class ApiException(HttpStatusCode statusCode, string message, IReadOnlyList<string>? errors = null)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public IReadOnlyList<string> Errors { get; } = errors ?? [];
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
}
