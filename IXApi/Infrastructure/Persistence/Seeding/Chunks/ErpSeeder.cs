using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.ERP.Shared.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.ERP.Inventory;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.ERP.AccountsReceivable;


namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    public class ErpSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var sysUser = await users.FindByNameAsync("sys");
            var createdBy = sysUser?.Id ?? "sys";

            #region Customer & Vendor Groups (AX CustGroup / VendGroup)
            var custGroupSeeds = new[]
            {
                new CustGroup { CustGroupId = "Consultant",   Name = "Consultant Customers",IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "Contractor",   Name = "Contractor Customers",IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "Government",   Name = "Government Customers",IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "IKKAff",       Name = "IKK Affiliates",      IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "SisComp",      Name = "Sister Companies",    IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "Traders",      Name = "Trading Customers",   IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "DOM",          Name = "Domestic",            IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "INT",          Name = "International",       IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-RTL",     Name = "Retail Customers",    IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-WHL",     Name = "Wholesale Customers", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-VIP",     Name = "VIP Customers",       IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-GOV",     Name = "Government",          IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
            };
            var existingCustGroupCodes = await db.CustGroups.IgnoreQueryFilters().Select(g => g.CustGroupId).ToListAsync(ct);
            var custGroupsToAdd = custGroupSeeds.Where(g => !existingCustGroupCodes.Contains(g.CustGroupId)).ToList();
            if (custGroupsToAdd.Any()) { await db.CustGroups.AddRangeAsync(custGroupsToAdd, ct); await db.SaveChangesAsync(ct); }

            #endregion

            #region UnitOfMeasure — Standard Units of Measure
            var uomSeeds = new[]
            {
                // Quantity
                new UnitOfMeasure { Symbol = "EA",       IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure { Symbol = "PCS",       IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new UnitOfMeasure {Symbol = "DOZ", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "PR", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "SET", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},

                // Weight
                new UnitOfMeasure {Symbol = "KG", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "G", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "TON", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "LB", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},

                // Volume
                new UnitOfMeasure {Symbol = "L", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "ML", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "GAL", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},

                // Length
                new UnitOfMeasure {Symbol = "M", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "CM", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "MM", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "FT", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},

                // Packaging
                new UnitOfMeasure {Symbol = "BOX", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "CTN", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "PCK", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "PLT", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "BAG", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "CAN", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "BTL", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "ROL", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},

                // Time (services / labour)
                new UnitOfMeasure {Symbol = "HR", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "DAY", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
                new UnitOfMeasure {Symbol = "MON", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy},
            };

            var existingUOMCodes = await db.UnitOfMeasures.IgnoreQueryFilters().Select(u => u.Symbol).ToListAsync(ct);
            var uomsToAdd = uomSeeds.Where(u => !existingUOMCodes.Contains(u.Symbol)).ToList();
            if (uomsToAdd.Any())
            {
                await db.UnitOfMeasures.AddRangeAsync(uomsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion

            /* 
#region UnitOfMeasureBaseUnit — Base unit per UOM class
            // UnitOfMeasureClass: 0=None 1=Quantity 2=Weight 3=Volume 4=Length 5=Area 6=Time
            var uomIdByCodeForBase = await db.UnitOfMeasures.IgnoreQueryFilters()
                .ToDictionaryAsync(u => u.Code!, u => u.Id, ct);

            var baseUnitSeeds = new[]
            {
                // Class 1 – Quantity  → EA
                new UnitOfMeasureBaseUnit { UnitOfMeasureId = uomIdByCodeForBase["EA"],  UnitOfMeasureClass = 1 },
                // Class 2 – Weight    → KG
                new UnitOfMeasureBaseUnit { UnitOfMeasureId = uomIdByCodeForBase["KG"],  UnitOfMeasureClass = 2 },
                // Class 3 – Volume    → L
                new UnitOfMeasureBaseUnit { UnitOfMeasureId = uomIdByCodeForBase["L"],   UnitOfMeasureClass = 3 },
                // Class 4 – Length    → M
                new UnitOfMeasureBaseUnit { UnitOfMeasureId = uomIdByCodeForBase["M"],   UnitOfMeasureClass = 4 },
                // Class 6 – Time      → HR
                new UnitOfMeasureBaseUnit { UnitOfMeasureId = uomIdByCodeForBase["HR"],  UnitOfMeasureClass = 6 },
            };

            var existingBaseUnitKeys = await db.UnitOfMeasureBaseUnits.IgnoreQueryFilters()
                .Select(b => new { b.UnitOfMeasureId, b.UnitOfMeasureClass }).ToListAsync(ct);
            var baseUnitsToAdd = baseUnitSeeds
                .Where(b => !existingBaseUnitKeys.Any(e => e.UnitOfMeasureId == b.UnitOfMeasureId && e.UnitOfMeasureClass == b.UnitOfMeasureClass))
                .ToList();
            if (baseUnitsToAdd.Any())
            {
                await db.UnitOfMeasureBaseUnits.AddRangeAsync(baseUnitsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion

            #region UnitOfMeasureConversion — Standard global conversion factors
            // Factor: how many base units does 1 FromUOM equal (ToUOM is always the base unit).
            // Denominator=1, Numerator=1 means use Factor directly.
            var conversionSeeds = new (string From, string To, decimal Factor)[]
            {
                // Weight (base = KG)
                ("G",   "KG",  0.001m),
                ("TON", "KG",  1000m),
                ("LB",  "KG",  0.453592m),

                // Volume (base = L)
                ("ML",  "L",   0.001m),
                ("GAL", "L",   3.78541m),

                // Length (base = M)
                ("CM",  "M",   0.01m),
                ("MM",  "M",   0.001m),
                ("FT",  "M",   0.3048m),

                // Quantity (base = EA)
                ("PCS", "EA",  1m),
                ("DOZ", "EA",  12m),
                ("PR",  "EA",  2m),
            };

            var existingConvKeys = await db.UnitOfMeasureConversions.IgnoreQueryFilters()
                .Select(c => new { c.FromUnitOfMeasureId, c.ToUnitOfMeasureId }).ToListAsync(ct);

            var conversionsToAdd = conversionSeeds
                .Where(s => uomIdByCodeForBase.ContainsKey(s.From) && uomIdByCodeForBase.ContainsKey(s.To))
                .Select(s => new UnitOfMeasureConversion
                {
                    FromUnitOfMeasureId = uomIdByCodeForBase[s.From],
                    ToUnitOfMeasureId   = uomIdByCodeForBase[s.To],
                    Factor              = s.Factor,
                    Numerator           = 1,
                    Denominator         = 1,
                    InnerOffset         = 0,
                    OuterOffset         = 0,
                    Rounding            = 0,
                })
                .Where(c => !existingConvKeys.Any(e => e.FromUnitOfMeasureId == c.FromUnitOfMeasureId && e.ToUnitOfMeasureId == c.ToUnitOfMeasureId))
                .ToList();

            if (conversionsToAdd.Any())
            {
                await db.UnitOfMeasureConversions.AddRangeAsync(conversionsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion

            #region UnitOfMeasureTranslation — Arabic (ar) translations
            var translationSeeds = new (string Code, string Description)[]
            {
                ("EA",  "قطعة"),    ("PCS", "حبة"),      ("DOZ", "دزينة"),
                ("PR",  "زوج"),     ("SET", "طقم"),
                ("KG",  "كيلوجرام"),("G",   "جرام"),     ("TON", "طن"),       ("LB",  "رطل"),
                ("L",   "لتر"),     ("ML",  "مليلتر"),   ("GAL", "جالون"),
                ("M",   "متر"),     ("CM",  "سنتيمتر"),  ("MM",  "مليمتر"),   ("FT",  "قدم"),
                ("BOX", "صندوق"),   ("CTN", "كرتون"),    ("PCK", "حزمة"),
                ("PLT", "منصة"),    ("BAG", "كيس"),      ("CAN", "علبة"),
                ("BTL", "زجاجة"),   ("ROL", "لفة"),
                ("HR",  "ساعة"),    ("DAY", "يوم"),      ("MON", "شهر"),
            };

            var existingTranslKeys = await db.UnitOfMeasureTranslations.IgnoreQueryFilters()
                .Select(t => new { t.UnitOfMeasureId, t.LanguageId }).ToListAsync(ct);

            var translationsToAdd = translationSeeds
                .Where(s => uomIdByCodeForBase.ContainsKey(s.Code))
                .Select(s => new UnitOfMeasureTranslation
                {
                    UnitOfMeasureId = uomIdByCodeForBase[s.Code],
                    LanguageId      = "ar",
                    Description     = s.Description,
                })
                .Where(t => !existingTranslKeys.Any(e => e.UnitOfMeasureId == t.UnitOfMeasureId && e.LanguageId == t.LanguageId))
                .ToList();

            if (translationsToAdd.Any())
            {
                await db.UnitOfMeasureTranslations.AddRangeAsync(translationsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion

            */ 
#region InventItemGroups — Standard Item Groups
            // InventItemGroup no longer carries a business code/name — it is keyed by RecId only.
            // Seed a fixed number of groups (idempotent by count) and map the legacy seed codes to
            // them by creation order so the rest of the seeder can still look groups up by code.
            var groupCodes = new[] { "RAW", "FIN", "CONS", "SPR" };
            var existingGroupCount = await db.InventItemGroups.IgnoreQueryFilters().CountAsync(ct);
            if (existingGroupCount < groupCodes.Length)
            {
                var groupsToAdd = Enumerable.Range(0, groupCodes.Length - existingGroupCount)
                    .Select(_ => new InventItemGroup { IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy })
                    .ToList();
                await db.InventItemGroups.AddRangeAsync(groupsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion

            #region InventItems + UOM conversions / barcodes / prices — Sample Items
            var orderedGroups = await db.InventItemGroups.IgnoreQueryFilters().OrderBy(g => g.RecId).Take(groupCodes.Length).ToListAsync(ct);
            var groupIdByCode = groupCodes.Zip(orderedGroups, (code, g) => (code, g.RecId)).ToDictionary(x => x.code, x => x.RecId);
            var uomIdByCode = await db.UnitOfMeasures.IgnoreQueryFilters().ToDictionaryAsync(u => u.Symbol, u => u.RecId, ct);
            var existingItemCodes = await db.InventTables.IgnoreQueryFilters().Select(i => i.ItemId).ToListAsync(ct);

            // Each item carries its UOM lines: (uom code, packing unit, barcode, isPurchasing, isSelling, isInventory).
            var itemSeeds = new[]
            {
                new
                {
                    Code = "ITM-1001", Name = "Bottled Water 500ml", NameAR = "مياه معبأة 500 مل",
                    Group = "CONS", Description = "Drinking water, 500 ml bottle",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("BTL", 1m,  "6280001000017", false, true,  true),
                        ("CTN", 24m, "6280001000024", true,  false, false),
                    }
                },
                new
                {
                    Code = "ITM-1002", Name = "A4 Paper Ream", NameAR = "رزمة ورق A4",
                    Group = "CONS", Description = "A4 printing paper, 500 sheets per ream",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("PCK", 1m, "6280001000031", false, true, true),
                        ("BOX", 5m, "6280001000048", true,  false, false),
                    }
                },
                new
                {
                    Code = "ITM-1003", Name = "Steel Rod 12mm", NameAR = "قضيب حديد 12 مم",
                    Group = "RAW", Description = "Reinforcement steel rod, 12 mm diameter",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("M",   1m,    "6280001000055", false, true,  true),
                        ("TON", 1000m, "6280001000062", true,  false, false),
                    }
                },
                new
                {
                    Code = "ITM-1004", Name = "T-Shirt Cotton", NameAR = "تي شيرت قطن",
                    Group = "FIN", Description = "100% cotton T-shirt",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("PCS", 1m,  "6280001000079", false, true,  true),
                        ("DOZ", 12m, "6280001000086", true,  false, false),
                    }
                },
                new
                {
                    Code = "ITM-1005", Name = "Cooking Oil 1L", NameAR = "زيت طبخ 1 لتر",
                    Group = "FIN", Description = "Vegetable cooking oil, 1 litre bottle",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("BTL", 1m,  "6280001000093", false, true,  true),
                        ("CTN", 12m, "6280001000109", true,  false, false),
                    }
                },
            };

            var itemsToAdd = new List<InventTable>();
            var modulesToAdd = new List<InventTableModule>();

            foreach (var seed in itemSeeds)
            {
                if (existingItemCodes.Contains(seed.Code)) continue;
                if (!groupIdByCode.TryGetValue(seed.Group, out var groupId)) continue;

                // Only keep UOM rows whose UOM code resolved to a real InventUOM id.
                var uomRows = seed.Uoms
                    .Where(u => uomIdByCode.ContainsKey(u.Uom))
                    .ToList();
                if (uomRows.Count == 0) continue;

                // Resolve the role rows. The Inventory row is the BASE unit (FactorToBase = 1).
                // Selling/Purchasing fall back to the base row when no explicit row is flagged.
                var baseRow = uomRows.FirstOrDefault(u => u.Inv);
                if (baseRow == default) continue; // every item must have a base/inventory unit
                var sellRow = uomRows.FirstOrDefault(u => u.Sell);
                if (sellRow == default) sellRow = baseRow;
                var buyRow = uomRows.FirstOrDefault(u => u.Buy);
                if (buyRow == default) buyRow = baseRow;

                var item = new InventTable
                {
                    ItemId = seed.Code,

                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                };
                itemsToAdd.Add(item);

                // Seeding InventTableModule unitID for invent, purch, sales and price
                modulesToAdd.Add(new InventTableModule
                {
                    ItemId = seed.Code,
                    ModuleType = ModuleInventPurchSales.Inventory,
                    UnitId = baseRow.Uom,
                    Price = 10m,
                    PriceUnit = 1m,
                    LineDisc = "",
                    TaxItemGroupId = "FULL",

                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy
                });

                modulesToAdd.Add(new InventTableModule
                {
                    ItemId = seed.Code,
                    ModuleType = ModuleInventPurchSales.Purchase,
                    UnitId = buyRow.Uom,
                    Price = 12m,
                    PriceUnit = 1m,
                    LineDisc = "",
                    TaxItemGroupId = "FULL",
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy
                });

                modulesToAdd.Add(new InventTableModule
                {
                    ItemId = seed.Code,
                    ModuleType = ModuleInventPurchSales.Sales,
                    UnitId = sellRow.Uom,
                    Price = 15m,
                    PriceUnit = 1m,
                    LineDisc = "",
                    TaxItemGroupId = "FULL",

                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy
                });
            }

            if (itemsToAdd.Any())
            {
                await db.InventTables.AddRangeAsync(itemsToAdd, ct);
                await db.InventTableModules.AddRangeAsync(modulesToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion

            #region Sample Customers & Vendors (AX CustTable / VendTable)
            var existingCustCodes = await db.CustTables.IgnoreQueryFilters().Select(p => p.AccountNum).ToListAsync(ct);
            var custsToAdd = new List<CustTable>();

            if (!existingCustCodes.Contains("C00015"))
                custsToAdd.Add(new CustTable { AccountNum = "C00015", CustGroupId = "Consultant", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, CurrencyCode = "SAR", TaxGroupId = "DOM" });

            if (!existingCustCodes.Contains("CUST-100"))
                custsToAdd.Add(new CustTable { AccountNum = "CUST-100", CustGroupId = "Consultant", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, CurrencyCode = "SAR", TaxGroupId = "DOM" });

            if (!existingCustCodes.Contains("CUST-200"))
                custsToAdd.Add(new CustTable { AccountNum = "CUST-200", CustGroupId = "CUST-RTL", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, CurrencyCode = "SAR", TaxGroupId = "DOM" });

            if (!existingCustCodes.Contains("CUST-300"))
                custsToAdd.Add(new CustTable { AccountNum = "CUST-300", CustGroupId = "Consultant", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, CurrencyCode = "USD", TaxGroupId = "DOM" });

            if (!existingCustCodes.Contains("CUS-00001"))
                custsToAdd.Add(new CustTable { AccountNum = "CUS-00001", CustGroupId = "CUST-RTL", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, CurrencyCode = "SAR", TaxGroupId = "DOM" });

            if (!existingCustCodes.Contains("CUS-00002"))
                custsToAdd.Add(new CustTable { AccountNum = "CUS-00002", CustGroupId = "CUST-VIP", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, CurrencyCode = "USD", TaxGroupId = "DOM" });

            if (custsToAdd.Any()) { await db.CustTables.AddRangeAsync(custsToAdd, ct); await db.SaveChangesAsync(ct); }
            #endregion

            #region Currencies — Currencies and Exchange Rates
            var currencySeeds = new[]
            {
                new Currency { CurrencyCode = "SAR", Symbol = "ر.س", RoundingPrecision = 0.01m, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new Currency { CurrencyCode = "USD",  Symbol = "$",  RoundingPrecision = 0.01m, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new Currency { CurrencyCode = "EUR",  RoundingPrecision = 0.01m, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new Currency { CurrencyCode = "AED",  Symbol = "د.إ",  RoundingPrecision = 0.01m, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
            };

            var existingCurrencyCodes = await db.Currencies.IgnoreQueryFilters().Select(c => c.CurrencyCode).ToListAsync(ct);
            var currenciesToAdd = currencySeeds.Where(c => !existingCurrencyCodes.Contains(c.CurrencyCode)).ToList();
            if (currenciesToAdd.Any())
            {
                await db.Currencies.AddRangeAsync(currenciesToAdd, ct);
                await db.SaveChangesAsync(ct);
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

            var existingTypeNames = await db.Set<ExchangeRateType>().IgnoreQueryFilters().Select(t => t.Name).ToListAsync(ct);
            var typesToAdd = typeSeeds.Where(t => !existingTypeNames.Contains(t.Name)).ToList();
            if (typesToAdd.Any())
            {
                await db.Set<ExchangeRateType>().AddRangeAsync(typesToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            var allTypes = await db.Set<ExchangeRateType>().IgnoreQueryFilters().ToListAsync(ct);
            var pairsToAdd = new List<ExchangeRateCurrencyPair>();
            var ratesToAdd = new List<ExchangeRate>();

            var existingPairs = await db.Set<ExchangeRateCurrencyPair>().IgnoreQueryFilters()
                .Select(p => new { p.FromCurrencyCode, p.ToCurrencyCode, p.ExchangeRateType }).ToListAsync(ct);
            var existingRates = await db.Set<ExchangeRate>().IgnoreQueryFilters()
                .Select(r => r.ExchangeRateCurrencyPair).ToListAsync(ct);

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
                await db.Set<ExchangeRateCurrencyPair>().AddRangeAsync(pairsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // Now seed ExchangeRates for all pairs that don't have them
            var updatedPairs = await db.Set<ExchangeRateCurrencyPair>().IgnoreQueryFilters().ToListAsync(ct);
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
                await db.Set<ExchangeRate>().AddRangeAsync(ratesToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion

            #region Fiscal Calendars
            // Seed Fiscal Calendar: HBMC
            var existingCalendar = await db.Set<FiscalCalendar>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CalendarId == "HBMC", ct);
            if (existingCalendar == null)
            {
                existingCalendar = new FiscalCalendar { CalendarId = "HBMC", Description = "HBMC Default calendar", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" };
                await db.Set<FiscalCalendar>().AddAsync(existingCalendar, ct);
                await db.SaveChangesAsync(ct);
            }

            // Seed Fiscal Calendar Year: 2026
            var existingYear = await db.Set<FiscalCalendarYear>().IgnoreQueryFilters().FirstOrDefaultAsync(y => y.FiscalCalendar == existingCalendar.RecId && y.Name == "2026", ct);
            if (existingYear == null)
            {
                existingYear = new FiscalCalendarYear { FiscalCalendar = existingCalendar.RecId, Name = "2026", StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" };
                await db.Set<FiscalCalendarYear>().AddAsync(existingYear, ct);
                await db.SaveChangesAsync(ct);
            }

            // Seed Fiscal Calendar Periods
            var existingPeriods = await db.Set<FiscalCalendarPeriod>().IgnoreQueryFilters().Where(p => p.FiscalCalendar == existingCalendar.RecId && p.FiscalCalendarYear == existingYear.RecId).ToListAsync(ct);
            if (!existingPeriods.Any())
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
                await db.Set<FiscalCalendarPeriod>().AddRangeAsync(periods, ct);
                await db.SaveChangesAsync(ct);
            }

            var existingLedger = await db.Set<Ledger>().IgnoreQueryFilters().FirstOrDefaultAsync(ct);
            if (existingLedger != null)
            {
                var ledgerYear = await db.Set<LedgerFiscalCalendarYear>().IgnoreQueryFilters().FirstOrDefaultAsync(y => y.Ledger == existingLedger.RecId && y.FiscalCalendarYear == existingYear.RecId, ct);
                if (ledgerYear == null)
                {
                    ledgerYear = new LedgerFiscalCalendarYear { Ledger = existingLedger.RecId, FiscalCalendarYear = existingYear.RecId, Status = FiscalPeriodStatus.Open, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" };
                    await db.Set<LedgerFiscalCalendarYear>().AddAsync(ledgerYear, ct);
                    await db.SaveChangesAsync(ct);
                }

                var periods = await db.Set<FiscalCalendarPeriod>().IgnoreQueryFilters().Where(p => p.FiscalCalendarYear == existingYear.RecId).ToListAsync(ct);
                var existingLedgerPeriods = await db.Set<LedgerFiscalCalendarPeriod>().IgnoreQueryFilters().Where(p => p.Ledger == existingLedger.RecId).Select(p => p.FiscalCalendarPeriod).ToListAsync(ct);
                var ledgerPeriodsToAdd = periods.Where(p => !existingLedgerPeriods.Contains(p.RecId)).Select(p => new LedgerFiscalCalendarPeriod { Ledger = existingLedger.RecId, FiscalCalendarPeriod = p.RecId, Status = FiscalPeriodStatus.Open, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }).ToList();
                if (ledgerPeriodsToAdd.Any())
                {
                    await db.Set<LedgerFiscalCalendarPeriod>().AddRangeAsync(ledgerPeriodsToAdd, ct);
                    await db.SaveChangesAsync(ct);
                }
            }
            #endregion

            #region Address Entities
            // 0. Location Roles
            var rolesToSeed = new[] {
                new LogisticsLocationRole { Name = "Business", IsPostalAddress = IAX.IXApi.Modules.ERP.Common.NoYes.Yes, IsContactInfo = IAX.IXApi.Modules.ERP.Common.NoYes.Yes },
                new LogisticsLocationRole { Name = "Delivery", IsPostalAddress = IAX.IXApi.Modules.ERP.Common.NoYes.Yes, IsContactInfo = IAX.IXApi.Modules.ERP.Common.NoYes.No },
                new LogisticsLocationRole { Name = "Invoice", IsPostalAddress = IAX.IXApi.Modules.ERP.Common.NoYes.Yes, IsContactInfo = IAX.IXApi.Modules.ERP.Common.NoYes.No },
                new LogisticsLocationRole { Name = "Home", IsPostalAddress = IAX.IXApi.Modules.ERP.Common.NoYes.Yes, IsContactInfo = IAX.IXApi.Modules.ERP.Common.NoYes.Yes },
                new LogisticsLocationRole { Name = "Remit-to", IsPostalAddress = IAX.IXApi.Modules.ERP.Common.NoYes.Yes, IsContactInfo = IAX.IXApi.Modules.ERP.Common.NoYes.No }
            };

            foreach (var r in rolesToSeed)
            {
                var existingRole = await db.Set<LogisticsLocationRole>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Name == r.Name, ct);
                if (existingRole == null)
                {
                    r.CreatedBy = createdBy;
                    r.OwnerAccountId = createdBy;
                    r.DataAreaId = "dat";
                    r.IsActive = true;
                    await db.Set<LogisticsLocationRole>().AddAsync(r, ct);
                }
            }
            await db.SaveChangesAsync(ct);

            // 1. Countries
            var countryUS = await db.Set<LogisticsAddressCountryRegion>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CountryRegionId == "US", ct);
            if (countryUS == null)
            {
                countryUS = new LogisticsAddressCountryRegion { CountryRegionId = "US", IsoCode = "US", AddrFormat = "USA", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCountryRegion>().AddAsync(countryUS, ct);
            }
            var countrySA = await db.Set<LogisticsAddressCountryRegion>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CountryRegionId == "SA", ct);
            if (countrySA == null)
            {
                countrySA = new LogisticsAddressCountryRegion { CountryRegionId = "SA", IsoCode = "SA", AddrFormat = "SAU", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCountryRegion>().AddAsync(countrySA, ct);
            }
            await db.SaveChangesAsync(ct);

            // 2. States
            var stateCA = await db.Set<LogisticsAddressState>().IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StateId == "CA", ct);
            if (stateCA == null)
            {
                stateCA = new LogisticsAddressState { StateId = "CA", Name = "California", CountryRegionId = "US", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressState>().AddAsync(stateCA, ct);
            }
            var stateRIY = await db.Set<LogisticsAddressState>().IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StateId == "RIY", ct);
            if (stateRIY == null)
            {
                stateRIY = new LogisticsAddressState { StateId = "RIY", Name = "Riyadh", CountryRegionId = "SA", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressState>().AddAsync(stateRIY, ct);
            }
            await db.SaveChangesAsync(ct);

            // 2.5. Counties (Cities have a Foreign Key pointing to Counties, so they must be seeded first!)
            var countyCA = await db.Set<LogisticsAddressCounty>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CountryRegionId == "US" && c.StateId == "CA" && c.CountyId == "", ct);
            if (countyCA == null)
            {
                countyCA = new LogisticsAddressCounty { CountryRegionId = "US", StateId = "CA", CountyId = "", Name = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCounty>().AddAsync(countyCA, ct);
            }
            var countyRIY = await db.Set<LogisticsAddressCounty>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CountryRegionId == "SA" && c.StateId == "RIY" && c.CountyId == "", ct);
            if (countyRIY == null)
            {
                countyRIY = new LogisticsAddressCounty { CountryRegionId = "SA", StateId = "RIY", CountyId = "", Name = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCounty>().AddAsync(countyRIY, ct);
            }
            await db.SaveChangesAsync(ct);

            // 3. Cities
            var cityLA = await db.Set<LogisticsAddressCity>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CityKey == "US-CA-LA", ct);
            if (cityLA == null)
            {
                cityLA = new LogisticsAddressCity { CityKey = "US-CA-LA", Name = "Los Angeles", Description = "Los Angeles City", CountryRegionId = "US", StateId = "CA", CountyId = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCity>().AddAsync(cityLA, ct);
            }
            var cityRuh = await db.Set<LogisticsAddressCity>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CityKey == "SA-RIY-RUH", ct);
            if (cityRuh == null)
            {
                cityRuh = new LogisticsAddressCity { CityKey = "SA-RIY-RUH", Name = "Riyadh", Description = "Riyadh City", CountryRegionId = "SA", StateId = "RIY", CountyId = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCity>().AddAsync(cityRuh, ct);
            }
            await db.SaveChangesAsync(ct);

            // 3.5. Fallbacks and Auxiliaries (ZipCodes, Districts)
            var zipLA = await db.Set<LogisticsAddressZipCode>().IgnoreQueryFilters().FirstOrDefaultAsync(z => z.ZipCode == "90001", ct);
            if (zipLA == null)
            {
                zipLA = new LogisticsAddressZipCode { ZipCode = "90001", CountryRegionId = "US", State = "CA", County = "", City = "Los Angeles", CityAlias = "LA", DistrictName = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressZipCode>().AddAsync(zipLA, ct);
            }
            var zipRuh = await db.Set<LogisticsAddressZipCode>().IgnoreQueryFilters().FirstOrDefaultAsync(z => z.ZipCode == "12211", ct);
            if (zipRuh == null)
            {
                zipRuh = new LogisticsAddressZipCode { ZipCode = "12211", CountryRegionId = "SA", State = "RIY", County = "", City = "Riyadh", CityAlias = "RUH", DistrictName = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressZipCode>().AddAsync(zipRuh, ct);
            }

            var districtFallback = await db.Set<LogisticsAddressDistrict>().IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Name == "", ct);
            if (districtFallback == null)
            {
                districtFallback = new LogisticsAddressDistrict { Name = "", Description = "Default Fallback", City = cityLA.RecId, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressDistrict>().AddAsync(districtFallback, ct);
            }
            await db.SaveChangesAsync(ct);

            // 4. Locations
            var locDat = await db.Set<LogisticsLocation>().IgnoreQueryFilters().FirstOrDefaultAsync(l => l.LocationId == "DAT-LOC-1", ct);
            if (locDat == null)
            {
                locDat = new LogisticsLocation { LocationId = "DAT-LOC-1", Description = "DAT Headquarters", IsPostalAddress = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsLocation>().AddAsync(locDat, ct);
            }
            var locHbmc = await db.Set<LogisticsLocation>().IgnoreQueryFilters().FirstOrDefaultAsync(l => l.LocationId == "HBMC-LOC-1", ct);
            if (locHbmc == null)
            {
                locHbmc = new LogisticsLocation { LocationId = "HBMC-LOC-1", Description = "HBMC Headquarters", IsPostalAddress = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsLocation>().AddAsync(locHbmc, ct);
            }
            await db.SaveChangesAsync(ct);

            // 4.5 Pre-seed Parties to satisfy PrivateForParty constraint on Addresses
            var datParty = await db.Set<DirPartyTable>().IgnoreQueryFilters().FirstOrDefaultAsync(p => p.PartyNumber == "DAT-001", ct);
            if (datParty == null)
            {
                datParty = new DirPartyTable { Name = "Company accounts data", NameAlias = "dat", PartyNumber = "DAT-001", LanguageId = "en-us", AddressBookNames = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = NoYes.Yes };
                await db.Set<DirPartyTable>().AddAsync(datParty, ct);
            }
            
            var hbmcParty = await db.Set<DirPartyTable>().IgnoreQueryFilters().FirstOrDefaultAsync(p => p.PartyNumber == "HBMC-001", ct);
            if (hbmcParty == null)
            {
                hbmcParty = new DirPartyTable { Name = "AlHayat Building Materials Company", NameAlias = "HBMC", PartyNumber = "HBMC-001", LanguageId = "ar", AddressBookNames = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "HBMC", IsActive = NoYes.Yes };
                await db.Set<DirPartyTable>().AddAsync(hbmcParty, ct);
            }
            await db.SaveChangesAsync(ct);

            // 5. Postal Addresses
            var postalDat = await db.Set<LogisticsPostalAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Location == locDat.RecId, ct);
            if (postalDat == null)
            {
                postalDat = new LogisticsPostalAddress { Location = locDat.RecId, Address = "123 Main St\nLos Angeles, CA 90001\nUSA", Street = "Main St", StreetNumber = "123", City = "Los Angeles", CityRecId = cityLA.RecId, State = "CA", County = "", CountryRegionId = "US", ZipCode = "90001", ZipCodeRecId = zipLA.RecId, DistrictName = "", District = districtFallback.RecId, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(100), CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsPostalAddress>().AddAsync(postalDat, ct);
            }
            var postalHbmc = await db.Set<LogisticsPostalAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Location == locHbmc.RecId, ct);
            if (postalHbmc == null)
            {
                postalHbmc = new LogisticsPostalAddress { Location = locHbmc.RecId, Address = "King Fahd Rd\nRiyadh, RIY 12211\nSaudi Arabia", Street = "King Fahd Rd", StreetNumber = "1", City = "Riyadh", CityRecId = cityRuh.RecId, State = "RIY", County = "", CountryRegionId = "SA", ZipCode = "12211", ZipCodeRecId = zipRuh.RecId, DistrictName = "", District = districtFallback.RecId, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(100), CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsPostalAddress>().AddAsync(postalHbmc, ct);
            }
            await db.SaveChangesAsync(ct);

            // 6. Electronic Addresses (Email, Phone)
            var emailDat = await db.Set<LogisticsElectronicAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.ElectronicAddressId == "DAT-EML-1", ct);
            if (emailDat == null)
            {
                emailDat = new LogisticsElectronicAddress { ElectronicAddressId = "DAT-EML-1", Location = locDat.RecId, Description = "Primary Email", Type = ElectronicAddressType.Email, Locator = "info@dat.com", IsPrimary = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsElectronicAddress>().AddAsync(emailDat, ct);
            }
            var phoneDat = await db.Set<LogisticsElectronicAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.ElectronicAddressId == "DAT-PHN-1", ct);
            if (phoneDat == null)
            {
                phoneDat = new LogisticsElectronicAddress { ElectronicAddressId = "DAT-PHN-1", Location = locDat.RecId, Description = "Primary Phone", Type = ElectronicAddressType.Phone, Locator = "+15551234567", IsPrimary = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsElectronicAddress>().AddAsync(phoneDat, ct);
            }
            var emailHbmc = await db.Set<LogisticsElectronicAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.ElectronicAddressId == "HBMC-EML-1", ct);
            if (emailHbmc == null)
            {
                emailHbmc = new LogisticsElectronicAddress { ElectronicAddressId = "HBMC-EML-1", Location = locHbmc.RecId, Description = "Primary Email", Type = ElectronicAddressType.Email, Locator = "info@hbmc.com.sa", IsPrimary = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsElectronicAddress>().AddAsync(emailHbmc, ct);
            }
            var phoneHbmc = await db.Set<LogisticsElectronicAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.ElectronicAddressId == "HBMC-PHN-1", ct);
            if (phoneHbmc == null)
            {
                phoneHbmc = new LogisticsElectronicAddress { ElectronicAddressId = "HBMC-PHN-1", Location = locHbmc.RecId, Description = "Primary Phone", Type = ElectronicAddressType.Phone, Locator = "+966112345678", IsPrimary = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsElectronicAddress>().AddAsync(phoneHbmc, ct);
            }
            await db.SaveChangesAsync(ct);
            #endregion

            #region Company Info
            // Update Company Info for DAT
            datParty.PrimaryAddressLocation = locDat.RecId;
            datParty.PrimaryContactEmail = emailDat.RecId;
            datParty.PrimaryContactPhone = phoneDat.RecId;
            db.Set<DirPartyTable>().Update(datParty);
            await db.SaveChangesAsync(ct);

            var datCompany = await db.Set<CompanyInfo>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.DataArea == "dat", ct);
            if (datCompany == null)
            {
                datCompany = new CompanyInfo { DataArea = "dat", Name = "Company accounts data", Party = datParty.RecId, LanguageId = "en-us", TimeZone = "(GMT-08:00) Pacific Time (US & Canada)", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" };
                await db.Set<CompanyInfo>().AddAsync(datCompany, ct);
                await db.SaveChangesAsync(ct);
            }

            // Update Company Info for HBMC
            hbmcParty.PrimaryAddressLocation = locHbmc.RecId;
            hbmcParty.PrimaryContactEmail = emailHbmc.RecId;
            hbmcParty.PrimaryContactPhone = phoneHbmc.RecId;
            db.Set<DirPartyTable>().Update(hbmcParty);
            await db.SaveChangesAsync(ct);

            var hbmcCompany = await db.Set<CompanyInfo>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.DataArea == "HBMC", ct);
            if (hbmcCompany == null)
            {
                hbmcCompany = new CompanyInfo { DataArea = "HBMC", Name = "AlHayat Building Materials Company", Party = hbmcParty.RecId, LanguageId = "ar", TimeZone = "(GMT+03:00) Riyadh", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "HBMC" };
                await db.Set<CompanyInfo>().AddAsync(hbmcCompany, ct);
                await db.SaveChangesAsync(ct);
            }

            // Seed DirPartyLocation
            var datPartyLoc = await db.Set<DirPartyLocation>().IgnoreQueryFilters().FirstOrDefaultAsync(pl => pl.Party == datParty.RecId && pl.Location == locDat.RecId, ct);
            if (datPartyLoc == null)
            {
                datPartyLoc = new DirPartyLocation { Party = datParty.RecId, Location = locDat.RecId, IsPrimary = NoYes.Yes, IsPostalAddress = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<DirPartyLocation>().AddAsync(datPartyLoc, ct);
                await db.SaveChangesAsync(ct);
            }
            
            var hbmcPartyLoc = await db.Set<DirPartyLocation>().IgnoreQueryFilters().FirstOrDefaultAsync(pl => pl.Party == hbmcParty.RecId && pl.Location == locHbmc.RecId, ct);
            if (hbmcPartyLoc == null)
            {
                hbmcPartyLoc = new DirPartyLocation { Party = hbmcParty.RecId, Location = locHbmc.RecId, IsPrimary = NoYes.Yes, IsPostalAddress = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "HBMC", IsActive = true };
                await db.Set<DirPartyLocation>().AddAsync(hbmcPartyLoc, ct);
                await db.SaveChangesAsync(ct);
            }

            #endregion

            #region Modes of Delivery (DlvMode)
            var existingDlvModes = await db.Set<DlvMode>().IgnoreQueryFilters().Select(x => x.Code).ToListAsync(ct);
            var dlvModeSeeds = new[]
            {
                new DlvMode { Code = "DHL", Txt = "DHL Express", DisplayOrder = 1, ShipCarrierDlvType = WHSShipCarrierDlvType.Ground, MarkupGroup = "", McrExpedite = "", DomPriority = 1, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new DlvMode { Code = "FEDEX", Txt = "FedEx Ground", DisplayOrder = 2, ShipCarrierDlvType = WHSShipCarrierDlvType.Ground, MarkupGroup = "", McrExpedite = "", DomPriority = 2, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
            };
            foreach (var mode in dlvModeSeeds)
            {
                if (!existingDlvModes.Contains(mode.Code))
                {
                    await db.Set<DlvMode>().AddAsync(mode, ct);
                }
            }
            await db.SaveChangesAsync(ct);
            #endregion

            #region Delivery Terms (DlvTerm)
            var existingDlvTerms = await db.Set<DlvTerm>().IgnoreQueryFilters().Select(x => x.Code).ToListAsync(ct);
            var dlvTermSeeds = new[]
            {
                new DlvTerm { Code = "FOB", Txt = "Free on Board", ShipCarrierFreeMinimum = 0, FreightChargeTerm = 0, TaxLocationRole = 0, ItmGoodsInTransitControl = NoYes.No, ItmPortMandatory = NoYes.No, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                new DlvTerm { Code = "CIF", Txt = "Cost, Insurance & Freight", ShipCarrierFreeMinimum = 1000m, FreightChargeTerm = 1, TaxLocationRole = 0, ItmGoodsInTransitControl = NoYes.Yes, ItmPortMandatory = NoYes.Yes, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
            };
            foreach (var term in dlvTermSeeds)
            {
                if (!existingDlvTerms.Contains(term.Code))
                {
                    await db.Set<DlvTerm>().AddAsync(term, ct);
                }
            }
            await db.SaveChangesAsync(ct);
            #endregion

            #region Payment Terms (PaymTerm)
            var existingPaymTerms = await db.Set<PaymTerm>().IgnoreQueryFilters().Select(x => x.PaymTermId).ToListAsync(ct);
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
                    await db.Set<PaymTerm>().AddAsync(term, ct);
                }
            }
            await db.SaveChangesAsync(ct);
            #endregion

            #region Payment Schedules (PaymSched & PaymSchedLine)
            var existingPaymScheds = await db.Set<PaymSched>().IgnoreQueryFilters().Select(x => x.Name).ToListAsync(ct);
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
                await db.Set<PaymSched>().AddAsync(sched, ct);
                await db.SaveChangesAsync(ct);

                var lines = new[]
                {
                    new PaymSchedLine { Name = "3 Instal", LineNum = 1, Qty = 0, PercentAmount = 34, Value = 0, CfmPrepayment = NoYes.Yes, McrShipping = NoYes.Yes, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new PaymSchedLine { Name = "3 Instal", LineNum = 2, Qty = 1, PercentAmount = 33, Value = 0, CfmPrepayment = NoYes.No, McrShipping = NoYes.No, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" },
                    new PaymSchedLine { Name = "3 Instal", LineNum = 3, Qty = 2, PercentAmount = 33, Value = 0, CfmPrepayment = NoYes.No, McrShipping = NoYes.No, IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat" }
                };
                await db.Set<PaymSchedLine>().AddRangeAsync(lines, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion
        }
    }
}



