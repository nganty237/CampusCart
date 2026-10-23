using System.Collections.ObjectModel;
using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class ChatViewModel : BaseViewModel, IQueryAttributable
{
    private static readonly TimeSpan MessageRefreshInterval = TimeSpan.FromSeconds(2);
    private readonly HashSet<Guid> _knownMessageIds = [];
    private readonly SemaphoreSlim _messageRefreshLock = new(1, 1);
    private CancellationTokenSource? _pollingCancellation;
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
        StopPolling();
        Conversation = conversation;
        Messages.Clear();
        _knownMessageIds.Clear();
        _pollingCancellation = new CancellationTokenSource();
        _ = PollConversationAsync(conversation, _pollingCancellation.Token);
    }

    public void StopPolling()
    {
        _pollingCancellation?.Cancel();
        _pollingCancellation = null;
    }

    private async Task PollConversationAsync(Conversation conversation, CancellationToken cancellationToken)
    {
        await RefreshMessagesAsync(conversation, cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;

        using var timer = new PeriodicTimer(MessageRefreshInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
                await RefreshMessagesAsync(conversation, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

    private async Task RefreshMessagesAsync(Conversation conversation, CancellationToken cancellationToken)
    {
        try
        {
            await _messageRefreshLock.WaitAsync(cancellationToken);
            try
            {
                var items = await App.Api.GetMessagesAsync(conversation.Id, cancellationToken);
                foreach (var item in items)
                    AddMessage(item);
                StatusMessage = string.Empty;
            }
            finally { _messageRefreshLock.Release(); }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (ApiException ex) { StatusMessage = ex.Message; }
    }

    private async void SendMessage()
    {
        var text = DraftMessage.Trim();
        if (text.Length == 0 || Conversation is null) return;
        try
        {
            var item = await App.Api.SendMessageAsync(Conversation.Id, text);
            AddMessage(item);
            DraftMessage = string.Empty; StatusMessage = string.Empty;
        }
        catch (ApiException ex) { StatusMessage = ex.Message; }
    }

    private void AddMessage(MessageDto item)
    {
        if (!_knownMessageIds.Add(item.Id)) return;
        Messages.Add(new ChatMessage(item.Body, item.CreatedAtUtc.ToLocalTime().ToString("HH:mm"),
            item.SenderId == AppSession.UserId, SenderName: item.SenderFullName));
    }
}
