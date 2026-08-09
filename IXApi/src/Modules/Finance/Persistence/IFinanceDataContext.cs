using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IAX.IXApi.Modules.Finance.Persistence;

public interface IFinanceDataContext
{
    DatabaseFacade Database { get; }
    DbSet<TaxData> TaxData { get; }
    DbSet<TaxGroupHeading> TaxGroupHeadings { get; }
    DbSet<TaxGroupData> TaxGroupDatas { get; }
    DbSet<TaxOnItem> TaxOnItems { get; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
