using System.Collections.ObjectModel;
using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class MessagesViewModel : BaseViewModel
{
    private string _searchText = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;
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

    public async Task LoadConversationsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var items = await App.Api.GetConversationsAsync();
            _allConversations.Clear(); _allConversations.AddRange(items.Select(ApiUiMapper.ToConversation));
            StatusMessage = items.Count == 0 ? "Aucune conversation pour le moment." : string.Empty;
            FilterConversations();
        }
        catch (ApiException ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
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
