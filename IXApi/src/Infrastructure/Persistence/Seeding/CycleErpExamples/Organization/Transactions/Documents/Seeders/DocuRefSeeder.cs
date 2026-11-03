using System.Security.Cryptography;
using System.Text;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class DocuRefSeeder
{
    internal static async Task SeedAsync(DbContext db, Guid id, int tableId, long customerId, string name, string notes, CancellationToken ct)
    {
            if (await db.Set<DocuRef>().IgnoreQueryFilters().AnyAsync(x => x.RefTableId == tableId && x.RefRecId == customerId
                && x.RefCompanyId == "dat" && (x.DocumentId == id || x.Name == name), ct)) return;
            var type = await db.Set<DocuType>().IgnoreQueryFilters().SingleAsync(x => x.TypeId == "Note", ct);
            var value = DocuValueSeeder.CreateForAttachment(name, notes, type.TypeGroup, DateTime.UtcNow);

            db.Set<DocuRef>().Add(DocuRefDemoData.Create(id, tableId, customerId, type, value, name, notes, DateTime.UtcNow));
    }
}
