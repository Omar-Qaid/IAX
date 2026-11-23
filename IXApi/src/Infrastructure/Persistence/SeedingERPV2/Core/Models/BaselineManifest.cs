using System.Text.Json;
using System.Text.Json.Serialization;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

/// <summary>
/// Reads a baseline data manifest, <c>Modules/{Module}/Data/baseline-*.json</c>, embedded as
/// <c>SeedingERPV2.Baseline.{file}</c>. Unknown members are rejected, so a typo in a manifest fails the run
/// instead of silently seeding a default.
/// </summary>
public static class BaselineManifest
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter() }
    };

    public static T Read<T>(string file)
    {
        using var stream = typeof(BaselineManifest).Assembly.GetManifestResourceStream("SeedingERPV2.Baseline." + file)
            ?? throw new InvalidOperationException("Missing embedded baseline manifest: " + file);
        return JsonSerializer.Deserialize<T>(stream, Options)
            ?? throw new InvalidOperationException("Empty baseline manifest: " + file);
    }
}
