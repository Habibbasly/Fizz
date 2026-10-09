# FizzBuzz

Web app FizzBuzz : API .NET 10 (Clean Architecture) + front Angular 20.

## Structure

```
FizzBuzz/
├── backend/
│   ├── FizzBuzz.slnx
│   ├── src/
│   │   ├── FizzBuzz.Domain/          # Entités et règles métier (aucune dépendance)
│   │   ├── FizzBuzz.Application/     # Cas d'usage, DTOs, options               → Domain
│   │   ├── FizzBuzz.Infrastructure/  # Implémentations techniques (vide pour l'instant) → Application
│   │   └── FizzBuzz.Api/             # Controllers, logging, health checks, CORS → Application, Infrastructure
│   └── tests/
│       ├── FizzBuzz.Domain.Tests/       # tests unitaires du domaine
│       ├── FizzBuzz.Application.Tests/  # tests unitaires du service
│       └── FizzBuzz.Api.Tests/          # tests d'intégration HTTP (WebApplicationFactory)
└── frontend/                         # Angular (standalone, signals, zoneless)
    └── src/
        ├── environments/  # environment.ts (dev), .preprod.ts, .prod.ts
        └── app/
            ├── core/      # services HTTP, modèles
            ├── features/  # fonctionnalités (fizzbuzz : pages + composants)
            └── shared/    # composants réutilisables
```

## Lancer en local

Backend (http://localhost:5029) :

```bash
cd backend
dotnet run --project src/FizzBuzz.Api --launch-profile http   # ou preprod / production
dotnet test
```

Documentation interactive (hors Production) : Swagger UI sur http://localhost:5029/swagger.

Frontend (http://localhost:4200, `/api` est proxifié vers le backend) :

```bash
cd frontend
npm install
npm start
```

## Tests

| Niveau | Où | Commande |
|---|---|---|
| Unitaires domaine / service | `backend/tests/FizzBuzz.Domain.Tests`, `FizzBuzz.Application.Tests` | `cd backend && dotnet test` |
| Intégration HTTP (API en mémoire : validation, config, environnements, CORS, erreurs, health checks, contrat OpenAPI) | `backend/tests/FizzBuzz.Api.Tests` | `cd backend && dotnet test` |
| Intégration front (page + service, backend simulé) | `frontend/src/**/*.spec.ts` | `cd frontend && npm test` (ou `npm run test:ci`) |
| Bout en bout (vrai backend + front) | `frontend/e2e` | `cd frontend && npx playwright install chromium && npm run e2e` |

Les tests Playwright démarrent eux-mêmes le backend et `ng serve` (ou réutilisent ceux déjà lancés en local).
La CI GitHub Actions (`.github/workflows/ci.yml`) exécute les trois niveaux sur chaque PR vers `dev`, `preprod` et `main`.

## API

`GET /api/fizzbuzz?int1=3&int2=5&limit=15&str1=Fizz&str2=Buzz`

→ `["1","2","Fizz","4","Buzz","Fizz","7","8","Fizz","Buzz","11","Fizz","13","14","FizzBuzz"]`

Retourne les nombres de 1 à `limit` (inclus) en remplaçant les multiples de `int1` par `str1`,
ceux de `int2` par `str2`, et ceux des deux par `str1str2`.

Paramètres (tous obligatoires) — sinon `400 Bad Request` (ProblemDetails) :

| Paramètre | Contrainte |
|---|---|
| `int1`, `int2` | entiers > 0 |
| `limit` | entier entre 1 et `FizzBuzz:MaxLimit` (10 000 par défaut) |
| `str1`, `str2` | chaînes non vides |

## Environnements

Trois environnements : **Development**, **Preprod**, **Production**.

### Backend

L'environnement est choisi par la variable `ASPNETCORE_ENVIRONMENT` (`Development`, `Preprod`, `Production`).
`appsettings.json` contient les valeurs communes, surchargées par `appsettings.{Environment}.json`,
puis par les variables d'environnement (ex : `FizzBuzz__MaxLimit=5000`, `Cors__AllowedOrigins__0=https://...`).

| | Development | Preprod | Production |
|---|---|---|---|
| Niveau de log | Debug | Information | Information (Microsoft.AspNetCore : Warning) |
| Format des logs | texte lisible | JSON (Compact) | JSON (Compact) |
| CORS | `http://localhost:4200` | `https://preprod.fizzbuzz.example.com` | `https://fizzbuzz.example.com` |
| OpenAPI (`/openapi/v1.json`) + Swagger UI (`/swagger`) | oui | oui | non |
| HSTS | non | oui | oui |

> Les URLs `*.example.com` sont des valeurs d'exemple à remplacer par les vraies.

Les options (`FizzBuzz:MaxLimit`) sont validées au démarrage : une configuration invalide empêche l'application de démarrer.

### Frontend

| Commande | Fichier d'environnement |
|---|---|
| `npm start` / `npm run build:dev` | `environment.ts` |
| `npm run build:preprod` | `environment.preprod.ts` |
| `npm run build:prod` | `environment.prod.ts` |

## Exploitation

### Logging

Serilog, sortie console (stdout) : à collecter par la plateforme (Docker, Kubernetes, Azure App Service...).
Chaque requête HTTP produit une ligne de log (méthode, chemin, code de retour, durée).
Les logs sont enrichis avec `Application` et `Environment`. Les appels aux health checks ne sont pas tracés.

### Health checks

| Endpoint | Usage |
|---|---|
| `GET /health/live` | Liveness : le processus répond |
| `GET /health/ready` | Readiness : l'application et ses dépendances sont prêtes |

Réponse JSON : `{ "status": "Healthy", "totalDuration": ..., "checks": [...] }` — HTTP 200 si sain, 503 sinon.
Les vérifications de dépendances futures (base de données, etc.) s'ajoutent dans
`FizzBuzz.Api/HealthChecks/HealthCheckExtensions.cs`.

### Erreurs

Les exceptions non gérées renvoient une réponse `500` au format ProblemDetails (sans stack trace) et sont loguées.
L'API prend en compte les en-têtes `X-Forwarded-For` / `X-Forwarded-Proto` lorsqu'elle est derrière un reverse proxy.

## Branches

| Branche | Rôle |
|---|---|
| `dev` | Intégration continue : les branches de fonctionnalité y sont fusionnées par pull request |
| `preprod` | Recette : reçoit `dev` lorsqu'une version est prête à être validée |
| `main` | Production : reçoit `preprod` une fois la version validée |

Les changements descendent toujours dans le même sens : `feature → dev → preprod → main`.
