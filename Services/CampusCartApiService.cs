using System.Globalization;

namespace CampusCart.Services;

public sealed class CampusCartApiService(ApiClient api, SecureTokenStore tokens)
{
    public async Task RegisterAsync(string fullName, string email, string password) =>
        _ = await api.PostAsync<System.Text.Json.JsonElement>("api/v1/auth/register", new RegisterRequestDto(fullName, email, password));
    public async Task ConfirmEmailAsync(string email, string code) =>
        _ = await api.PostAsync<System.Text.Json.JsonElement>("api/v1/auth/confirm-email", new EmailCodeDto(email, code));
    public async Task ResendCodeAsync(string email) =>
        _ = await api.PostAsync<System.Text.Json.JsonElement>("api/v1/auth/resend-code", new EmailDto(email));
    public async Task<ProfileDto> LoginAsync(string email, string password)
    {
        var session = await api.PostAsync<TokenResponseDto>("api/v1/sessions/login?useCookies=false&useSessionCookies=false", new LoginRequestDto(email, password));
        await tokens.SaveAsync(session);
        return await GetProfileAsync();
    }
    public async Task<ProfileDto> RefreshSessionAsync()
    {
        var refresh = await tokens.GetRefreshTokenAsync();
        if (string.IsNullOrWhiteSpace(refresh)) throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Session expirée. Connecte-toi à nouveau.");
        var session = await api.PostAsync<TokenResponseDto>("api/v1/sessions/refresh", new RefreshTokenRequestDto(refresh));
        await tokens.SaveAsync(session);
        return await GetProfileAsync();
    }
    public async Task LogoutAsync() => await tokens.ClearAsync();
    public Task<ProfileDto> GetProfileAsync() => api.GetAsync<ProfileDto>("api/v1/profiles/me", true);
    public Task<ProfileDto> UpdateProfileAsync(string fullName) => api.PutAsync<ProfileDto>("api/v1/profiles/me", new UpdateProfileDto(fullName));
    public async Task<PagedDto<ListingDto>> GetListingsAsync(string? search = null, string? category = null)
    {
        var query = new List<string> { "page=1", "pageSize=50" };
        if (!string.IsNullOrWhiteSpace(search)) query.Add("search=" + Uri.EscapeDataString(search.Trim()));
        if (!string.IsNullOrWhiteSpace(category)) query.Add("category=" + Uri.EscapeDataString(category.Trim()));
        return await api.GetAsync<PagedDto<ListingDto>>($"api/v1/listings?{string.Join("&", query)}");
    }
    public Task<ListingDto> GetListingAsync(Guid id) => api.GetAsync<ListingDto>($"api/v1/listings/{id}");
    public Task<List<MyListingDto>> GetMyListingsAsync() => api.GetAsync<List<MyListingDto>>("api/v1/listings/mine", true);
    public Task<ListingDto> CreateListingAsync(string title, string category, string description, int price, string unit, string location) =>
        api.PostAsync<ListingDto>("api/v1/listings", new ListingRequestDto(title, category, description, price, unit, location), true);
    public Task<ListingDto> UpdateListingAsync(Guid id, string title, string category, string description, int price, string unit, string location) =>
        api.PutAsync<ListingDto>($"api/v1/listings/{id}", new ListingRequestDto(title, category, description, price, unit, location));
    public Task SetListingActiveAsync(Guid id, bool active) => api.PatchAsync($"api/v1/listings/{id}/active", new SetListingActiveDto(active));
    public Task DeleteListingAsync(Guid id) => api.DeleteAsync($"api/v1/listings/{id}", true);
    public Task<PagedDto<RequestDto>> GetRequestsAsync(bool sent) => api.GetAsync<PagedDto<RequestDto>>($"api/v1/requests?direction={(sent ? "sent" : "received")}&page=1&pageSize=50", true);
    public Task<RequestDto> CreateRequestAsync(CreateRequestDto request) => api.PostAsync<RequestDto>("api/v1/requests", request, true);
    public Task<System.Text.Json.JsonElement> TransitionRequestAsync(Guid id, string action) => api.PostAsync<System.Text.Json.JsonElement>($"api/v1/requests/{id}/{action}", new { }, true);
    public Task<List<ConversationDto>> GetConversationsAsync() => api.GetAsync<List<ConversationDto>>("api/v1/conversations", true);
    public Task<List<MessageDto>> GetMessagesAsync(Guid conversationId) => api.GetAsync<List<MessageDto>>($"api/v1/conversations/{conversationId}/messages?page=1&pageSize=100", true);
    public Task<MessageDto> SendMessageAsync(Guid conversationId, string body) => api.PostAsync<MessageDto>($"api/v1/conversations/{conversationId}/messages", new SendMessageDto(body), true);
    public Task<ReviewDto> CreateReviewAsync(Guid requestId, int rating, string? comment) => api.PostAsync<ReviewDto>("api/v1/reviews", new CreateReviewDto(requestId, rating, comment), true);
    public Task<ReviewsPageDto> GetReviewsAsync(Guid providerId) => api.GetAsync<ReviewsPageDto>($"api/v1/reviews/providers/{providerId}?page=1&pageSize=50");

    public static string CategoryForApi(string category) => category switch
    {
        "Dépannage" or "DÃ©pannage" or "Aide tech" => "Tech",
        "Soutien scolaire" => "Cours",
        "Autres services" or "Autre" => "Autre",
        _ => category
    };
    public static string CategoryForUi(string category) => category.ToUpperInvariant() switch
    {
        "TECH" or "AIDE TECH" => "Dépannage",
        "AUTRE" => "Autre",
        "IMPRESSION" => "Impression",
        "COURS" => "Cours",
        "LIVRAISON" => "Livraison",
        _ => category
    };
    public static string PriceLabel(int price, string unit) => $"{price.ToString("N0", CultureInfo.GetCultureInfo("fr-FR"))} FCFA / {unit}";
}
