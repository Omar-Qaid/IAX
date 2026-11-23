using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.GeneralLedger;
using IAX.IXApi.Modules.Finance.GeneralLedger.Ledger.Validation;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount.Validation;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable.Validation;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class GeneralLedgerApiContractTests
{
    [Fact]
    public void Journal_name_payload_preserves_workflow_and_tax_settings()
    {
        var source = new LedgerJournalName { JournalName = "GEN", Name = "General", NumberSequenceTable = 1,
            CurrencyCode = "SAR", WorkflowApproval = NoYes.Yes, LedgerJournalInclTax = NoYes.Yes };
        var dto = source.Adapt<LedgerJournalNameDto>();
        Assert.True(new LedgerJournalNameDtoValidator().Validate(dto).IsValid);
        var restored = dto.Adapt<LedgerJournalName>();
        Assert.Equal(NoYes.Yes, restored.WorkflowApproval);
        Assert.Equal(NoYes.Yes, restored.LedgerJournalInclTax);
        dto.WorkflowApproval = (NoYes)99;
        Assert.Contains(new LedgerJournalNameDtoValidator().Validate(dto).Errors,
            error => error.PropertyName == nameof(LedgerJournalNameDto.WorkflowApproval));
    }

    [Fact]
    public void Journal_header_payload_round_trips_the_inclusive_tax_setting()
    {
        var source = new LedgerJournalTable { JournalName = "GEN", Name = "Vendor invoice",
            LedgerJournalInclTax = NoYes.Yes, ExchRateSecondary = 2m,
            ReportingCurrencyExchRateSecondary = 3m, ReportingCurrencyFixedExchRate = NoYes.Yes };
        var dto = source.Adapt<LedgerJournalTableDto>();
        Assert.Equal(NoYes.Yes, dto.LedgerJournalInclTax);
        Assert.True(new LedgerJournalTableDtoValidator().Validate(dto).IsValid);
        var restored = dto.Adapt<LedgerJournalTable>();
        Assert.Equal(NoYes.Yes, restored.LedgerJournalInclTax);
        Assert.Equal(2m, restored.ExchRateSecondary);
        Assert.Equal(3m, restored.ReportingCurrencyExchRateSecondary);
        Assert.Equal(NoYes.Yes, restored.ReportingCurrencyFixedExchRate);
        dto.LedgerJournalInclTax = (NoYes)99;
        Assert.Contains(new LedgerJournalTableDtoValidator().Validate(dto).Errors,
            error => error.PropertyName == nameof(LedgerJournalTableDto.LedgerJournalInclTax));
    }

    [Fact]
    public void Journal_payload_round_trips_supported_posting_and_settlement_fields()
    {
        var source = new LedgerJournalTrans { JournalNum = "J1", AccountType = LedgerJournalACType.Cust,
            LedgerDimensionName = "C-001", Invoice = "INV-1", PostingProfile = "AR", Due = new DateTime(2026, 2, 1),
            Payment = "NET30", PaymMode = "BANK", Prepayment = NoYes.Yes, SettleVoucher = SettleVoucher.Selection,
            MarkedInvoiceRecId = 42, MarkedInvoiceCompany = "dat", TaxGroup = "VAT", TaxItemGroup = "ITEM",
            TaxCode = "TAX", ExchRateSecond = 2m, ReportingCurrencyExchRateSecondary = 3m,
            CashDiscCode = "D1", CashDiscBaseDate = new DateTime(2026, 1, 1), Cancel = NoYes.Yes };
        var dto = source.Adapt<LedgerJournalTransDto>();
        var restored = dto.Adapt<LedgerJournalTrans>();
        Assert.Equal(source.Invoice, restored.Invoice);
        Assert.Equal(source.PostingProfile, restored.PostingProfile);
        Assert.Equal(source.Due, restored.Due);
        Assert.Equal(source.Payment, restored.Payment);
        Assert.Equal(source.PaymMode, restored.PaymMode);
        Assert.Equal(source.Prepayment, restored.Prepayment);
        Assert.Equal(source.SettleVoucher, restored.SettleVoucher);
        Assert.Equal(source.MarkedInvoiceRecId, restored.MarkedInvoiceRecId);
        Assert.Equal(source.MarkedInvoiceCompany, restored.MarkedInvoiceCompany);
        Assert.Equal(source.TaxGroup, restored.TaxGroup);
        Assert.Equal(source.TaxItemGroup, restored.TaxItemGroup);
        Assert.Equal(source.TaxCode, restored.TaxCode);
        Assert.Equal(source.ExchRateSecond, restored.ExchRateSecond);
        Assert.Equal(source.ReportingCurrencyExchRateSecondary, restored.ReportingCurrencyExchRateSecondary);
        Assert.Equal(source.CashDiscCode, restored.CashDiscCode);
        Assert.Equal(source.CashDiscBaseDate, restored.CashDiscBaseDate);
        Assert.Equal(source.Cancel, restored.Cancel);
    }

    [Theory]
    [InlineData(LedgerJournalACType.Cust, "C-001")]
    [InlineData(LedgerJournalACType.Vend, "V-001")]
    [InlineData(LedgerJournalACType.Bank, "BANK-1")]
    public void Subledger_journal_payload_requires_and_maps_the_named_account(LedgerJournalACType accountType, string account)
    {
        var dto = new LedgerJournalTransDto { JournalNum = "J1", TransDate = new DateTime(2026, 1, 15),
            AccountType = accountType, LedgerDimensionName = account, AmountCurDebit = 100m };
        var validator = new LedgerJournalTransDtoValidator();
        Assert.True(validator.Validate(dto).IsValid);
        var entity = dto.Adapt<LedgerJournalTrans>();
        Assert.Equal(account, entity.LedgerDimensionName);
        Assert.Equal(accountType, entity.AccountType);
        Assert.Equal(0, entity.LedgerDimension);
        dto.LedgerDimensionName = " ";
        Assert.Contains(validator.Validate(dto).Errors, x => x.PropertyName == nameof(LedgerJournalTransDto.LedgerDimensionName));
    }

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
