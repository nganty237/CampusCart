using IconFont.Maui.MaterialDesignIcons;

namespace CampusCart.Models;

/// <summary>Shared sample catalogue used by the home and service search screens.</summary>
public static class ServiceCatalog
{
    public static IReadOnlyList<ServiceListing> Listings { get; } =
    [
        new() { Category = "IMPRESSION", Title = "Impression documents A4", Provider = "Messina Richard", ProviderInitials = "MR", ProviderLevel = "Étudiant en informatique L5", Location = "Bibliothèque centrale · Entrée", Price = "500 FCFA", PriceNote = "Lot de 20 pages", Description = "Tes cours et dossiers imprimés en noir et blanc. Envoie ton fichier PDF et récupère tes documents à la bibliothèque centrale.", Rating = "4,9 · 18 avis", IconGlyph = MaterialDesignIconsOutlinedRegular.Print, ImageBackground = Color.FromArgb("#EDF3FF") },
        new() { Category = "AIDE TECH", Title = "Dépannage ordinateur", Provider = "Armand T. · Licence 2", ProviderInitials = "AT", ProviderLevel = "Étudiant en informatique L2", Location = "Campus 2", Price = "2 000 FCFA", PriceNote = "À partir de", Description = "Diagnostic et dépannage de ton ordinateur : installation de logiciels, configuration et conseils pratiques.", Rating = "4,8 (18)", IconGlyph = MaterialDesignIconsOutlinedRegular.Computer, ImageBackground = Color.FromArgb("#EEF3F8") },
        new() { Category = "SOUTIEN SCOLAIRE", Title = "Cours de mathématiques", Provider = "Kevin M. · Licence 3", ProviderInitials = "KM", ProviderLevel = "Étudiant en Licence 3", Location = "Campus 1", Price = "3 000 FCFA/h", PriceNote = "Séance d'une heure", Description = "Cours et exercices de mathématiques adaptés à ton niveau. Révision possible en petit groupe sur le campus.", Rating = "4,9 (27)", IconGlyph = MaterialDesignIconsOutlinedRegular.School, ImageBackground = Color.FromArgb("#F4F1E9") },
        new() { Category = "RÉDACTION", Title = "Rédaction de rapports", Provider = "Brice F. · Licence 1", ProviderInitials = "BF", ProviderLevel = "Étudiant en Licence 1", Location = "Campus 2", Price = "2 500 FCFA", PriceNote = "Selon le document", Description = "Relecture et mise en forme de rapports universitaires pour un document clair et soigné.", Rating = "4,7 (21)", IconGlyph = MaterialDesignIconsOutlinedRegular.Description, ImageBackground = Color.FromArgb("#EEF1F7") }
    ];
}
