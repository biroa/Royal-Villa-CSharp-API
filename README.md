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
| `Program.cs` | Minimal hosting pipeline: OpenAPI registration, HTTPS, development docs. |
| `Properties/launchSettings.json` | Local Kestrel URLs and environment variables per profile. |
| `RoyalVillaApi.csproj` | SDK, TFMs, package references (`Microsoft.AspNetCore.OpenApi`, `Scalar.AspNetCore`). |
| `RoyalVillaApi.http` | Sample REST client snippets (optional IDE support). |

---

## Packages

- **Microsoft.AspNetCore.OpenApi** — Generates and serves the OpenAPI document.
- **Scalar.AspNetCore** — Embeds Scalar for browsing and testing API operations in development.

---

## Contributing

Describe your branch naming, PR process, or coding standards here as your team settles them.

---

## License

Add your license choice for this repository (for example MIT, proprietary, etc.).
