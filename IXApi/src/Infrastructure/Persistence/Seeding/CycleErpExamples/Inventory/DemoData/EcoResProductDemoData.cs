using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class EcoResProductDemoData
{
    internal static EcoResProduct Create(string code, string name) =>
        new EcoResProduct { DisplayProductNumber = code, SearchName = name, ProductType = ItemType.Item };
            internal static readonly ProductExample[] Items =
            {
                new ProductExample
                {
                    Code = "ITM-1001", Name = "Bottled Water 500ml", Group = "CONS", Description = "Drinking water, 500 ml bottle",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("BTL", 1m, "6280001000017", false, true, true),
                        ("CTN", 24m, "6280001000024", true, false, false),
                    }
                },
                new ProductExample
                {
                    Code = "ITM-1002", Name = "A4 Paper Ream", Group = "CONS", Description = "A4 printing paper, 500 sheets per ream",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("PCK", 1m, "6280001000031", false, true, true),
                        ("BOX", 5m, "6280001000048", true, false, false),
                    }
                },
                new ProductExample
                {
                    Code = "ITM-1003", Name = "Steel Rod 12mm", Group = "RAW", Description = "Reinforcement steel rod, 12 mm diameter",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("M", 1m, "6280001000055", false, true, true),
                        ("M", 1m, "6280001000062", true, false, false),
                    }
                },
                new ProductExample
                {
                    Code = "ITM-1004", Name = "T-Shirt Cotton", Group = "FIN", Description = "100% cotton T-shirt",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("PCS", 1m, "6280001000079", false, true, true),
                        ("DOZ", 12m, "6280001000086", true, false, false),
                    }
                },
                new ProductExample
                {
                    Code = "ITM-1005", Name = "Cooking Oil 1L", Group = "FIN", Description = "Vegetable cooking oil, 1 litre bottle",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("BTL", 1m, "6280001000093", false, true, true),
                        ("CTN", 12m, "6280001000109", true, false, false),
                    }
                },
            };

}

internal sealed class ProductExample
{
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
    public string Group { get; init; } = "";
    public string Description { get; init; } = "";
    public (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[] Uoms { get; init; } = [];
}
