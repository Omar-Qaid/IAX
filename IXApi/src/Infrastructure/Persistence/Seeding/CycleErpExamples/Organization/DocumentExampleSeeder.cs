using System.Security.Cryptography;
using System.Text;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed class DocumentExampleSeeder : BaseModuleSeeder
{
    public override string Id => "Organization.DocumentExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Organization.DocumentManagement", "Finance.AccountsReceivable.DemoCustVend" };
    public override Task SeedAsync(SeedingContext context, CancellationToken ct) => SeedExamplesAsync(context.Db, ct);

    public async Task SeedExamplesAsync(DbContext db, CancellationToken ct = default)
    {
        var customer = await db.Set<CustTable>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == "dat" && x.AccountNum == "CUST-100", ct);
        // 2001 is the shared customer attachment table ID in IXApp recordTableIds.ts.
        const int customerTableId = 2001;
        foreach (var (id, name, notes) in new[] {
            (new Guid("8514c27f-60da-42e2-9079-53692617f101"), "Example customer onboarding", "Training example: verify billing contact and agreed payment terms before the first order."),
            (new Guid("8514c27f-60da-42e2-9079-53692617f102"), "Example delivery instructions", "Training example: call the receiving team before delivery; inspect packaging on arrival.") })
        {
            if (await db.Set<DocuRef>().IgnoreQueryFilters().AnyAsync(x => x.RefTableId == customerTableId && x.RefRecId == customer.RecId
                && x.RefCompanyId == "dat" && (x.DocumentId == id || x.Name == name), ct)) continue;
            var type = await db.Set<DocuType>().IgnoreQueryFilters().SingleAsync(x => x.TypeId == "Note", ct);
            var value = new DocuValue { Name = name, FileType = "note", Type = type.TypeGroup, DataAreaId = "dat",
                DocumentHashNumber = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(notes))),
                CreatedBy = "sys", LastModifiedBy = "sys", LastModifiedAt = DateTime.UtcNow, OwnerAccountId = "sys" };
            db.Set<DocuRef>().Add(new DocuRef { DocumentId = id, RefTableId = customerTableId, RefRecId = customer.RecId,
                RefCompanyId = "dat", ActualCompanyId = "dat", DataAreaId = "dat", TypeId = type.TypeId,
                DocuType = type, DocuValue = value, Name = name, Notes = notes, CreatedBy = "sys",
                LastModifiedBy = "sys", LastModifiedAt = DateTime.UtcNow, OwnerAccountId = "sys" });
        }
        await db.SaveChangesAsync(ct);
    }
}
