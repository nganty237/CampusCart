using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class CreateRequestViewModel : BaseViewModel
{
    private string _requestedTime = "Aujourd'hui · 16:00";
    private string _handoverLocation = "Bibliothèque centrale · Entrée";
    private string _message = string.Empty;
    private string _validationMessage = string.Empty;

    public ServiceListing? Offer => RequestFlowStore.SelectedOffer;
    public string MessageLabel => $"Message pour {Offer?.Provider ?? "l'étudiant"}";
    public string RequestedTime { get => _requestedTime; set => SetProperty(ref _requestedTime, value); }
    public string HandoverLocation { get => _handoverLocation; set => SetProperty(ref _handoverLocation, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }
    public string ValidationMessage { get => _validationMessage; private set => SetProperty(ref _validationMessage, value); }
    public ICommand SubmitCommand { get; }
    public ICommand BackCommand { get; }

    public CreateRequestViewModel()
    {
        Message = Offer?.Category.Contains("IMPRESSION", StringComparison.OrdinalIgnoreCase) == true
            ? "Bonjour ! Je voudrais imprimer 20 pages de cours. Je peux passer à 16 h."
            : $"Bonjour ! Je suis intéressé(e) par ton service {Offer?.Title ?? ""}. Peux-tu me confirmer ta disponibilité ?";
        SubmitCommand = new Command(async () => await SubmitAsync());
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private async Task SubmitAsync()
    {
        if (Offer is null)
        {
            ValidationMessage = "Cette offre n'est plus disponible. Retourne à l'accueil pour en choisir une autre.";
            return;
        }

        if (string.IsNullOrWhiteSpace(RequestedTime) || string.IsNullOrWhiteSpace(HandoverLocation) || string.IsNullOrWhiteSpace(Message))
        {
            ValidationMessage = "Complète la date, le lieu et ton message avant d'envoyer la demande.";
            return;
        }

        ValidationMessage = string.Empty;
        if (!TryParseRequestedTime(RequestedTime, out var requestedFor))
        {
            ValidationMessage = "Indique une date future, par exemple « Demain · 14:00 ».";
            return;
        }
        try
        {
            var dto = await App.Api.CreateRequestAsync(new CreateRequestDto(Offer.Id, requestedFor.ToUniversalTime(),
                HandoverLocation.Trim(), Message.Trim()));
            RequestFlowStore.SetLatestRequest(ApiUiMapper.ToRequest(dto, true));
            await Shell.Current.GoToAsync(nameof(Pages.RequestConfirmationPage));
        }
        catch (ApiException ex) { ValidationMessage = ex.Message; }
    }

    private static bool TryParseRequestedTime(string input, out DateTimeOffset result)
    {
        var parts = input.Split('·', StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && TimeOnly.TryParse(parts[1], out var time))
        {
            var date = parts[0].Equals("Aujourd'hui", StringComparison.OrdinalIgnoreCase) ? DateOnly.FromDateTime(DateTime.Now)
                : parts[0].Equals("Demain", StringComparison.OrdinalIgnoreCase) ? DateOnly.FromDateTime(DateTime.Now.AddDays(1))
                : DateOnly.TryParse(parts[0], out var parsed) ? parsed : DateOnly.MinValue;
            if (date != DateOnly.MinValue) { result = date.ToDateTime(time, DateTimeKind.Local); return result > DateTimeOffset.Now; }
        }
        return DateTimeOffset.TryParse(input, out result) && result > DateTimeOffset.Now;
    }
}
