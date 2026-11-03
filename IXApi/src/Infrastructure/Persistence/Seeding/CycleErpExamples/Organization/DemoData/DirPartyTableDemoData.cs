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

internal static class DirPartyTableDemoData
{
    internal static DirPartyTable Create() =>
        new DirPartyTable { PartyNumber = "DEMO-CONTACT-01", Name = "Example receiving contact", NameAlias = "جهة اتصال الاستلام التجريبية",
                LanguageId = "ar-sa", IsActive = NoYes.Yes };

    internal static DirPartyTable CreateCustomerParty(string number, string name, string alias) => new()
    {
        PartyNumber = number, Name = name, NameAlias = alias, LanguageId = "ar-sa",
        AddressBookNames = string.Empty, DataAreaId = "dat", IsActive = NoYes.Yes,
        CreatedBy = "sys", OwnerAccountId = "sys"
    };
}
