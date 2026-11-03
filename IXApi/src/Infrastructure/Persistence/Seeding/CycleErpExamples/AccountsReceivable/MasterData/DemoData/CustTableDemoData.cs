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

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsReceivable;

internal static class CustTableDemoData
{
            internal static readonly (string Account, string PartyNumber, string Name, string NameAlias, string Group, string Currency)[] Customers =
            {
                (Account: "C00015", PartyNumber: "CUST-C00015", Name: "Consultant Customer", NameAlias: "عميل استشاري", Group: "Consultant", Currency: "SAR"),
                (Account: "CUST-100", PartyNumber: "CUST-100", Name: "Consulting Customer 100", NameAlias: "عميل استشارات 100", Group: "Consultant", Currency: "SAR"),
                (Account: "CUST-200", PartyNumber: "CUST-200", Name: "Retail Customer 200", NameAlias: "عميل تجزئة 200", Group: "CUST-RTL", Currency: "SAR"),
                (Account: "CUST-300", PartyNumber: "CUST-300", Name: "International Customer 300", NameAlias: "عميل دولي 300", Group: "Consultant", Currency: "USD"),
                (Account: "CUS-00001", PartyNumber: "CUS-00001", Name: "Retail Customer 1", NameAlias: "عميل تجزئة 1", Group: "CUST-RTL", Currency: "SAR"),
                (Account: "CUS-00002", PartyNumber: "CUS-00002", Name: "VIP Customer 2", NameAlias: "عميل مميز 2", Group: "CUST-VIP", Currency: "USD")
            };

    internal static CustTable Create(string account, long party, string group, string currency) => new()
    {
        AccountNum = account, Party = party, CustGroupId = group, IsActive = true,
        CreatedBy = "sys", OwnerAccountId = "sys", DataAreaId = "dat",
        CurrencyCode = currency, TaxGroupId = "DOM"
    };
}
