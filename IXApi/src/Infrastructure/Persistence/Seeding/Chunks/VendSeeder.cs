using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
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

            // 2. Seed Vendors (VendTable)
            var vendTableSeeds = new[]
            {
                new VendTable
                {
                    AccountNum = "VEND-100",
                    VendGroup = "DOM",
                    Currency = "SAR",
                    TaxGroup = "DOM",
                    PaymTermId = "Monthly",
                    PaymMode = "ELECTRONIC",
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendTable
                {
                    AccountNum = "VEND-200",
                    VendGroup = "INT",
                    Currency = "USD",
                    TaxGroup = "EXP",
                    PaymTermId = "Monthly",
                    PaymMode = "ELECTRONIC",
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendTable
                {
                    AccountNum = "VEND-300",
                    VendGroup = "SERVICES",
                    Currency = "SAR",
                    TaxGroup = "DOM",
                    PaymTermId = "Monthly",
                    PaymMode = "CHECK",
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendTable
                {
                    AccountNum = "VEND-ZATCA",
                    VendGroup = "GOV",
                    Currency = "SAR",
                    TaxGroup = "EXEMPT",
                    PaymTermId = "Monthly",
                    PaymMode = "ELECTRONIC",
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendTable
                {
                    AccountNum = "VEND-400",
                    VendGroup = "RAW_MAT",
                    Currency = "SAR",
                    TaxGroup = "DOM",
                    PaymTermId = "Monthly",
                    PaymMode = "ELECTRONIC",
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                }
            };

            var existingVendCodes = await db.Set<VendTable>()
                .IgnoreQueryFilters()
                .Select(v => v.AccountNum)
                .ToListAsync(ct);

            var vendsToAdd = vendTableSeeds.Where(v => !existingVendCodes.Contains(v.AccountNum)).ToList();
            if (vendsToAdd.Count > 0)
            {
                await db.Set<VendTable>().AddRangeAsync(vendsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}

