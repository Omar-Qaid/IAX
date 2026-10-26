using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance
{
    public class SalesDeliverySetupSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.AccountsReceivable.DeliverySetup";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Reference;
        public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.CompanyInfo" };
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "dat";

        public override async Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        {
            var db = context.Db;
            var createdBy = "sys";

            // 1. Modes of Delivery (DlvMode)
            var existingDlvModes = await db.Set<DlvMode>().IgnoreQueryFilters().Select(x => x.Code).ToListAsync(cancellationToken);
            var dlvModeSeeds = new[]
            {
                new DlvMode { Code = "DHL", Txt = "DHL Express", DisplayOrder = 1, ShipCarrierDlvType = WHSShipCarrierDlvType.Ground, MarkupGroup = "", McrExpedite = "", DomPriority = 1, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new DlvMode { Code = "FEDEX", Txt = "FedEx Ground", DisplayOrder = 2, ShipCarrierDlvType = WHSShipCarrierDlvType.Ground, MarkupGroup = "", McrExpedite = "", DomPriority = 2, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
            };
            foreach (var mode in dlvModeSeeds)
            {
                if (!existingDlvModes.Contains(mode.Code))
                {
                    await db.Set<DlvMode>().AddAsync(mode, cancellationToken);
                }
            }
            await db.SaveChangesAsync(cancellationToken);

            // 2. Delivery Terms (DlvTerm)
            var existingDlvTerms = await db.Set<DlvTerm>().IgnoreQueryFilters().Select(x => x.Code).ToListAsync(cancellationToken);
            var dlvTermSeeds = new[]
            {
                new DlvTerm { Code = "FOB", Txt = "Free on Board", ShipCarrierFreeMinimum = 0, FreightChargeTerm = 0, TaxLocationRole = 0, ItmGoodsInTransitControl = NoYes.No, ItmPortMandatory = NoYes.No, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new DlvTerm { Code = "CIF", Txt = "Cost, Insurance & Freight", ShipCarrierFreeMinimum = 1000m, FreightChargeTerm = 1, TaxLocationRole = 0, ItmGoodsInTransitControl = NoYes.Yes, ItmPortMandatory = NoYes.Yes, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
            };
            foreach (var term in dlvTermSeeds)
            {
                if (!existingDlvTerms.Contains(term.Code))
                {
                    await db.Set<DlvTerm>().AddAsync(term, cancellationToken);
                }
            }
            await db.SaveChangesAsync(cancellationToken);

            // 3. Payment Terms (PaymTerm)
            var existingPaymTerms = await db.Set<PaymTerm>().IgnoreQueryFilters().Select(x => x.PaymTermId).ToListAsync(cancellationToken);
            var paymTermSeeds = new[]
            {
                new PaymTerm { PaymTermId = "Net 30", Description = "Net 30 Days", NumOfMonths = 0, NumOfDays = 30, PaymMethod = PaymMethod.Net, Cash = NoYes.No, PostOffsettingAr = NoYes.No, CustomerUpdateDueDate = NoYes.Yes, VendorUpdateDueDate = NoYes.Yes, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new PaymTerm { PaymTermId = "COD", Description = "Cash on Delivery", NumOfMonths = 0, NumOfDays = 0, PaymMethod = PaymMethod.COD, Cash = NoYes.Yes, PostOffsettingAr = NoYes.No, CustomerUpdateDueDate = NoYes.No, VendorUpdateDueDate = NoYes.No, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new PaymTerm { PaymTermId = "Monthly", Description = "Monthly Payment Terms", NumOfMonths = 1, NumOfDays = 0, PaymMethod = PaymMethod.Net, Cash = NoYes.No, PostOffsettingAr = NoYes.No, CustomerUpdateDueDate = NoYes.Yes, VendorUpdateDueDate = NoYes.Yes, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
            };
            foreach (var term in paymTermSeeds)
            {
                if (!existingPaymTerms.Contains(term.PaymTermId))
                {
                    await db.Set<PaymTerm>().AddAsync(term, cancellationToken);
                }
            }
            await db.SaveChangesAsync(cancellationToken);

            // 4. Payment Schedules (PaymSched & PaymSchedLine)
            var existingPaymScheds = await db.Set<PaymSched>().IgnoreQueryFilters().Select(x => x.Name).ToListAsync(cancellationToken);
            if (!existingPaymScheds.Contains("3 Instal"))
            {
                var sched = new PaymSched
                {
                    Name = "3 Instal",
                    Description = "3 Installments (Equal)",
                    NumOfPayment = 3,
                    PayBy = PaymSchedAllocateMethod.Equal,
                    PeriodUnit = PeriodUnit.Month,
                    QtyUnit = 1,
                    TaxDistribution = PaymSchedTaxDist.Proportionate,
                    McrFlexiblePlan = NoYes.No,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                };
                await db.Set<PaymSched>().AddAsync(sched, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);

                var lines = new[]
                {
                    new PaymSchedLine { Name = "3 Instal", LineNum = 1, Qty = 0, PercentAmount = 34, Value = 0, CfmPrepayment = NoYes.Yes, McrShipping = NoYes.Yes, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new PaymSchedLine { Name = "3 Instal", LineNum = 2, Qty = 1, PercentAmount = 33, Value = 0, CfmPrepayment = NoYes.No, McrShipping = NoYes.No, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new PaymSchedLine { Name = "3 Instal", LineNum = 3, Qty = 2, PercentAmount = 33, Value = 0, CfmPrepayment = NoYes.No, McrShipping = NoYes.No, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
                };
                await db.Set<PaymSchedLine>().AddRangeAsync(lines, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
