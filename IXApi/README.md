# IAX IXApi

ASP.NET Core 9 modular-monolith API for the IAX platform.

The source is organized under `Modules`, `Shared`, `Infrastructure`, `Api`, and `Bootstrap`. See [ARCHITECTURE.md](ARCHITECTURE.md) for ownership and dependency rules.

## Local configuration

Secrets are intentionally absent from committed configuration. Configure them with environment variables or .NET user-secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DbConnString" "<SQL Server connection string>" --project IXApi.csproj
dotnet user-secrets set "JwtSettings:Secret" "<at least 32 random characters>" --project IXApi.csproj
```

The default JWT issuer is `IAX` and audience is `IAX.Client`. Override them through `JwtSettings__Issuer` and `JwtSettings__Audience` when required by the deployment.

Database initialization and seeding run at startup by default. Set `DatabaseInitialization__Enabled=false` only for build-time or infrastructure smoke checks that intentionally run without a database.

The notification worker is enabled by default. A database-free smoke check may set `Notifications__BackgroundServiceEnabled=false`.

## Validation

```powershell
dotnet restore IAX.slnx --configfile NuGet.Config
dotnet build IAX.slnx --configuration Release
dotnet test IAX.slnx --configuration Release
```

Scalar API reference is available in Development at `/scalar/v1`, with the OpenAPI document at `/openapi/v1.json`.

## Security defaults

- All endpoints require authentication unless explicitly marked `AllowAnonymous`.
- Public account registration is disabled; creating accounts requires the `Admin` role.
- JWT issuer, audience, signature, expiration, and lifetime are validated.
- Password lockout and strong password requirements are enabled.
- External authentication accepts local return URLs only and returns tokens in the URL fragment.
