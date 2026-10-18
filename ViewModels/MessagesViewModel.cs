using System.Collections.ObjectModel;
using System.Windows.Input;
using CampusCart.Models;

namespace CampusCart.ViewModels;

public sealed class MessagesViewModel : BaseViewModel
{
    private string _searchText = string.Empty;
    private readonly List<Conversation> _allConversations =
    [
        new("Messina Richard", "MR", "Impression documents A4", "Parfait, à 16 h à la bibliothèque !", "11:24", 1, Color.FromArgb("#DCE6FF")),
        new("Bikele Frank", "BF", "Cours de maths · L1", "Tu peux m'envoyer le chapitre ?", "10:48", 1, Color.FromArgb("#E8E5FF")),
        new("Patrick", "P", "Relecture de rapports", "Merci ! Je joins mon rapport.", "Hier", 0, Color.FromArgb("#DCE6FF")),
        new("Bikele Frank", "BF", "Relecture de rapports", "Merci pour les corrections, Gabriel.", "3 oct.", 0, Color.FromArgb("#E8E5FF"))
    ];

    public ObservableCollection<Conversation> Conversations { get; } = new();
    public ICommand OpenConversationCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value)) FilterConversations();
        }
    }

    public MessagesViewModel()
    {
        OpenConversationCommand = new Command<Conversation>(async conversation =>
        {
            if (conversation is null) return;

            await Shell.Current.GoToAsync(nameof(Pages.ChatPage), new Dictionary<string, object>
            {
                ["Conversation"] = conversation
            });
        });
        FilterConversations();
    }

    private void FilterConversations()
    {
        var query = SearchText.Trim();
        var conversations = string.IsNullOrWhiteSpace(query)
            ? _allConversations
            : _allConversations.Where(message =>
                message.ContactName.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                message.ServiceTitle.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                message.Preview.Contains(query, StringComparison.CurrentCultureIgnoreCase));

        Conversations.Clear();
        foreach (var conversation in conversations) Conversations.Add(conversation);
    }
}
