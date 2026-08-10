using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Api.Middleware;
using IAX.IXApi.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;
using System.Threading.Tasks;

namespace IAX.IXApi.Bootstrap.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseInfrastructureMiddleware(this IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMiddleware<CorrelationIdMiddleware>();
            // app.UseMiddleware<ActAsMiddleware>(); // Commented out as it was missing

            return app;
        }

        public static IApplicationBuilder UseApiDocumentation(this WebApplication app)
        {
            // .NET 9 uses app.MapOpenApi() in Program.cs
            app.MapScalarApiReference(options =>
            {
                options.Title = "Human Capital Management (HCM) API Reference";
                options.Theme = ScalarTheme.BluePlanet;
                options.DefaultHttpClient = new(ScalarTarget.JavaScript, ScalarClient.Fetch);
                options.ShowSidebar = true;
            }).AllowAnonymous();

            return app;
        }

        public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //await db.Database.EnsureDeletedAsync();
            //await db.Database.MigrateAsync();


            var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeederService>();
            await seeder.SeedAsync();
        }
    }
}
