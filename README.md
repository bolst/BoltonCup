# Bolton Cup

## Overview

This is the repository for the software tools developed for the Bolton Cup hockey tournament. It is a full-stack web platform for managing tournament registration, team and player data, game scheduling, scoring, and statistics.

**Website:** [https://boltoncup.ca](https://boltoncup.ca)

## Tech Stack

| Category | Technology |
|---|---|
| Runtime | .NET 10 / C# 13 |
| UI | Blazor WebAssembly, Blazor Server, MudBlazor |
| API | ASP.NET Core Web API |
| Database | PostgreSQL (via Entity Framework Core + Npgsql) |
| Auth | ASP.NET Core Identity, JWT Bearer tokens |
| Payments | Stripe |
| Object Storage | Cloudflare R2 (AWS S3 SDK) |
| Email | MailKit + RazorLight templating |
| Error Tracking | Sentry |
| API Docs | OpenAPI/Swagger + Scalar UI |
| Containerization | Docker |
| CI/CD | GitHub Actions |

## Project Structure

```
src/
├── BoltonCup.Core                    # Domain entities, interfaces, exceptions, commands/queries
├── BoltonCup.Infrastructure          # EF Core data access, repositories, services, email, S3
├── BoltonCup.WebAPI                  # ASP.NET Core REST API (16 controllers)
├── BoltonCup.WebClient               # Public-facing Blazor WASM website
├── BoltonCup.Admin                   # Blazor Server admin portal
├── BoltonCup.Auth                    # Blazor WASM authentication service
├── BoltonCup.Common                  # Shared auth/authorization Razor components
├── BoltonCup.Shared                  # Shared components, API SDK generation
├── BoltonCup.SessionStorage          # Blazor WASM browser session storage utilities
└── BoltonCup.WebClient.SitemapGenerator  # Console app for XML sitemap generation
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/)
- [Docker](https://www.docker.com/) (for containerized deployment)
- A [Stripe](https://stripe.com/) account (for payments)
- A [Cloudflare R2](https://developers.cloudflare.com/r2/) or AWS S3 bucket (for asset storage)
- An SMTP server (for email sending)
- A [Sentry](https://sentry.io/) DSN (for error tracking)

## Getting Started

### 1. Clone the repository

```sh
git clone https://github.com/bolst/BoltonCup.git
cd BoltonCup
```

### 2. Configure environment

Use [.NET user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development to avoid committing credentials to `appsettings.json`:

```sh
cd src/BoltonCup.WebAPI
dotnet user-secrets set "BoltonCup:ConnectionString" "<postgresql-connection-string>"
dotnet user-secrets set "CloudflareR2:AccountId" "<account-id>"
dotnet user-secrets set "CloudflareR2:AccessKey" "<access-key>"
dotnet user-secrets set "CloudflareR2:SecretKey" "<secret-key>"
dotnet user-secrets set "CloudflareR2:BaseUrl" "<base-url>"
dotnet user-secrets set "Stripe:ApiKey" "<stripe-secret-key>"
dotnet user-secrets set "Stripe:WebhookSecret" "<stripe-webhook-secret>"
dotnet user-secrets set "Sentry:Dsn" "<sentry-dsn>"
```

For production, set the equivalent values as environment variables or in your container orchestration platform. The expected configuration keys are:

```json
{
  "BoltonCup": {
    "ConnectionString": "<postgresql-connection-string>"
  },
  "CloudflareR2": {
    "AccountId": "<account-id>",
    "AccessKey": "<access-key>",
    "SecretKey": "<secret-key>",
    "BaseUrl": "<base-url>"
  },
  "Stripe": {
    "ApiKey": "<stripe-secret-key>",
    "WebhookSecret": "<stripe-webhook-secret>"
  },
  "Sentry": {
    "Dsn": "<sentry-dsn>"
  }
}
```

### 3. Install workloads and restore dependencies

```sh
dotnet workload install wasm-tools
dotnet restore
```

### 4. Apply database migrations

```sh
cd src
dotnet ef database update --project BoltonCup.Infrastructure --startup-project BoltonCup.WebAPI -c BoltonCupDbContext
```

### 5. Run the projects

Run the API and any client app you want to work on:

```sh
dotnet run --project src/BoltonCup.WebAPI
dotnet run --project src/BoltonCup.WebClient
dotnet run --project src/BoltonCup.Admin
```

## Deployment

The WebAPI and Admin portal are containerized. Docker images are published to Docker Hub automatically via GitHub Actions on changes to their respective source directories.

| Image | Docker Hub |
|---|---|
| WebAPI | `bolst/boltoncup-webapi:latest` |
| Admin | `bolst/boltoncup-admin:latest` |

The public WebClient (Blazor WASM) is deployed as a static site to GitHub Pages.

### CI/CD Pipelines

| Workflow | Trigger | Action |
|---|---|---|
| `publish-dockerized-webapi` | Push to `src/BoltonCup.WebAPI/**` | Build and push WebAPI Docker image |
| `publish-dockerized-admin` | Push to `src/BoltonCup.Admin/**` | Build and push Admin Docker image |
| `publish-ghpages` | Push to `src/BoltonCup.WebClient/**` | Build WASM app and deploy to GitHub Pages |
| `publish-ghpages-auth` | Push to `src/BoltonCup.Auth/**` | Build and deploy Auth app to GitHub Pages |

### Adding a New EF Core Migration

```sh
cd src
dotnet ef migrations add <MigrationName> --project BoltonCup.Infrastructure --startup-project BoltonCup.WebAPI -c BoltonCupDbContext
```

## Using the shared library `BoltonCup.Shared`

Add to `_Imports.razor`:

```razor
@using BoltonCup.Shared.Data
@using BoltonCup.Shared.Components.Shared
```

Add to the relevant sections of `Program.cs`:

```c#
using BoltonCup.Shared.Data;
```

```c#
builder.Services.AddBoltonCupServices(builder.Configuration);
```