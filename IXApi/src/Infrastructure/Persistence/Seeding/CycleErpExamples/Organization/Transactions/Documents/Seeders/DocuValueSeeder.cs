using System.Security.Cryptography;
using System.Text;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class DocuValueSeeder
{
    // The reference owns the insert, so its value is persisted atomically through the navigation.
    internal static DocuValue CreateForAttachment(string name, string notes, int typeGroup, DateTime now) =>
        DocuValueDemoData.Create(name, notes, typeGroup, now);
}
