using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

/// <summary>Owns the two-save tax code/rate operation, following the existing submission transaction pattern.</summary>
internal static class TaxCodeSaveTransaction
{
    public static async Task<T> RunAsync<T>(DbContext db, Func<Task<T>> save, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction != null)
            throw new InvalidOperationException("Tax code and rate saving must own its transaction.");
        if (db.ChangeTracker.HasChanges())
            throw new InvalidOperationException("Save or discard pending changes before saving a tax code and rate.");
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var before = db.ChangeTracker.Entries().ToDictionary(x => x.Entity,
                x => (Current: x.CurrentValues.Clone(), Original: x.OriginalValues.Clone()));
            await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
            var committing = false;
            try
            {
                var result = await save();
                committing = true;
                await transaction.CommitAsync(ct);
                return result;
            }
            catch (Exception failure)
            {
                if (committing)
                    throw new InvalidOperationException("The tax save commit outcome is unknown. Reload the code and rate before retrying.", failure);
                await transaction.RollbackAsync(CancellationToken.None);
                foreach (var entry in db.ChangeTracker.Entries().ToList())
                {
                    if (!before.TryGetValue(entry.Entity, out var values)) entry.State = EntityState.Detached;
                    else
                    {
                        entry.CurrentValues.SetValues(values.Current);
                        entry.OriginalValues.SetValues(values.Original);
                        entry.State = EntityState.Unchanged;
                    }
                }
                throw;
            }
        });
    }
}
