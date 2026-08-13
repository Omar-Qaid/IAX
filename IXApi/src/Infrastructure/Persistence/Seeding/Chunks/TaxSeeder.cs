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
    /// Seeds full relational tax setup for VAT compliance:
    /// - TaxAuthorityAddress (Sales Tax Authorities: ZATCA, FTA)
    /// - TaxPeriodHead (Sales Tax Settlement Periods: Monthly, Quarterly)
    /// - TaxReportPeriod (Settlement Period Intervals)
    /// - TaxExemptCodeTable (Exemption Reason Codes)
    /// - TaxGroupHeading (Sales Tax Groups: DOM, EXP, EXEMPT)
    /// - TaxItemGroupHeading (Item Sales Tax Groups: FULL, REDUCED, EXEMPT)
    /// - TaxTable (Sales Tax Codes: VAT15, VAT5, VAT0, EXEMPT)
    /// - TaxData (Tax Rates & Validity Periods)
    /// - TaxGroupData (Tax Group <-> Tax Code Mapping)
    /// - TaxOnItem (Item Tax Group <-> Tax Code Mapping)
    /// </summary>
    public class TaxSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var sysUser = await users.FindByNameAsync("sys");
            var createdBy = sysUser?.Id ?? "sys";

            // 1. Seed TaxAuthorityAddress (Sales Tax Authorities)
            var authoritySeeds = new[]
            {
                new TaxAuthorityAddress
                {
                    TaxAuthority = "ZATCA",
                    Name = "Zakat, Tax and Customs Authority",
                    TaxAuthorityId = "ZATCA_HQ",
                    AccountNum = "VEND-ZATCA",
                    Address = "Riyadh, Kingdom of Saudi Arabia",
                    Email = "vat@zatca.gov.sa",
                    Mobile = "+966 11 200 0000",
                    Phone = "+966 11 200 0000",
                    Url = "https://zatca.gov.sa",
                    RoundOff = 0.01m,
                    RoundOffType = TaxRoundOffType.Ordinary,
                    RoundOffGainLedgerDimension = null,
                    RoundOffLossLedgerDimension = null,
                    Location = null,
                    TaxReportLayout = TaxReportLayout.Default,
                    UseDefaultLayout = NoYes.Yes,
                    DataAreaId = "dat"
                },
                new TaxAuthorityAddress
                {
                    TaxAuthority = "FTA",
                    Name = "Federal Tax Authority",
                    TaxAuthorityId = "FTA_HQ",
                    AccountNum = "VEND-200",
                    Address = "Abu Dhabi, United Arab Emirates",
                    Email = "info@tax.gov.ae",
                    Mobile = "+971 600 599 999",
                    Phone = "+971 600 599 999",
                    Url = "https://tax.gov.ae",
                    RoundOff = 0.01m,
                    RoundOffType = TaxRoundOffType.Ordinary,
                    RoundOffGainLedgerDimension = null,
                    RoundOffLossLedgerDimension = null,
                    Location = null,
                    TaxReportLayout = TaxReportLayout.Default,
                    UseDefaultLayout = NoYes.Yes,
                    DataAreaId = "dat"
                }
            };

            var existingAuthorities = await db.Set<TaxAuthorityAddress>()
                .IgnoreQueryFilters()
                .Select(x => x.TaxAuthority)
                .ToListAsync(ct);

            var existingVendAccounts = await db.Set<VendTable>()
                .IgnoreQueryFilters()
                .Select(v => v.AccountNum)
                .ToListAsync(ct);

            var authoritiesToAdd = authoritySeeds.Where(x => !existingAuthorities.Contains(x.TaxAuthority)).ToList();
            foreach (var auth in authoritiesToAdd)
            {
                if (!string.IsNullOrEmpty(auth.AccountNum) && !existingVendAccounts.Contains(auth.AccountNum))
                {
                    auth.AccountNum = string.Empty;
                }
            }

            if (authoritiesToAdd.Count > 0)
            {
                await db.Set<TaxAuthorityAddress>().AddRangeAsync(authoritiesToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // 2. Seed TaxPeriodHead (Sales Tax Settlement Periods)
            var periodHeadSeeds = new[]
            {
                new TaxPeriodHead
                {
                    TaxPeriod = "Monthly",
                    Name = "Monthly VAT Settlement Period",
                    TaxAuthority = "ZATCA",
                    PeriodUnit = TaxPeriodUnit.Month,
                    QtyUnit = 1,
                    DataAreaId = "dat"
                },
                new TaxPeriodHead
                {
                    TaxPeriod = "Quarterly",
                    Name = "Quarterly VAT Settlement Period",
                    TaxAuthority = "ZATCA",
                    PeriodUnit = TaxPeriodUnit.Quarter,
                    QtyUnit = 1,
                    DataAreaId = "dat"
                }
            };

            var existingPeriodHeads = await db.Set<TaxPeriodHead>()
                .IgnoreQueryFilters()
                .Select(x => x.TaxPeriod)
                .ToListAsync(ct);

            var periodHeadsToAdd = periodHeadSeeds.Where(x => !existingPeriodHeads.Contains(x.TaxPeriod)).ToList();
            if (periodHeadsToAdd.Count > 0)
            {
                await db.Set<TaxPeriodHead>().AddRangeAsync(periodHeadsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // 3. Seed TaxReportPeriod (Settlement Period Intervals)
            var periodIntervalSeeds = new[]
            {
                new TaxReportPeriod
                {
                    TaxPeriod = "Monthly",
                    FromDate = new DateTime(2026, 1, 1),
                    ToDate = new DateTime(2026, 1, 31),
                    Closed = NoYes.No,
                    DataAreaId = "dat"
                },
                new TaxReportPeriod
                {
                    TaxPeriod = "Monthly",
                    FromDate = new DateTime(2026, 2, 1),
                    ToDate = new DateTime(2026, 2, 28),
                    Closed = NoYes.No,
                    DataAreaId = "dat"
                },
                new TaxReportPeriod
                {
                    TaxPeriod = "Quarterly",
                    FromDate = new DateTime(2026, 1, 1),
                    ToDate = new DateTime(2026, 3, 31),
                    Closed = NoYes.No,
                    DataAreaId = "dat"
                }
            };

            var existingPeriodIntervals = await db.Set<TaxReportPeriod>()
                .IgnoreQueryFilters()
                .Select(x => new { x.TaxPeriod, x.FromDate })
                .ToListAsync(ct);

            var periodIntervalsToAdd = periodIntervalSeeds
                .Where(x => !existingPeriodIntervals.Any(e => e.TaxPeriod == x.TaxPeriod && e.FromDate == x.FromDate))
                .ToList();

            if (periodIntervalsToAdd.Count > 0)
            {
                await db.Set<TaxReportPeriod>().AddRangeAsync(periodIntervalsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // 4. Seed TaxExemptCodeTable
            var exemptSeeds = new[]
            {
                new TaxExemptCodeTable { ExemptCode = "NONE", Description = "No Exemption - Standard VAT Applies", DataAreaId = "dat" },
                new TaxExemptCodeTable { ExemptCode = "EXPORT", Description = "Zero-rated export of goods outside GCC", DataAreaId = "dat" },
                new TaxExemptCodeTable { ExemptCode = "GOV_EXEMPT", Description = "Governmental / Sovereign Exemption", DataAreaId = "dat" },
                new TaxExemptCodeTable { ExemptCode = "MED_EXEMPT", Description = "Exempt Qualifying Medicines and Medical Devices", DataAreaId = "dat" }
            };

            var existingExemptCodes = await db.Set<TaxExemptCodeTable>()
                .IgnoreQueryFilters()
                .Select(x => x.ExemptCode)
                .ToListAsync(ct);

            var exemptToAdd = exemptSeeds.Where(x => !existingExemptCodes.Contains(x.ExemptCode)).ToList();
            if (exemptToAdd.Count > 0)
            {
                await db.Set<TaxExemptCodeTable>().AddRangeAsync(exemptToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // 5. Seed TaxGroupHeading (Sales Tax Groups)
            var taxGroupHeadingSeeds = new[]
            {
                new TaxGroupHeading
                {
                    TaxGroup = "DOM",
                    TaxGroupName = "Domestic Standard Sales Tax Group",
                    TaxGroupSetup = TaxGroupSetup.Standard,
                    Source = TaxGroupSource.Customer,
                    TaxGroupRounding = TaxGroupRounding.None,
                    TaxReverseOnCashDisc = NoYes.No,
                    DataAreaId = "dat"
                },
                new TaxGroupHeading
                {
                    TaxGroup = "EXP",
                    TaxGroupName = "Export / Zero-Rated Sales Tax Group",
                    TaxGroupSetup = TaxGroupSetup.Standard,
                    Source = TaxGroupSource.Customer,
                    TaxGroupRounding = TaxGroupRounding.None,
                    TaxReverseOnCashDisc = NoYes.No,
                    DataAreaId = "dat"
                },
                new TaxGroupHeading
                {
                    TaxGroup = "EXEMPT",
                    TaxGroupName = "Exempt / Non-Taxable Sales Tax Group",
                    TaxGroupSetup = TaxGroupSetup.Standard,
                    Source = TaxGroupSource.Customer,
                    TaxGroupRounding = TaxGroupRounding.None,
                    TaxReverseOnCashDisc = NoYes.No,
                    DataAreaId = "dat"
                }
            };

            var existingTaxGroups = await db.TaxGroupHeadings
                .IgnoreQueryFilters()
                .Select(x => x.TaxGroup)
                .ToListAsync(ct);

            var taxGroupsToAdd = taxGroupHeadingSeeds.Where(x => !existingTaxGroups.Contains(x.TaxGroup)).ToList();
            if (taxGroupsToAdd.Count > 0)
            {
                await db.TaxGroupHeadings.AddRangeAsync(taxGroupsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // 6. Seed TaxItemGroupHeading (Item Sales Tax Groups)
            var taxItemGroupHeadingSeeds = new[]
            {
                new TaxItemGroupHeading
                {
                    TaxItemGroup = "FULL",
                    Name = "Standard Rated Items (15%)",
                    Source = TaxGroupSource.Customer,
                    EuSalesListType = EuSalesListType.Item,
                    DataAreaId = "dat"
                },
                new TaxItemGroupHeading
                {
                    TaxItemGroup = "REDUCED",
                    Name = "Reduced Rate Items (5%)",
                    Source = TaxGroupSource.Customer,
                    EuSalesListType = EuSalesListType.Item,
                    DataAreaId = "dat"
                },
                new TaxItemGroupHeading
                {
                    TaxItemGroup = "EXEMPT",
                    Name = "Exempt / Zero Rated Items",
                    Source = TaxGroupSource.Customer,
                    EuSalesListType = EuSalesListType.Item,
                    DataAreaId = "dat"
                }
            };

            var existingTaxItemGroups = await db.Set<TaxItemGroupHeading>()
                .IgnoreQueryFilters()
                .Select(x => x.TaxItemGroup)
                .ToListAsync(ct);

            var taxItemGroupsToAdd = taxItemGroupHeadingSeeds.Where(x => !existingTaxItemGroups.Contains(x.TaxItemGroup)).ToList();
            if (taxItemGroupsToAdd.Count > 0)
            {
                await db.Set<TaxItemGroupHeading>().AddRangeAsync(taxItemGroupsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // 7. Seed TaxTable (Sales Tax Codes)
            var taxTableSeeds = new[]
            {
                new TaxTable
                {
                    TaxCode = "VAT15",
                    TaxName = "Standard VAT 15%",
                    TaxPeriod = "Monthly",
                    TaxAccountGroup = "STANDARD",
                    TaxCurrencyCode = "SAR",
                    TaxUnit = "EA",
                    TaxBase = TaxBase.Net,
                    TaxCalcMethod = TaxCalcMethod.Line,
                    TaxLimitBase = TaxLimitBase.Line,
                    PrintCode = "VAT15",
                    DataAreaId = "dat"
                },
                new TaxTable
                {
                    TaxCode = "VAT5",
                    TaxName = "Reduced VAT 5%",
                    TaxPeriod = "Monthly",
                    TaxAccountGroup = "STANDARD",
                    TaxCurrencyCode = "SAR",
                    TaxUnit = "EA",
                    TaxBase = TaxBase.Net,
                    TaxCalcMethod = TaxCalcMethod.Line,
                    TaxLimitBase = TaxLimitBase.Line,
                    PrintCode = "VAT5",
                    DataAreaId = "dat"
                },
                new TaxTable
                {
                    TaxCode = "VAT0",
                    TaxName = "Zero Rated VAT 0%",
                    TaxPeriod = "Monthly",
                    TaxAccountGroup = "STANDARD",
                    TaxCurrencyCode = "SAR",
                    TaxUnit = "EA",
                    TaxBase = TaxBase.Net,
                    TaxCalcMethod = TaxCalcMethod.Line,
                    TaxLimitBase = TaxLimitBase.Line,
                    PrintCode = "VAT0",
                    DataAreaId = "dat"
                },
                new TaxTable
                {
                    TaxCode = "EXEMPT",
                    TaxName = "Exempt Tax Code 0%",
                    TaxPeriod = "Monthly",
                    TaxAccountGroup = "STANDARD",
                    TaxCurrencyCode = "SAR",
                    TaxUnit = "EA",
                    TaxBase = TaxBase.Net,
                    TaxCalcMethod = TaxCalcMethod.Line,
                    TaxLimitBase = TaxLimitBase.Line,
                    PrintCode = "EXEMPT",
                    DataAreaId = "dat"
                }
            };

            var existingTaxCodes = await db.TaxTables
                .IgnoreQueryFilters()
                .Select(x => x.TaxCode)
                .ToListAsync(ct);

            var taxTablesToAdd = taxTableSeeds.Where(x => !existingTaxCodes.Contains(x.TaxCode)).ToList();
            if (taxTablesToAdd.Count > 0)
            {
                await db.TaxTables.AddRangeAsync(taxTablesToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // 8. Seed TaxData (Tax Value Rates)
            var taxDataSeeds = new[]
            {
                new TaxData
                {
                    TaxCode = "VAT15",
                    TaxFromDate = new DateTime(2020, 7, 1),
                    TaxToDate = new DateTime(2099, 12, 31),
                    TaxValue = 15.00m,
                    VatExemptPct = 0.00m,
                    DataAreaId = "dat"
                },
                new TaxData
                {
                    TaxCode = "VAT5",
                    TaxFromDate = new DateTime(2018, 1, 1),
                    TaxToDate = new DateTime(2099, 12, 31),
                    TaxValue = 5.00m,
                    VatExemptPct = 0.00m,
                    DataAreaId = "dat"
                },
                new TaxData
                {
                    TaxCode = "VAT0",
                    TaxFromDate = new DateTime(2018, 1, 1),
                    TaxToDate = new DateTime(2099, 12, 31),
                    TaxValue = 0.00m,
                    VatExemptPct = 0.00m,
                    DataAreaId = "dat"
                },
                new TaxData
                {
                    TaxCode = "EXEMPT",
                    TaxFromDate = new DateTime(2018, 1, 1),
                    TaxToDate = new DateTime(2099, 12, 31),
                    TaxValue = 0.00m,
                    VatExemptPct = 100.00m,
                    DataAreaId = "dat"
                }
            };

            var existingTaxDataCodes = await db.TaxData
                .IgnoreQueryFilters()
                .Select(x => x.TaxCode)
                .ToListAsync(ct);

            var taxDataToAdd = taxDataSeeds.Where(x => !existingTaxDataCodes.Contains(x.TaxCode)).ToList();
            if (taxDataToAdd.Count > 0)
            {
                await db.TaxData.AddRangeAsync(taxDataToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // 9. Seed TaxGroupData (Sales Tax Group <-> Tax Code Mapping)
            var taxGroupDataSeeds = new[]
            {
                new TaxGroupData { TaxGroup = "DOM", TaxCode = "VAT15", TaxExemptCode = "NONE", ExemptTax = NoYes.No, DataAreaId = "dat" },
                new TaxGroupData { TaxGroup = "DOM", TaxCode = "VAT5", TaxExemptCode = "NONE", ExemptTax = NoYes.No, DataAreaId = "dat" },
                new TaxGroupData { TaxGroup = "EXP", TaxCode = "VAT0", TaxExemptCode = "EXPORT", ExemptTax = NoYes.Yes, DataAreaId = "dat" },
                new TaxGroupData { TaxGroup = "EXEMPT", TaxCode = "EXEMPT", TaxExemptCode = "GOV_EXEMPT", ExemptTax = NoYes.Yes, DataAreaId = "dat" }
            };

            var existingTaxGroupData = await db.TaxGroupDatas
                .IgnoreQueryFilters()
                .Select(x => new { x.TaxGroup, x.TaxCode })
                .ToListAsync(ct);

            var groupDataToAdd = taxGroupDataSeeds
                .Where(x => !existingTaxGroupData.Any(e => e.TaxGroup == x.TaxGroup && e.TaxCode == x.TaxCode))
                .ToList();

            if (groupDataToAdd.Count > 0)
            {
                await db.TaxGroupDatas.AddRangeAsync(groupDataToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // 10. Seed TaxOnItem (Item Sales Tax Group <-> Tax Code Mapping)
            var taxOnItemSeeds = new[]
            {
                new TaxOnItem { TaxItemGroup = "FULL", TaxCode = "VAT15", TaxExemptCode = "NONE", DataAreaId = "dat" },
                new TaxOnItem { TaxItemGroup = "REDUCED", TaxCode = "VAT5", TaxExemptCode = "NONE", DataAreaId = "dat" },
                new TaxOnItem { TaxItemGroup = "EXEMPT", TaxCode = "EXEMPT", TaxExemptCode = "MED_EXEMPT", DataAreaId = "dat" }
            };

            var existingTaxOnItems = await db.TaxOnItems
                .IgnoreQueryFilters()
                .Select(x => new { x.TaxItemGroup, x.TaxCode })
                .ToListAsync(ct);

            var itemDataToAdd = taxOnItemSeeds
                .Where(x => !existingTaxOnItems.Any(e => e.TaxItemGroup == x.TaxItemGroup && e.TaxCode == x.TaxCode))
                .ToList();

            if (itemDataToAdd.Count > 0)
            {
                await db.TaxOnItems.AddRangeAsync(itemDataToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // 11. Seed TaxLedgerAccountGroup (Ledger Posting Groups)
            var ledgerAccountGroupSeeds = new[]
            {
                new TaxLedgerAccountGroup
                {
                    TaxAccountGroup = "VAT-STD",
                    Name = "Standard VAT Posting Group",
                    DataAreaId = "dat"
                },
                new TaxLedgerAccountGroup
                {
                    TaxAccountGroup = "VAT-EXP",
                    Name = "Export Tax Posting Group",
                    DataAreaId = "dat"
                },
                new TaxLedgerAccountGroup
                {
                    TaxAccountGroup = "VAT-EXEMPT",
                    Name = "Exempt Tax Posting Group",
                    DataAreaId = "dat"
                }
            };

            var existingLedgerGroups = await db.Set<TaxLedgerAccountGroup>()
                .IgnoreQueryFilters()
                .Select(x => x.TaxAccountGroup)
                .ToListAsync(ct);

            var ledgerGroupsToAdd = ledgerAccountGroupSeeds
                .Where(x => !existingLedgerGroups.Contains(x.TaxAccountGroup))
                .ToList();

            if (ledgerGroupsToAdd.Count > 0)
            {
                await db.Set<TaxLedgerAccountGroup>().AddRangeAsync(ledgerGroupsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}



