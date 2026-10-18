# CampusCart

CampusCart est une application mobile .NET MAUI qui met en relation les étudiants pour trouver ou proposer des services sur le campus : cours, impression, dépannage et autres coups de main.

## Prérequis

- .NET 10 SDK
- La charge de travail .NET MAUI
- Un émulateur Android ou un appareil Android configuré

## Lancer le projet

```powershell
dotnet build CampusCart.csproj -f net10.0-android
```

Ouvre ensuite le projet dans Visual Studio ou déploie l’application sur un émulateur ou un appareil Android.

## Organisation

- `Pages/` : écrans de l’application
- `Controls/` : composants d’interface réutilisables
- `ViewModels/` : données et actions des écrans
- `Models/` : modèles de données
- `Services/` : état et données du prototype
- `Resources/` : images, polices et styles

Les offres et autres données du prototype sont conservées en mémoire pendant la session.
