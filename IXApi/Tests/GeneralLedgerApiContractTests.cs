using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.GeneralLedger;
using IAX.IXApi.Modules.Finance.GeneralLedger.Ledger.Validation;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount.Validation;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable.Validation;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class GeneralLedgerApiContractTests
{
    [Fact]
    public void Core_general_ledger_dtos_expose_the_required_setup_and_journal_fields()
    {
        Require<LedgerDto>(nameof(LedgerDto.ChartOfAccounts), nameof(LedgerDto.FiscalCalendar),
            nameof(LedgerDto.AccountingCurrency), nameof(LedgerDto.DefaultExchangeRateType));
        Require<MainAccountDto>(nameof(MainAccountDto.MainAccountId), nameof(MainAccountDto.LedgerChartOfAccounts),
            nameof(MainAccountDto.Type), nameof(MainAccountDto.ValidateCurrency));
        Require<LedgerJournalNameDto>(nameof(LedgerJournalNameDto.JournalName), nameof(LedgerJournalNameDto.NumberSequenceTable),
            nameof(LedgerJournalNameDto.JournalType), nameof(LedgerJournalNameDto.CurrencyCode));
        Require<LedgerJournalTableDto>(nameof(LedgerJournalTableDto.JournalNum), nameof(LedgerJournalTableDto.JournalName),
            nameof(LedgerJournalTableDto.Posted), nameof(LedgerJournalTableDto.JournalTotalDebit));
        Require<LedgerJournalTransDto>(nameof(LedgerJournalTransDto.JournalNum), nameof(LedgerJournalTransDto.TransDate),
            nameof(LedgerJournalTransDto.LedgerDimension), nameof(LedgerJournalTransDto.AmountCurDebit),
            nameof(LedgerJournalTransDto.AmountCurCredit), nameof(LedgerJournalTransDto.CurrencyCode));
    }

    [Fact]
    public void Core_general_ledger_validators_accept_a_basic_ledger_journal_payload()
    {
        Assert.True(new LedgerDtoValidator().Validate(new LedgerDto
        {
            Name = "DAT", ChartOfAccounts = 1, FiscalCalendar = 2,
            AccountingCurrency = "SAR", DefaultExchangeRateType = 3
        }).IsValid);
        Assert.True(new MainAccountDtoValidator().Validate(new MainAccountDto
        {
            MainAccountId = "110100", Name = "Cash", LedgerChartOfAccounts = 1,
            Type = MainAccountType.Asset
        }).IsValid);
        Assert.True(new LedgerJournalNameDtoValidator().Validate(new LedgerJournalNameDto
        {
            JournalName = "GEN", Name = "General journal", NumberSequenceTable = 1,
            JournalType = LedgerJournalType.Daily, CurrencyCode = "SAR"
        }).IsValid);
        Assert.True(new LedgerJournalTableDtoValidator().Validate(new LedgerJournalTableDto
        {
            JournalName = "GEN", Name = "January adjustment"
        }).IsValid);
        Assert.True(new LedgerJournalTransDtoValidator().Validate(new LedgerJournalTransDto
        {
            JournalNum = "J0001", TransDate = new DateTime(2026, 1, 31),
            AccountType = LedgerJournalACType.Ledger, LedgerDimension = 10,
            AmountCurDebit = 100m
        }).IsValid);
    }

    [Fact]
    public void Journal_line_validator_rejects_missing_accounts_dates_and_invalid_amount_sides()
    {
        var validator = new LedgerJournalTransDtoValidator();
        var result = validator.Validate(new LedgerJournalTransDto
        {
            JournalNum = "J0001", AmountCurDebit = 100m, AmountCurCredit = 100m
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(LedgerJournalTransDto.TransDate));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(LedgerJournalTransDto.LedgerDimension));
        Assert.Contains(result.Errors, x => x.ErrorMessage.Contains("not both", StringComparison.OrdinalIgnoreCase));
    }

    private static void Require<T>(params string[] names)
    {
        foreach (var name in names)
            Assert.NotNull(typeof(T).GetProperty(name));
    }
}
