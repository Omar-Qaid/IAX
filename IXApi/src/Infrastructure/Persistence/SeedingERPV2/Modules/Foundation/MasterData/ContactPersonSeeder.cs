using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
public sealed class ContactPersonSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.ContactPersonSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.HcmWorkerSeeder", "ERPV2.DirPartyTableSeeder", "ERPV2.CurrencySeeder", "ERPV2.PaymTermSeeder", "ERPV2.TaxGroupHeadingSeeder", "ERPV2.Foundation.PaymSchedLineSeeder", "ERPV2.Foundation.DlvModeSeeder", "ERPV2.Foundation.DimensionSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var worker = await c.RequireAsync<HcmWorker>(x => x.PersonnelNumber == "FNV2-BUYER", ct);
        var vendor = await c.RequireAsync<DirPartyTable>(x => x.PartyNumber == $"APV2-{c.Company}-LOCAL", ct);
        await c.EnsureAsync<PaymTerm>(x => x.PaymTermId == "FNV2-60", new() { PaymTermId = "FNV2-60", Description = "Foundation two installments", NumOfDays = 60, PaymMethod = PaymMethod.Net, PaymSched = "FNV2" }, ct);
        var dimensions = await c.RequireAsync<DimensionAttributeValueSet>(x => x.DepartmentValue == "FNV2-FIN", ct);
        var account = $"FNV2-{c.Company}-CUSTOMER";
        await c.EnsureAsync<IAX.IXApi.Modules.Finance.AccountsReceivable.CustTable>(x => x.AccountNum == account,
            new() { AccountNum = account, Party = vendor.RecId, CurrencyCode = "SAR", PaymTermId = "FNV2-60", TaxGroupId = "APV2", DlvModeId = "FNV2", DefaultDimension = dimensions.RecId }, ct);
        await c.EnsureAsync<ContactPerson>(x => x.ContactPersonId == "FNV2-BUYER", new() { ContactPersonId = "FNV2-BUYER", Party = worker.Person, ContactForParty = vendor.RecId, CustAccount = account, MainResponsibleWorker = worker.RecId }, ct);
    }
}
