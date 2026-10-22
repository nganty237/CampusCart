using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class RatingViewModel : BaseViewModel
{
    private int _rating;
    private string _comment = string.Empty;
    private string _message = string.Empty;

    public ServiceRequest? Request => RequestFlowStore.RequestToReview;
    public string RatingLabel => _rating == 0 ? "Choisis une note" : $"{_rating}/5 · {RatingDescription} " + new string('★', _rating);
    public string RatingDescription => _rating switch
    {
        1 => "Très décevant",
        2 => "Décevant",
        3 => "Correct",
        4 => "Très bien",
        5 => "Excellent",
        _ => string.Empty
    };
    public Color Star1Color => StarColor(1);
    public Color Star2Color => StarColor(2);
    public Color Star3Color => StarColor(3);
    public Color Star4Color => StarColor(4);
    public Color Star5Color => StarColor(5);
    public int Rating
    {
        get => _rating;
        private set
        {
            if (!SetProperty(ref _rating, value)) return;
            OnPropertyChanged(nameof(RatingLabel));
            OnPropertyChanged(nameof(RatingDescription));
            OnPropertyChanged(nameof(Star1Color));
            OnPropertyChanged(nameof(Star2Color));
            OnPropertyChanged(nameof(Star3Color));
            OnPropertyChanged(nameof(Star4Color));
            OnPropertyChanged(nameof(Star5Color));
        }
    }
    public string Comment { get => _comment; set => SetProperty(ref _comment, value); }
    public string Message { get => _message; private set => SetProperty(ref _message, value); }
    public string SubmitLabel => Request is not null && RequestFlowStore.GetReview(Request) is not null
        ? "Mettre à jour mon avis"
        : "Envoyer mon avis";

    public ICommand BackCommand { get; }
    public ICommand Rate1Command { get; }
    public ICommand Rate2Command { get; }
    public ICommand Rate3Command { get; }
    public ICommand Rate4Command { get; }
    public ICommand Rate5Command { get; }
    public ICommand SubmitCommand { get; }

    public RatingViewModel()
    {
        if (Request is not null && RequestFlowStore.GetReview(Request) is { } review)
        {
            _rating = review.Rating;
            _comment = review.Comment;
        }

        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        Rate1Command = new Command(() => Rating = 1);
        Rate2Command = new Command(() => Rating = 2);
        Rate3Command = new Command(() => Rating = 3);
        Rate4Command = new Command(() => Rating = 4);
        Rate5Command = new Command(() => Rating = 5);
        SubmitCommand = new Command(Submit);
    }

    private Color StarColor(int value) => _rating >= value
        ? Color.FromArgb("#E6A72E")
        : Color.FromArgb("#D8DDE7");

    private void Submit()
    {
        if (Request is null)
        {
            Message = "Aucune demande à évaluer n'a été sélectionnée.";
            return;
        }

        if (Rating == 0)
        {
            Message = "Sélectionne une note en étoiles avant d'envoyer ton avis.";
            return;
        }

        RequestFlowStore.SubmitReview(Request, Rating, Comment);
        Message = "Merci, ton avis a été enregistré sur cet appareil.";
        OnPropertyChanged(nameof(SubmitLabel));
    }
}
