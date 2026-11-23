using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// The account structure criteria rules, taken from the account structures documentation. Several
/// of these pin behaviour that looks like a bug until you read the rule, so they are here to stop
/// a later "fix" from breaking parity.
/// </summary>
public class DimensionCriteriaMatcherTests
{
    // THE one to not get wrong: criteria are compared as strings, never as numbers. 1000000 is
    // seven digits and numerically far above 399999, but string comparison puts it inside the
    // range. Do not "fix" this into a numeric comparison.
    [Fact]
    public void Ranges_compare_as_strings_not_numbers()
    {
        Assert.True(DimensionCriteriaMatcher.InRange("1000000", "100000", "399999", false, false));
        Assert.True(DimensionCriteriaMatcher.InRange("611000", "400000", "899999", false, false));
        // Leading zeros are the documented way to make different lengths behave.
        Assert.False(DimensionCriteriaMatcher.InRange("1000000", "0100000", "0399999", false, false));
    }

    [Theory]
    [InlineData("130100", "100000", "399999", true)]
    [InlineData("399999", "100000", "399999", true)]
    [InlineData("400000", "100000", "399999", false)]
    [InlineData("099999", "100000", "399999", false)]
    public void An_inclusive_range_includes_both_bounds(string value, string from, string to, bool expected) =>
        Assert.Equal(expected, DimensionCriteriaMatcher.InRange(value, from, to, false, false));

    [Fact]
    public void Open_bounds_are_exclusive()
    {
        Assert.False(DimensionCriteriaMatcher.InRange("100000", "100000", "399999", true, false));
        Assert.False(DimensionCriteriaMatcher.InRange("399999", "100000", "399999", false, true));
        Assert.True(DimensionCriteriaMatcher.InRange("100001", "100000", "399999", true, true));
    }

    [Theory]
    [InlineData("*", "ADMIN", true)]
    [InlineData("*", "", false)]            // `*` means every NON-BLANK value
    [InlineData("500*", "500100", true)]
    [InlineData("500*", "100500", false)]
    [InlineData("*500", "100500", true)]
    [InlineData("*500", "500100", false)]
    // "is like": contains 500 anywhere. 150000 and 250099 are the documentation's own examples —
    // both contain the substring 500 one character in, which is easy to misread.
    [InlineData("*500*", "150000", true)]
    [InlineData("*500*", "250099", true)]
    [InlineData("*500*", "1500099", true)]
    [InlineData("*500*", "123456", false)]
    [InlineData("ADMIN", "ADMIN", true)]
    [InlineData("ADMIN", "admin", false)]   // Values compare exactly, not case-insensitively
    public void Wildcards_follow_the_documented_operators(string pattern, string value, bool expected) =>
        Assert.Equal(expected, DimensionCriteriaMatcher.MatchesWildcard(value, pattern));

    [Fact]
    public void Criteria_are_separated_by_semicolons_and_quote_marks_mean_blank()
    {
        var parsed = DimensionCriteriaMatcher.Parse("ADMIN;SALES;SERVICE");
        Assert.Equal(3, parsed.Count);
        Assert.All(parsed, x => Assert.False(x.IsBlank));
        Assert.Equal(["ADMIN", "SALES", "SERVICE"], parsed.Select(x => x.WildCardString));

        var withBlank = DimensionCriteriaMatcher.Parse("*;\"\"");
        Assert.Equal(2, withBlank.Count);
        Assert.Contains(withBlank, x => x.IsBlank);
        Assert.Contains(withBlank, x => x.WildCardString == "*");

        var range = Assert.Single(DimensionCriteriaMatcher.Parse("100000..399999"));
        Assert.Equal("100000", range.RangeFrom);
        Assert.Equal("399999", range.RangeTo);
        Assert.True(range.IsRange);

        // A comma is a valid character in a dimension value, so it never separates criteria.
        var comma = Assert.Single(DimensionCriteriaMatcher.Parse("40000,49999"));
        Assert.Equal("40000,49999", comma.WildCardString);
    }

    [Fact]
    public void Blank_is_allowed_only_when_the_segment_is_optional()
    {
        var anyValue = new[] { Criterion(wildcard: "*") };
        Assert.True(DimensionCriteriaMatcher.Allows(anyValue, NoYes.Yes, ""));
        // `*` on its own does not allow blank: the segment must be optional.
        Assert.False(DimensionCriteriaMatcher.Allows(anyValue, NoYes.No, ""));
        Assert.True(DimensionCriteriaMatcher.Allows(anyValue, NoYes.No, "ADMIN"));
    }

    [Fact]
    public void A_value_is_allowed_when_any_one_criterion_matches()
    {
        var criteria = new[] { Criterion(wildcard: "ADMIN"), Criterion(wildcard: "SALES") };
        Assert.True(DimensionCriteriaMatcher.Allows(criteria, NoYes.No, "SALES"));
        Assert.False(DimensionCriteriaMatcher.Allows(criteria, NoYes.No, "SERVICE"));
    }

    [Fact]
    public void Disjoint_main_account_ranges_do_not_overlap()
    {
        var balanceSheet = DimensionCriteriaMatcher.Parse("100000..399999");
        var profitAndLoss = DimensionCriteriaMatcher.Parse("400000..899999");
        Assert.False(DimensionCriteriaMatcher.MayOverlap(balanceSheet, profitAndLoss));
        Assert.False(DimensionCriteriaMatcher.MayOverlap(profitAndLoss, balanceSheet));
    }

    [Fact]
    public void Touching_or_containing_ranges_overlap()
    {
        Assert.True(DimensionCriteriaMatcher.MayOverlap(
            DimensionCriteriaMatcher.Parse("100000..399999"),
            DimensionCriteriaMatcher.Parse("399999..500000")));
        Assert.True(DimensionCriteriaMatcher.MayOverlap(
            DimensionCriteriaMatcher.Parse("100000..399999"),
            DimensionCriteriaMatcher.Parse("200000..250000")));
        // A value inside another structure's range is an overlap too.
        Assert.True(DimensionCriteriaMatcher.MayOverlap(
            DimensionCriteriaMatcher.Parse("100000..399999"),
            DimensionCriteriaMatcher.Parse("130100")));
    }

    // Where disjointness cannot be established, an overlap is reported. Refusing an activation is
    // recoverable; an ambiguous structure silently misroutes every posting.
    [Fact]
    public void A_wildcard_is_treated_as_overlapping()
    {
        Assert.True(DimensionCriteriaMatcher.MayOverlap(
            DimensionCriteriaMatcher.Parse("100000..399999"),
            DimensionCriteriaMatcher.Parse("*")));
        Assert.True(DimensionCriteriaMatcher.MayOverlap(
            DimensionCriteriaMatcher.Parse("4*"),
            DimensionCriteriaMatcher.Parse("100000..399999")));
    }

    // Blank allowance is not a value, so it never makes two structures overlap.
    [Fact]
    public void Allowing_blank_does_not_create_an_overlap()
    {
        Assert.False(DimensionCriteriaMatcher.MayOverlap(
            DimensionCriteriaMatcher.Parse("100000..399999;\"\""),
            DimensionCriteriaMatcher.Parse("400000..899999;\"\"")));
    }

    private static DimensionConstraintNodeCriteria Criterion(
        string rangeFrom = "", string rangeTo = "", string wildcard = "") => new()
        {
            RangeFrom = rangeFrom, RangeTo = rangeTo, WildCardString = wildcard
        };
}
