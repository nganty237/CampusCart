using System.Collections.ObjectModel;
using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class MessagesViewModel : BaseViewModel
{
    private static readonly TimeSpan ConversationRefreshInterval = TimeSpan.FromSeconds(4);
    private string _searchText = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;
    private CancellationTokenSource? _refreshCancellation;
    private readonly List<Conversation> _allConversations = [];
    public ObservableCollection<Conversation> Conversations { get; } = new();
    public string UserInitials => AppSession.Initials;
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public ICommand OpenConversationCommand { get; }
    public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) FilterConversations(); } }

    public MessagesViewModel()
    {
        AppSession.Changed += (_, _) => OnPropertyChanged(nameof(UserInitials));
        OpenConversationCommand = new Command<Conversation>(async conversation =>
        {
            if (conversation is null) return;
            await Shell.Current.GoToAsync(nameof(Pages.ChatPage), new Dictionary<string, object> { ["Conversation"] = conversation });
        });
    }

    public async Task LoadConversationsAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var items = await App.Api.GetConversationsAsync(cancellationToken);
            var refreshed = items.Select(ApiUiMapper.ToConversation).ToList();
            StatusMessage = items.Count == 0 ? "Aucune conversation pour le moment." : string.Empty;
            if (_allConversations.SequenceEqual(refreshed)) return;
            _allConversations.Clear(); _allConversations.AddRange(refreshed);
            FilterConversations();
        }
        catch (ApiException ex) { StatusMessage = ex.Message; }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally { IsBusy = false; }
    }

    public void StartAutoRefresh()
    {
        if (_refreshCancellation is not null) return;
        _refreshCancellation = new CancellationTokenSource();
        _ = RefreshConversationsLoopAsync(_refreshCancellation.Token);
    }

    public void StopAutoRefresh()
    {
        _refreshCancellation?.Cancel();
        _refreshCancellation = null;
    }

    private async Task RefreshConversationsLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(ConversationRefreshInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
                await LoadConversationsAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

    private void FilterConversations()
    {
        var query = SearchText.Trim();
        var filtered = string.IsNullOrWhiteSpace(query) ? _allConversations : _allConversations.Where(item =>
            item.ContactName.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
            item.ServiceTitle.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
            item.Preview.Contains(query, StringComparison.CurrentCultureIgnoreCase));
        Conversations.Clear(); foreach (var conversation in filtered) Conversations.Add(conversation);
    }
}
