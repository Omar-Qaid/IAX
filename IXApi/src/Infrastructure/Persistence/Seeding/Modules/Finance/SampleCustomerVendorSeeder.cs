using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance
{
    public class SampleCustomerVendorSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.AccountsReceivable.DemoCustVend";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Demo;
        public override IReadOnlyCollection<string> DependsOn => new[]
        {
            "Finance.AccountsReceivable.CustGroups",
            "Finance.Foundation.Logistics", "Finance.GeneralLedger.Currencies", "Finance.GeneralLedger.Tax"
        };
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "dat";

        public override async Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        {
            var db = context.Db;
            var createdBy = "sys";

            var customerSeeds = new[]
            {
                (Account: "C00015", PartyNumber: "CUST-C00015", Name: "Consultant Customer", NameAlias: "عميل استشاري", Group: "Consultant", Currency: "SAR"),
                (Account: "CUST-100", PartyNumber: "CUST-100", Name: "Consulting Customer 100", NameAlias: "عميل استشارات 100", Group: "Consultant", Currency: "SAR"),
                (Account: "CUST-200", PartyNumber: "CUST-200", Name: "Retail Customer 200", NameAlias: "عميل تجزئة 200", Group: "CUST-RTL", Currency: "SAR"),
                (Account: "CUST-300", PartyNumber: "CUST-300", Name: "International Customer 300", NameAlias: "عميل دولي 300", Group: "Consultant", Currency: "USD"),
                (Account: "CUS-00001", PartyNumber: "CUS-00001", Name: "Retail Customer 1", NameAlias: "عميل تجزئة 1", Group: "CUST-RTL", Currency: "SAR"),
                (Account: "CUS-00002", PartyNumber: "CUS-00002", Name: "VIP Customer 2", NameAlias: "عميل مميز 2", Group: "CUST-VIP", Currency: "USD")
            };

            var existingCustomers = await db.CustTables.IgnoreQueryFilters()
                .Where(x => x.DataAreaId == "dat")
                .ToDictionaryAsync(x => x.AccountNum, StringComparer.OrdinalIgnoreCase, cancellationToken);

            var partyNumbers = customerSeeds.Select(x => x.PartyNumber).ToArray();
            var customerParties = await db.Set<DirPartyTable>().IgnoreQueryFilters()
                .Where(x => partyNumbers.Contains(x.PartyNumber))
                .ToDictionaryAsync(x => x.PartyNumber, StringComparer.OrdinalIgnoreCase, cancellationToken);

            foreach (var seed in customerSeeds.Where(x => !existingCustomers.ContainsKey(x.Account)))
            {
                if (customerParties.ContainsKey(seed.PartyNumber)) continue;
                var party = new DirPartyTable
                {
                    PartyNumber = seed.PartyNumber,
                    Name = seed.Name,
                    NameAlias = seed.NameAlias,
                    LanguageId = "ar-sa",
                    AddressBookNames = string.Empty,
                    DataAreaId = "dat",
                    IsActive = NoYes.Yes,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy
                };
                db.Set<DirPartyTable>().Add(party);
                customerParties[seed.PartyNumber] = party;
            }
            await db.SaveChangesAsync(cancellationToken);

            var custsToAdd = new List<CustTable>();
            foreach (var seed in customerSeeds.Where(x => !existingCustomers.ContainsKey(x.Account)))
            {
                custsToAdd.Add(new CustTable
                {
                    AccountNum = seed.Account,
                    Party = customerParties[seed.PartyNumber].RecId,
                    CustGroupId = seed.Group,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat",
                    CurrencyCode = seed.Currency,
                    TaxGroupId = "DOM"
                });
            }

            if (custsToAdd.Any())
            {
                await db.CustTables.AddRangeAsync(custsToAdd, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
