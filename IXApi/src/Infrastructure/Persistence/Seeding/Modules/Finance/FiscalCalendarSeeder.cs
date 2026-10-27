using System;
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
    public class FiscalCalendarSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.GeneralLedger.FiscalCalendars";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Reference;
        public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.CompanyInfo" };
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "dat";

        public override Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
            => SeedDataAsync(context.Db, cancellationToken);

        public async Task SeedDataAsync(DbContext db, CancellationToken cancellationToken = default)
        {
            var createdBy = "sys";

            // Seed Fiscal Calendar: HBMC
            var existingCalendar = await db.Set<FiscalCalendar>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.DataAreaId == "dat" && c.CalendarId == "HBMC", cancellationToken);
            if (existingCalendar == null)
            {
                existingCalendar = new FiscalCalendar { CalendarId = "HBMC", Description = "HBMC Default calendar", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" };
                await db.Set<FiscalCalendar>().AddAsync(existingCalendar, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            // Seed Fiscal Calendar Year: 2026
            var existingYear = await db.Set<FiscalCalendarYear>().IgnoreQueryFilters().FirstOrDefaultAsync(y => y.FiscalCalendar == existingCalendar.RecId && y.Name == "2026", cancellationToken);
            if (existingYear == null)
            {
                existingYear = new FiscalCalendarYear { FiscalCalendar = existingCalendar.RecId, Name = "2026", StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" };
                await db.Set<FiscalCalendarYear>().AddAsync(existingYear, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            // Seed Fiscal Calendar Periods
            var existingPeriods = await db.Set<FiscalCalendarPeriod>().IgnoreQueryFilters().Where(p => p.FiscalCalendar == existingCalendar.RecId && p.FiscalCalendarYear == existingYear.RecId).ToListAsync(cancellationToken);

            {
                var periods = new List<FiscalCalendarPeriod>
                {
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 0", ShortName = "P0", Type = FiscalPeriodType.Opening, StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), Month = 1, Quarter = 1, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 1", ShortName = "P1", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc), Month = 1, Quarter = 1, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 2", ShortName = "P2", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc), Month = 2, Quarter = 1, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 3", ShortName = "P3", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc), Month = 3, Quarter = 1, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 4", ShortName = "P4", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc), Month = 4, Quarter = 2, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 5", ShortName = "P5", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc), Month = 5, Quarter = 2, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 6", ShortName = "P6", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc), Month = 6, Quarter = 2, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 7", ShortName = "P7", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Utc), Month = 7, Quarter = 3, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 8", ShortName = "P8", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc), Month = 8, Quarter = 3, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 9", ShortName = "P9", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc), Month = 9, Quarter = 3, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 10", ShortName = "P10", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 10, 31, 0, 0, 0, DateTimeKind.Utc), Month = 10, Quarter = 4, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 11", ShortName = "P11", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 11, 30, 0, 0, 0, DateTimeKind.Utc), Month = 11, Quarter = 4, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 12", ShortName = "P12", Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), Month = 12, Quarter = 4, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new FiscalCalendarPeriod { FiscalCalendar = existingCalendar.RecId, FiscalCalendarYear = existingYear.RecId, Name = "Period 13", ShortName = "P13", Type = FiscalPeriodType.Closing, StartDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), Month = 12, Quarter = 4, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
                };
                // A customized date range is still the same named period. Do not
                // insert a second period over it when repairing a partial seed.
                await db.Set<FiscalCalendarPeriod>().AddRangeAsync(periods.Where(p => !existingPeriods.Any(e =>
                    e.Name == p.Name || (e.Type == p.Type && e.StartDate == p.StartDate && e.EndDate == p.EndDate))), cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            var existingLedger = await db.Set<Ledger>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.DataAreaId == "dat" && x.FiscalCalendar == existingCalendar.RecId, cancellationToken);
            if (existingLedger != null)
            {
                var ledgerYear = await db.Set<LedgerFiscalCalendarYear>().IgnoreQueryFilters().FirstOrDefaultAsync(y => y.Ledger == existingLedger.RecId && y.FiscalCalendarYear == existingYear.RecId, cancellationToken);
                if (ledgerYear == null)
                {
                    ledgerYear = new LedgerFiscalCalendarYear { Ledger = existingLedger.RecId, FiscalCalendarYear = existingYear.RecId, Status = FiscalPeriodStatus.Open, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" };
                    await db.Set<LedgerFiscalCalendarYear>().AddAsync(ledgerYear, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);
                }

                var periods = await db.Set<FiscalCalendarPeriod>().IgnoreQueryFilters().Where(p => p.FiscalCalendarYear == existingYear.RecId).ToListAsync(cancellationToken);
                var existingLedgerPeriods = await db.Set<LedgerFiscalCalendarPeriod>().IgnoreQueryFilters().Where(p => p.Ledger == existingLedger.RecId).Select(p => p.FiscalCalendarPeriod).ToListAsync(cancellationToken);
                var ledgerPeriodsToAdd = periods.Where(p => !existingLedgerPeriods.Contains(p.RecId)).Select(p => new LedgerFiscalCalendarPeriod { Ledger = existingLedger.RecId, FiscalCalendarPeriod = p.RecId, Status = FiscalPeriodStatus.Open, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }).ToList();
                if (ledgerPeriodsToAdd.Any())
                {
                    await db.Set<LedgerFiscalCalendarPeriod>().AddRangeAsync(ledgerPeriodsToAdd, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
