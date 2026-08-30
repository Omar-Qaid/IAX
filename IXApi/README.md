# IAX IXApi

ASP.NET Core 9 modular-monolith API for the IAX platform.

The source is organized under `Modules`, `Shared`, `Infrastructure`, `Api`, and `Bootstrap`. See [ARCHITECTURE.md](ARCHITECTURE.md) for ownership and dependency rules.

## Local configuration

Secrets are intentionally absent from committed configuration. Configure them with environment variables or .NET user-secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DbConnString" "<SQL Server connection string>" --project IXApi.csproj
dotnet user-secrets set "JwtSettings:Secret" "<at least 32 random characters>" --project IXApi.csproj
```

The checked-in non-secret defaults use JWT issuer `IXApi` and audience `IXApp`. Override them through `JwtSettings__Issuer` and `JwtSettings__Audience` when required by the deployment.

Database initialization and seeding are disabled by default. Enable them only in an explicitly controlled non-production environment with `DatabaseInitialization__Enabled=true`. Production startup rejects this setting.

Database seeding creates roles, permissions, synthetic lookup/workflow data, and no user accounts. Provision the first administrator through an approved operational process; the repository intentionally contains no default administrator password.

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
- Password lockout and a 12-character mixed-character password policy are enabled.
- External authentication accepts local return URLs only and returns tokens in the URL fragment.
