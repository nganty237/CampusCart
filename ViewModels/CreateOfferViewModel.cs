using System.Globalization;
using System.Windows.Input;
using CampusCart.Models;
using CampusCart.Services;

namespace CampusCart.ViewModels;

public sealed class CreateOfferViewModel : BaseViewModel
{
    private string _validationMessage = string.Empty;

    public OfferDraft Draft => OfferManagementStore.Draft;
    public string TitleCountLabel => $"{Draft.Title.Length} / 60 caractères";
    public string PublisherName => AppSession.FullName;
    public string PublisherInitials => AppSession.GetInitials(PublisherName);
    public string PublisherRole => "Étudiant en ingénierie L2";
    public int CurrentStep => OfferManagementStore.CurrentStep;
    public bool IsStepOne => CurrentStep == 1;
    public bool IsStepTwo => CurrentStep == 2;
    public bool IsStepThree => CurrentStep == 3;
    public string StepCaption => $"Étape {CurrentStep} sur 3 · {CurrentStep switch { 1 => "Ton service", 2 => "Ton tarif", _ => "Derniers détails" }}";
    public string PrimaryActionLabel => IsStepThree ? "Publier mon offre" : "Continuer";
    public Color StepOneColor => Color.FromArgb("#315CEB");
    public Color StepTwoColor => CurrentStep >= 2 ? Color.FromArgb("#315CEB") : Color.FromArgb("#E1E5EC");
    public Color StepThreeColor => CurrentStep >= 3 ? Color.FromArgb("#315CEB") : Color.FromArgb("#E1E5EC");
    public string ValidationMessage { get => _validationMessage; private set => SetProperty(ref _validationMessage, value); }
    public string PricePreview => string.IsNullOrWhiteSpace(Draft.Price) || string.IsNullOrWhiteSpace(Draft.Unit)
        ? "Ton tarif apparaîtra ici"
        : $"{Draft.Price} FCFA / {Draft.Unit}";

    public ICommand ContinueCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand ChooseCategoryCommand { get; }

    public CreateOfferViewModel()
    {
        ContinueCommand = new Command(async () => await ContinueAsync());
        ExitCommand = new Command(async () =>
        {
            if (CurrentStep > 1)
            {
                ChangeStep(CurrentStep - 1);
                return;
            }

            await Shell.Current.GoToAsync("..");
        });
        ChooseCategoryCommand = new Command<string>(category =>
        {
            if (!string.IsNullOrWhiteSpace(category)) Draft.Category = category;
        });
        Draft.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(OfferDraft.Price) or nameof(OfferDraft.Unit))
                OnPropertyChanged(nameof(PricePreview));
            if (args.PropertyName == nameof(OfferDraft.Title))
                OnPropertyChanged(nameof(TitleCountLabel));
        };
        OnStepChanged();
    }

    private async Task ContinueAsync()
    {
        ValidationMessage = string.Empty;
        if (CurrentStep == 1)
        {
            if (Draft.Title.Trim().Length < 4)
            {
                ValidationMessage = "Choisis un titre précis d'au moins 4 caractères.";
                return;
            }
            ChangeStep(2);
            return;
        }

        if (CurrentStep == 2)
        {
            var normalizedPrice = Draft.Price.Replace(" ", string.Empty).Replace("\u00A0", string.Empty).Replace("\u202F", string.Empty);
            if (!decimal.TryParse(normalizedPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price <= 0)
            {
                ValidationMessage = "Saisis un tarif supérieur à 0 FCFA.";
                return;
            }
            if (string.IsNullOrWhiteSpace(Draft.Unit))
            {
                ValidationMessage = "Précise l'unité du tarif (heure, rapport, page...).";
                return;
            }
            ChangeStep(3);
            return;
        }

        if (Draft.Description.Trim().Length < 15)
        {
            ValidationMessage = "Décris ton service en au moins 15 caractères.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Draft.Availability))
        {
            ValidationMessage = "Indique où et quand tu es disponible.";
            return;
        }

        OfferManagementStore.PublishDraft();
        await Shell.Current.GoToAsync(nameof(Pages.OfferPublishedPage));
    }

    private void ChangeStep(int step)
    {
        OfferManagementStore.CurrentStep = Math.Clamp(step, 1, 3);
        ValidationMessage = string.Empty;
        OnStepChanged();
    }

    private void OnStepChanged()
    {
        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(IsStepOne));
        OnPropertyChanged(nameof(IsStepTwo));
        OnPropertyChanged(nameof(IsStepThree));
        OnPropertyChanged(nameof(StepCaption));
        OnPropertyChanged(nameof(PrimaryActionLabel));
        OnPropertyChanged(nameof(StepOneColor));
        OnPropertyChanged(nameof(StepTwoColor));
        OnPropertyChanged(nameof(StepThreeColor));
    }
}
