using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Identity.Authentication.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IAX.IXApi.Bootstrap.Extensions
{
    public static class AppSettingsExtensions
    {
        public static IServiceCollection AddCustomConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            // JWT
            IConfigurationSection jwtSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSection);

            // File Uploads
            //IConfigurationSection fileUploadsSection = configuration.GetSection("Uploads");
            //services.Configure<FileUploadSettings>(fileUploadsSection);

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            bool isDevelopment)
        {
            JwtSettings? jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

            if (jwtSettings == null ||
                string.IsNullOrWhiteSpace(jwtSettings.Secret) ||
                jwtSettings.Secret.Length < 32 ||
                string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
                string.IsNullOrWhiteSpace(jwtSettings.Audience))
            {
                throw new InvalidOperationException(
                    "JwtSettings must define a secret of at least 32 characters, an issuer, and an audience. " +
                    "Use environment variables or user-secrets; do not commit the signing secret.");
            }

            // APIs authenticate with the JWT bearer scheme, but the external-login
            // (OAuth) handshake completes against Identity's external cookie scheme,
            // so we leave DefaultSignInScheme as the one AddIdentity registered
            // (IdentityConstants.ExternalScheme) and only override authenticate/challenge.
            AuthenticationBuilder authBuilder = services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = !isDevelopment;
                x.SaveToken = false;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero,
                };
                x.Events  = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        // SignalR WebSocket/SSE handshakes can't send an Authorization header, so the
                        // JS client passes the JWT as the `access_token` query param. Read it for hub
                        // requests so the WebSocket transport authenticates (otherwise it fails the
                        // handshake and silently falls back to long-polling).
                        var accessToken = ctx.Request.Query["access_token"];
                        var path = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            ctx.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async ctx =>
                    {
                        var blacklist = ctx.HttpContext.RequestServices.GetRequiredService<ITokenBlacklist>();
                        var jti = ctx.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
                        if (!string.IsNullOrEmpty(jti))
                        {
                            var isRevoked = await blacklist.IsBlacklistedAsync(jti);
                            if (isRevoked)
                            {
                                ctx.Fail("Token has been revoked (logged out).");
                            }
                        }
                        // Blacklist check passed – token is valid.
                    }
                };
            });

            // ----- External / social sign-in (optional, config-driven) -----
            // Each provider is only registered when its ClientId is present, so the
            // app still boots before real credentials are filled into appsettings.
            // Microsoft covers Azure AD work/school accounts AND personal accounts
            // (outlook.com / hotmail.com / live.com) via the v2.0 "common" endpoint.
            IConfigurationSection microsoft = configuration.GetSection("Authentication:Microsoft");
            if (!string.IsNullOrWhiteSpace(microsoft["ClientId"]))
            {
                authBuilder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = microsoft["ClientId"]!;
                    options.ClientSecret = microsoft["ClientSecret"] ?? "";
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    // Restrict to a single Azure tenant by setting Authentication:Microsoft:TenantId;
                    // leave it blank to allow any Microsoft (work, school, or personal) account.
                    string? tenantId = microsoft["TenantId"];
                    if (!string.IsNullOrWhiteSpace(tenantId))
                    {
                        options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize";
                        options.TokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                    }
                    // The redirect URI Azure receives is built from this path. It MUST match a
                    // redirect URI registered on the Azure app (Web platform), exactly, with no
                    // query string. Defaults to "/signin-microsoft" when not configured.
                    string? callbackPath = microsoft["CallbackPath"];
                    if (!string.IsNullOrWhiteSpace(callbackPath))
                    {
                        // Strip any query/fragment — PathString must be a bare path, and Azure
                        // redirect URIs cannot contain query strings.
                        int cut = callbackPath.IndexOfAny(new[] { '?', '#' });
                        if (cut >= 0) callbackPath = callbackPath[..cut];
                        options.CallbackPath = callbackPath;
                    }
                    options.SaveTokens = true;
                });
            }

            // Google covers gmail.com (and Google Workspace) accounts.
            IConfigurationSection google = configuration.GetSection("Authentication:Google");
            if (!string.IsNullOrWhiteSpace(google["ClientId"]))
            {
                authBuilder.AddGoogle(options =>
                {
                    options.ClientId = google["ClientId"]!;
                    options.ClientSecret = google["ClientSecret"] ?? "";
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.SaveTokens = true;
                });
            }

            return services;
        }

        public static IApplicationBuilder UseCustomFileServer(this IApplicationBuilder app, IConfiguration configuration)
        {
            FileUploadSettings? fileUploadSettings = configuration.GetSection("Uploads").Get<FileUploadSettings>();

            if (fileUploadSettings == null)
            {
                throw new InvalidOperationException("Uploads section must be specified in appsettings.json");
            }

            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(fileUploadSettings.FileUploadDirectory),
                RequestPath = new PathString(fileUploadSettings.FileVirtualDirectory),
                EnableDirectoryBrowsing = false
            });

            return app;
        }
    }
}
