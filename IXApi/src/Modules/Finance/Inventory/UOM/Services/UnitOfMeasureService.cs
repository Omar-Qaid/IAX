using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class UnitOfMeasureService(IUnitOfWork unitOfWork) : IUnitOfMeasureService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<List<UnitOfMeasureDto>> ListAsync(CancellationToken ct = default)
    {
        var units = await Db.Set<UnitOfMeasure>().AsNoTracking().OrderBy(x => x.Symbol).ToListAsync(ct);
        var conversions = await Db.Set<UnitOfMeasureConversion>().AsNoTracking().OrderBy(x => x.Product).ThenBy(x => x.FromUnitOfMeasure).ToListAsync(ct);
        var products = await Db.Set<EcoResProduct>().AsNoTracking().ToDictionaryAsync(x => x.RecId, x => x.DisplayProductNumber, ct);
        var symbols = units.ToDictionary(x => x.RecId, x => x.Symbol);
        return units.Select(unit => new UnitOfMeasureDto
        {
            RecId = unit.RecId,
            Symbol = unit.Symbol,
            UnitOfMeasureClass = (int)unit.UnitOfMeasureClass,
            SystemOfUnits = (int)unit.SystemOfUnits,
            DecimalPrecision = unit.DecimalPrecision,
            Conversions = conversions.Where(x => x.FromUnitOfMeasure == unit.RecId).Select(x => Map(x, symbols, products)).ToList(),
        }).ToList();
    }

    public async Task<(UnitOfMeasureDto? Unit, string? Error)> CreateAsync(UnitOfMeasureInputDto input, CancellationToken ct = default)
    {
        var error = ValidateUnit(input);
        if (error is not null) return (null, error);
        var symbol = input.Symbol.Trim();
        var normalizedSymbol = symbol.ToUpperInvariant();
        if (await Db.Set<UnitOfMeasure>().AnyAsync(x => x.Symbol.ToUpper() == normalizedSymbol, ct)) return (null, $"Unit '{symbol}' already exists.");
        var row = new UnitOfMeasure();
        Apply(row, input);
        Db.Add(row);
        await unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<(UnitOfMeasureDto? Unit, string? Error)> UpdateAsync(long recId, UnitOfMeasureInputDto input, CancellationToken ct = default)
    {
        var error = ValidateUnit(input);
        if (error is not null) return (null, error);
        var row = await Db.Set<UnitOfMeasure>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return (null, "Unit of measure was not found.");
        var symbol = input.Symbol.Trim();
        var used = await IsUsedAsync(row, ct);
        if (used && (!string.Equals(row.Symbol, symbol, StringComparison.Ordinal)
            || row.UnitOfMeasureClass != (UnitOfMeasureClass)input.UnitOfMeasureClass
            || row.SystemOfUnits != (SystemOfUnits)input.SystemOfUnits
            || row.DecimalPrecision != input.DecimalPrecision))
            return (null, "A referenced unit's symbol, class, system, and precision cannot be changed.");
        var normalizedSymbol = symbol.ToUpperInvariant();
        if (await Db.Set<UnitOfMeasure>().AnyAsync(x => x.RecId != recId && x.Symbol.ToUpper() == normalizedSymbol, ct)) return (null, $"Unit '{symbol}' already exists.");
        Apply(row, input);
        await unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<string?> DeleteAsync(long recId, CancellationToken ct = default)
    {
        var row = await Db.Set<UnitOfMeasure>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return "Unit of measure was not found.";
        if (await IsUsedAsync(row, ct))
            return "The unit cannot be deleted because it is referenced by setup or a journal.";
        Db.Remove(row);
        await unitOfWork.CompleteAsync(ct);
        return null;
    }

    public async Task<(UnitOfMeasureConversionDto? Conversion, string? Error)> SaveConversionAsync(long? recId, UnitOfMeasureConversionInputDto input, CancellationToken ct = default)
    {
        if (input.FromUnitOfMeasure <= 0 || input.ToUnitOfMeasure <= 0) return (null, "From unit and to unit are required.");
        if (input.FromUnitOfMeasure == input.ToUnitOfMeasure) return (null, "From unit and to unit must be different.");
        if (input.Factor < 0 || input.Numerator < 0 || input.Denominator < 0)
            return (null, "Conversion factor and fraction cannot be negative.");
        var hasFraction = input.Numerator > 0 || input.Denominator > 0;
        if (hasFraction && (input.Numerator == 0 || input.Denominator == 0))
            return (null, "Both conversion fraction values must be positive.");
        if (input.Factor == 0 && !hasFraction)
            return (null, "A positive factor or fraction is required.");
        if (input.Factor > 0 && hasFraction)
            return (null, "Use either a factor or a fraction, not both.");
        if (input.InnerOffset != 0 || input.OuterOffset != 0)
            return (null, "Offset conversions are not supported for basic inventory quantities.");
        if (!Enum.IsDefined(typeof(UnitOfMeasureRounding), input.Rounding)) return (null, "Rounding method is invalid.");
        var units = await Db.Set<UnitOfMeasure>().Where(x => x.RecId == input.FromUnitOfMeasure || x.RecId == input.ToUnitOfMeasure).ToListAsync(ct);
        if (units.Count != 2) return (null, "A selected unit of measure was not found.");
        if (units[0].UnitOfMeasureClass != units[1].UnitOfMeasureClass)
            return (null, "The conversion units must belong to the same unit class.");
        if (input.Product != 0 && !await Db.Set<EcoResProduct>().AnyAsync(x => x.RecId == input.Product, ct)) return (null, "The selected shared product was not found.");
        if (await Db.Set<UnitOfMeasureConversion>().AnyAsync(x => x.RecId != (recId ?? 0) && x.Product == input.Product
            && ((x.FromUnitOfMeasure == input.FromUnitOfMeasure && x.ToUnitOfMeasure == input.ToUnitOfMeasure)
                || (x.FromUnitOfMeasure == input.ToUnitOfMeasure && x.ToUnitOfMeasure == input.FromUnitOfMeasure)), ct))
            return (null, "This unit conversion already exists for the selected scope.");
        var row = recId.HasValue ? await Db.Set<UnitOfMeasureConversion>().FirstOrDefaultAsync(x => x.RecId == recId.Value, ct) : null;
        if (recId.HasValue && row is null) return (null, "Unit conversion was not found.");
        row ??= new UnitOfMeasureConversion();
        if (row.RecId == 0) Db.Add(row);
        row.FromUnitOfMeasure = input.FromUnitOfMeasure; row.ToUnitOfMeasure = input.ToUnitOfMeasure; row.Product = input.Product;
        row.Factor = input.Factor; row.Numerator = input.Numerator; row.Denominator = input.Denominator;
        row.InnerOffset = input.InnerOffset; row.OuterOffset = input.OuterOffset; row.Rounding = (UnitOfMeasureRounding)input.Rounding;
        await unitOfWork.CompleteAsync(ct);
        var symbols = units.ToDictionary(x => x.RecId, x => x.Symbol);
        var products = input.Product == 0 ? new Dictionary<long, string>() : await Db.Set<EcoResProduct>().AsNoTracking().Where(x => x.RecId == input.Product).ToDictionaryAsync(x => x.RecId, x => x.DisplayProductNumber, ct);
        return (Map(row, symbols, products), null);
    }

    public async Task<string?> DeleteConversionAsync(long recId, CancellationToken ct = default)
    {
        var row = await Db.Set<UnitOfMeasureConversion>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return "Unit conversion was not found.";
        Db.Remove(row);
        await unitOfWork.CompleteAsync(ct);
        return null;
    }

    private static string? ValidateUnit(UnitOfMeasureInputDto input) =>
        string.IsNullOrWhiteSpace(input.Symbol) ? "Symbol is required." :
        input.Symbol.Trim().Length > 10 ? "Symbol cannot exceed 10 characters." :
        !Enum.IsDefined(typeof(UnitOfMeasureClass), input.UnitOfMeasureClass) ? "Unit class is invalid." :
        !Enum.IsDefined(typeof(SystemOfUnits), input.SystemOfUnits) ? "System of units is invalid." :
        input.DecimalPrecision is < 0 or > 6 ? "Decimal precision must be between 0 and 6." : null;

    private async Task<bool> IsUsedAsync(UnitOfMeasure row, CancellationToken ct) =>
        await Db.Set<InventTableModule>().IgnoreQueryFilters().AnyAsync(x => x.UnitId == row.Symbol, ct) ||
        await Db.Set<InventItemBarcode>().IgnoreQueryFilters().AnyAsync(x => x.UnitId == row.Symbol, ct) ||
        await Db.Set<InventParameters>().IgnoreQueryFilters().AnyAsync(x => x.DefaultUnitId == row.Symbol, ct) ||
        await Db.Set<InventJournalTrans>().IgnoreQueryFilters().AnyAsync(x => x.Unit == row.Symbol, ct) ||
        await Db.Set<UnitOfMeasureConversion>().IgnoreQueryFilters().AnyAsync(
            x => x.FromUnitOfMeasure == row.RecId || x.ToUnitOfMeasure == row.RecId, ct);

    private static void Apply(UnitOfMeasure row, UnitOfMeasureInputDto input)
    { row.Symbol = input.Symbol.Trim(); row.UnitOfMeasureClass = (UnitOfMeasureClass)input.UnitOfMeasureClass; row.SystemOfUnits = (SystemOfUnits)input.SystemOfUnits; row.DecimalPrecision = input.DecimalPrecision; }
    private static UnitOfMeasureDto Map(UnitOfMeasure x) => new() { RecId = x.RecId, Symbol = x.Symbol, UnitOfMeasureClass = (int)x.UnitOfMeasureClass, SystemOfUnits = (int)x.SystemOfUnits, DecimalPrecision = x.DecimalPrecision };
    private static UnitOfMeasureConversionDto Map(UnitOfMeasureConversion x, IReadOnlyDictionary<long, string> symbols, IReadOnlyDictionary<long, string> products) => new()
    { RecId = x.RecId, FromUnitOfMeasure = x.FromUnitOfMeasure, FromSymbol = symbols.GetValueOrDefault(x.FromUnitOfMeasure, ""), ToUnitOfMeasure = x.ToUnitOfMeasure, ToSymbol = symbols.GetValueOrDefault(x.ToUnitOfMeasure, ""), Product = x.Product, ProductNumber = products.GetValueOrDefault(x.Product, ""), Factor = x.Factor, Numerator = x.Numerator, Denominator = x.Denominator, InnerOffset = x.InnerOffset, OuterOffset = x.OuterOffset, Rounding = (int)x.Rounding };
}
