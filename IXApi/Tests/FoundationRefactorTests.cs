using IAX.IXApi.Modules.Finance.Foundation.Tax;
using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Foundation.Currency;
using IAX.IXApi.Modules.Finance.Foundation.LegalEntities;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Xunit;

namespace IXApi.Tests;

public sealed class FoundationRefactorTests
{
    [Fact]
    public void Bulk_currency_mapping_preserves_keys_company_and_rate_parent()
    {
        var config = new TypeAdapterConfig();
        new CurrencyBulkMapping().Register(config);
        var pair = new ExchangeRateCurrencyPair { RecId = 11, DataAreaId = "dat" };
        var input = new BulkExchangeRatePairDto { RecId = 99, DataAreaId = "other", FromCurrencyCode = "USD", ToCurrencyCode = "SAR", ExchangeRateType = 5 };
        input.Adapt(pair, config);
        Assert.Equal(11, pair.RecId);
        Assert.Equal("dat", pair.DataAreaId);
        Assert.Equal("USD", pair.FromCurrencyCode);
        var rate = new ExchangeRate { RecId = 12, ExchangeRateCurrencyPair = 11, DataAreaId = "dat" };
        new ExchangeRateBulkMappingSource(new ExchangeRateDto { RecId = 99, ExchangeRateCurrencyPair = 99, ExchangeRateValue = 3.75m }).Adapt(rate, config);
        Assert.Equal(12, rate.RecId);
        Assert.Equal(11, rate.ExchangeRateCurrencyPair);
        Assert.Equal(3.75m, rate.ExchangeRateValue);
    }

    [Fact]
    public void Address_mapping_preserves_identity_and_resolved_geography()
    {
        var config = new TypeAdapterConfig();
        new AddressBookMapping().Register(config);
        var postal = new LogisticsPostalAddress { RecId = 12, Location = 44, DataAreaId = "dat" };
        var input = new AddressInfoDto { Id = "99", Location = 99, Street = "Main", CountryRegionId = "SA" };
        new PostalAddressWriteSource(input, ("SAU", "R", "C", "Riyadh", 10L, "12345", 11L, "D", 12L)).Adapt(postal, config);
        Assert.Equal(12, postal.RecId);
        Assert.Equal(44, postal.Location);
        Assert.Equal("SAU", postal.CountryRegionId);
        Assert.Equal("Main, Riyadh, R 12345, SAU", postal.Address);
        Assert.Equal(DateTime.MinValue, postal.ValidFrom);
        Assert.Equal(DateTime.MaxValue, postal.ValidTo);
        var contact = new LogisticsElectronicAddress { RecId = 1, Location = 44 };
        new ElectronicAddressWriteSource(new ContactInfoDto { Number = "123", Primary = true }, ElectronicAddressType.Phone).Adapt(contact, config);
        Assert.Equal(44, contact.Location);
        Assert.Equal(NoYes.Yes, contact.IsPrimary);
        Assert.Equal("123", new ElectronicAddressReadSource(contact, null).Adapt<ContactInfoDto>(config).Number);
    }

    [Fact]
    public void Company_save_mapping_preserves_party_and_legacy_logos()
    {
        var config = new TypeAdapterConfig();
        new CompanyOperationMapping().Register(config);
        var company = new CompanyInfo { RecId = 1, Party = 2, Logo = [1, 2], DataAreaId = "dat" };
        new CompanyWriteSource(new CompanyInfoDto { RecId = 99, Party = 99, Name = "Company", Logo = "ignored" }).Adapt(company, config);
        Assert.Equal(1, company.RecId);
        Assert.Equal(2, company.Party);
        Assert.Equal(new byte[] { 1, 2 }, company.Logo);
        Assert.Equal("Company", company.Name);
    }

    [Fact]
    public async Task Address_book_does_not_commit_its_callers_transaction()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new DbContext(new DbContextOptionsBuilder().UseSqlite(connection).Options);
        await using var transaction = await db.Database.BeginTransactionAsync();
        var unitOfWork = new CountingUnitOfWork(db);
        var partyLocations = DispatchProxy.Create<IPartyLocationService, EmptyPartyLocations>();
        var service = new GlobalAddressBookService(unitOfWork, null!, null!, null!, partyLocations);
        await service.UpdateGlobalAddressBookAsync(1, [], []);
        Assert.Equal(0, unitOfWork.TransactionCalls);
        Assert.Same(transaction, db.Database.CurrentTransaction);
    }

    [Fact]
    public void Tax_line_mapping_preserves_identity_and_operation_specific_exemption_defaults()
    {
        var config = new TypeAdapterConfig();
        new TaxOperationMapping().Register(config);
        var dto = new TaxGroupDataDto { RecId = 99, DataAreaId = "other", TaxGroup = "other", TaxCode = "VAT", TaxExemptCode = "", UseTax = NoYes.Yes };
        var created = new TaxGroupLineCreateSource(dto, "dat", "DOM", true).Adapt<TaxGroupData>(config);
        Assert.Equal("", created.TaxExemptCode);
        Assert.Equal(0, created.RecId);
        Assert.Equal("dat", created.DataAreaId);
        Assert.Equal("DOM", created.TaxGroup);
        created.RecId = 10;
        new TaxGroupLineWriteSource(dto).Adapt(created, config);
        Assert.Equal("NONE", created.TaxExemptCode);
        Assert.Equal(10, created.RecId);
        Assert.Equal("DOM", created.TaxGroup);
        Assert.Equal(NoYes.Yes, created.UseTax);
        var response = new TaxGroupLineReadSource(created, 15m).Adapt<TaxGroupDataDto>(config);
        Assert.Null(response.TaxCodeName);
        Assert.Equal(15m, response.TaxValue);
        Assert.Equal(10, response.RecId);
        var item = new TaxOnItem { RecId = 20, TaxCode = "VAT", TaxItemGroup = "GOODS", DataAreaId = "dat" };
        new TaxItemGroupLineWriteSource(new TaxOnItemDto { RecId = 999, TaxCode = "BAD", TaxExemptCode = " " }).Adapt(item, config);
        Assert.Equal(20, item.RecId);
        Assert.Equal("VAT", item.TaxCode);
        Assert.Equal("NONE", item.TaxExemptCode);
    }

    public class EmptyPartyLocations : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? method, object?[]? args) => method?.Name == "GetPartyLocationsAsync"
            ? Task.FromResult(new List<DirPartyLocation>()) : throw new NotSupportedException();
    }

    private sealed class CountingUnitOfWork(DbContext db) : IUnitOfWork
    {
        public int TransactionCalls { get; private set; }
        public DbContext Context => db;
        public IGenericRepository<T> Repository<T>() where T : class => throw new NotSupportedException();
        public Task<int> CompleteAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
        public Task BeginTransactionAsync(CancellationToken ct = default) { TransactionCalls++; return Task.CompletedTask; }
        public Task CommitTransactionAsync(CancellationToken ct = default) { TransactionCalls++; return Task.CompletedTask; }
        public Task RollbackTransactionAsync(CancellationToken ct = default) { TransactionCalls++; return Task.CompletedTask; }
        public void Dispose() { }
    }
}
