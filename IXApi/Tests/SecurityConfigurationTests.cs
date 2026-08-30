using System.Text.Json;
using IAX.IXApi.Bootstrap.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class SecurityConfigurationTests
{
    [Fact]
    public void Identity_password_policy_requires_a_strong_password()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCustomIdentity();
        using var provider = services.BuildServiceProvider();

        var password = provider.GetRequiredService<IOptions<IdentityOptions>>().Value.Password;

        Assert.True(password.RequiredLength >= 12);
        Assert.True(password.RequiredUniqueChars >= 4);
        Assert.True(password.RequireUppercase);
        Assert.True(password.RequireLowercase);
        Assert.True(password.RequireDigit);
        Assert.True(password.RequireNonAlphanumeric);
    }

    [Fact]
    public void Committed_application_settings_do_not_contain_runtime_secrets()
    {
        var path = Path.Combine(FindApiRoot(), "appsettings.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path), new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        });

        var root = document.RootElement;
        var connectionString = root.GetProperty("ConnectionStrings").GetProperty("DbConnString").GetString();
        var jwtSecret = root.GetProperty("JwtSettings").GetProperty("Secret").GetString();

        Assert.True(string.IsNullOrWhiteSpace(connectionString));
        Assert.True(string.IsNullOrWhiteSpace(jwtSecret));
    }

    [Fact]
    public void Previously_compromised_signing_secret_is_rejected()
    {
        var compromisedSecret = string.Concat(
            "THIS_IS_A_VERY_LONG_",
            "SECRET_KEY_AT_LEAST_32_CHARACTERS");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = compromisedSecret,
                ["JwtSettings:Issuer"] = "IXApi",
                ["JwtSettings:Audience"] = "IXApp",
            })
            .Build();
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() =>
            services.AddJwtAuthentication(configuration, isDevelopment: false));
    }

    private static string FindApiRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "IXApi.csproj")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate the IXApi project root.");
    }
}
