# RoyalVillaApi

ASP.NET Core minimal API project for the Royal Villa backend. It exposes API metadata in development through **OpenAPI** and an interactive reference UI powered by **Scalar**.

---

## Prerequisites

| Requirement | Notes |
|-------------|-------|
| [.NET SDK 10](https://dotnet.microsoft.com/download) | Target framework is `net10.0`. |
| [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) | Recommended for Cursor / VS Code debugging (`coreclr`). |

Restore and build:

```bash
dotnet restore
dotnet build
```

---

## Run the application

For the most reliable results, start with an explicit launch profile and then follow the URLs printed in the console (`Now listening on: ...`).

| Profile | Command | URLs (see `Properties/launchSettings.json`) |
|---------|---------|-----------------------------------------------|
| **HTTPS** | `dotnet run --project RoyalVillaApi.csproj --launch-profile https` | `https://localhost:7274` and `http://localhost:5028` |
| **HTTP only** | `dotnet run --project RoyalVillaApi.csproj --launch-profile http` | `http://localhost:5028` |
| **Plain `dotnet run`** | `dotnet run` | Uses a launch profile automatically; verify with the `Now listening on:` output (may be HTTP). |

Ensure `ASPNETCORE_ENVIRONMENT` is **`Development`** when you want interactive docs (see below). The launch profiles already set this.

---

## Development-only features

These endpoints are registered only when the app runs in **`Development`**:

| Resource | Typical URL |
|----------|-------------|
| OpenAPI JSON | `https://localhost:7274/openapi/v1.json` (or `http://localhost:5028/openapi/v1.json`) |
| Scalar UI | `https://localhost:7274/scalar/` (or `http://localhost:5028/scalar/`) |

Production builds use **HSTS** and **HTTPS redirection**. OpenAPI and Scalar are not mapped outside Development.

Implementation notes:

- The OpenAPI document’s **`servers`** entry is aligned with each request’s scheme and host (`https://localhost:7274`, etc.).
- Scalar uses a **dynamic base server URL** so “try it” calls match how you loaded the UI (HTTP vs HTTPS).

---

## HTTPS development certificate

`https://localhost` uses ASP.NET Core’s **development HTTPS certificate**. Browsers warn until that certificate is trusted.

1. Generate or refresh trust:

   ```bash
   dotnet dev-certs https --trust
   ```

2. On **Linux**, you may also need extra steps so Chromium-based browsers trust the certificate. Microsoft documents this workflow here: **[Trust the HTTPS development certificate](https://aka.ms/dev-certs-trust)**. A common approach is installing the PEM from `~/.aspnet/dev-certs/trust/` into the system certificate store (`update-ca-certificates` on Debian/Ubuntu derivatives).

Use **`localhost`** in the URL, not `127.0.0.1`, so it matches the certificate.

---

## Debugging in Cursor / VS Code

The repository includes `.vscode/launch.json` and `.vscode/tasks.json`.

| Configuration | Purpose |
|----------------|---------|
| **RoyalVillaApi: Development (HTTPS)** | Build, then `dotnet run` with HTTPS profile + `Development` env. Optionally opens Scalar in the browser. |
| **RoyalVillaApi: Development (HTTP only)** | Same, HTTP-only profile (avoids local TLS warnings if you have not trusted the dev cert yet). |

Start debugging from the Run and Debug view or press **F5** after selecting a configuration.

---

## Project layout (high level)

| Path | Role |
|------|------|
| `Program.cs` | Minimal hosting pipeline: OpenAPI, EF Core (`AddDbContext<AppDbContext>` + PostgreSQL), HTTPS, development docs. |
| `Properties/launchSettings.json` | Local Kestrel URLs and environment variables per profile. |
| `Data/AppDbContext.cs` | EF Core `DbContext`; register entities with `DbSet<>` when you add models. |
| `RoyalVillaApi.csproj` | SDK, TFMs, packages (OpenAPI, Scalar, EF Core, Npgsql provider). |
| `RoyalVillaApi.http` | Sample REST client snippets (optional IDE support). |

---

## Packages

- **Microsoft.AspNetCore.OpenApi** — Generates and serves the OpenAPI document.
- **Scalar.AspNetCore** — Embeds Scalar for browsing and testing API operations in development.
- **Microsoft.EntityFrameworkCore** — ORM and change tracking for database access.
- **Microsoft.EntityFrameworkCore.Design** — Design-time support for EF Core (for example `dotnet ef` migrations); referenced with private assets so it is not published with the app.
- **Npgsql.EntityFrameworkCore.PostgreSQL** — EF Core database provider for PostgreSQL.

---

## Contributing

Describe your branch naming, PR process, or coding standards here as your team settles them.

---

## License

Add your license choice for this repository (for example MIT, proprietary, etc.).

---

## Installation steps

Follow these steps to install dependencies and align your machine with what this repository already configures in code.

### 1. Install the .NET SDK

Install [.NET SDK 10](https://dotnet.microsoft.com/download) (the project targets `net10.0`). Confirm with:

```bash
dotnet --version
```

### 2. Install and run PostgreSQL

Install PostgreSQL on your machine and start the database service. Create a database for this API (name should match `Database=` in your connection string, for example `royalvilla`):

```bash
createdb royalvilla
```

Alternatively, in `psql`:

```sql
CREATE DATABASE royalvilla;
```

### 3. Configure the connection string

The app reads **`ConnectionStrings:DefaultConnection`** from configuration (see `appsettings.json`). Use an [Npgsql connection string](https://www.npgsql.org/doc/connection-string-parameters.html), for example:

```text
Host=localhost;Port=5432;Database=royalvilla;Username=YOUR_USER;Password=YOUR_PASSWORD
```

Adjust `Port`, `Username`, and `Password` for your local instance. For production or shared repos, prefer [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables instead of committing passwords.

### 4. Restore and build

From the directory that contains `RoyalVillaApi.csproj` (the same folder as this README):

```bash
dotnet restore
dotnet build
```

### 5. (Optional) EF Core CLI for migrations

If you plan to manage the schema with migrations, install the global EF tool once:

```bash
dotnet tool install --global dotnet-ef
```

After you add entity types and `DbSet<>` mappings to `Data/AppDbContext.cs`, you can create and apply migrations, for example:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### What this repository already wires up

You do not need to repeat these steps unless you are recreating the setup from scratch; they document the current codebase:

| Area | What is configured |
|------|--------------------|
| **Packages** (`RoyalVillaApi.csproj`) | `Microsoft.EntityFrameworkCore` (10.0.7), `Microsoft.EntityFrameworkCore.Design` (10.0.7, private assets), `Npgsql.EntityFrameworkCore.PostgreSQL` (10.0.0), plus OpenAPI and Scalar packages. |
| **`Program.cs`** | `builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));` |
| **`Data/AppDbContext.cs`** | `AppDbContext` deriving from `DbContext` with constructor injection of `DbContextOptions<AppDbContext>`. |

Until you add models and query the context at runtime, the API will build without contacting PostgreSQL; any endpoint that uses `AppDbContext` will require PostgreSQL to be running and the connection string to be valid.
