using System.Reflection;
using System.Text.Json;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Foundation.Currency;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using IAX.IXApi.Modules.Finance.Foundation.Tax;
using IAX.IXApi.Modules.Finance.Foundation.LegalEntities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Querying;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

[Collection("AccountsPayable phase one")]
public sealed class FoundationServiceStageTests
{
    private static readonly Type[] CrudServices = typeof(FinanceModule).Assembly.GetTypes()
        .Where(t => !t.IsAbstract && t.Namespace?.StartsWith("IAX.IXApi.Modules.Finance.Foundation.") == true
            && t.BaseType is { IsGenericType: true } b && b.GetGenericTypeDefinition() == typeof(BaseService<>))
        .OrderBy(t => t.FullName, StringComparer.Ordinal).ToArray();

    [Fact]
    public async Task Foundation_services_SQLite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        var actor = new Identity();
        await using var db = new SqliteModel(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options, actor);
        await ExecuteAsync(db, actor, "SQLite", () => db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'")));
    }

    [SqlServerPhaseOneFact]
    public async Task Foundation_services_SQLServer()
    {
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER"),
            InitialCatalog = "IXApi_FoundationServices_" + Guid.NewGuid().ToString("N"),
            IntegratedSecurity = true, TrustServerCertificate = true
        };
        var actor = new Identity();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options;
        await using var db = new ApplicationDbContext(options, companyContext: actor);
        try { await ExecuteAsync(db, actor, "SQL Server", async () => { await db.Database.EnsureCreatedAsync(); }); }
        finally
        {
            await using var cleanup = new ApplicationDbContext(options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    private static async Task ExecuteAsync(ApplicationDbContext db, Identity actor, string engine, Func<Task> initialize)
    {
        var run = Guid.NewGuid().ToString("N");
        var failures = new List<string>();
        void Log(string name, string status, string? detail = null)
        {
            var path = Environment.GetEnvironmentVariable("IXAPI_FOUNDATION_STAGE_REPORT")
                ?? Path.Combine(AppContext.BaseDirectory, "foundation-service-results.jsonl");
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.AppendAllText(path, JsonSerializer.Serialize(new { runId = run, provider = engine, stage = name, status, detail, atUtc = DateTime.UtcNow }) + Environment.NewLine);
        }
        async Task Stage(string name, Func<Task> action)
        {
            Log(name, "RUNNING");
            try { await action(); Log(name, "PASSED"); }
            catch (Exception ex)
            {
                var error = ex is TargetInvocationException { InnerException: not null } target ? target.InnerException : ex;
                failures.Add(name + ": " + error!.Message); Log(name, "FAILED", error.ToString());
            }
            finally { actor.Area = "dat"; db.ChangeTracker.Clear(); }
        }
        Log("run", "STARTED");
        await initialize();
        var services = new ServiceCollection().AddLogging().AddSingleton(db)
            .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
            .AddScoped<IUnitOfWork, UnitOfWork>().AddScoped<ISysNumberSequenceService, SysNumberSequenceService>();
        services.AddFinanceModule(new ConfigurationBuilder().Build()).AddFoundationTransactionExamples();
        foreach (var type in CrudServices)
        {
            services.AddScoped(type);
            var entity = type.BaseType!.GetGenericArguments()[0];
            services.AddScoped(typeof(IBaseService<>).MakeGenericType(entity), p => p.GetRequiredService(type));
        }
        // Use the same mapping registrations as the application, including identity-preserving operation mappings.
        TypeAdapterConfig.GlobalSettings.Scan(typeof(FinanceModule).Assembly);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<FoundationCycleSeeder>().PrepareAsync(new(2026, "foundation-services-test"));
        // Additional setup for number-allocating operations; the master example inserts its own fixed keys.
        var numbers = new SysNumberSequenceService(sp.GetRequiredService<IUnitOfWork>(),actor);
        foreach (var name in new[] { "DirPartyTable", "LogisticsLocation", "HcmWorker", "HcmShowroom" })
            await numbers.AddAsync(new SysNumberSequence { DataAreaId = "dat", NumberSequence = name, Txt = "Foundation service test", Format = "FT-######", AnnotatedFormat = "FT-{SEQ}", Lowest = 1, Highest = 999999, NextRec = 1, InUse = 1 });
        await Stage("Foundation CRUD inventory", () =>
        {
            Assert.True(CrudServices.Length >= 30); return Task.CompletedTask;
        });
        foreach (var type in CrudServices)
            await Stage(type.Name + ": read/create/update/delete", async () =>
            {
                var test = typeof(FoundationServiceStageTests).GetMethod(nameof(CheckCrudAsync), BindingFlags.Static | BindingFlags.NonPublic)!
                    .MakeGenericMethod(type.BaseType!.GetGenericArguments()[0]);
                await (Task)test.Invoke(null, [db, sp.GetRequiredService(type),sp])!;
            });
        await Stage("Currency pair bulk save and effective rate", () => CheckCurrencyAsync(db, sp));
        await Stage("Tax group commands, lines and queries", () => CheckTaxGroupsAsync(db, sp));
        await Stage("Tax item group commands, lines and queries", () => CheckTaxItemGroupsAsync(db, sp));
        await Stage("Tax period create, update, query and delete", () => CheckTaxPeriodsAsync(db, sp));
        await Stage("Party, location, postal and electronic addresses", () => CheckAddressBookAsync(db, sp));
        await Stage("Legal entity and address book transaction", () => CheckCompanyAsync(db, sp));
        await Stage("Worker, showroom and assignment services", () => CheckWorkersAsync(db, sp));
        await Stage("Tax codes, rates and save transaction", () => CheckTaxCodesAsync(db, sp));
        await Stage("Account structure service and dimensions", () => CheckAccountStructureAsync(db, sp));
        await Stage("Tax and charges financial effects, reporting and settlement", () => CheckTaxPostingAsync(db, sp));
        await Stage("Tracked primary keys respect company isolation", async () =>
        {
            var currency = await db.Set<Currency>().SingleAsync(x => x.CurrencyCode == "SAR");
            actor.Area = "oth";
            Assert.Null(await sp.GetRequiredService<ICurrencyService>().GetByIdAsync(currency.RecId));
            Assert.Null(await sp.GetRequiredService<ICurrencyService>().GetByCompositeKeyAsync([currency.RecId]));
        });
        await Stage("Tracked tax keys cannot read, update or delete another company", async () =>
        {
            var group = await db.Set<TaxGroupHeading>().SingleAsync(x => x.TaxGroup == "APV2");
            var item = await db.Set<TaxItemGroupHeading>().SingleAsync(x => x.TaxItemGroup == "APV2");
            var line = await db.Set<TaxGroupData>().SingleAsync(x => x.TaxGroup == "APV2");
            var itemLine = await db.Set<TaxOnItem>().SingleAsync(x => x.TaxItemGroup == "APV2");
            actor.Area = "oth";
            Assert.Null(await sp.GetRequiredService<ITaxGroupQueryService>().GetByIdAsync(group.RecId.ToString()));
            Assert.Null(await sp.GetRequiredService<ITaxItemGroupQueryService>().GetByIdAsync(item.RecId.ToString()));
            Assert.Null(await sp.GetRequiredService<ITaxGroupCommandService>().UpdateAsync(group.RecId.ToString(),new()));
            Assert.Null(await sp.GetRequiredService<ITaxItemGroupCommandService>().UpdateAsync(item.RecId.ToString(),new()));
            Assert.False(await sp.GetRequiredService<ITaxGroupLineService>().DeleteLineAsync(line.RecId));
            Assert.False(await sp.GetRequiredService<ITaxItemGroupLineService>().DeleteLineAsync(itemLine.RecId));
            Assert.Null((await sp.GetRequiredService<ITaxGroupLineService>().AddLineAsync(group.RecId.ToString(),new() { TaxCode = "APV2-15" })).Line);
            Assert.Null(await sp.GetRequiredService<ITaxItemGroupLineService>().AddLineAsync(item.RecId.ToString(),new() { TaxCode = "APV2-15" }));
        });
        Log("run", failures.Count == 0 ? "PASSED" : "FAILED", string.Join(Environment.NewLine, failures));
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static async Task CheckCrudAsync<T>(ApplicationDbContext db, IBaseService<T> service, IServiceProvider sp) where T : class, new()
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var all = (await service.GetAllAsync(Array.Empty<string>())).ToArray();
            Assert.NotEmpty(all);
            var metadata = db.Model.FindEntityType(typeof(T))!;
            var key = Assert.Single(metadata.FindPrimaryKey()!.Properties);
            var sample = all.OrderBy(x => Convert.ToInt64(key.PropertyInfo!.GetValue(x))).First();
            var id = key.PropertyInfo!.GetValue(sample)!;
            Assert.NotNull(await service.GetByIdAsync(id));
            var missingId = key.ClrType == typeof(byte) ? byte.MaxValue : key.ClrType == typeof(short) ? short.MaxValue : key.ClrType == typeof(int) ? int.MaxValue : long.MaxValue;
            Assert.Null(await service.GetByIdAsync(Convert.ChangeType(missingId, key.ClrType)));
            var page = await service.GetPagedAsync(new QueryFilterDto { PageNumber = 1, PageSize = 1 });
            Assert.Equal(all.Length, page.TotalCount);
            var clone = new T();
            foreach (var property in metadata.GetProperties().Where(p => !p.IsPrimaryKey() && p.ValueGenerated == ValueGenerated.Never && p.PropertyInfo?.CanWrite == true))
                property.PropertyInfo!.SetValue(clone, property.PropertyInfo.GetValue(sample));
            foreach (var unique in metadata.GetIndexes().Where(i => i.IsUnique).Select(i => i.Properties)
                .Concat(metadata.GetKeys().Where(k => !k.IsPrimaryKey()).Select(k => k.Properties)))
            {
                var code = unique.FirstOrDefault(p => p.ClrType == typeof(string) && p.Name != "DataAreaId" && !p.IsForeignKey());
                if (code != null) code.PropertyInfo!.SetValue(clone, "T" + Guid.NewGuid().ToString("N")[..Math.Min(7, (code.GetMaxLength() ?? 8) - 1)]);
            }
            if (clone is ExchangeRateCurrencyPair pair)
                pair.ExchangeRateType = (await sp.GetRequiredService<IExchangeRateTypeService>().AddAsync(new ExchangeRateType { Name = "FT-CLONE", Description = "CRUD fixture" })).RecId;
            if (clone is LogisticsPostalAddress postal)
                postal.Location = (await sp.GetRequiredService<ILocationService>().CreateLocationAsync("CRUD address",true)).RecId;
            var created = await service.AddAsync(clone);
            var newId = key.PropertyInfo.GetValue(created)!;
            Assert.NotEqual(id, newId);
            db.ChangeTracker.Clear();
            var saved = await service.GetByIdAsync(newId);
            Assert.NotNull(saved);
            var description = metadata.GetProperties().FirstOrDefault(p => p.ClrType == typeof(string) && !p.IsForeignKey() && !p.IsKey()
                && p.Name is "Name" or "Description" or "Txt" or "TaxName");
            if (description != null) description.PropertyInfo!.SetValue(saved, "Verified");
            await service.UpdateAsync(saved!);
            db.ChangeTracker.Clear();
            var updated = await service.GetByIdAsync(newId);
            Assert.NotNull(updated);
            if (description != null) Assert.Equal("Verified", description.PropertyInfo!.GetValue(updated));
            await service.RemoveAsync(updated!);
            db.ChangeTracker.Clear();
            Assert.Null(await service.GetByIdAsync(newId));
        }
        finally { await transaction.RollbackAsync(); db.ChangeTracker.Clear(); }
    }

    private static async Task CheckCurrencyAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var rateType = await sp.GetRequiredService<IExchangeRateTypeService>().AddAsync(new ExchangeRateType { Name = "FTEST", Description = "Service validation" });
        var pairs = sp.GetRequiredService<IExchangeRateCurrencyPairService>();
        var input = new BulkExchangeRatePairDto
        {
            DataAreaId = "dat", ExchangeRateType = rateType.RecId, FromCurrencyCode = "USD", ToCurrencyCode = "SAR",
            ExchangeRateDisplayFactor = ExchangeRateDisplayFactor.One,
            ExchangeRates = [new() { ExchangeRateValue = 3.75m, ValidFrom = new(2026,1,1), ValidTo = new(2026,12,31) },
                new() { ExchangeRateValue = 3.80m, ValidFrom = new(2026,5,20), ValidTo = new(2026,12,31) }]
        };
        await pairs.BulkSaveAsync(input);
        db.ChangeTracker.Clear();
        var lookup = new EffectiveExchangeRateLookup(db);
        Assert.Equal(3.75m,(await lookup.FindAsync("dat",rateType.RecId,"USD","SAR",new(2026,4,15),default)).Rate!.ExchangeRateValue);
        Assert.Equal(3.80m,(await lookup.FindAsync("dat",rateType.RecId,"USD","SAR",new(2026,5,20),default)).Rate!.ExchangeRateValue);
        Assert.NotNull((await lookup.FindAsync("oth",rateType.RecId,"USD","SAR",new(2026,4,15),default)).Error);
        var rates = await db.Set<ExchangeRate>().Where(x => x.ExchangeRateCurrencyPair == input.RecId).OrderBy(x => x.ValidFrom).ToListAsync();
        Assert.Equal(2,rates.Count);
        input.ExchangeRates = [new() { RecId = rates[0].RecId, ExchangeRateCurrencyPair = long.MaxValue, ExchangeRateValue = 3.70m, ValidFrom = rates[0].ValidFrom, ValidTo = rates[0].ValidTo }];
        await pairs.BulkSaveAsync(input);
        db.ChangeTracker.Clear();
        var updated = Assert.Single(await db.Set<ExchangeRate>().Where(x => x.ExchangeRateCurrencyPair == input.RecId).ToListAsync());
        Assert.Equal(rates[0].RecId,updated.RecId); Assert.Equal(input.RecId,updated.ExchangeRateCurrencyPair);
        Assert.Equal(3.70m,updated.ExchangeRateValue);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => pairs.BulkSaveAsync(new() { RecId = long.MaxValue }));
    }
    private static async Task CheckTaxGroupsAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var command = sp.GetRequiredService<ITaxGroupCommandService>();
        var query = sp.GetRequiredService<ITaxGroupQueryService>();
        var lines = sp.GetRequiredService<ITaxGroupLineService>();
        var created = await command.CreateAsync(new() { TaxGroup = "FT-GRP", TaxGroupName = "Original", DataAreaId = "dat",
            Lines = [new() { TaxCode = "APV2-15", TaxExemptCode = "APV2-NONE" }] });
        db.ChangeTracker.Clear();
        var read = await query.GetByIdAsync("FT-GRP");
        Assert.NotNull(read); Assert.Equal(15m,Assert.Single(read.Lines).TaxValue);
        Assert.Contains(await query.GetAllAsync(), x => x.RecId == created.RecId);
        var changed = await command.UpdateAsync(created.RecId.ToString(), new() { RecId = long.MaxValue, TaxGroup = "OTHER", TaxGroupName = "Changed", DataAreaId = "dat",
            Lines = [new() { TaxCode = "APV2-15", TaxExemptCode = "APV2-NONE" }] });
        Assert.Equal(created.RecId,changed!.RecId); Assert.Equal("FT-GRP",changed.TaxGroup);
        var line = await lines.AddLineAsync("FT-GRP",new() { TaxCode = "APV2-15", TaxExemptCode = "APV2-NONE", UseTax = NoYes.Yes });
        Assert.True(line.Updated); Assert.Equal(NoYes.Yes,line.Line!.UseTax);
        Assert.True(await lines.DeleteLineAsync(line.Line.RecId));
        Assert.False(await lines.DeleteLineAsync(long.MaxValue));
        Assert.Null((await lines.AddLineAsync("missing",new() { TaxCode = "APV2-15" })).Line);
        Assert.Null(await command.UpdateAsync("missing",new()));
    }
    private static async Task CheckTaxItemGroupsAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var command = sp.GetRequiredService<ITaxItemGroupCommandService>();
        var query = sp.GetRequiredService<ITaxItemGroupQueryService>();
        var lines = sp.GetRequiredService<ITaxItemGroupLineService>();
        var created = await command.CreateAsync(new() { TaxItemGroup = "FT-ITEM", Name = "Original", DataAreaId = "dat",
            Lines = [new() { TaxCode = "APV2-15", TaxExemptCode = "APV2-NONE" }] });
        db.ChangeTracker.Clear();
        var read = await query.GetByIdAsync("FT-ITEM");
        Assert.NotNull(read); Assert.Single(read.Lines);
        Assert.Contains(await query.GetAllAsync(), x => x.RecId == created.RecId);
        var changed = await command.UpdateAsync(created.RecId.ToString(), new() { RecId = long.MaxValue, TaxItemGroup = "OTHER", Name = "Changed", DataAreaId = "dat",
            Lines = [new() { TaxCode = "APV2-15", TaxExemptCode = "APV2-NONE" }] });
        Assert.Equal(created.RecId,changed!.RecId); Assert.Equal("FT-ITEM",changed.TaxItemGroup);
        var line = await lines.AddLineAsync("FT-ITEM",new() { TaxCode = "APV2-15", TaxExemptCode = "APV2-NONE" });
        Assert.NotNull(line); Assert.True(await lines.DeleteLineAsync(line.RecId));
        Assert.False(await lines.DeleteLineAsync(long.MaxValue));
        Assert.Null(await lines.AddLineAsync("missing",new() { TaxCode = "APV2-15" }));
        Assert.Null(await command.UpdateAsync("missing",new()));
    }
    private static async Task CheckTaxPeriodsAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var service = sp.GetRequiredService<ITaxPeriodService>();
        var authority = await db.Set<TaxAuthorityAddress>().SingleAsync();
        var input = new TaxPeriodHeadDto { DataAreaId = "dat", TaxPeriod = "FT-2026", Name = "Test period", TaxAuthority = authority.TaxAuthority,
            Intervals = [new() { FromDate = new(2026,1,1), ToDate = new(2026,1,31) }] };
        var header = await service.CreateAsync(input);
        db.ChangeTracker.Clear();
        var read = await service.GetByIdAsync(header.RecId.ToString());
        Assert.NotNull(read); Assert.Single(read.Intervals);
        Assert.Contains(await service.GetAllAsync(),x => x.RecId == header.RecId);
        input.RecId = long.MaxValue; input.TaxPeriod = "WRONG"; input.Name = "Changed";
        input.Intervals = [new() { FromDate = new(2026,2,1), ToDate = new(2026,2,28) }];
        var changed = await service.UpdateAsync(header.RecId.ToString(),input);
        Assert.Equal(header.RecId,changed!.RecId); Assert.Equal("FT-2026",changed.TaxPeriod);
        Assert.Equal(new DateTime(2026,2,1),Assert.Single((await service.GetByIdAsync("FT-2026"))!.Intervals).FromDate);
        Assert.True(await service.DeleteAsync("FT-2026"));
        Assert.Null(await service.GetByIdAsync("FT-2026"));
        Assert.False(await service.DeleteAsync("missing"));
    }
    private static async Task CheckAddressBookAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var parties = sp.GetRequiredService<IPartyService>();
        var party = await parties.CreatePartyAsync("Foundation service test","en-us");
        Assert.True(party.RecId > 0); Assert.NotEmpty(party.PartyNumber);
        await parties.UpdatePartyNameAsync(party.RecId,"Changed party");
        Assert.Equal("Changed party",(await db.Set<DirPartyTable>().AsNoTracking().SingleAsync(x => x.RecId == party.RecId)).Name);
        var addressBook = sp.GetRequiredService<IGlobalAddressBookService>();
        var query = sp.GetRequiredService<IAddressBookQueryService>();
        var address = await addressBook.CreatePartyAddressAsync(party.RecId,new() { Street = "Test street", CountryRegionId = "SAU", Primary = true });
        var addresses = await query.GetPartyAddressesAsync(party.RecId);
        Assert.Equal("Test street",Assert.Single(addresses).Street);
        address.Street = "Updated street";
        await addressBook.UpdatePartyAddressAsync(party.RecId,address);
        Assert.Equal("Updated street",Assert.Single(await query.GetPartyAddressesAsync(party.RecId)).Street);
        var contact = await addressBook.CreatePartyContactAsync(party.RecId,new() { Type = "Phone", Number = "0501234567", Primary = true });
        Assert.Equal("0501234567",Assert.Single(await query.GetPartyContactsAsync(party.RecId)).Number);
        contact.Number = "0507654321";
        await addressBook.UpdatePartyContactAsync(party.RecId,contact);
        Assert.Equal("0507654321",Assert.Single(await query.GetPartyContactsAsync(party.RecId)).Number);
        Assert.True(await addressBook.DeletePartyContactAsync(party.RecId,contact.Location));
        Assert.Empty(await query.GetPartyContactsAsync(party.RecId));
        Assert.True(await addressBook.DeletePartyAddressAsync(party.RecId,address.Location));
        Assert.Empty(await query.GetPartyAddressesAsync(party.RecId));
        Assert.NotEmpty((System.Collections.IEnumerable)await query.GetCountryRegionsAsync());
    }

    private static async Task CheckCompanyAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var service = sp.GetRequiredService<ICompanyInfoService>();
        var input = new CompanyInfoDto { DataArea = "ft1", Name = "Test legal entity", CurrencyCode = "SAR", LanguageId = "en-us",
            Addresses = [new() { Street = "Company street", CountryRegionId = "SAU", Primary = true }] };
        var company = await service.CreateCompanyWithAddressBookAsync(input,default);
        Assert.True(company.Party > 0);
        var party = company.Party;
        input.Name = "Updated legal entity"; input.RecId = long.MaxValue; input.Party = long.MaxValue;
        var updated = await service.UpdateCompanyWithAddressBookAsync(company.RecId.ToString(),input,default);
        Assert.Equal(company.RecId,updated.RecId); Assert.Equal(party,updated.Party);
        Assert.Equal(input.Name,(await db.Set<DirPartyTable>().SingleAsync(x => x.RecId == party)).Name);
        var dto = new CompanyInfoDto { RecId = company.RecId, Party = party };
        await service.PopulateGlobalAddressBookAsync([dto],default);
        Assert.Single(dto.Addresses);
    }
    private static async Task CheckWorkersAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var workers = sp.GetRequiredService<IHcmWorkerService>();
        var showrooms = sp.GetRequiredService<IHcmShowroomService>();
        var existing = await db.Set<HcmWorker>().SingleAsync(x => x.PersonnelNumber == "FNV2-BUYER");
        var input = new HcmWorkerDto { DataAreaId = "dat", PersonnelNumber = "FT-WORKER", Name = "Service worker", NameAlias = "Worker",
            GenderId = existing.GenderId, NationalityId = existing.NationalityId, HireDate = new(2026,1,1) };
        var worker = await workers.CreateFromDtoAsync(input);
        Assert.True(worker.Person > 0); Assert.NotEqual(existing.Person,worker.Person);
        input.Name = "Updated worker"; input.RecId = long.MaxValue; input.Person = long.MaxValue;
        var changed = await workers.UpdateFromDtoAsync(worker.RecId.ToString(),input);
        Assert.Equal(worker.RecId,changed!.RecId); Assert.Equal(worker.Person,changed.Person);
        Assert.Equal("Updated worker",changed.Name);
        Assert.Contains(await workers.GetWorkerListAsync(), x => x.RecId == worker.RecId);
        Assert.Contains((await workers.GetWorkerLookupAsync(1,10,"FT-WORKER",worker.RecId)).Data,x => x.RecId == worker.RecId);
        var showroom = await showrooms.CreateFromDtoAsync(new() { DataAreaId = "dat", PersonnelNumber = "FT-SHOW", Name = "Service showroom" });
        Assert.True(showroom.Party > 0);
        await showrooms.SaveWorkerAssignmentAsync(showroom.RecId,null,new(worker.RecId,new(2026,1,1),null,true));
        Assert.Equal(worker.RecId,Assert.Single(await showrooms.GetWorkerAssignmentsAsync(showroom.RecId)).HcmWorkerId);
        Assert.Equal(showroom.RecId,Assert.Single(await workers.GetShowroomAssignmentsAsync(worker.RecId)).HcmShowroomId);
        await workers.GetOrganizationAssignmentsV1Async(worker.RecId);
        await workers.GetAssignmentChainAsync(worker.RecId);
        var partyCount = await db.Set<DirPartyTable>().CountAsync();
        await Assert.ThrowsAsync<KeyNotFoundException>(() => workers.AddWorkerAsync(new HcmWorker { PersonnelNumber = "FT-FAIL", GenderId = existing.GenderId, NationalityId = existing.NationalityId },"Must roll back",null,long.MaxValue));
        db.ChangeTracker.Clear();
        Assert.Equal(partyCount,await db.Set<DirPartyTable>().CountAsync());
        Assert.False(await db.Set<HcmWorker>().AnyAsync(x => x.PersonnelNumber == "FT-FAIL"));
    }
    private static async Task CheckTaxCodesAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var service = sp.GetRequiredService<ITaxTableService>();
        var input = new TaxTableDto { TaxCode = "FT-VAT", TaxName = "Test rate", TaxPeriod = "Monthly", TaxAccountGroup = "APV2", TaxCurrencyCode = "SAR", TaxValue = 10m };
        var created = await service.CreateFromDtoAsync(input);
        Assert.NotNull(created); Assert.Equal(10m,created.TaxValue);
        var rate = await db.Set<TaxData>().SingleAsync(x => x.TaxCode == "FT-VAT");
        var from = rate.TaxFromDate;
        input.TaxValue = 18m; input.TaxName = "Updated rate";
        await service.UpdateFromDtoAsync("FT-VAT",input);
        db.ChangeTracker.Clear();
        Assert.Equal(18m,(await service.GetDetailAsync("FT-VAT"))!.TaxValue);
        Assert.Equal(from,(await db.Set<TaxData>().SingleAsync(x => x.TaxCode == "FT-VAT")).TaxFromDate);
        Assert.Contains(await service.GetDetailsAsync(),x => x.TaxCode == "FT-VAT");
        Assert.True(await service.DeleteByCodeAsync("FT-VAT"));
        Assert.Null(await service.GetDetailAsync("FT-VAT"));
    }
    private static async Task CheckAccountStructureAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IAccountStructureService>();
        var created = await service.CreateAsync(new("FT-STRUCT","Service test",[new("MainAccount","*")]),default);
        Assert.Null(created.Error); Assert.True(created.Result!.IsDraft);
        var activated = await service.ActivateAsync("FT-STRUCT",default);
        Assert.Null(activated.Error); Assert.False(activated.Result!.IsDraft);
        var assigned = await service.AssignToLedgerAsync("FT-STRUCT",default);
        Assert.Null(assigned.Error); Assert.True(assigned.Result!.IsAssignedToLedger);
        Assert.Contains((await service.ListAsync(default)).Items!,x => x.Name == "FT-STRUCT");
        Assert.NotNull((await service.ActivateAsync("missing",default)).Error);
    }
    private static async Task CheckTaxPostingAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var examples = sp.GetRequiredService<FoundationTransactionExamples>();
        await examples.RunAsync(new(2026,"foundation-services-test"));
        Assert.Equal(435m,(await db.Set<TaxTrans>().ToListAsync()).Sum(x => x.TaxAmount));
        Assert.Equal(52.50m,Assert.Single(await db.Set<TaxJournalTrans>().ToListAsync()).TaxAmount);
        Assert.Equal(-3335m,(await db.Set<BankAccountTrans>().ToListAsync()).Sum(x => x.AmountCur));
        var stock = await db.Set<InventSum>().Where(x => x.ItemId == "APV2-STOCK").ToListAsync();
        Assert.Equal(100m,stock.Sum(x => x.PhysicalInvent)); Assert.Equal(2500m,stock.Sum(x => x.PostedValue));
        var counts = (await db.Set<GeneralJournalAccountEntry>().CountAsync(),await db.Set<TaxTrans>().CountAsync());
        await examples.RunAsync(new(2026,"foundation-services-test"));
        Assert.Equal(counts,(await db.Set<GeneralJournalAccountEntry>().CountAsync(),await db.Set<TaxTrans>().CountAsync()));
        var reports = sp.GetRequiredService<ITaxReportService>();
        var before = await reports.RunAsync(new(2026,4,1),new(2026,4,30),null,null);
        Assert.Null(before.Error); Assert.Equal(435m,before.Result!.IncomingTax);
        var settlement = sp.GetRequiredService<ITaxSettlementService>();
        var input = new TaxSettlementInputDto("Monthly",new(2026,4,1),new(2026,4,30));
        Assert.NotNull((await settlement.PreviewAsync(input)).Error); // Missing settlement-account setup is refused.
        var ledger = await db.Set<Ledger>().SingleAsync();
        var account = await sp.GetRequiredService<IMainAccountService>().AddAsync(new MainAccount { MainAccountId = "281099", Name = "Sales tax settlement", Type = MainAccountType.Liability, LedgerChartOfAccounts = ledger.ChartOfAccounts, CurrencyCode = "SAR" });
        var dimension = await new LedgerDimensionResolver(db).ResolveAsync("dat",account,[],0,default);
        var group = await db.Set<TaxLedgerAccountGroup>().SingleAsync(x => x.TaxAccountGroup == "APV2");
        group.TaxReportLedgerDimension = dimension.RecId;
        await sp.GetRequiredService<ITaxLedgerAccountGroupService>().UpdateAsync(group);
        var preview = await settlement.PreviewAsync(input);
        Assert.Null(preview.Error); Assert.Equal(435m,preview.Result!.SettlementAmount);
        Assert.Equal(counts.Item1,await db.Set<GeneralJournalAccountEntry>().CountAsync());
        var posted = await settlement.SettleAndPostAsync(input);
        Assert.Null(posted.Error); Assert.Equal(435m,posted.Result!.IncomingTaxCleared);
        Assert.Equal(435m,posted.Result.SettlementAmount);
        Assert.NotNull((await settlement.SettleAndPostAsync(input)).Error);
        var report = await reports.RunAsync(new(2026,4,1),new(2026,4,30),"Monthly",null);
        Assert.Null(report.Error); Assert.Equal(435m,report.Result!.IncomingTax);
        Assert.Equal(435m,report.Result.SettledTax); Assert.Equal(0m,report.Result.UnsettledTax);
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.All(entries.GroupBy(x => x.GeneralJournalEntry),voucher => Assert.Equal(0m,voucher.Sum(x => x.AccountingCurrencyAmount)));
        Assert.Equal(435m,entries.Where(x => x.LedgerDimension == dimension.RecId).Sum(x => x.AccountingCurrencyAmount));
    }

    private sealed class Identity : ICompanyExecutionContext, ICurrentUserService
    {
        public string Area { get; set; } = "dat";
        public string GetDataAreaId() => Area;
        public bool IsRequestedCompanyAuthorized() => true;
        public string GetCurrentUserId() => "foundation-services-test";
        public string GetOwnerAccountId() => "foundation-services-test";
    }
    private sealed class SqliteModel(DbContextOptions<ApplicationDbContext> options, Identity actor) : ApplicationDbContext(options, companyContext: actor)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            foreach (var entity in builder.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties())
            {
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                property.SetColumnType(type == typeof(byte[]) ? "BLOB" : type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal) || type == typeof(Guid) ? "TEXT" : "INTEGER");
                property.SetDefaultValueSql(null);
                if (property.Name == "RowVersion") property.SetDefaultValueSql("X'00'");
            }
        }
    }
}
