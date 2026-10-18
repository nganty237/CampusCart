using System.Globalization;

namespace CampusCart.Services;

public static class AppSession
{
    public static bool IsAuthenticated { get; private set; }
    public static string FullName { get; private set; } = "Nganty Gabriel";
    public static string Email { get; private set; } = string.Empty;

    public static void SignIn(string email, string? fullName = null)
    {
        Email = email.Trim();
        FullName = string.IsNullOrWhiteSpace(fullName)
            ? BuildNameFromEmail(Email)
            : fullName.Trim();
        IsAuthenticated = true;
    }

    public static void SignOut()
    {
        IsAuthenticated = false;
        Email = string.Empty;
        FullName = "Étudiant CampusCart";
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
