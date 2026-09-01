using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>
/// Seeds reusable workflow master data and a catalog of example request forms.
/// Runtime requests and history are intentionally not seeded. Add new simple
/// forms to the form catalog instead of duplicating seeding logic.
/// </summary>
public sealed partial class WfProcessSeedData : ISeeder
{
    private const string ProcessCode = "PAYMENT_REQUEST";
    private const string RequiredRule = "<Validation><Required>true</Required></Validation>";

    public async Task SeedAsync(
        ApplicationDbContext db,
        RoleManager<AspNetRole> roles,
        UserManager<AspNetUser> users,
        CancellationToken ct)
    {
        _ = roles;
        var owner = (await users.FindByNameAsync("sys"))?.Id ?? "sys";

        await SeedMasterDataAsync(db, owner, ct);
        await SeedPaymentRequestExampleAsync(db, owner, ct);
        foreach (var form in AdditionalForms())
            await SeedFormAsync(db, form, owner, ct);
    }
}
