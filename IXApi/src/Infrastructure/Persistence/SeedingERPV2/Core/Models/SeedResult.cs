namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

public sealed record SeedResult(string Company, IReadOnlyList<string> CompletedSeeders);
