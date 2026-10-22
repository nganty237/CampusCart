using System.Collections.ObjectModel;
using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class ChatViewModel : BaseViewModel, IQueryAttributable
{
    private Conversation? _conversation;
    private string _draftMessage = string.Empty;
    private string _statusMessage = string.Empty;
    public Conversation? Conversation
    {
        get => _conversation;
        private set
        {
            if (!SetProperty(ref _conversation, value)) return;
            OnPropertyChanged(nameof(ContactName)); OnPropertyChanged(nameof(ServiceTitle));
            OnPropertyChanged(nameof(RequestSummary)); OnPropertyChanged(nameof(RequestPrice));
            OnPropertyChanged(nameof(RequestStatus)); OnPropertyChanged(nameof(RequestScheduleAndLocation));
        }
    }
    public string ContactName => Conversation?.ContactName ?? "Conversation";
    public string ServiceTitle => Conversation?.ServiceTitle ?? string.Empty;
    public string ConversationTimeLabel => Conversation?.TimeLabel ?? string.Empty;
    public string RequestSummary => Conversation is null ? string.Empty : $"{Conversation.RequestSummary} · {Conversation.Price}";
    public string RequestPrice => Conversation?.Price ?? string.Empty;
    public string RequestStatus => Conversation?.Status ?? "En cours";
    public string RequestScheduleAndLocation => Conversation is null ? string.Empty : $"{Conversation.Schedule} · {Conversation.Location}";
    public string DraftMessage { get => _draftMessage; set => SetProperty(ref _draftMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public ObservableCollection<ChatMessage> Messages { get; } = new();
    public ICommand SendCommand { get; }
    public ICommand GoBackCommand { get; }

    public ChatViewModel()
    {
        SendCommand = new Command(SendMessage, () => !string.IsNullOrWhiteSpace(DraftMessage));
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        PropertyChanged += (_, args) => { if (args.PropertyName == nameof(DraftMessage)) ((Command)SendCommand).ChangeCanExecute(); };
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("Conversation", out var value) || value is not Conversation conversation) return;
        Conversation = conversation;
        _ = LoadMessagesAsync(conversation);
    }

    private async Task LoadMessagesAsync(Conversation conversation)
    {
        Messages.Clear();
        try
        {
            var items = await App.Api.GetMessagesAsync(conversation.Id);
            foreach (var item in items)
                Messages.Add(new ChatMessage(item.Body, item.CreatedAtUtc.ToLocalTime().ToString("HH:mm"), item.SenderId == AppSession.UserId));
            StatusMessage = string.Empty;
        }
        catch (ApiException ex) { StatusMessage = ex.Message; }
    }

    private async void SendMessage()
    {
        var text = DraftMessage.Trim();
        if (text.Length == 0 || Conversation is null) return;
        try
        {
            var item = await App.Api.SendMessageAsync(Conversation.Id, text);
            Messages.Add(new ChatMessage(item.Body, item.CreatedAtUtc.ToLocalTime().ToString("HH:mm"), true));
            DraftMessage = string.Empty; StatusMessage = string.Empty;
        }
        catch (ApiException ex) { StatusMessage = ex.Message; }
    }
}
