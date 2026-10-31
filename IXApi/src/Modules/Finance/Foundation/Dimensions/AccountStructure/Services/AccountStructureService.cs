using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;

/// <summary>
/// The company-scoped API over <see cref="AccountStructureBuilder"/>, which holds the behaviour.
/// </summary>
public sealed class AccountStructureService(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IAccountStructureService
{
    private DbContext Db => unitOfWork.Context;
    private AccountStructureBuilder Builder => new(Db);

    public async Task<(AccountStructureDto? Result, string? Error)> CreateAsync(
        AccountStructureCreateDto input, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var (structure, error) = await Builder.DefineAsync(area, input, ct);
        return structure == null ? (null, error) : (await Builder.DescribeAsync(area, structure, ct), null);
    }

    public async Task<(AccountStructureDto? Result, string? Error)> ActivateAsync(string name, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var (structure, error) = await Builder.ActivateAsync(area, name, ct);
        return structure == null ? (null, error) : (await Builder.DescribeAsync(area, structure, ct), null);
    }

    public async Task<(AccountStructureDto? Result, string? Error)> AssignToLedgerAsync(string name, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var (structure, error) = await Builder.AssignToLedgerAsync(area, name, ct);
        return structure == null ? (null, error) : (await Builder.DescribeAsync(area, structure, ct), null);
    }

    public async Task<(IReadOnlyList<AccountStructureDto>? Items, string? Error)> ListAsync(CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var builder = Builder;
        var structures = await Db.Set<DimensionHierarchy>().AsNoTracking()
            .Where(x => x.DataAreaId == area
                && x.StructureType == DimensionHierarchyStructureType.AccountStructure)
            .OrderBy(x => x.Name).ToListAsync(ct);
        var described = new List<AccountStructureDto>();
        foreach (var structure in structures) described.Add(await builder.DescribeAsync(area, structure, ct));
        return (described, null);
    }
}
