using System.Security.Cryptography;
using System.Text;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class DocuValueDemoData
{
    internal static DocuValue Create(string name, string notes, int typeGroup, DateTime now) =>
        new DocuValue { Name = name, FileType = "note", Type = typeGroup, DataAreaId = "dat",
                DocumentHashNumber = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(notes))),
                CreatedBy = "sys", LastModifiedBy = "sys", LastModifiedAt = now, OwnerAccountId = "sys" };
}
