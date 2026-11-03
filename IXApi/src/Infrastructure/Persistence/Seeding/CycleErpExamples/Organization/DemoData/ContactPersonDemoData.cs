using System.Linq.Expressions;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using Microsoft.EntityFrameworkCore;


namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class ContactPersonDemoData
{
    internal static ContactPerson Create(long partyId, long customerParty, string customerAccount, long workerId) =>
        new ContactPerson { ContactPersonId = "DEMO-CONTACT-01", Party = partyId, ContactForParty = customerParty,
                CustAccount = customerAccount, MainResponsibleWorker = workerId,
                TimeAvailableFrom = 8 * 3600, TimeAvailableTo = 17 * 3600 };

}
