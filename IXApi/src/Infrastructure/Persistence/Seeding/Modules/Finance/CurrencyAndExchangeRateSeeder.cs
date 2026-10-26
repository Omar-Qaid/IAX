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
    public class CurrencyAndExchangeRateSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.GeneralLedger.Currencies";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Reference;
        public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.CompanyInfo" };
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "dat";

        public override async Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        {
            var db = context.Db;
            var createdBy = "sys";

            var currencySeeds = new[]
            {
                new Currency { CurrencyCode = "SAR", Symbol = "ر.س", RoundingPrecision = 0.01m, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new Currency { CurrencyCode = "USD", Symbol = "$", RoundingPrecision = 0.01m, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new Currency { CurrencyCode = "EUR", RoundingPrecision = 0.01m, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new Currency { CurrencyCode = "AED", Symbol = "د.إ", RoundingPrecision = 0.01m, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
            };

            var existingCurrencyCodes = await db.Currencies.IgnoreQueryFilters().Select(c => c.CurrencyCode).ToListAsync(cancellationToken);
            var currenciesToAdd = currencySeeds.Where(c => !existingCurrencyCodes.Contains(c.CurrencyCode)).ToList();
            if (currenciesToAdd.Any())
            {
                await db.Currencies.AddRangeAsync(currenciesToAdd, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            var typeSeeds = new[]
            {
                new ExchangeRateType { Name = "Default", Description = "Default global rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Average", Description = "Default average rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Budget", Description = "Default budget rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Closing", Description = "Default closing rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Spot", Description = "Current market spot rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Forward", Description = "Forward contract rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Historical", Description = "Historical fixed rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Projected", Description = "Projected future rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Intercompany", Description = "Internal transfer rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Realized", Description = "Realized gain/loss rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Unrealized", Description = "Unrealized gain/loss rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Tax", Description = "Tax authority rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "IFRS", Description = "IFRS compliance rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "GAAP", Description = "Local GAAP rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Management", Description = "Management reporting rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Regulatory", Description = "Regulatory reporting rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Daily", Description = "End of day rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Weekly", Description = "End of week rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Monthly", Description = "End of month rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Commercial", Description = "Commercial business rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Retail", Description = "Retail customer rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Wholesale", Description = "Wholesale partner rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Corporate", Description = "Corporate treasury rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new ExchangeRateType { Name = "Market", Description = "Open market rate", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
            };

            var existingTypeNames = await db.Set<ExchangeRateType>().IgnoreQueryFilters().Select(t => t.Name).ToListAsync(cancellationToken);
            var typesToAdd = typeSeeds.Where(t => !existingTypeNames.Contains(t.Name)).ToList();
            if (typesToAdd.Any())
            {
                await db.Set<ExchangeRateType>().AddRangeAsync(typesToAdd, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            var allTypes = await db.Set<ExchangeRateType>().IgnoreQueryFilters().ToListAsync(cancellationToken);
            var pairsToAdd = new List<ExchangeRateCurrencyPair>();
            var ratesToAdd = new List<ExchangeRate>();

            var existingPairs = await db.Set<ExchangeRateCurrencyPair>().IgnoreQueryFilters()
                .Select(p => new { p.FromCurrencyCode, p.ToCurrencyCode, p.ExchangeRateType }).ToListAsync(cancellationToken);
            var existingRates = await db.Set<ExchangeRate>().IgnoreQueryFilters()
                .Select(r => r.ExchangeRateCurrencyPair).ToListAsync(cancellationToken);

            foreach (var t in allTypes)
            {
                var pairSeeds = new[]
                {
                    new ExchangeRateCurrencyPair { FromCurrencyCode = "AED", ToCurrencyCode = "SAR", ExchangeRateType = t.RecId, ExchangeRateDisplayFactor = ExchangeRateDisplayFactor.One, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new ExchangeRateCurrencyPair { FromCurrencyCode = "EUR", ToCurrencyCode = "SAR", ExchangeRateType = t.RecId, ExchangeRateDisplayFactor = ExchangeRateDisplayFactor.One, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new ExchangeRateCurrencyPair { FromCurrencyCode = "USD", ToCurrencyCode = "SAR", ExchangeRateType = t.RecId, ExchangeRateDisplayFactor = ExchangeRateDisplayFactor.One, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
                };

                foreach (var p in pairSeeds)
                {
                    if (!existingPairs.Any(e => e.FromCurrencyCode == p.FromCurrencyCode && e.ToCurrencyCode == p.ToCurrencyCode && e.ExchangeRateType == p.ExchangeRateType)
                        && !pairsToAdd.Any(e => e.FromCurrencyCode == p.FromCurrencyCode && e.ToCurrencyCode == p.ToCurrencyCode && e.ExchangeRateType == p.ExchangeRateType))
                    {
                        pairsToAdd.Add(p);
                    }
                }
            }

            if (pairsToAdd.Any())
            {
                await db.Set<ExchangeRateCurrencyPair>().AddRangeAsync(pairsToAdd, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            // Seed ExchangeRates for all pairs that don't have them
            var updatedPairs = await db.Set<ExchangeRateCurrencyPair>().IgnoreQueryFilters().ToListAsync(cancellationToken);
            foreach (var pair in updatedPairs)
            {
                if (!existingRates.Contains(pair.RecId) && !ratesToAdd.Any(r => r.ExchangeRateCurrencyPair == pair.RecId))
                {
                    decimal rateVal = 1.0m;
                    if (pair.FromCurrencyCode == "USD") rateVal = 3.75m;
                    if (pair.FromCurrencyCode == "EUR") rateVal = 4.10m;
                    if (pair.FromCurrencyCode == "AED") rateVal = 1.02m;

                    ratesToAdd.Add(new ExchangeRate
                    {
                        ExchangeRateCurrencyPair = pair.RecId,
                        ExchangeRateValue = rateVal,
                        ValidFrom = new DateTime(2024, 7, 1),
                        ValidTo = new DateTime(2026, 12, 31),
                        IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat"
                    });
                }
            }

            if (ratesToAdd.Any())
            {
                await db.Set<ExchangeRate>().AddRangeAsync(ratesToAdd, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
