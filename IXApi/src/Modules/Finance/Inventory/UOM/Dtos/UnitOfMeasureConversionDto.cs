namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class UnitOfMeasureConversionDto
{
    public string Id => RecId.ToString();
    public long RecId { get; set; }
    public long FromUnitOfMeasure { get; set; }
    public string FromSymbol { get; set; } = string.Empty;
    public long ToUnitOfMeasure { get; set; }
    public string ToSymbol { get; set; } = string.Empty;
    public long Product { get; set; }
    public string ProductNumber { get; set; } = string.Empty;
    public decimal Factor { get; set; }
    public int Numerator { get; set; }
    public int Denominator { get; set; }
    public decimal InnerOffset { get; set; }
    public decimal OuterOffset { get; set; }
    public int Rounding { get; set; }
}

public sealed class UnitOfMeasureInputDto
{
    public string Symbol { get; set; } = string.Empty;
    public int UnitOfMeasureClass { get; set; }
    public int SystemOfUnits { get; set; }
    public int DecimalPrecision { get; set; }
}

public sealed class UnitOfMeasureConversionInputDto
{
    public long FromUnitOfMeasure { get; set; }
    public long ToUnitOfMeasure { get; set; }
    public long Product { get; set; }
    public decimal Factor { get; set; }
    public int Numerator { get; set; }
    public int Denominator { get; set; }
    public decimal InnerOffset { get; set; }
    public decimal OuterOffset { get; set; }
    public int Rounding { get; set; }
}
