using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
/// <summary>Validate mapped references in the selected company, including relationships owned by other modules.</summary>
public sealed class FoundationIntegrityValidator : ISeedValidator
{
    private static readonly MethodInfo LoadMethod = typeof(FoundationIntegrityValidator).GetMethod(nameof(Load), BindingFlags.NonPublic | BindingFlags.Static)!;
    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        var schedule = await c.RequireAsync<PaymSched>(x => x.Name == "FNV2", ct);
        var lines = await c.Rows<PaymSchedLine>().Where(x => x.Name == schedule.Name).ToListAsync(ct);
        if (lines.Count != schedule.NumOfPayment || lines.Sum(x => x.Value) != 100m)
            throw new InvalidOperationException("Foundation payment schedule must allocate 100 percent across its installments.");
        var customer = await c.RequireAsync<CustTable>(x => x.AccountNum == $"FNV2-{c.Company}-CUSTOMER", ct);
        var dimensions = await c.RequireAsync<DimensionAttributeValueSet>(x => x.RecId == customer.DefaultDimension, ct);
        var term = await c.RequireAsync<PaymTerm>(x => x.PaymTermId == customer.PaymTermId, ct);
        if (term.PaymSched != schedule.Name || dimensions.DepartmentValue != "FNV2-FIN")
            throw new InvalidOperationException("Foundation customer schedule or default dimension differs from the fixture.");
        await c.RequireAsync<DlvMode>(x => x.Code == customer.DlvModeId, ct);
        foreach (var code in await c.Rows<TaxTable>().Where(x => x.TaxCode == "APV2-15").ToListAsync(ct))
            await c.RequireAsync<TaxPeriodHead>(x => x.TaxPeriod == code.TaxPeriod, ct);
        var cache = new Dictionary<IEntityType, object[]>();
        async Task<object[]> Rows(IEntityType type)
        {
            if (!cache.TryGetValue(type, out var rows))
            {
                rows = await (Task<object[]>)LoadMethod.MakeGenericMethod(type.ClrType).Invoke(null, [c, ct])!;
                cache[type] = rows;
            }
            return rows;
        }
        foreach (var type in c.Db.Model.GetEntityTypes().Where(x => FoundationTableCatalog.MasterTypes.Contains(x.ClrType.Name)))
        {
            var rows = await Rows(type);
            if (rows.Length == 0) throw new InvalidOperationException($"Foundation table {type.ClrType.Name} was not seeded in {c.Company}.");
            foreach (var fk in type.GetForeignKeys())
            {
                var parents = await Rows(fk.PrincipalEntityType);
                foreach (var row in rows)
                {
                    var key = fk.Properties.Select(p => p.PropertyInfo?.GetValue(row)).ToArray();
                    if (key.Any(x => x == null)) continue;
                    if (!parents.Any(parent => fk.PrincipalKey.Properties.Select(p => p.PropertyInfo?.GetValue(parent)).SequenceEqual(key)))
                        throw new InvalidOperationException($"Foundation reference {type.ClrType.Name} -> {fk.PrincipalEntityType.ClrType.Name} is missing or crosses company {c.Company}.");
                }
            }
        }
    }
    private static async Task<object[]> Load<T>(SeedContext c, CancellationToken ct) where T : class
    {
        var query = c.Db.Set<T>().IgnoreQueryFilters().AsNoTracking();
        if (c.Db.Model.FindEntityType(typeof(T))!.FindProperty("DataAreaId") != null)
            query = query.Where(x => EF.Property<string>(x, "DataAreaId") == c.Company);
        return (await query.ToListAsync(ct)).Cast<object>().ToArray();
    }
}
