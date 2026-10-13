using IAX.IXApi.Modules.Finance.AccountsReceivable;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class SalesOrderCopyValidatorTests
{
    private readonly SalesOrderCopyValidator validator = new();

    [Fact]
    public void Preserves_request_validation_order_and_messages()
    {
        var invalidMode = validator.Validate(new SalesOrderCopyRequest("other", [], 0));
        Assert.Equal("Copy mode must be fromAll or fromJournal.", invalidMode.Errors[0].ErrorMessage);

        var noLines = validator.Validate(new SalesOrderCopyRequest("fromAll", [], 0));
        Assert.Equal("Select at least one source line.", noLines.Errors[0].ErrorMessage);

        var invalidLine = validator.Validate(new SalesOrderCopyRequest("fromAll", ["bad"], 0));
        Assert.Equal("One or more source line identifiers are invalid.", invalidLine.Errors[0].ErrorMessage);

        var invalidFactor = validator.Validate(new SalesOrderCopyRequest("fromAll", ["123"], 0));
        Assert.Equal("Quantity factor must be greater than zero.", invalidFactor.Errors[0].ErrorMessage);

        var reverseSign = validator.Validate(new SalesOrderCopyRequest("fromAll", ["123"], InvertSign: true));
        Assert.Equal("Reverse-sign copying requires a return-order flow and is not supported for sales orders.",
            reverseSign.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Copy_service_rejects_missing_request_before_data_access()
    {
        var service = new SalesOrderCopyService(null!, null!, null!, validator);
        var result = await service.CopyAsync(1, null, CancellationToken.None);

        Assert.Equal(SalesOrderCopyStatus.InvalidRequest, result.Status);
        Assert.Equal("A copy request is required.", result.Error);
    }
}
