using CampusCart.Api.Infrastructure.Persistence;
using CampusCart.Api.Modules.Auth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var keyRingPath = builder.Configuration["DataProtection:KeyRingPath"];
if (builder.Environment.IsDevelopment())
{
    keyRingPath ??= Path.Combine(builder.Environment.ContentRootPath, ".data-protection-keys");
}

if (!string.IsNullOrWhiteSpace(keyRingPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));
}

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});
builder.Services.Configure<AllowedEmailDomainsOptions>(
    builder.Configuration.GetSection("Registration"));
builder.Services.AddScoped<IUserValidator<CampusCartUser>, CampusCartUserValidator>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddTransient<IAccountEmailSender, DevelopmentAccountEmailSender>();
    builder.Services.AddTransient<IEmailSender<CampusCartUser>, DevelopmentEmailSender>();
}
else
{
    throw new InvalidOperationException(
        "Configure a production email sender before running CampusCart.Api outside Development.");
}

var verificationPepper = builder.Configuration["Verification:CodePepper"];
if (string.IsNullOrWhiteSpace(verificationPepper))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Configure Verification:CodePepper outside Development.");

    verificationPepper = "development-only-campuscart-verification-pepper";
}
builder.Services.AddSingleton(new EmailVerificationCodeService(verificationPepper));

var connectionString = builder.Configuration.GetConnectionString("CampusCart");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure ConnectionStrings:CampusCart before starting CampusCart.Api.");
}

builder.Services.AddDbContext<CampusCartDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddIdentityApiEndpoints<CampusCartUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<CampusCartDbContext>();

var app = builder.Build();

app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var identityEndpoints = app.MapGroup("/api/v1/sessions");
var disabledIdentityPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "/api/v1/sessions/register",
    "/api/v1/sessions/confirmEmail",
    "/api/v1/sessions/resendConfirmationEmail"
};
identityEndpoints.AddEndpointFilter(async (context, next) =>
{
    var path = context.HttpContext.Request.Path.Value?.TrimEnd('/');
    if (path is not null && disabledIdentityPaths.Contains(path))
    {
        return Results.NotFound();
    }

    return await next(context);
});
identityEndpoints.MapIdentityApi<CampusCartUser>()
    .RequireRateLimiting("auth");
app.MapControllers();
app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }))
    .AllowAnonymous();
app.MapGet("/health/ready", async (CampusCartDbContext db, CancellationToken cancellationToken) =>
{
    var databaseAvailable = await db.Database.CanConnectAsync(cancellationToken);
    return databaseAvailable
        ? Results.Ok(new { status = "ready", database = "available" })
        : Results.Problem(
            title: "Database unavailable",
            detail: "The API is running but cannot connect to PostgreSQL.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
}).AllowAnonymous();

app.Run();
