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
using IAX.IXApi.Modules.Finance.Foundation.CashDiscount;
using IAX.IXApi.Modules.Finance.Foundation.PaymentTerms;
using IAX.IXApi.Modules.Finance.Foundation.Markup;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
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
        foreach (var name in new[] { "DirPartyTable", "LogisticsLocation", "HcmWorker", "HcmShowroom", "TaxSettlement" })
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
        await Stage("Payment due dates, schedules and cash discount arithmetic", async () =>
        {
            var discount = await db.Set<CashDisc>().SingleAsync();
            var date = new DateTime(2026,4,15);
            var terms = await CashDiscountResolver.ResolveAsync(db,"dat",discount.CashDiscCode,date,-2875m,default);
            Assert.True(terms.Applies); Assert.Equal(-57.50m,terms.PossibleAmount);
            Assert.Equal(new DateTime(2026,4,25),terms.DiscountDate);
            Assert.True(CashDiscountCalculator.IsWithinPeriod(new(2026,4,25),terms.DiscountDate));
            Assert.False(CashDiscountCalculator.IsWithinPeriod(new(2026,4,26),terms.DiscountDate));
            Assert.False((await CashDiscountResolver.ResolveAsync(db,"oth",discount.CashDiscCode,date,-2875m,default)).Applies);
            var schedule = (await db.Set<PaymSchedLine>().Where(x => x.Name == "FNV2").ToListAsync()).OrderBy(x => x.LineNum).ToList();
            Assert.Equal(new[] { 50m,50m },schedule.Select(x => x.Value));
            Assert.Equal(100m,schedule.Sum(x => x.Value));
            var term = await db.Set<PaymTerm>().SingleAsync(x => x.PaymTermId == "APV2-N30");
            Assert.Equal(new DateTime(2026,5,15),PaymentDueDateCalculator.Calculate(date,term));
        });
        await Stage("Tax group commands, lines and queries", () => CheckTaxGroupsAsync(db, sp));
        await Stage("Tax item group commands, lines and queries", () => CheckTaxItemGroupsAsync(db, sp));
        await Stage("Tax period create, update, query and delete", () => CheckTaxPeriodsAsync(db, sp));
        await Stage("Party, location, postal and electronic addresses", () => CheckAddressBookAsync(db, sp));
        await Stage("Legal entity and address book transaction", () => CheckCompanyAsync(db, sp));
        await Stage("Worker, showroom and assignment services", () => CheckWorkersAsync(db, sp));
        await Stage("Organization positions, transfer history and overlap rejection", async () =>
        {
            var organization = sp.GetRequiredService<IOrganizationStructureService>();
            var start = new DateOnly(2026,1,1);
            var transfer = new DateOnly(2026,7,1);
            var worker = await db.Set<HcmWorker>().SingleAsync(x => x.PersonnelNumber == "FT-WORKER");
            var unit = await organization.CreateUnitAsync(new("FT-UNIT","Service unit",8,start),default);
            var role = await organization.CreateRoleAsync(new("FT-ROLE","Service role"),default);
            var first = await organization.CreatePositionAsync(new("FT-POS1","First position",unit,role,start,null),default);
            var second = await organization.CreatePositionAsync(new("FT-POS2","Second position",unit,role,start,null),default);
            var assignment = await organization.AssignWorkerAsync(new(worker.RecId,first,start,null),default);
            await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => organization.AssignWorkerAsync(new(worker.RecId,second,start,null),default));
            var changed = await organization.TransferAsync(assignment,new(second,transfer,null),default);
            Assert.Equal(first,Assert.Single(await organization.GetWorkerAssignmentsAsync(worker.RecId,transfer.AddDays(-1))).PositionId);
            Assert.Equal(changed,Assert.Single(await organization.GetWorkerAssignmentsAsync(worker.RecId,transfer)).AssignmentId);
            Assert.Contains(await organization.GetUnitsAsync(start,default),x => x.Id == unit);
            Assert.Contains(await organization.GetRolesAsync(default),x => x.Id == role);
            Assert.Contains(await organization.GetPositionsAsync(start,default),x => x.Id == second);
        });
        await Stage("Tax codes, rates and save transaction", () => CheckTaxCodesAsync(db, sp));
        await Stage("Account structure service and dimensions", () => CheckAccountStructureAsync(db, sp));
        await Stage("Charge commands, calculation, update, deletion and validation", async () =>
        {
            var orderResult = await sp.GetRequiredService<IPurchOrderService>().CreateAsync(new("APV2-dat-LOCAL",null,"SAR",new(2026,4,15),"APV2-SITE","APV2-WH","FT-CHARGES",new(2026,4,15)),default);
            Assert.Equal(200,orderResult.StatusCode);
            var order = (PurchTable)orderResult.Data!;
            var charges = sp.GetRequiredService<IMarkupTransService>();
            Assert.Null((await charges.GetChargeCodesAsync("purchase")).Error);
            var input = new MarkupTransDto { MarkupCode = "APV2-FRT", CurrencyCode = "SAR", Value = 50m, MarkupCategory = MarkupCategory.Fixed, TaxGroup = "APV2", TaxItemGroup = "APV2", TransDate = new(2026,4,15) };
            var created = await charges.CreateAsync("purchase",order.RecId,input);
            Assert.Null(created.Error); Assert.Equal(50m,created.Data!.CalculatedAmount);
            input.Value = 80m;
            var changed = await charges.UpdateAsync(created.Data.RecId,input);
            Assert.Null(changed.Error); Assert.Equal(80m,changed.Data!.CalculatedAmount);
            if (db.Database.IsSqlServer()) Assert.Single((await charges.GetDocumentChargesAsync("purchase",order.RecId)).Data!);
            else Assert.Single(await db.Set<IAX.IXApi.Modules.Finance.AccountsReceivable.MarkupTrans>().Where(x => x.TransRecId == order.RecId && !x.IsDeleted).ToListAsync());
            Assert.Null((await charges.DeleteAsync(created.Data.RecId)).Error);
            if (db.Database.IsSqlServer()) Assert.Empty((await charges.GetDocumentChargesAsync("purchase",order.RecId)).Data!);
            else Assert.Empty(await db.Set<IAX.IXApi.Modules.Finance.AccountsReceivable.MarkupTrans>().Where(x => x.TransRecId == order.RecId && !x.IsDeleted).ToListAsync());
            Assert.NotNull((await charges.CreateAsync("invalid",order.RecId,input)).Error);
            input.Value = -1m;
            Assert.NotNull((await charges.CreateAsync("purchase",order.RecId,input)).Error);
        });
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
        Assert.Equal(3.75m,(await sp.GetRequiredService<IExchangeRateService>().FindEffectiveAsync(rateType.RecId,"USD","SAR",new(2026,4,15))).Rate!.ExchangeRateValue);
        var converter = new LedgerCurrencyConverter(db);
        Assert.Equal(3018.75m,await converter.ConvertAsync("dat",805m,"USD","SAR",rateType.RecId,new(2026,4,15),default));
        Assert.Equal(3059m,await converter.ConvertAsync("dat",805m,"USD","SAR",rateType.RecId,new(2026,5,20),default));
        Assert.Equal(1.01m,await converter.ConvertAsync("dat",1.005m,"SAR","SAR",rateType.RecId,new(2026,4,15),default));
        var ledger = await db.Set<Ledger>().SingleAsync();
        var available = await new LedgerExchangeRateCheck(db).CheckAsync("dat",ledger,[new LedgerJournalTrans { CurrencyCode = "USD", TransDate = new(2026,4,15) }],default);
        Assert.Empty(available.Errors);
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
        var otherParty = await parties.CreatePartyAsync("Other party","en-us");
        Assert.False(await addressBook.DeletePartyAddressAsync(otherParty.RecId,address.Location));
        Assert.False(await addressBook.DeletePartyContactAsync(otherParty.RecId,contact.Location));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => addressBook.UpdatePartyAddressAsync(otherParty.RecId,address));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => addressBook.UpdatePartyContactAsync(otherParty.RecId,contact));
        Assert.Null(db.Database.CurrentTransaction);
        var locationsBefore = await db.Set<LogisticsLocation>().CountAsync();
        await Assert.ThrowsAsync<ArgumentException>(() => addressBook.CreatePartyAddressAsync(party.RecId,new() { CountryRegionId = "UNKNOWN" }));
        db.ChangeTracker.Clear();
        Assert.Equal(locationsBefore,await db.Set<LogisticsLocation>().CountAsync());
        Assert.Null(db.Database.CurrentTransaction);
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
            Addresses = [new() { Street = "Company street", CountryRegionId = "SAU", Primary = true }],
            Contacts = [new() { Type = "Phone", Number = "0501112222", Primary = true }] };
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
        Assert.Equal("0501112222",Assert.Single(dto.Contacts).Number);
        var query = sp.GetRequiredService<IAddressBookQueryService>();
        var contact = Assert.Single(await query.GetPartyContactsAsync(party));
        Assert.True(await sp.GetRequiredService<IGlobalAddressBookService>().DeletePartyContactAsync(party,contact.Location));
        Assert.Single(await query.GetPartyAddressesAsync(party));
    }
    private static async Task CheckWorkersAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var workers = sp.GetRequiredService<IHcmWorkerService>();
        var showrooms = sp.GetRequiredService<IHcmShowroomService>();
        var existing = await db.Set<HcmWorker>().SingleAsync(x => x.PersonnelNumber == "FNV2-BUYER");
        var input = new HcmWorkerDto { DataAreaId = "dat", IsActive = true, PersonnelNumber = "FT-WORKER", Name = "Service worker", NameAlias = "Worker",
            GenderId = existing.GenderId, NationalityId = existing.NationalityId, HireDate = new(2026,1,1) };
        var worker = await workers.CreateFromDtoAsync(input);
        Assert.True(worker.Person > 0); Assert.NotEqual(existing.Person,worker.Person);
        input.Name = "Updated worker"; input.RecId = long.MaxValue; input.Person = long.MaxValue;
        var changed = await workers.UpdateFromDtoAsync(worker.RecId.ToString(),input);
        Assert.Equal(worker.RecId,changed!.RecId); Assert.Equal(worker.Person,changed.Person);
        Assert.Equal("Updated worker",changed.Name);
        Assert.Contains(await workers.GetWorkerListAsync(), x => x.RecId == worker.RecId);
        Assert.Contains((await workers.GetWorkerLookupAsync(1,10,"FT-WORKER",worker.RecId)).Data,x => x.RecId == worker.RecId);
        var showroom = await showrooms.CreateFromDtoAsync(new() { DataAreaId = "dat", IsActive = true, PersonnelNumber = "FT-SHOW", Name = "Service showroom" });
        Assert.True(showroom.Party > 0);
        var changedShowroom = await showrooms.UpdateFromDtoAsync(showroom.RecId.ToString(),new() { DataAreaId = "dat", IsActive = true, RecId = long.MaxValue, Party = long.MaxValue, PersonnelNumber = "FT-SHOW", Name = "Updated showroom" });
        Assert.Equal(showroom.RecId,changedShowroom!.RecId); Assert.Equal(showroom.Party,changedShowroom.Party);
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
        var input = new TaxTableDto { TaxCode = "FT-VAT", TaxName = "Test rate", TaxPeriod = "Monthly", TaxAccountGroup = "APV2", TaxCurrencyCode = "SAR", TaxUnit = "EA", TaxValue = 10m };
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
        // A referenced master cannot be deleted while its rate rows still depend on it.
        await Assert.ThrowsAsync<DbUpdateException>(() => service.DeleteByCodeAsync("FT-VAT"));
        db.ChangeTracker.Clear();
        Assert.NotNull(await service.GetDetailAsync("FT-VAT"));
    }
    private static async Task CheckAccountStructureAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IAccountStructureService>();
        var created = await service.CreateAsync(new("FT-STRUCT","Service test",[new("MainAccount","281099")]),default);
        Assert.Null(created.Error); Assert.True(created.Result!.IsDraft);
        var activated = await service.ActivateAsync("FT-STRUCT",default);
        Assert.Null(activated.Error); Assert.False(activated.Result!.IsDraft);
        var assigned = await service.AssignToLedgerAsync("FT-STRUCT",default);
        Assert.Null(assigned.Error); Assert.True(assigned.Result!.IsAssignedToLedger);
        Assert.Contains((await service.ListAsync(default)).Items!,x => x.Name == "FT-STRUCT");
        Assert.Null((await service.ActivateAsync("missing",default)).Result);
    }
    private static async Task CheckTaxPostingAsync(ApplicationDbContext db, IServiceProvider sp)
    {
        var examples = sp.GetRequiredService<FoundationTransactionExamples>();
        db.ChangeTracker.Clear();
        await examples.RunAsync(new(2026,"foundation-services-test"));
        Assert.Equal(435m,(await db.Set<TaxTrans>().ToListAsync()).Sum(x => x.TaxAmount));
        Assert.Equal(52.50m,Assert.Single(await db.Set<TaxJournalTrans>().ToListAsync()).TaxAmount);
        Assert.Equal(-3335m,(await db.Set<BankAccountTrans>().ToListAsync()).Sum(x => x.AmountCur));
        var stock = await db.Set<InventSum>().Where(x => x.ItemId == "APV2-STOCK").ToListAsync();
        Assert.Equal(100m,stock.Sum(x => x.PhysicalInvent)); Assert.Equal(2500m,stock.Sum(x => x.PostedValue));
        var counts = (await db.Set<GeneralJournalAccountEntry>().CountAsync(),await db.Set<TaxTrans>().CountAsync());
        db.ChangeTracker.Clear();
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
        var structure = await db.Set<DimensionHierarchy>().SingleAsync(x => x.Name == "FT-STRUCT");
        var dimension = await new LedgerDimensionResolver(db).ResolveAsync("dat",account,[],structure.RecId,default);
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
        Assert.Equal(0m,entries.Where(x => x.LedgerDimension == group.TaxIncomingLedgerDimension).Sum(x => x.AccountingCurrencyAmount));
        var afterSettlement = await db.Set<InventSum>().AsNoTracking().Where(x => x.ItemId == "APV2-STOCK").ToListAsync();
        Assert.Equal(100m,afterSettlement.Sum(x => x.PhysicalInvent));
        Assert.Equal(2500m,afterSettlement.Sum(x => x.PostedValue));
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
