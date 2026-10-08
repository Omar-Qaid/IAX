using IAX.IXApi.Modules.Finance.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed record TaxGroupReadSource(TaxGroupHeading Heading);
public sealed record TaxGroupLineReadSource(TaxGroupData Line, decimal Value);
public sealed record TaxItemGroupReadSource(TaxItemGroupHeading Heading);
public sealed record TaxItemGroupLineReadSource(TaxOnItem Line, decimal Value);
public sealed record TaxGroupLineWriteSource(TaxGroupDataDto Line, bool InitialCreate = false);
public sealed record TaxItemGroupLineWriteSource(TaxOnItemDto Line);

public sealed record TaxGroupLineCreateSource(TaxGroupDataDto Line, string DataAreaId, string TaxGroup, bool InitialCreate);
public sealed record TaxItemGroupLineCreateSource(TaxOnItemDto Line, string DataAreaId, string TaxItemGroup);

public sealed class TaxOperationMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TaxGroupReadSource, TaxGroupDto>().MapWith(s => new TaxGroupDto
        {
            RecId = s.Heading.RecId,
            DataAreaId = s.Heading.DataAreaId,
            TaxGroup = s.Heading.TaxGroup,
            TaxGroupName = s.Heading.TaxGroupName,
            TaxGroupSetup = s.Heading.TaxGroupSetup,
            Source = s.Heading.Source,
            TaxGroupRounding = s.Heading.TaxGroupRounding,
            TaxReverseOnCashDisc = s.Heading.TaxReverseOnCashDisc,
            EuTrade_W = s.Heading.EuTrade_W,
            MandatorySalesDate_W = s.Heading.MandatorySalesDate_W,
            FillSalesDate_W = s.Heading.FillSalesDate_W,
            FillVatDueDatePeriodNumber = s.Heading.FillVatDueDatePeriodNumber,
            FillVatDueDate_W = s.Heading.FillVatDueDate_W,
            FillVatDueDateBasedOn = s.Heading.FillVatDueDateBasedOn,
            FillVatDueDatePeriod = s.Heading.FillVatDueDatePeriod,
            TaxPrintDetail = s.Heading.TaxPrintDetail
        });
        config.NewConfig<TaxGroupLineReadSource, TaxGroupDataDto>().MapWith(s => new TaxGroupDataDto
        {
            RecId = s.Line.RecId,
            DataAreaId = s.Line.DataAreaId,
            TaxGroup = s.Line.TaxGroup,
            TaxCode = s.Line.TaxCode,
            TaxExemptCode = s.Line.TaxExemptCode,
            ExemptTax = s.Line.ExemptTax,
            UseTax = s.Line.UseTax,
            IntracomVat = s.Line.IntracomVat,
            ReverseCharge_W = s.Line.ReverseCharge_W,
            TaxCodeName = s.Line.TaxTable?.TaxName,
            TaxValue = s.Value
        });
        config.NewConfig<TaxItemGroupReadSource, TaxItemGroupDto>().MapWith(s => new TaxItemGroupDto
        {
            RecId = s.Heading.RecId,
            DataAreaId = s.Heading.DataAreaId,
            TaxItemGroup = s.Heading.TaxItemGroup,
            Name = s.Heading.Name,
            Source = s.Heading.Source,
            EuSalesListType = s.Heading.EuSalesListType
        });
        config.NewConfig<TaxItemGroupLineReadSource, TaxOnItemDto>().MapWith(s => new TaxOnItemDto
        {
            RecId = s.Line.RecId,
            DataAreaId = s.Line.DataAreaId,
            TaxItemGroup = s.Line.TaxItemGroup,
            TaxCode = s.Line.TaxCode,
            TaxExemptCode = s.Line.TaxExemptCode,
            TaxCodeName = s.Line.TaxTable?.TaxName,
            TaxValue = s.Value
        });
        config.NewConfig<TaxGroupLineWriteSource, TaxGroupData>().IgnoreNonMapped(true)
            .Map(d => d.TaxExemptCode, s => s.InitialCreate ? s.Line.TaxExemptCode ?? "NONE" : string.IsNullOrWhiteSpace(s.Line.TaxExemptCode) ? "NONE" : s.Line.TaxExemptCode)
            .Map(d => d.ExemptTax, s => s.Line.ExemptTax)
            .Map(d => d.UseTax, s => s.Line.UseTax)
            .Map(d => d.IntracomVat, s => s.Line.IntracomVat)
            .Map(d => d.ReverseCharge_W, s => s.Line.ReverseCharge_W);
        config.NewConfig<TaxItemGroupLineWriteSource, TaxOnItem>().IgnoreNonMapped(true)
            .Map(d => d.TaxExemptCode, s => string.IsNullOrWhiteSpace(s.Line.TaxExemptCode) ? "NONE" : s.Line.TaxExemptCode);
        config.NewConfig<TaxGroupLineCreateSource, TaxGroupData>().IgnoreNonMapped(true)
            .Map(d => d.DataAreaId, s => s.DataAreaId)
            .Map(d => d.TaxGroup, s => s.TaxGroup)
            .Map(d => d.TaxCode, s => s.Line.TaxCode ?? string.Empty)
            .Map(d => d.TaxExemptCode, s => s.InitialCreate ? s.Line.TaxExemptCode ?? "NONE" : string.IsNullOrWhiteSpace(s.Line.TaxExemptCode) ? "NONE" : s.Line.TaxExemptCode)
            .Map(d => d.ExemptTax, s => s.Line.ExemptTax)
            .Map(d => d.UseTax, s => s.Line.UseTax)
            .Map(d => d.IntracomVat, s => s.Line.IntracomVat)
            .Map(d => d.ReverseCharge_W, s => s.Line.ReverseCharge_W);
        config.NewConfig<TaxItemGroupLineCreateSource, TaxOnItem>().IgnoreNonMapped(true)
            .Map(d => d.DataAreaId, s => s.DataAreaId)
            .Map(d => d.TaxItemGroup, s => s.TaxItemGroup)
            .Map(d => d.TaxCode, s => s.Line.TaxCode)
            .Map(d => d.TaxExemptCode, s => string.IsNullOrWhiteSpace(s.Line.TaxExemptCode) ? "NONE" : s.Line.TaxExemptCode);
    }
}
