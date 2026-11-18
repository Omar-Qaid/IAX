using System.ComponentModel.DataAnnotations;
using System.Reflection;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Tax;
using IAX.IXApi.Modules.Finance.Tax.Validation;
using Xunit;

namespace IXApi.Tests;

public sealed class TaxAuthorityValidationTests
{
    private static TaxAuthorityAddressDto Valid() => new() { TaxAuthority = "AUTH", Name = "Tax office" };

    public static IEnumerable<object[]> BoundedFields() => typeof(TaxAuthorityAddress)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(p => p.PropertyType == typeof(string) && p.GetCustomAttribute<StringLengthAttribute>() != null)
        .Select(p => new object[] { p.Name, p.GetCustomAttribute<StringLengthAttribute>()!.MaximumLength });

    [Theory]
    [MemberData(nameof(BoundedFields))]
    public void Rejects_values_that_exceed_existing_storage_limits(string field, int limit)
    {
        var dto = Valid();
        typeof(TaxAuthorityAddressDto).GetProperty(field)!.SetValue(dto, new string('A', limit + 1));
        var result = new TaxAuthorityAddressDtoValidator().Validate(dto);
        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }

    [Theory]
    [InlineData("TaxAuthority", null)]
    [InlineData("TaxAuthority", "")]
    [InlineData("TaxAuthority", "   ")]
    [InlineData("Name", null)]
    [InlineData("Name", "")]
    [InlineData("Name", "   ")]
    public void Requires_the_entity_required_code_and_name(string field, string? value)
    {
        var dto = Valid();
        typeof(TaxAuthorityAddressDto).GetProperty(field)!.SetValue(dto, value);
        var result = new TaxAuthorityAddressDtoValidator().Validate(dto);
        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }

    [Fact]
    public void Accepts_values_at_the_existing_storage_limits()
    {
        var dto = Valid();
        foreach (var entry in BoundedFields())
            typeof(TaxAuthorityAddressDto).GetProperty((string)entry[0])!.SetValue(dto, new string('A', (int)entry[1]));
        Assert.True(new TaxAuthorityAddressDtoValidator().Validate(dto).IsValid);
    }

    [Fact]
    public void Contact_details_are_optional()
    {
        Assert.True(new TaxAuthorityAddressDtoValidator().Validate(Valid()).IsValid);
    }
}
