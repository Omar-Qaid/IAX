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

internal static class DirPartyLocationDemoData
{
    internal static DirPartyLocation Create(long partyId, long locationId, string company) =>
        new DirPartyLocation { Party = partyId, Location = locationId, IsPostalAddress = NoYes.Yes,
                    IsRoleBusiness = NoYes.Yes, AssignmentDate = new DateTime(2026, 1, 1), DataAreaId = company };

}
