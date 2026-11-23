using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

/// <summary>
/// The balance sheet (BS) and profit and loss (PL) account structures, defined, activated and assigned to the
/// ledger through <see cref="AccountStructureBuilder"/>. Their main account ranges do not overlap, which is how
/// the main account selects a structure. An existing structure only has its ledger assignment converged, because
/// a structure activated before the ledger existed is never assigned and every posting then fails.
/// </summary>
public sealed class BaselineAccountStructureSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.Foundation.AccountStructures";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineFinancialDimensionSeeder.SeederId, BaselineLedgerSeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var builder = new AccountStructureBuilder(c.Db);
        foreach (var structure in GeneralLedgerBaselineCatalog.Load().AccountStructures)
        {
            var existing = await c.Rows<DimensionHierarchy>().FirstOrDefaultAsync(x => x.Name == structure.Name, ct);
            if (existing != null && (existing.IsDeleted || existing.IsDraft == NoYes.Yes))
                throw new InvalidOperationException($"Account structure '{structure.Name}' is deleted or a draft; review it before seeding.");
            if (existing == null)
            {
                var input = new AccountStructureCreateDto(structure.Name, structure.Description,
                    structure.Segments.Select(x => new AccountStructureSegmentDto(x.Dimension, x.Criteria)).ToArray());
                var (defined, error) = await builder.DefineAsync(c.Company, input, ct);
                if (defined == null) throw new InvalidOperationException($"Account structure '{structure.Name}' could not be defined: {error}");
                var (activated, activationError) = await builder.ActivateAsync(c.Company, structure.Name, ct);
                if (activated == null) throw new InvalidOperationException($"Account structure '{structure.Name}' could not be activated: {activationError}");
            }
            // Activation alone does not assign the structure, so the assignment is converged on every run, the first included.
            var (_, assignmentError) = await builder.AssignToLedgerAsync(c.Company, structure.Name, ct);
            if (assignmentError != null)
                throw new InvalidOperationException($"Account structure '{structure.Name}' could not be assigned to the ledger: {assignmentError}");
        }
    }
}
