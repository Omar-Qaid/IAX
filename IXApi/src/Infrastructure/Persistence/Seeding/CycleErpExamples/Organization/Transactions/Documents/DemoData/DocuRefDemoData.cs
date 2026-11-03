using System.Security.Cryptography;
using System.Text;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class DocuRefDemoData
{
    internal static DocuRef Create(Guid id, int tableId, long customerId, DocuType type, DocuValue value, string name, string notes, DateTime now) =>
        new DocuRef { DocumentId = id, RefTableId = tableId, RefRecId = customerId,
                RefCompanyId = "dat", ActualCompanyId = "dat", DataAreaId = "dat", TypeId = type.TypeId,
                DocuType = type, DocuValue = value, Name = name, Notes = notes, CreatedBy = "sys",
                LastModifiedBy = "sys", LastModifiedAt = now, OwnerAccountId = "sys" };
}
