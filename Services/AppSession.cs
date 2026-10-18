using System.Globalization;

namespace CampusCart.Services;

public static class AppSession
{
    public static event EventHandler? Changed;

    public static bool IsAuthenticated { get; private set; }
    public static string FullName { get; private set; } = "Etudiant CampusCart";
    public static string Email { get; private set; } = string.Empty;
    public static string Initials => GetInitials(FullName);

    public static void SignIn(string email, string? fullName = null)
    {
        Email = email.Trim();
        FullName = string.IsNullOrWhiteSpace(fullName)
            ? BuildNameFromEmail(Email)
            : fullName.Trim();
        IsAuthenticated = true;
        Changed?.Invoke(null, EventArgs.Empty);
    }

    public static string GetInitials(string? fullName)
    {
        var parts = (fullName ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return string.Concat(parts
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));
    }

    public static void SignOut()
    {
        IsAuthenticated = false;
        Email = string.Empty;
        FullName = "Étudiant CampusCart";
        Changed?.Invoke(null, EventArgs.Empty);
    }

    private static string BuildNameFromEmail(string email)
    {
        var name = email.Split('@', 2)[0]
            .Replace('.', ' ')
            .Replace('_', ' ')
            .Replace('-', ' ');

        return CultureInfo.GetCultureInfo("fr-FR").TextInfo.ToTitleCase(name);
    }
}
