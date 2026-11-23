using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Services;
using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

namespace IAX.IXApi.Tests;

public sealed partial class AccountsPayablePhaseOneSeedTests
{
    /// <summary>
    /// Accounts payable service by service: every <see cref="BaseService{T}"/> in the module gets its
    /// read/create/update/delete contract checked, so the setup and master-data services that the
    /// cycle stages never touch are verified too. Each service is one recorded stage.
    /// </summary>
    [Fact]
    public async Task AP_services_one_by_one_SQLite()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;

        var runId = Guid.NewGuid().ToString("N");
        var failures = new List<string>();
        void Record(string stage, string name, string status, string? detail = null) =>
            DetailedApRecord(runId, "SQLite", stage, name, status, detail);
        Record("run", "فحص خدمات الحسابات الدائنة خدمة بخدمة", "STARTED");

        async Task Stage(string stage, string name, Func<Task> action)
        {
            Record(stage, name, "RUNNING");
            try { await action(); Record(stage, name, "PASSED"); }
            catch (Exception ex)
            {
                var error = ex is TargetInvocationException { InnerException: not null } t ? t.InnerException! : ex;
                failures.Add(stage + ": " + error.Message); Record(stage, name, "FAILED", error.ToString());
            }
            finally { db.ChangeTracker.Clear(); }
        }

        var crudServices = typeof(IAX.IXApi.Modules.Finance.FinanceModule).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.Namespace?.StartsWith("IAX.IXApi.Modules.Finance.AccountsPayable") == true
                && t.BaseType is { IsGenericType: true } b && b.GetGenericTypeDefinition() == typeof(BaseService<>))
            .OrderBy(t => t.Name, StringComparer.Ordinal).ToArray();
        Assert.NotEmpty(crudServices);

        var actor = new TestIdentity("cycle-test");
        var services = new ServiceCollection().AddLogging().AddSingleton<ApplicationDbContext>(db).AddSingleton(db)
            .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
            .AddAccountsPayableTransactionExamples();
        foreach (var type in crudServices)
        {
            services.AddScoped(type);
            services.AddScoped(typeof(IBaseService<>).MakeGenericType(type.BaseType!.GetGenericArguments()[0]),
                p => p.GetRequiredService(type));
        }
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        // The cycle seeder supplies the vendors, groups, payment methods, parameters and posting
        // profiles the read paths need; running one scenario adds the vendor transactions.
        await sp.GetRequiredService<AccountsPayableCycleSeeder>().PrepareAsync(new(2026, "ap-service-test"));
        await sp.GetRequiredService<AccountsPayableTransactionExamples>().RunAsync(new(2026, "ap-service-test", ["01"]));
        db.ChangeTracker.Clear();

        // Posting a pending invoice consumes its VendInvoiceInfoTable row, so the cycle leaves none
        // behind. Hold one through the pending-invoice service itself and leave it unposted, so the
        // read paths of that service have a row to work with.
        await Stage("fixture", "فاتورة معلقة غير مرحلة لفحص خدمة الفواتير المعلقة", async () =>
        {
            var date = new DateTime(2026, 4, 15);
            var account = $"APV2-{db.GetDataAreaId()}-LOCAL";
            var created = await sp.GetRequiredService<IPurchOrderService>().CreateAsync(
                new(account, null, "SAR", date, "APV2-SITE", "APV2-WH", "AP-SERVICE-PENDING", date.AddDays(-1)), default);
            Assert.True(created.StatusCode == 200, created.Message);
            var order = (PurchTable)created.Data!;
            Assert.True((await sp.GetRequiredService<IPurchOrderService>().AddLineAsync(order.RecId,
                new("APV2-STOCK", 10m, "EA", 25m, 1m, "APV2-SITE", "APV2-WH", date, null), default)).StatusCode == 200);
            var line = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == order.PurchId);
            Assert.True((await sp.GetRequiredService<IPurchConfirmationService>().ConfirmAsync(order.RecId, default, date)).StatusCode == 200);
            Assert.True((await sp.GetRequiredService<IProductReceiptService>().PostAsync(order.RecId,
                new("AP-SERVICE-R1", date.AddDays(1), [new(line.RecId, 10m)]), default)).StatusCode == 200);
            Assert.True((await sp.GetRequiredService<IVendInvoiceInfoService>().CreateFromOrderAsync(order.RecId,
                new("AP-SERVICE-I1", date.AddDays(2), date.AddDays(2), Lines: [new(line.RecId, 10m)]), default)).StatusCode == 200);
            Assert.NotEmpty(await db.Set<VendInvoiceInfoTable>().Where(x => x.DataAreaId == "dat").ToListAsync());
        });

        // PurchInventorySupplyService is a process service, not CRUD: a stocked purchase order line
        // must create its inventory origin, the purchase-line relation, and an Ordered receipt
        // transaction, and raise InventSum.Ordered as expected supply. The cycle runs it on every
        // stocked line but never checked what it produced.
        await Stage("PurchInventorySupplyService", "طلب الشراء ينشئ عرضاً مخزنياً مطلوباً", async () =>
        {
            var date = new DateTime(2026, 4, 15);
            var account = $"APV2-{db.GetDataAreaId()}-LOCAL";
            var created = await sp.GetRequiredService<IPurchOrderService>().CreateAsync(
                new(account, null, "SAR", date, "APV2-SITE", "APV2-WH", "AP-SERVICE-SUPPLY", date.AddDays(-1)), default);
            Assert.True(created.StatusCode == 200, created.Message);
            var order = (PurchTable)created.Data!;
            var before = (await db.Set<InventSum>().Where(x => x.DataAreaId == "dat" && x.ItemId == "APV2-STOCK")
                .ToListAsync()).Sum(x => x.Ordered);
            Assert.True((await sp.GetRequiredService<IPurchOrderService>().AddLineAsync(order.RecId,
                new("APV2-STOCK", 40m, "EA", 25m, 1m, "APV2-SITE", "APV2-WH", date, null), default)).StatusCode == 200);
            db.ChangeTracker.Clear();

            var line = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == order.PurchId);
            Assert.False(string.IsNullOrWhiteSpace(line.InventTransId));
            var origin = await db.Set<InventTransOrigin>().SingleAsync(x => x.DataAreaId == "dat" && x.InventTransId == line.InventTransId);
            Assert.True(await db.Set<InventTransOriginPurchLine>().AnyAsync(x => x.InventTransOrigin == origin.RecId
                && x.PurchLineInventTransId == line.InventTransId));
            var ordered = await db.Set<InventTrans>().SingleAsync(x => x.DataAreaId == "dat" && x.InventTransOrigin == origin.RecId);
            Assert.Equal(40m, ordered.Qty);
            Assert.Equal(IAX.IXApi.Modules.Finance.Common.StatusReceipt.Ordered, ordered.StatusReceipt);
            Assert.Equal(IAX.IXApi.Modules.Finance.Common.StatusIssue.None, ordered.StatusIssue);
            var after = (await db.Set<InventSum>().Where(x => x.DataAreaId == "dat" && x.ItemId == "APV2-STOCK")
                .ToListAsync()).Sum(x => x.Ordered);
            Assert.Equal(before + 40m, after);
        });

        foreach (var type in crudServices)
        {
            var entity = type.BaseType!.GetGenericArguments()[0];
            Record(type.Name, entity.Name + ": read/create/update/delete", "RUNNING");
            try
            {
                var check = typeof(FoundationServiceStageTests).GetMethod(nameof(FoundationServiceStageTests.CheckCrudAsync),
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!.MakeGenericMethod(entity);
                await (Task)check.Invoke(null, [db, sp.GetRequiredService(typeof(IBaseService<>).MakeGenericType(entity)), sp])!;
                Record(type.Name, entity.Name + ": read/create/update/delete", "PASSED");
            }
            catch (Exception ex)
            {
                var error = ex is TargetInvocationException { InnerException: not null } t ? t.InnerException! : ex;
                failures.Add(type.Name + ": " + error.Message);
                Record(type.Name, entity.Name + ": read/create/update/delete", "FAILED", error.ToString());
            }
            finally { db.ChangeTracker.Clear(); }
        }
        Record("run", "فحص خدمات الحسابات الدائنة خدمة بخدمة", failures.Count == 0 ? "PASSED" : "FAILED");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public async Task Detailed_AP_stages_SQLite()
    {
        await using var database = await SqliteTestDatabase.CreateAsync(createSchema: false);
        await RunDetailedApStagesAsync(database.Context, "SQLite", database.CreateSchemaAsync);
    }

    [SqlServerFact]
    public async Task Detailed_AP_stages_SQLServer()
    {
        var connection = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(TestEnvironment.SqlServerConnectionString("IXApi_APStage_" + Guid.NewGuid().ToString("N")));
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options);
        var created = false;
        var completed = false;
        var runId = Guid.NewGuid().ToString("N");
        try
        {
            await RunDetailedApStagesAsync(db, "SQL Server", async () =>
            {
                created = await db.Database.EnsureCreatedAsync();
                Assert.True(created);
                var migrations = db.GetService<IMigrationsAssembly>();
                var generator = db.GetService<IMigrationsSqlGenerator>();
                foreach (var migrationId in new[]
                {
                    "20261108221500_AddVendPackingSlipVersion",
                    "20261108221600_AddVendPackingSlipTransHistory",
                    "20261109055928_AddPurchaseOrderPrepaymentStorage",
                    "20261109071429_AddVendorAdvancePostingProfile"
                })
                {
                    var migration = migrations.CreateMigration(migrations.Migrations[migrationId], db.Database.ProviderName!);
                    foreach (var command in generator.Generate(migration.DownOperations))
                        await db.Database.ExecuteSqlRawAsync(command.CommandText);
                    foreach (var command in generator.Generate(migration.UpOperations))
                        await db.Database.ExecuteSqlRawAsync(command.CommandText);
                }
            }, runId);
            completed = true;
        }
        finally
        {
            if (created)
            {
                DetailedApRecord(runId,"SQL Server","cleanup","تنظيف قاعدة الاختبار المؤقتة","RUNNING",connection.InitialCatalog);
                try
                {
                    // UnitOfWork owns/disposes its context; cleanup uses a fresh context.
                    await using var cleanup = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options);
                    Assert.True(await cleanup.Database.EnsureDeletedAsync());
                    DetailedApRecord(runId,"SQL Server","cleanup","تنظيف قاعدة الاختبار المؤقتة","PASSED",connection.InitialCatalog);
                    if (completed) DetailedApRecord(runId,"SQL Server","run","اختبار دورة الحسابات الدائنة","PASSED");
                }
                catch (Exception error)
                {
                    DetailedApRecord(runId,"SQL Server","cleanup","تنظيف قاعدة الاختبار المؤقتة","FAILED",error.ToString());
                    DetailedApRecord(runId,"SQL Server","run","اختبار دورة الحسابات الدائنة","FAILED",error.ToString());
                    throw;
                }
            }
        }
    }
}
