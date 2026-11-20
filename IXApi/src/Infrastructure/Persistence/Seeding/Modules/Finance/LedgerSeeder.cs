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
    /// <summary>
    /// The company's ledger and the open status of its fiscal periods.
    /// <para>
    /// Every subledger posting resolves this row first: <c>SubledgerPostingSupport.LoadLedgerContextAsync</c>
    /// requires exactly one ledger for the company, a non-empty accounting currency, an operating
    /// fiscal period containing the posting date, and that period's ledger status to be open.
    /// Without it, inventory journals, sales invoices, customer payments and settlement all fail.
    /// </para>
    /// </summary>
    public class LedgerSeeder : BaseModuleSeeder
    {
        private const string Company = "dat";
        private const string CalendarId = "HBMC";
        private const string AccountingCurrency = "SAR";

        public override string Id => "Finance.GeneralLedger.Ledger";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Reference;

        public override IReadOnlyCollection<string> DependsOn => new[]
        {
            "Finance.Foundation.CompanyInfo",
            "Finance.GeneralLedger.FiscalCalendars",
            "Finance.GeneralLedger.Currencies",
        };

        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => Company;

        public override Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
            => SeedDataAsync(context.Db, cancellationToken);

        public async Task SeedDataAsync(DbContext db, CancellationToken cancellationToken = default)
        {
            const string createdBy = "sys";

            var company = await db.Set<CompanyInfo>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.DataArea == Company, cancellationToken);
            var calendar = await db.Set<FiscalCalendar>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.DataAreaId == Company && x.CalendarId == CalendarId, cancellationToken);
            // The calendar seeder owns these; without them a ledger would point at nothing.
            if (company == null || calendar == null) return;

            var rateType = await db.Set<ExchangeRateType>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.DataAreaId == Company && x.Name == "Default", cancellationToken);

            var ledger = await db.Set<Ledger>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.DataAreaId == Company, cancellationToken);
            if (ledger == null)
            {
                ledger = new Ledger
                {
                    Name = CalendarId,
                    Description = "Primary ledger",
                    PrimaryForLegalEntity = company.RecId,
                    FiscalCalendar = calendar.RecId,
                    AccountingCurrency = AccountingCurrency,
                    DefaultExchangeRateType = rateType?.RecId ?? 0,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = Company,
                };
                await db.Set<Ledger>().AddAsync(ledger, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // Converge only what posting requires, so an operator's other ledger choices stand.
                var changed = false;
                if (string.IsNullOrWhiteSpace(ledger.AccountingCurrency))
                {
                    ledger.AccountingCurrency = AccountingCurrency;
                    changed = true;
                }
                if (ledger.FiscalCalendar == 0)
                {
                    ledger.FiscalCalendar = calendar.RecId;
                    changed = true;
                }
                if (ledger.PrimaryForLegalEntity == 0)
                {
                    ledger.PrimaryForLegalEntity = company.RecId;
                    changed = true;
                }
                if (changed) await db.SaveChangesAsync(cancellationToken);
            }

            // Open the ledger's years and periods. A period with no ledger status, or one that is
            // not open, makes every posting on a date inside it fail.
            var years = await db.Set<FiscalCalendarYear>().IgnoreQueryFilters()
                .Where(x => x.FiscalCalendar == calendar.RecId).ToListAsync(cancellationToken);
            foreach (var year in years)
            {
                if (!await db.Set<LedgerFiscalCalendarYear>().IgnoreQueryFilters().AnyAsync(
                        x => x.Ledger == ledger.RecId && x.FiscalCalendarYear == year.RecId, cancellationToken))
                {
                    await db.Set<LedgerFiscalCalendarYear>().AddAsync(new LedgerFiscalCalendarYear
                    {
                        Ledger = ledger.RecId,
                        FiscalCalendarYear = year.RecId,
                        Status = FiscalPeriodStatus.Open,
                        CreatedBy = createdBy,
                        OwnerAccountId = createdBy,
                        DataAreaId = Company,
                    }, cancellationToken);
                }
            }

            var periods = await db.Set<FiscalCalendarPeriod>().IgnoreQueryFilters()
                .Where(x => x.FiscalCalendar == calendar.RecId).ToListAsync(cancellationToken);
            foreach (var period in periods)
            {
                if (!await db.Set<LedgerFiscalCalendarPeriod>().IgnoreQueryFilters().AnyAsync(
                        x => x.Ledger == ledger.RecId && x.FiscalCalendarPeriod == period.RecId, cancellationToken))
                {
                    await db.Set<LedgerFiscalCalendarPeriod>().AddAsync(new LedgerFiscalCalendarPeriod
                    {
                        Ledger = ledger.RecId,
                        FiscalCalendarPeriod = period.RecId,
                        Status = FiscalPeriodStatus.Open,
                        CreatedBy = createdBy,
                        OwnerAccountId = createdBy,
                        DataAreaId = Company,
                    }, cancellationToken);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
