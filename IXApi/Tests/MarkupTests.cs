using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Foundation.Markup;
using Mapster;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class MarkupTests
{
    [Fact]
    public void Request_mapping_preserves_document_identity_and_posted_values()
    {
        var config = new TypeAdapterConfig();
        new MarkupMapping().Register(config);
        var date = new DateTime(2025, 1, 2);
        var entity = new MarkupTrans
        {
            RecId = 123, DataAreaId = "dat", TransRecId = 456, TransTableId = 2003,
            LineNum = 2, TransDate = date, ModuleType = MarkupModuleType.Customer,
            Posted = 17, TaxAmount = 8
        };
        var input = new MarkupTransDto
        {
            RecId = 999, DataAreaId = "other", TransRecId = 999, TransTableId = 0,
            LineNum = 99, TransDate = date.AddDays(1), ModuleType = MarkupModuleType.Vendor,
            Posted = 99, TaxAmount = 99, MarkupCode = " FRT ", Txt = " Freight ",
            CurrencyCode = " usd ", TaxGroup = " vat ", TaxItemGroup = " std ",
            Voucher = " V1 ", Value = 10, MarkupCategory = MarkupCategory.Fixed
        };
        input.Adapt(entity, config);
        Assert.Equal(123, entity.RecId);
        Assert.Equal("dat", entity.DataAreaId);
        Assert.Equal(456, entity.TransRecId);
        Assert.Equal(2003, entity.TransTableId);
        Assert.Equal(2, entity.LineNum);
        Assert.Equal(date, entity.TransDate);
        Assert.Equal(MarkupModuleType.Customer, entity.ModuleType);
        Assert.Equal(17, entity.Posted);
        Assert.Equal(8, entity.TaxAmount);
        Assert.Equal("FRT", entity.MarkupCode);
        Assert.Equal("Freight", entity.Txt);
        Assert.Equal("USD", entity.CurrencyCode);
        Assert.Equal("VAT", entity.TaxGroup);
        Assert.Equal("STD", entity.TaxItemGroup);
        Assert.Equal("V1", entity.Voucher);
        Assert.Equal(10, entity.Value);
        Assert.Equal(NoYes.Yes, entity.IsModified);
        var response = entity.Adapt<MarkupTransDto>(config);
        Assert.Equal(entity.RecId, response.RecId);
        Assert.Equal(entity.Posted, response.Posted);
        Assert.Equal(entity.TransRecId, response.TransRecId);
    }

    [Fact]
    public async Task Validation_preserves_first_error_and_percentage_boundary()
    {
        var validator = new MarkupTransValidator();
        var input = new MarkupTransDto { MarkupCode = " ", MarkupCategory = (MarkupCategory)999, Value = -1 };
        Assert.Equal("Charges code is required.", Assert.Single((await validator.ValidateAsync(input)).Errors).ErrorMessage);
        input.MarkupCode = "FRT";
        Assert.Equal("Charge category must be Fixed, Pcs, or Percentage.", Assert.Single((await validator.ValidateAsync(input)).Errors).ErrorMessage);
        input.MarkupCategory = MarkupCategory.Percent;
        Assert.Equal("Charges value cannot be negative.", Assert.Single((await validator.ValidateAsync(input)).Errors).ErrorMessage);
        input.Value = 101;
        Assert.Equal("Percentage charges value cannot exceed 100.", Assert.Single((await validator.ValidateAsync(input)).Errors).ErrorMessage);
        input.Value = 100;
        Assert.True((await validator.ValidateAsync(input)).IsValid);
        input.Value = 101;
        input.MarkupCategory = MarkupCategory.Fixed;
        Assert.True((await validator.ValidateAsync(input)).IsValid);
    }
}
