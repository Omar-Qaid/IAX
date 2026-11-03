using System.Linq.Expressions;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;

/// <summary>Complete missing setup for the existing example company; existing values always win.</summary>
public sealed partial class ErpMasterSetupExampleSeeder : BaseModuleSeeder
{
    public override string Id => "ERP.MasterSetupExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] {
        "Finance.OperationalExamples", "Finance.Foundation.AccountStructures", "Finance.Foundation.HcmExamples" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        await SeedDataAsync(context.Db, ct);
    }

    public async Task SeedDataAsync(DbContext db, CancellationToken ct = default)
    {
        await SeedLedgerSetupAsync(db, ct);
        if (!await db.Set<VendLedgerAccounts>().IgnoreQueryFilters().AnyAsync(x =>
                x.DataAreaId == Scope && x.PostingProfile == "DEMO" && !x.IsDeleted, ct))
            throw new InvalidOperationException("The example vendor posting profile DEMO is missing.");
        await EnsureAsync(db, x => x.DataAreaId == Scope,
            new VendParameters { DataAreaId = Scope, Key = 0, VendPostingProfile = "DEMO", CreatedBy = "sys", OwnerAccountId = "sys" }, ct);
        await EnsureAsync(db, x => x.DataAreaId == Scope,
            new LedgerParameters { DataAreaId = Scope, Key_ = 0, CreatedBy = "sys", OwnerAccountId = "sys" }, ct);
        foreach (var (code, name) in new[] { ("ELECTRONIC", "Electronic vendor payment"), ("CHECK", "Vendor check payment") })
            await EnsureAsync(db, x => x.DataAreaId == Scope && x.PaymMode == code,
                new VendPaymModeTable { DataAreaId = Scope, PaymMode = code, Name = name,
                    AccountType = IAX.IXApi.Modules.Finance.Common.LedgerJournalACType.Bank,
                    PaymJournalNameId = "DEMO-AP", CreatedBy = "sys", OwnerAccountId = "sys" }, ct);
        await SeedWorkerAssignmentsAsync(db, ct);
    }

    private static async Task<T> EnsureAsync<T>(DbContext db, Expression<Func<T, bool>> key, T row, CancellationToken ct) where T : class
    {
        var existing = await db.Set<T>().IgnoreQueryFilters().SingleOrDefaultAsync(key, ct);
        if (existing != null)
        {
            var deleted = db.Entry(existing).Metadata.FindProperty("IsDeleted");
            if (deleted != null && db.Entry(existing).Property("IsDeleted").CurrentValue is true)
                throw new InvalidOperationException($"An example {typeof(T).Name} key belongs to a deleted record; it was preserved.");
            return existing;
        }
        db.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }
}
