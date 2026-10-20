using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.Inventory           {
    public class UnitOfMeasureDto : EntityDto<long>
    {
        public string Id => RecId.ToString();
        public string Symbol { get; set; } = string.Empty;
        public int UnitOfMeasureClass { get; set; }
        public int SystemOfUnits { get; set; }
        public int DecimalPrecision { get; set; }
        public List<UnitOfMeasureConversionDto> Conversions { get; set; } = [];
    }
}

