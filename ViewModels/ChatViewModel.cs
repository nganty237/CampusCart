using System.Collections.ObjectModel;
using System.Windows.Input;
using CampusCart.Models;

namespace CampusCart.ViewModels;

public sealed class ChatViewModel : BaseViewModel, IQueryAttributable
{
    private Conversation? _conversation;
    private string _draftMessage = string.Empty;

    public Conversation? Conversation
    {
        get => _conversation;
        private set
        {
            if (!SetProperty(ref _conversation, value)) return;
            OnPropertyChanged(nameof(ContactName));
            OnPropertyChanged(nameof(ServiceTitle));
            OnPropertyChanged(nameof(RequestSummary));
            OnPropertyChanged(nameof(RequestPrice));
            OnPropertyChanged(nameof(RequestStatus));
            OnPropertyChanged(nameof(RequestScheduleAndLocation));
        }
    }

    public string ContactName => Conversation?.ContactName ?? "Conversation";
    public string ServiceTitle => Conversation?.ServiceTitle ?? string.Empty;
    public string RequestSummary => Conversation is null ? string.Empty : $"{Conversation.RequestSummary} · {Conversation.Price}";
    public string RequestPrice => Conversation?.Price ?? string.Empty;
    public string RequestStatus => Conversation?.Status ?? "En cours";
    public string RequestScheduleAndLocation => Conversation is null
        ? string.Empty
        : $"{Conversation.Schedule} · {Conversation.Location}";

    public string DraftMessage
    {
        get => _draftMessage;
        set => SetProperty(ref _draftMessage, value);
    }

    public ObservableCollection<ChatMessage> Messages { get; } = new();
    public ICommand SendCommand { get; }
    public ICommand GoBackCommand { get; }

    public ChatViewModel()
    {
        SendCommand = new Command(SendMessage, () => !string.IsNullOrWhiteSpace(DraftMessage));
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(DraftMessage))
                ((Command)SendCommand).ChangeCanExecute();
        };
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("Conversation", out var value) || value is not Conversation conversation) return;

        Conversation = conversation;
        LoadConversationMessages(conversation);
    }

    private void LoadConversationMessages(Conversation conversation)
    {
        Messages.Clear();

        if (conversation.ContactName == "Messina Richard")
        {
            Messages.Add(new("Bonjour ! Je voudrais imprimer 20 pages de cours. Je peux passer à 16 h.", "11:20", true));
            Messages.Add(new("Salut Gabriel ! Oui, envoie-moi ton PDF. Tes documents seront prêts à 16 h.", "11:21", false));
            Messages.Add(new("Merci Richard ! Voici mon document :", "11:22", true, "cours-biologie.pdf"));
            Messages.Add(new("Parfait, à 16 h à la bibliothèque !", "11:24", false));
            return;
        }

        Messages.Add(new($"Bonjour ! Je te contacte pour le service « {conversation.ServiceTitle} ».", conversation.TimeLabel, true));
        Messages.Add(new(conversation.Preview, conversation.TimeLabel, false));
    }

    private void SendMessage()
    {
        var text = DraftMessage.Trim();
        if (text.Length == 0) return;

        Messages.Add(new ChatMessage(text, DateTime.Now.ToString("HH:mm"), true));
        DraftMessage = string.Empty;
    }
}
