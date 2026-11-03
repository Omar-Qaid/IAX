using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class VendTableDemoData
{
            internal static readonly (string Account, string PartyNumber, string Name, string NameAlias, string Group, string Currency, string TaxGroup, string PaymTermId, string PaymMode)[] Vendors =
            {
                (Account: "VEND-100", PartyNumber: "VEND-100", Name: "Domestic Trade Vendor 100", NameAlias: "مورد تجاري محلي 100", Group: "DOM", Currency: "SAR", TaxGroup: "DOM", PaymTermId: "Monthly", PaymMode: "ELECTRONIC"),
                (Account: "VEND-200", PartyNumber: "VEND-200", Name: "International Vendor 200", NameAlias: "مورد دولي 200", Group: "INT", Currency: "USD", TaxGroup: "EXP", PaymTermId: "Monthly", PaymMode: "ELECTRONIC"),
                (Account: "VEND-300", PartyNumber: "VEND-300", Name: "Service Vendor 300", NameAlias: "مورد خدمات 300", Group: "SERVICES", Currency: "SAR", TaxGroup: "DOM", PaymTermId: "Monthly", PaymMode: "CHECK"),
                (Account: "VEND-ZATCA", PartyNumber: "VEND-ZATCA", Name: "ZATCA Authority", NameAlias: "هيئة الزكاة والضريبة والجمارك", Group: "GOV", Currency: "SAR", TaxGroup: "EXEMPT", PaymTermId: "Monthly", PaymMode: "ELECTRONIC"),
                (Account: "VEND-400", PartyNumber: "VEND-400", Name: "Raw Material Supplier 400", NameAlias: "مورد مواد خام 400", Group: "RAW_MAT", Currency: "SAR", TaxGroup: "DOM", PaymTermId: "Monthly", PaymMode: "ELECTRONIC")
            };
    internal static VendTable Create(string account, long party, string group, string currency,
        string taxGroup, string term, string mode, string createdBy) => new()
    {
        AccountNum = account, Party = party, VendGroup = group, Currency = currency,
        TaxGroup = taxGroup, PaymTermId = term, PaymMode = mode, IsActive = true,
        CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat"
    };
}
