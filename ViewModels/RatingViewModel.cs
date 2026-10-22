using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class RatingViewModel : BaseViewModel
{
    private int _rating;
    private string _comment = string.Empty;
    private string _message = string.Empty;
    private bool _isBusy;
    public ServiceRequest? Request => RequestFlowStore.RequestToReview;
    public string RatingLabel => _rating == 0 ? "Choisis une note" : $"{_rating}/5 · {RatingDescription} " + new string('★', _rating);
    public string RatingDescription => _rating switch { 1 => "Très décevant", 2 => "Décevant", 3 => "Correct", 4 => "Très bien", 5 => "Excellent", _ => string.Empty };
    public Color Star1Color => StarColor(1); public Color Star2Color => StarColor(2); public Color Star3Color => StarColor(3); public Color Star4Color => StarColor(4); public Color Star5Color => StarColor(5);
    public int Rating { get => _rating; private set { if (!SetProperty(ref _rating, value)) return; OnPropertyChanged(nameof(RatingLabel)); OnPropertyChanged(nameof(RatingDescription)); OnPropertyChanged(nameof(Star1Color)); OnPropertyChanged(nameof(Star2Color)); OnPropertyChanged(nameof(Star3Color)); OnPropertyChanged(nameof(Star4Color)); OnPropertyChanged(nameof(Star5Color)); } }
    public string Comment { get => _comment; set => SetProperty(ref _comment, value); }
    public string Message { get => _message; private set => SetProperty(ref _message, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string SubmitLabel => "Envoyer mon avis";
    public ICommand BackCommand { get; } public ICommand Rate1Command { get; } public ICommand Rate2Command { get; } public ICommand Rate3Command { get; } public ICommand Rate4Command { get; } public ICommand Rate5Command { get; } public ICommand SubmitCommand { get; }

    public RatingViewModel()
    {
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        Rate1Command = new Command(() => Rating = 1); Rate2Command = new Command(() => Rating = 2); Rate3Command = new Command(() => Rating = 3); Rate4Command = new Command(() => Rating = 4); Rate5Command = new Command(() => Rating = 5);
        SubmitCommand = new Command(async () => await SubmitAsync());
    }
    private Color StarColor(int value) => _rating >= value ? Color.FromArgb("#E6A72E") : Color.FromArgb("#D8DDE7");
    private async Task SubmitAsync()
    {
        if (Request is null || Request.Id == Guid.Empty) { Message = "Cette demande ne possède pas d’identifiant API."; return; }
        if (Rating is < 1 or > 5) { Message = "Sélectionne une note en étoiles avant d’envoyer ton avis."; return; }
        if (IsBusy) return;
        IsBusy = true;
        try { await App.Api.CreateReviewAsync(Request.Id, Rating, Comment); Message = "Merci, ton avis a été envoyé."; }
        catch (ApiException ex) { Message = ex.Message; }
        finally { IsBusy = false; }
    }
}
