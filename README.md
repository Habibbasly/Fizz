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

## Choix d'architecture

Un FizzBuzz tiendrait dans une Minimal API d'un seul fichier. L'architecture en couches est un **choix délibéré** :
le projet est traité comme une application destinée à vivre en production et à évoluer, pas comme un exercice jetable.

**Ce que le découpage apporte :**

- **Un métier isolé et testable.** `FizzBuzz.Domain` ne dépend d'aucun framework : `FizzBuzzRule` et `FizzBuzzGame`
  se testent en mémoire, sans HTTP ni configuration. La règle « str1str2 pour les multiples des deux » découle de la
  concaténation ordonnée des règles, pas d'un cas particulier codé en dur.
- **Une API qui ne fait que du HTTP.** Le contrôleur traduit la requête, délègue au service et convertit les erreurs
  en ProblemDetails. Changer de transport (Minimal API, gRPC, CLI) ne touche ni au domaine ni aux cas d'usage.
- **Des dépendances à sens unique.** `Api → Application → Domain` : le métier ne connaît jamais l'infrastructure.
- **Des tests alignés sur les couches.** Tests unitaires pour Domain et Application, tests d'intégration HTTP pour l'Api.

**`FizzBuzz.Infrastructure` est volontairement vide.** Il matérialise l'emplacement des implémentations techniques
(persistance, cache, appels externes) : par exemple, historiser les requêtes ou exposer des statistiques d'usage
se ferait dans ce projet, sans modifier le domaine.

**Le compromis assumé :** plus de projets et de fichiers qu'il n'en faut pour le besoin actuel.
Pour un utilitaire interne sans perspective d'évolution, une Minimal API dans un seul projet serait le bon choix.
Ici, le surcoût est faible et l'architecture montre comment le projet absorberait de nouvelles fonctionnalités.

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

## Lancer avec Docker

Prérequis : Docker (ou Docker Desktop) avec Compose. Depuis la racine du dépôt :

```bash
docker compose up --build
```

| Service | URL | Rôle |
|---|---|---|
| `web` | http://localhost:4200 | Front Angular servi par nginx, qui relaie `/api` vers le backend |
| `api` | http://localhost:5029 | API .NET (Swagger sur http://localhost:5029/swagger, health check sur `/health/ready`) |

Compose lance l'API en `Development` pour exposer Swagger. Pour se rapprocher de la production :

```bash
ASPNETCORE_ENVIRONMENT=Production docker compose up --build
```

Les images peuvent aussi être construites séparément :

```bash
docker build -t fizzbuzz-api ./backend
docker run --rm -p 5029:8080 fizzbuzz-api          # Production par défaut

docker build -t fizzbuzz-web ./frontend
```

Les deux images sont multi-stage (SDK / Node pour le build, runtime léger ensuite) et s'exécutent avec un utilisateur non-root.

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

Les paramètres sont validés explicitement (`FizzBuzzRequestValidator`) avant d'appeler le domaine : une saisie
invalide n'est pas traitée par exception. La réponse 400 est un `ValidationProblemDetails` qui liste toutes les erreurs
par paramètre, au même format que les erreurs de binding (paramètre manquant ou non numérique) :

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "int1": ["Le diviseur doit être strictement positif."],
    "limit": ["La limite ne peut pas dépasser 10000."]
  }
}
```

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
Aucun commit n'est poussé directement sur `dev`, `preprod` ou `main` : tout passe par une merge request (pull request sur GitHub).

### Traiter un ticket

1. **Partir de `dev` à jour**

   ```bash
   git checkout dev
   git pull origin dev
   ```

2. **Créer une branche dédiée au ticket**, nommée d'après son type, son numéro et son sujet :

   ```bash
   git checkout -b feature/FB-42-historique-requetes
   ```

   | Préfixe | Usage |
   |---|---|
   | `feature/` | Nouvelle fonctionnalité |
   | `fix/` | Correction de bug |
   | `chore/` | Outillage, dépendances, CI, documentation |
   | `hotfix/` | Correction urgente en production (voir plus bas) |

3. **Développer par petits commits**, chacun cohérent et compilable, avec un message à l'impératif qui cite le ticket :

   ```bash
   git commit -m "FB-42 Add request history endpoint"
   ```

   Avant de pousser : `dotnet test` dans `backend/`, puis `npm run lint` et `npm run test:ci` dans `frontend/`.

4. **Se resynchroniser avec `dev`** si elle a avancé pendant le développement, puis pousser la branche :

   ```bash
   git fetch origin
   git rebase origin/dev        # résoudre les conflits éventuels, relancer les tests
   git push -u origin feature/FB-42-historique-requetes
   ```

5. **Ouvrir une merge request vers `dev`**. La description indique le ticket, ce qui change, comment le tester
   et les points d'attention (migration, configuration, rupture de contrat d'API).

6. **Validation**
   - la CI doit être verte : tests backend, lint, build et tests front, tests de bout en bout ;
   - au moins une revue de code approuvée ; les remarques sont traitées par de nouveaux commits sur la même branche ;
   - l'auteur ne fusionne pas sans approbation.

7. **Fusion dans `dev`** depuis l'interface (squash ou merge commit selon la convention de l'équipe),
   puis suppression de la branche :

   ```bash
   git checkout dev
   git pull origin dev
   git branch -d feature/FB-42-historique-requetes
   ```

### Livraison

- **Vers la recette :** une merge request `dev → preprod` regroupe les tickets d'une version. Après déploiement,
  la recette valide les tickets en préproduction.
- **Vers la production :** une fois la recette validée, une merge request `preprod → main` est fusionnée et la
  version est étiquetée (`git tag v1.2.0`).
- **Hotfix :** une branche `hotfix/...` part de `main` et passe par une merge request vers `main`, puis la
  correction est reportée dans `preprod` et `dev` pour ne pas être perdue à la livraison suivante.
