using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;
using System.Security.Cryptography;
using System.Text;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using Microsoft.EntityFrameworkCore;


namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal sealed class DocumentExampleService
{
    internal async Task SeedExamplesAsync(DbContext db, CancellationToken ct = default)
    {
        var customer = await db.Set<CustTable>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == "dat" && x.AccountNum == "CUST-100", ct);
        // 2001 is the shared customer attachment table ID in IXApp recordTableIds.ts.
        const int customerTableId = 2001;
        foreach (var (id, name, notes) in new[] {
            (new Guid("8514c27f-60da-42e2-9079-53692617f101"), "Example customer onboarding", "Training example: verify billing contact and agreed payment terms before the first order."),
            (new Guid("8514c27f-60da-42e2-9079-53692617f102"), "Example delivery instructions", "Training example: call the receiving team before delivery; inspect packaging on arrival.") })
        {
            await DocuRefSeeder.SeedAsync(db, id, customerTableId, customer.RecId, name, notes, ct);
        }
        await db.SaveChangesAsync(ct);
    }
}
