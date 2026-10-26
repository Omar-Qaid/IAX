using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance
{
    public class UnitOfMeasureSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.Inventory.UnitOfMeasure";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Reference;
        public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.CompanyInfo" };
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "global";

        public override async Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        {
            var db = context.Db;
            var createdBy = "sys";

            // 1. Units of Measure
            var uomSeeds = new[]
            {
                // Quantity
                new UnitOfMeasure { Symbol = "EA", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "PCS", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "DOZ", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "PR", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "SET", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },

                // Weight
                new UnitOfMeasure { Symbol = "KG", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "G", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "TON", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "LB", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },

                // Volume
                new UnitOfMeasure { Symbol = "L", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "ML", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "GAL", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },

                // Length
                new UnitOfMeasure { Symbol = "M", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "CM", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "MM", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "FT", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },

                // Packaging
                new UnitOfMeasure { Symbol = "BOX", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "CTN", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "PCK", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "PLT", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "BAG", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "CAN", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "BTL", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "ROL", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },

                // Time (services / labour)
                new UnitOfMeasure { Symbol = "HR", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "DAY", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "MON", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
            };

            var existingUOMCodes = await db.UnitOfMeasures.IgnoreQueryFilters().Select(u => u.Symbol).ToListAsync(cancellationToken);
            var uomsToAdd = uomSeeds.Where(u => !existingUOMCodes.Contains(u.Symbol)).ToList();
            if (uomsToAdd.Any())
            {
                await db.UnitOfMeasures.AddRangeAsync(uomsToAdd, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            // 2. Base Units per UOM Class
            var allUoms = await db.UnitOfMeasures.IgnoreQueryFilters().ToListAsync(cancellationToken);
            var uomBySymbol = allUoms.Where(u => !string.IsNullOrEmpty(u.Symbol)).ToDictionary(u => u.Symbol, u => u.RecId);

            if (uomBySymbol.ContainsKey("EA") && uomBySymbol.ContainsKey("KG") && uomBySymbol.ContainsKey("L") && uomBySymbol.ContainsKey("M") && uomBySymbol.ContainsKey("HR"))
            {
                var baseUnitSeeds = new[]
                {
                    new UnitOfMeasureBaseUnit { UnitOfMeasure = uomBySymbol["EA"], UnitOfMeasureClass = IAX.IXApi.Modules.Finance.Common.UnitOfMeasureClass.Quantity },
                    new UnitOfMeasureBaseUnit { UnitOfMeasure = uomBySymbol["KG"], UnitOfMeasureClass = IAX.IXApi.Modules.Finance.Common.UnitOfMeasureClass.Mass },
                    new UnitOfMeasureBaseUnit { UnitOfMeasure = uomBySymbol["L"], UnitOfMeasureClass = IAX.IXApi.Modules.Finance.Common.UnitOfMeasureClass.Volume },
                    new UnitOfMeasureBaseUnit { UnitOfMeasure = uomBySymbol["M"], UnitOfMeasureClass = IAX.IXApi.Modules.Finance.Common.UnitOfMeasureClass.Length },
                    new UnitOfMeasureBaseUnit { UnitOfMeasure = uomBySymbol["HR"], UnitOfMeasureClass = IAX.IXApi.Modules.Finance.Common.UnitOfMeasureClass.Time },
                };

                var existingBaseUnitKeys = await db.UnitOfMeasureBaseUnits.IgnoreQueryFilters()
                    .Select(b => new { b.UnitOfMeasure, b.UnitOfMeasureClass }).ToListAsync(cancellationToken);
                var baseUnitsToAdd = baseUnitSeeds
                    .Where(b => !existingBaseUnitKeys.Any(e => e.UnitOfMeasure == b.UnitOfMeasure && e.UnitOfMeasureClass == b.UnitOfMeasureClass))
                    .ToList();
                if (baseUnitsToAdd.Any())
                {
                    await db.UnitOfMeasureBaseUnits.AddRangeAsync(baseUnitsToAdd, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);
                }

                // 3. Conversions
                var conversionSeeds = new (string From, string To, decimal Factor)[]
                {
                    ("G", "KG", 0.001m),
                    ("TON", "KG", 1000m),
                    ("LB", "KG", 0.453592m),
                    ("ML", "L", 0.001m),
                    ("GAL", "L", 3.78541m),
                    ("CM", "M", 0.01m),
                    ("MM", "M", 0.001m),
                    ("FT", "M", 0.3048m),
                    ("PCS", "EA", 1m),
                    ("DOZ", "EA", 12m),
                    ("PR", "EA", 2m),
                };

                var existingConvKeys = await db.UnitOfMeasureConversions.IgnoreQueryFilters()
                    .Select(c => new { c.FromUnitOfMeasure, c.ToUnitOfMeasure }).ToListAsync(cancellationToken);

                var conversionsToAdd = conversionSeeds
                    .Where(s => uomBySymbol.ContainsKey(s.From) && uomBySymbol.ContainsKey(s.To))
                    .Select(s => new UnitOfMeasureConversion
                    {
                        FromUnitOfMeasure = uomBySymbol[s.From],
                        ToUnitOfMeasure = uomBySymbol[s.To],
                        Factor = s.Factor,
                        Numerator = 1,
                        Denominator = 1,
                        InnerOffset = 0,
                        OuterOffset = 0,
                        Rounding = IAX.IXApi.Modules.Finance.Common.UnitOfMeasureRounding.Nearest,
                    })
                    .Where(c => !existingConvKeys.Any(e => e.FromUnitOfMeasure == c.FromUnitOfMeasure && e.ToUnitOfMeasure == c.ToUnitOfMeasure))
                    .ToList();

                if (conversionsToAdd.Any())
                {
                    await db.UnitOfMeasureConversions.AddRangeAsync(conversionsToAdd, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
