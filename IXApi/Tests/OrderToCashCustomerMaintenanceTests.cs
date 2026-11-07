using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

public sealed partial class AccountsReceivableCycleStageTests
{
    [SqlServerPhaseOneFact]
    public async Task Order_to_cash_customer_creation_updates_address_and_rolls_back_invalid_changes_SQLServer()
    {
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER"),
            InitialCatalog = "IXApi_OTCCustomer_" + Guid.NewGuid().ToString("N"),
            IntegratedSecurity = true, TrustServerCertificate = true
        };
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options;
        await using var db = new ApplicationDbContext(options);
        var audit = new OtcAudit();
        var documents = new List<object>();
        audit.FailureEvidence = () => new { documents };
        try
        {
            await db.Database.EnsureCreatedAsync();
            using var provider = CycleServices(db);
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            await services.GetRequiredService<AccountsReceivableCycleSeeder>().PrepareAsync(new(2026,"otc-customer"));
            var work = services.GetRequiredService<IUnitOfWork>();
            var actor = new ArCycleIdentity();
            var numbers = services.GetRequiredService<ISysNumberSequenceService>();
            // Master seeders use stable keys; these sequences exercise interactive maintenance only.
            foreach (var name in new[] { "Customer", "DirPartyTable", "LogisticsLocation" })
                await numbers.AddAsync(new SysNumberSequence { DataAreaId=Company, NumberSequence=name,
                    Txt="Customer maintenance acceptance", Format="CM-######", AnnotatedFormat="CM-{SEQ}",
                    Lowest=1, Highest=999999, NextRec=1, InUse=1 });
            var customers = new CustomerService(work,actor);
            var quick = new CustomerQuickService(customers,new PartyService(work,actor,numbers),numbers,work,actor,
                new LocationService(work,actor,numbers),new PostalAddressService(work,actor),new PartyLocationService(work,actor));
            CustomerQuickCreateDto Input(string name,string? street,string? country="SA") => new()
            {
                Name=name, NameAlias="Customer alias", CustGroupId="ARV2", CurrencyCode="SAR",
                PaymTermId="APV2-N30", PaymModeId="ARV2", TaxGroupId="ARV2", CountryRegionId=country,
                Street=street, InventSiteId="APV2-SITE", InventLocationId="APV2-WH", InvoiceAccount=Customer
            };
            async Task<LogisticsPostalAddress> Address(long party)
            {
                var location = await db.Set<DirPartyLocation>().Where(x => x.Party==party && x.IsPostalAddress==NoYes.Yes && x.IsPrimary==NoYes.Yes)
                    .Select(x => x.Location).SingleAsync();
                return await db.Set<LogisticsPostalAddress>().AsNoTracking().SingleAsync(x => x.Location==location);
            }
            await audit.Stage(["MD-01"],"Customer maintenance persists names, primary address and setup atomically",async () =>
            {
                var before = await CountsAsync(db);
                var created = await quick.QuickCreateAsync(Input(" Original customer "," First street "));
                var id = created.Customer.RecId;
                var partyId = created.Party.RecId;
                var first = await Address(partyId);
                Assert.Equal("First street",first.Street);
                Assert.Equal("SA",first.CountryRegionId);
                db.ChangeTracker.Clear();
                var update = await quick.QuickUpdateAsync(id,Input(" Updated customer "," Second street "));
                Assert.Equal(CustomerQuickUpdateStatus.Updated,update.Status);
                db.ChangeTracker.Clear();
                var address = await Address(partyId);
                documents.Add(new { id,partyId,address.RecId,address.Location,address.Street,address.CountryRegionId });
                Assert.Equal("Second street",address.Street);
                Assert.Equal(first.RecId,address.RecId);
                Assert.Equal(first.Location,address.Location);
                Assert.Equal(first.ValidFrom,address.ValidFrom);
                Assert.Equal(first.ValidTo,address.ValidTo);
                Assert.Equal(first.State,address.State);
                Assert.Equal(first.County,address.County);
                Assert.Equal(first.CityRecId,address.CityRecId);
                Assert.Equal(first.ZipCodeRecId,address.ZipCodeRecId);
                var customer = await db.Set<CustTable>().AsNoTracking().SingleAsync(x => x.RecId==id);
                var party = await db.Set<DirPartyTable>().AsNoTracking().SingleAsync(x => x.RecId==partyId);
                Assert.Equal("Updated customer",party.Name);
                Assert.Equal("Customer alias",party.NameAlias);
                Assert.Equal("ARV2",customer.CustGroupId);
                Assert.Equal("APV2-N30",customer.PaymTermId);
                Assert.Equal("ARV2",customer.PaymModeId);
                Assert.Equal(Customer,customer.InvoiceAccount);
                Assert.Equal("APV2-WH",customer.InventLocationId);
                var defaults = await customers.GetSalesOrderDefaultsAsync(customer.AccountNum,default);
                Assert.Contains("Second street",defaults!.Address);

                // Omitted street preserves the address; adding one later creates a primary link once.
                await quick.QuickUpdateAsync(id,Input("Updated customer",null));
                Assert.Equal("Second street",(await Address(partyId)).Street);
                var noAddress = await quick.QuickCreateAsync(Input("No address",null));
                await quick.QuickUpdateAsync(noAddress.Customer.RecId,Input("New address","Third street"));
                Assert.Equal("Third street",(await Address(noAddress.Party.RecId)).Street);
                db.ChangeTracker.Clear();

                var primary = await db.Set<DirPartyLocation>().SingleAsync(x => x.Party==partyId && x.Location==first.Location);
                Assert.Equal(NoYes.Yes,primary.IsLocationOwner);
                primary.IsLocationOwner = NoYes.No;
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
                var notOwner = await Assert.ThrowsAsync<ArgumentException>(() => quick.QuickUpdateAsync(id,Input("Must not change","Not owned")));
                Assert.Contains("location owner",notOwner.Message);
                Assert.Empty(db.ChangeTracker.Entries());
                Assert.Equal("Second street",(await Address(partyId)).Street);
                primary = await db.Set<DirPartyLocation>().SingleAsync(x => x.Party==partyId && x.Location==first.Location);
                primary.IsLocationOwner = NoYes.Yes;
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();

                // A geography failure must not save the new name or leave tracked mutations for a later save.
                var failedInput = Input("Must roll back","Invalid street","ZZZ");
                await Assert.ThrowsAsync<ArgumentException>(() => quick.QuickUpdateAsync(id,failedInput));
                Assert.Empty(db.ChangeTracker.Entries());
                await db.SaveChangesAsync();
                Assert.Equal("Updated customer",(await db.Set<DirPartyTable>().AsNoTracking().SingleAsync(x => x.RecId==partyId)).Name);
                Assert.Equal("SA",(await db.Set<CustTable>().AsNoTracking().SingleAsync(x => x.RecId==id)).CountryRegionId);
                Assert.Equal("Second street",(await Address(partyId)).Street);
                var partyCount = await db.Set<DirPartyTable>().CountAsync();
                var customerCount = await db.Set<CustTable>().CountAsync();
                var locationCount = await db.Set<LogisticsLocation>().CountAsync();
                var counters = await db.Set<SysNumberSequence>().AsNoTracking().OrderBy(x => x.RecId).Select(x => x.NextRec).ToListAsync();
                await Assert.ThrowsAsync<ArgumentException>(() => quick.QuickCreateAsync(failedInput));
                Assert.Empty(db.ChangeTracker.Entries());
                await db.SaveChangesAsync();
                Assert.Equal(partyCount,await db.Set<DirPartyTable>().CountAsync());
                Assert.Equal(customerCount,await db.Set<CustTable>().CountAsync());
                Assert.Equal(locationCount,await db.Set<LogisticsLocation>().CountAsync());
                Assert.Equal(counters,await db.Set<SysNumberSequence>().AsNoTracking().OrderBy(x => x.RecId).Select(x => x.NextRec).ToListAsync());
                var foreignActor = new ArCycleIdentity("other-company-user","oth");
                await using (var foreignDb = new ApplicationDbContext(options,companyContext:foreignActor))
                {
                    using var foreignWork = new UnitOfWork(foreignDb);
                    var foreignNumbers = new SysNumberSequenceService(foreignWork,foreignActor);
                    var foreignQuick = new CustomerQuickService(new CustomerService(foreignWork,foreignActor),
                        new PartyService(foreignWork,foreignActor,foreignNumbers),foreignNumbers,foreignWork,foreignActor,
                        new LocationService(foreignWork,foreignActor,foreignNumbers),new PostalAddressService(foreignWork,foreignActor),
                        new PartyLocationService(foreignWork,foreignActor));
                    var foreignResult = await foreignQuick.QuickUpdateAsync(id,Input("Cross company","Must not change"));
                    Assert.Equal(CustomerQuickUpdateStatus.CustomerNotFound,foreignResult.Status);
                }
                Assert.Equal("Second street",(await Address(partyId)).Street);
                Assert.Equal(before,await CountsAsync(db)); // Master maintenance creates no financial/inventory transactions.
                return new { documents,customer.AccountNum,customer.CustGroupId,customer.PaymTermId,customer.InvoiceAccount,
                    rollbackVerified=true, financialEffects=false };
            });
            audit.Complete();
        }
        finally
        {
            await using var cleanup = new ApplicationDbContext(options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
