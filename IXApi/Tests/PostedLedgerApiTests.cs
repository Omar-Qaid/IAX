using IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

public class PostedLedgerApiTests
{
    [Fact]
    public async Task Posted_accounting_services_reject_all_generic_mutations_before_accessing_storage()
    {
        await RejectWrites<GeneralJournalEntry>(new GeneralJournalEntryService(null!));
        await RejectWrites<GeneralJournalAccountEntry>(new GeneralJournalAccountEntryService(null!));
        await RejectWrites<SubledgerVoucherGeneralJournalEntry>(new SubledgerVoucherGeneralJournalEntryService(null!));

        static async Task RejectWrites<T>(IBaseService<T> service) where T : class, new()
        {
            var entity = new T();
            await Assert.ThrowsAsync<NotSupportedException>(() => service.AddAsync(entity));
            await Assert.ThrowsAsync<NotSupportedException>(() => service.AddRangeAsync([entity]));
            await Assert.ThrowsAsync<NotSupportedException>(() => service.UpdateAsync(entity));
            await Assert.ThrowsAsync<NotSupportedException>(() => service.UpdateRangeAsync([entity]));
            await Assert.ThrowsAsync<NotSupportedException>(() => service.UpdateWithDetailsAsync<object>(entity, _ => Array.Empty<object>()));
            await Assert.ThrowsAsync<NotSupportedException>(() => service.RemoveAsync(entity));
            await Assert.ThrowsAsync<NotSupportedException>(() => service.RemoveRangeAsync([entity]));
        }
    }

    [Fact]
    public void Posted_accounting_controllers_expose_reads_but_no_generic_writes()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore().AddApplicationPart(typeof(GeneralJournalEntryController).Assembly);
        using var provider = services.BuildServiceProvider();
        var actions = provider.GetRequiredService<IActionDescriptorCollectionProvider>()
            .ActionDescriptors.Items.OfType<ControllerActionDescriptor>().ToList();
        foreach (var controller in new[] { typeof(GeneralJournalEntryController),
            typeof(GeneralJournalAccountEntryController), typeof(SubledgerVoucherGeneralJournalEntryController) })
        {
            var names = actions.Where(x => x.ControllerTypeInfo.AsType() == controller)
                .Select(x => x.ActionName).ToHashSet();
            Assert.Contains("GetAll", names);
            Assert.Contains("GetById", names);
            Assert.Contains("GetPaged", names);
            foreach (var write in new[] { "Create", "Update", "Delete", "CreateRange", "UpdateRange", "DeleteRange" })
                Assert.DoesNotContain(write, names);
        }
    }
}
