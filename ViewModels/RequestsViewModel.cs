using System.Collections.ObjectModel;
using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class RequestsViewModel : BaseViewModel
{
    private bool _showSent = true;

    private readonly ServiceRequest[] _receivedRequests =
    [
        new("Aujourd'hui · 11:30 · 1 000 FCFA", "À répondre", "Relecture de rapports",
            "Patrick", "P", "Campus ENSPD", Color.FromArgb("#E8E5FF"), Color.FromArgb("#315CEB")),
        new("Demain · 09:00 · 2 500 FCFA", "Acceptée", "Cours de mathématiques",
            "Messina Richard", "MR", "Bibliothèque centrale", Color.FromArgb("#DCE6FF"), Color.FromArgb("#2DA875"))
    ];

    public ObservableCollection<ServiceRequest> Requests { get; } = new();
    public string UserInitials => AppSession.Initials;
    public string SentTabLabel => $"Envoyées · {RequestFlowStore.SentRequests.Count}";
    public string ReceivedTabLabel => $"Reçues · {RequestFlowStore.ReceivedRequestCount}";
    public Color SentTabBackground => _showSent ? Color.FromArgb("#315CEB") : Colors.White;
    public Color ReceivedTabBackground => _showSent ? Colors.White : Color.FromArgb("#315CEB");
    public Color SentTabTextColor => _showSent ? Colors.White : Color.FromArgb("#727D91");
    public Color ReceivedTabTextColor => _showSent ? Color.FromArgb("#727D91") : Colors.White;
    public ICommand ShowSentCommand { get; }
    public ICommand ShowReceivedCommand { get; }

    public RequestsViewModel()
    {
        AppSession.Changed += OnSessionChanged;

        ShowSentCommand = new Command(() => SetTab(true));
        ShowReceivedCommand = new Command(() => SetTab(false));
        LoadRequests();
    }

    private void OnSessionChanged(object? sender, EventArgs e) =>
        OnPropertyChanged(nameof(UserInitials));

    public void RefreshRequests()
    {
        OnPropertyChanged(nameof(SentTabLabel));
        OnPropertyChanged(nameof(ReceivedTabLabel));
        LoadRequests();
    }

    private void SetTab(bool showSent)
    {
        if (_showSent == showSent) return;
        _showSent = showSent;
        OnPropertyChanged(nameof(SentTabLabel));
        OnPropertyChanged(nameof(ReceivedTabLabel));
        OnPropertyChanged(nameof(SentTabBackground));
        OnPropertyChanged(nameof(ReceivedTabBackground));
        OnPropertyChanged(nameof(SentTabTextColor));
        OnPropertyChanged(nameof(ReceivedTabTextColor));
        LoadRequests();
    }

    private void LoadRequests()
    {
        Requests.Clear();
        var requests = _showSent ? RequestFlowStore.SentRequests : _receivedRequests;
        foreach (var request in requests) Requests.Add(request);
    }
}
