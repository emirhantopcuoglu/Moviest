# Moviest

A Turkish-language film discovery platform built with ASP.NET Core 8 MVC. Browse popular, trending, and upcoming movies, manage a personal watchlist, and explore actor profiles — all powered by the TMDB API.

## Features

- **Movie Discovery** — Popular, now playing, top rated, upcoming, and weekly trending movies with pagination
- **Search** — Full-text search with sorting (by rating, year, title) and minimum rating filter; live autocomplete in the sidebar
- **Genres** — Browse movies by genre category
- **Movie Details** — Poster, overview, cast, trailers (embedded YouTube), and similar movies
- **Actor Profiles** — Actor biography, birth info, and full filmography
- **Watchlist** — Add/remove movies; mark as watched; leave a personal rating (1–10); filter and sort
- **Authentication** — Register, login, logout with ASP.NET Core Identity
- **2FA** — TOTP-based two-factor authentication via any authenticator app
- **Account** — Profile view, password change
- **Admin Panel** — User statistics dashboard and user management (delete regular users)

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 + SQL Server |
| Auth | ASP.NET Core Identity (TOTP 2FA) |
| External API | TMDB (The Movie Database) v3 |
| Frontend | Bootstrap 5.3, Bootstrap Icons 1.11 |
| Caching | In-memory cache (IMemoryCache) |

## Security

- Security headers middleware (CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy)
- Rate limiting: auth endpoints 10 req/5 min, API endpoints 60 req/min
- Account lockout after 5 failed login attempts (15-minute cooldown)
- Anti-CSRF tokens on all state-changing requests
- HTTPS redirect + HSTS in production
- HttpOnly, Secure, SameSite=Lax auth cookie
- Watchlist items are strictly scoped to the authenticated user

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server (local or remote)
- [TMDB API key](https://developer.themoviedb.org/docs/getting-started) (free)

## Setup

### 1. Clone

```bash
git clone https://github.com/your-username/moviest.git
cd moviest
```

### 2. Configure secrets

The project uses [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development. No credentials are stored in source files.

```bash
dotnet user-secrets set "ApiSettings:Key" "<your-tmdb-api-key>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=MoviestDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "AdminCredentials:Email" "admin@example.com"
dotnet user-secrets set "AdminCredentials:Password" "Admin@123456"
```

> In production, provide these values via environment variables or a secrets manager. The keys match the paths in `appsettings.json`.

### 3. Apply database migrations

```bash
dotnet ef database update
```

This creates the database and seeds the admin user defined in `AdminCredentials`.

### 4. Run

```bash
dotnet run
```

Navigate to `https://localhost:7062`.

## Project Structure

```
Moviest/
├── Controllers/         # MVC controllers
├── Models/              # View models & API response models
├── Services/            # TMDB API service (IMovieService)
├── Data/                # DbContext, seed data, EF migrations
├── Middleware/          # Security headers middleware
├── Constants/           # Roles, config keys, TMDB endpoint paths
├── Views/               # Razor views
│   ├── Shared/          # Layouts (_Layout, SideNavbarLayout)
│   ├── Movies/          # Movie pages
│   ├── Watchlist/       # Watchlist page
│   ├── Account/         # Auth & profile pages
│   └── Admin/           # Admin pages
└── wwwroot/             # Static assets (CSS, JS, Bootstrap)
```

## Configuration Reference

| Key | Description |
|-----|-------------|
| `ApiSettings:Key` | TMDB v3 API key |
| `ApiSettings:BaseUrl` | TMDB base URL (default: `https://api.themoviedb.org/3/`) |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `AdminCredentials:Email` | Email for the seeded admin account |
| `AdminCredentials:Password` | Password for the seeded admin account |

## License

MIT
