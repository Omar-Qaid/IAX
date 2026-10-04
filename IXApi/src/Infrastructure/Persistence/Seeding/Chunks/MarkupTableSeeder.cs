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
    public class MarkupTableSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var seeds = new[]
            {
                new MarkupTable
                {
                    MarkupCode = "Freight",
                    Txt = "Shipping & Freight Charges",
                    ModuleType = ModuleInventPurchSales.Sales,
                    TaxItemGroup = "FULL",
                    CustType = 0, // Ledger
                    VendType = 0, // Ledger
                    IncludeIntoIntrastatInvoiceValue = NoYes.No,
                    IncludeIntoIntrastatStatisticalValue = NoYes.No,
                    IsShipping = NoYes.Yes,
                    DataAreaId = "dat"
                },
                new MarkupTable
                {
                    MarkupCode = "Insurance",
                    Txt = "Transit Insurance Fee",
                    ModuleType = ModuleInventPurchSales.Sales,
                    TaxItemGroup = "EXEMPT",
                    CustType = 0,
                    VendType = 0,
                    IncludeIntoIntrastatInvoiceValue = NoYes.No,
                    IncludeIntoIntrastatStatisticalValue = NoYes.No,
                    IsShipping = NoYes.No,
                    DataAreaId = "dat"
                },
                new MarkupTable
                {
                    MarkupCode = "Handling",
                    Txt = "Warehouse Handling Fee",
                    ModuleType = ModuleInventPurchSales.Sales,
                    TaxItemGroup = "FULL",
                    CustType = 0,
                    VendType = 0,
                    IncludeIntoIntrastatInvoiceValue = NoYes.No,
                    IncludeIntoIntrastatStatisticalValue = NoYes.No,
                    IsShipping = NoYes.No,
                    DataAreaId = "dat"
                },
                new MarkupTable
                {
                    MarkupCode = "ServiceFee",
                    Txt = "Administrative Service Charge",
                    ModuleType = ModuleInventPurchSales.Sales,
                    TaxItemGroup = "FULL",
                    CustType = 0,
                    VendType = 0,
                    IncludeIntoIntrastatInvoiceValue = NoYes.No,
                    IncludeIntoIntrastatStatisticalValue = NoYes.No,
                    IsShipping = NoYes.No,
                    DataAreaId = "dat"
                },
                new MarkupTable
                {
                    MarkupCode = "Sample",
                    Txt = "Customer Samples",
                    ModuleType = ModuleInventPurchSales.Sales,
                    TaxItemGroup = "EXEMPT",
                    CustType = 1, // Customer
                    VendType = 0, // Ledger
                    IncludeIntoIntrastatInvoiceValue = NoYes.No,
                    IncludeIntoIntrastatStatisticalValue = NoYes.No,
                    Refundable = NoYes.No,
                    McrProrate = NoYes.No,
                    IsShipping = NoYes.No,
                    DataAreaId = "dat"
                },
                new MarkupTable
                {
                    MarkupCode = "ShipCharge",
                    Txt = "Shipping Charges",
                    ModuleType = ModuleInventPurchSales.Sales,
                    TaxItemGroup = "FULL",
                    CustType = 1, // Customer
                    VendType = 0, // Ledger
                    IncludeIntoIntrastatInvoiceValue = NoYes.Yes,
                    IncludeIntoIntrastatStatisticalValue = NoYes.Yes,
                    Refundable = NoYes.Yes,
                    McrProrate = NoYes.Yes,
                    IsShipping = NoYes.Yes,
                    DataAreaId = "dat"
                },
                new MarkupTable
                {
                    MarkupCode = "PFreight",
                    Txt = "Purchase Freight Charges",
                    ModuleType = ModuleInventPurchSales.Purchase,
                    TaxItemGroup = "FULL",
                    CustType = 0, // Ledger
                    VendType = 1, // Vendor
                    UseInMatching = NoYes.Yes,
                    IncludeIntoIntrastatInvoiceValue = NoYes.Yes,
                    IncludeIntoIntrastatStatisticalValue = NoYes.Yes,
                    IsShipping = NoYes.Yes,
                    DataAreaId = "dat"
                },
                new MarkupTable
                {
                    MarkupCode = "PHandling",
                    Txt = "Purchase Handling Fee",
                    ModuleType = ModuleInventPurchSales.Purchase,
                    TaxItemGroup = "FULL",
                    CustType = 0, // Ledger
                    VendType = 1, // Vendor
                    UseInMatching = NoYes.Yes,
                    IncludeIntoIntrastatInvoiceValue = NoYes.No,
                    IncludeIntoIntrastatStatisticalValue = NoYes.No,
                    IsShipping = NoYes.No,
                    DataAreaId = "dat"
                },
                new MarkupTable
                {
                    MarkupCode = "PInsurance",
                    Txt = "Purchase Transit Insurance",
                    ModuleType = ModuleInventPurchSales.Purchase,
                    TaxItemGroup = "EXEMPT",
                    CustType = 0, // Ledger
                    VendType = 1, // Vendor
                    UseInMatching = NoYes.Yes,
                    IncludeIntoIntrastatInvoiceValue = NoYes.Yes,
                    IncludeIntoIntrastatStatisticalValue = NoYes.No,
                    IsShipping = NoYes.No,
                    DataAreaId = "dat"
                }
            };

            var existingCodes = await db.MarkupTables
                .IgnoreQueryFilters()
                .Select(p => p.MarkupCode)
                .ToListAsync(ct);

            var existingCodeSet = existingCodes.ToHashSet(System.StringComparer.OrdinalIgnoreCase);
            var toAdd = seeds.Where(s => !existingCodeSet.Contains(s.MarkupCode)).ToList();
            if (toAdd.Count > 0)
            {
                await db.MarkupTables.AddRangeAsync(toAdd, ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}



