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

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    /// <summary>
    /// Seeds Vendor Groups (VendGroup) and Vendors (VendTable) for Accounts Payable sub-ledger.
    /// </summary>
    public class VendSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var sysUser = await users.FindByNameAsync("sys");
            var createdBy = sysUser?.Id ?? "sys";

            // Lookup default ExchangeRateType if seeded by ErpSeeder
            var defaultExchType = await db.Set<ExchangeRateType>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Name == "Default", ct);
            long? defaultExchTypeId = defaultExchType?.RecId;

            // 1. Seed Vendor Groups (VendGroup)
            var vendGroupSeeds = new[]
            {
                new VendGroup
                {
                    VendGroupCode = "DOM",
                    Name = "Domestic Trade Vendors",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "INT",
                    Name = "International Trade Vendors",
                    PaymTermId = "Monthly",
                    TaxGroupId = "EXP",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "SERVICES",
                    Name = "Service & Subcontractor Vendors",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "RAW_MAT",
                    Name = "Raw Material Suppliers",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "EQUIPMENT",
                    Name = "Capital Machinery & Equipment",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "GOV",
                    Name = "Government Authorities & Municipalities",
                    PaymTermId = "Monthly",
                    TaxGroupId = "EXEMPT",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "CONSULTANT",
                    Name = "Consulting & Professional Services",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "AFFILIATE",
                    Name = "Sister Companies & Group Affiliates",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                }
            };

            var existingVendGroupCodes = await db.Set<VendGroup>()
                .IgnoreQueryFilters()
                .Select(g => g.VendGroupCode)
                .ToListAsync(ct);

            var vendGroupsToAdd = vendGroupSeeds.Where(g => !existingVendGroupCodes.Contains(g.VendGroupCode)).ToList();
            if (vendGroupsToAdd.Count > 0)
            {
                await db.Set<VendGroup>().AddRangeAsync(vendGroupsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // 2. Seed Vendors (VendTable & DirPartyTable)
            var vendorSeeds = new[]
            {
                (Account: "VEND-100", PartyNumber: "VEND-100", Name: "Domestic Trade Vendor 100", NameAlias: "مورد تجاري محلي 100", Group: "DOM", Currency: "SAR", TaxGroup: "DOM", PaymTermId: "Monthly", PaymMode: "ELECTRONIC"),
                (Account: "VEND-200", PartyNumber: "VEND-200", Name: "International Vendor 200", NameAlias: "مورد دولي 200", Group: "INT", Currency: "USD", TaxGroup: "EXP", PaymTermId: "Monthly", PaymMode: "ELECTRONIC"),
                (Account: "VEND-300", PartyNumber: "VEND-300", Name: "Service Vendor 300", NameAlias: "مورد خدمات 300", Group: "SERVICES", Currency: "SAR", TaxGroup: "DOM", PaymTermId: "Monthly", PaymMode: "CHECK"),
                (Account: "VEND-ZATCA", PartyNumber: "VEND-ZATCA", Name: "ZATCA Authority", NameAlias: "هيئة الزكاة والضريبة والجمارك", Group: "GOV", Currency: "SAR", TaxGroup: "EXEMPT", PaymTermId: "Monthly", PaymMode: "ELECTRONIC"),
                (Account: "VEND-400", PartyNumber: "VEND-400", Name: "Raw Material Supplier 400", NameAlias: "مورد مواد خام 400", Group: "RAW_MAT", Currency: "SAR", TaxGroup: "DOM", PaymTermId: "Monthly", PaymMode: "ELECTRONIC")
            };

            var existingVendors = await db.Set<VendTable>()
                .IgnoreQueryFilters()
                .Where(v => v.DataAreaId == "dat")
                .ToDictionaryAsync(v => v.AccountNum, StringComparer.OrdinalIgnoreCase, ct);

            var vendorPartyNumbers = vendorSeeds.Select(x => x.PartyNumber).ToArray();
            var vendorParties = await db.Set<DirPartyTable>()
                .IgnoreQueryFilters()
                .Where(x => vendorPartyNumbers.Contains(x.PartyNumber))
                .ToDictionaryAsync(x => x.PartyNumber, StringComparer.OrdinalIgnoreCase, ct);

            foreach (var seed in vendorSeeds.Where(x => !existingVendors.ContainsKey(x.Account)))
            {
                if (vendorParties.ContainsKey(seed.PartyNumber)) continue;
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
                vendorParties[seed.PartyNumber] = party;
            }
            await db.SaveChangesAsync(ct);

            var vendsToAdd = new List<VendTable>();
            foreach (var seed in vendorSeeds.Where(x => !existingVendors.ContainsKey(x.Account)))
            {
                vendsToAdd.Add(new VendTable
                {
                    AccountNum = seed.Account,
                    Party = vendorParties[seed.PartyNumber].RecId,
                    VendGroup = seed.Group,
                    Currency = seed.Currency,
                    TaxGroup = seed.TaxGroup,
                    PaymTermId = seed.PaymTermId,
                    PaymMode = seed.PaymMode,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                });
            }

            if (vendsToAdd.Count > 0)
            {
                await db.Set<VendTable>().AddRangeAsync(vendsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}



