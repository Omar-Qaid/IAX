using IAX.IXApi.Api.Filters;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Interceptors;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Identity.Authentication.Authentication;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.RateLimiting;

namespace IAX.IXApi.Bootstrap.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
        {
            services.AddScoped<AuditInterceptor>();
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DbConnString"), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                });
                options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
                options.ConfigureWarnings(warnings => warnings.Ignore(
                    CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning,
                    CoreEventId.SensitiveDataLoggingEnabledWarning,
                    SqlServerEventId.ByteIdentityColumnWarning
                ));
                if (isDevelopment)
                {
                    options.EnableSensitiveDataLogging();
                }
            });
            services.AddScoped<IAX.IXApi.Modules.Identity.Persistence.IIdentityDataContext>(sp =>
                sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IAX.IXApi.Modules.Administration.Persistence.IAdministrationDataContext>(sp =>
                sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IAX.IXApi.Modules.Organization.Persistence.IOrganizationDataContext>(sp =>
                sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IAX.IXApi.Modules.Finance.Persistence.IFinanceDataContext>(sp =>
                sp.GetRequiredService<ApplicationDbContext>());

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            services.AddHttpContextAccessor();

            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddHealthChecks();
            services.AddResponseCompression();
            services.AddRequestDecompression();

            return services;
        }

        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((doc, ctx, ct) =>
                {
                    doc.Components ??= new OpenApiComponents();
                    doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Description = "Enter 'Bearer {your JWT token}'"
                    };

                    doc.SecurityRequirements.Add(new OpenApiSecurityRequirement
                    {
                        [doc.Components.SecuritySchemes["Bearer"]] = new List<string>()
                    });

                    return Task.CompletedTask;
                });
            });

            services.AddProblemDetails(opts =>
            {
                opts.CustomizeProblemDetails = ctx =>
                {
                    ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
                };
            });

            services.AddExceptionHandler<IAX.IXApi.Api.Middleware.GlobalExceptionHandler>();

            services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = ErrorResponse.GenerateErrorResponse;
            });

            return services;
        }

        public static IServiceCollection AddCustomIdentity(this IServiceCollection services)
        {
            services.AddIdentity<AspNetUser, AspNetRole>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.User.RequireUniqueEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(o =>
            {
                o.AddFixedWindowLimiter("tight", opts =>
                {
                    opts.PermitLimit = 150;
                    opts.Window = TimeSpan.FromSeconds(10);
                    opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opts.QueueLimit = 100;
                });

                o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                {
                    var userId = ctx.User?.Identity?.IsAuthenticated == true
                        ? ctx.User.FindFirst("sub")?.Value ?? ctx.User.Identity!.Name!
                        : ctx.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: userId,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 50,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });
                });

                o.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
            });

            return services;
        }

        public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
        {
            // Dynamic policy provider resolves "permission:Module.Action" policies at runtime.
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            return services;
        }

        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
