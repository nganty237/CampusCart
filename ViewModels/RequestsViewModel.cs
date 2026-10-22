using System.Collections.ObjectModel;
using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class RequestsViewModel : BaseViewModel
{
    private bool _showSent = true;
    private int _sentCount;
    private int _receivedCount;
    private string _statusMessage = string.Empty;
    private bool _isBusy;
    public ObservableCollection<ServiceRequest> Requests { get; } = new();
    public string UserInitials => AppSession.Initials;
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string SentTabLabel => $"Envoyées · {_sentCount}";
    public string ReceivedTabLabel => $"Reçues · {_receivedCount}";
    public Color SentTabBackground => _showSent ? Color.FromArgb("#315CEB") : Colors.White;
    public Color ReceivedTabBackground => _showSent ? Colors.White : Color.FromArgb("#315CEB");
    public Color SentTabTextColor => _showSent ? Colors.White : Color.FromArgb("#727D91");
    public Color ReceivedTabTextColor => _showSent ? Color.FromArgb("#727D91") : Colors.White;
    public ICommand ShowSentCommand { get; }
    public ICommand ShowReceivedCommand { get; }
    public ICommand ReviewRequestCommand { get; }

    public RequestsViewModel()
    {
        AppSession.Changed += (_, _) => OnPropertyChanged(nameof(UserInitials));
        ShowSentCommand = new Command(() => SetTab(true));
        ShowReceivedCommand = new Command(() => SetTab(false));
        ReviewRequestCommand = new Command<ServiceRequest>(async request =>
        {
            if (request is null || !request.CanReview) return;
            RequestFlowStore.SelectRequestForReview(request);
            await Shell.Current.GoToAsync(nameof(Pages.RatingPage));
        });
        LoadRequests();
    }

    public void RefreshRequests() => LoadRequests();

    private void SetTab(bool sent)
    {
        if (_showSent == sent) return;
        _showSent = sent;
        OnPropertyChanged(nameof(SentTabBackground)); OnPropertyChanged(nameof(ReceivedTabBackground));
        OnPropertyChanged(nameof(SentTabTextColor)); OnPropertyChanged(nameof(ReceivedTabTextColor));
        LoadRequests();
    }

    private async void LoadRequests()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var sent = await App.Api.GetRequestsAsync(true);
            var received = await App.Api.GetRequestsAsync(false);
            _sentCount = sent.TotalCount; _receivedCount = received.TotalCount;
            Requests.Clear();
            foreach (var item in (_showSent ? sent.Items : received.Items))
            {
                var request = ApiUiMapper.ToRequest(item, _showSent);
                request.ReviewCommand = ReviewRequestCommand;
                request.AcceptCommand = new Command(async () => await TransitionAsync(request, "accept"));
                request.DeclineCommand = new Command(async () => await TransitionAsync(request, "decline"));
                request.CancelCommand = new Command(async () => await TransitionAsync(request, "cancel"));
                request.CompleteCommand = new Command(async () => await TransitionAsync(request, "confirm-completion"));
                Requests.Add(request);
            }
            StatusMessage = string.Empty;
        }
        catch (ApiException ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
        OnPropertyChanged(nameof(SentTabLabel)); OnPropertyChanged(nameof(ReceivedTabLabel));
    }

    private async Task TransitionAsync(ServiceRequest request, string action)
    {
        try { await App.Api.TransitionRequestAsync(request.Id, action); LoadRequests(); }
        catch (ApiException ex) { StatusMessage = ex.Message; }
    }
}
