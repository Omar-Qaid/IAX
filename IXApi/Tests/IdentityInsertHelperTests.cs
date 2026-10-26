using System;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using Xunit;

namespace IXApi.Tests
{
    public class IdentityInsertHelperTests
    {
        [Theory]
        [InlineData("CustTable", "[CustTable]")]
        [InlineData("[CustTable]", "[CustTable]")]
        [InlineData("dbo.CustTable", "[dbo].[CustTable]")]
        [InlineData("[dbo].[CustTable]", "[dbo].[CustTable]")]
        [InlineData("custom_schema.my_table_123", "[custom_schema].[my_table_123]")]
        public void SanitizeTableName_ValidIdentifiers_ReturnsSafelyEscapedSql(string input, string expected)
        {
            var sanitized = IdentityInsertHelper.SanitizeTableName(input);
            Assert.Equal(expected, sanitized);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Table; DROP TABLE Users;--")]
        [InlineData("Table' OR 1=1--")]
        [InlineData("Schema.Table.Extra")]
        [InlineData("Invalid-Hyphen")]
        public void SanitizeTableName_DangerousOrMalformedIdentifiers_ThrowsArgumentException(string invalidInput)
        {
            Assert.Throws<ArgumentException>(() => IdentityInsertHelper.SanitizeTableName(invalidInput));
        }
    }
}
