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

## API backend

L'API ASP.NET Core est dans CampusCart.Api. Elle cible .NET 10 et utilise PostgreSQL avec Entity Framework Core.

Pour démarrer PostgreSQL en local :

~~~powershell
docker compose up -d postgres
$env:ConnectionStrings__CampusCart = "Host=localhost;Port=5433;Database=campuscart_dev;Username=campuscart_dev;Password=campuscart_dev_only"
dotnet tool install --global dotnet-ef --version 10.0.13
dotnet ef migrations add InitialCreate --project CampusCart.Api\CampusCart.Api.csproj --startup-project CampusCart.Api\CampusCart.Api.csproj --framework net10.0
dotnet ef database update --project CampusCart.Api\CampusCart.Api.csproj --startup-project CampusCart.Api\CampusCart.Api.csproj --framework net10.0
dotnet run --project CampusCart.Api
~~~

Les identifiants du conteneur sont réservés au développement local. Les inscriptions acceptent les adresses se terminant par `@enspd.cm`; le code de vérification est visible dans les logs en environnement Development. Les clés de protection sont conservées localement dans `CampusCart.Api/.data-protection-keys`. En production, configure `DataProtection__KeyRingPath` vers un stockage persistant et partagé, ainsi qu'un fournisseur d'e-mail avant de démarrer l'API.

La restauration NuGet et la compilation doivent être exécutées après avoir libéré de l'espace disque; l'environnement a refusé la restauration avec une erreur d'espace insuffisant.
