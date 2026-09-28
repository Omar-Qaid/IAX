using IAX.IXApi.Modules.Organization.DocumentManagement.Storage;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Requests;

internal static class WorkflowSubmissionTransaction
{
    internal static async Task<T> RunAsync<T>(DbContext db, IFileStorageProvider storage,
        Func<List<string>, Task<T>> submit, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction != null)
            throw new InvalidOperationException("Request submission must own its transaction.");
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            var trackedBefore = db.ChangeTracker.Entries().Select(entry => entry.Entity).ToHashSet();
            var paths = new List<string>();
            var commitStarted = false;
            await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
            try
            {
                var result = await submit(paths);
                commitStarted = true;
                await transaction.CommitAsync(ct);
                return result;
            }
            catch (Exception failure)
            {
                // A connection failure during COMMIT does not prove rollback. Deleting files
                // or automatically retrying here could damage a committed request or duplicate it.
                if (commitStarted)
                    throw new InvalidOperationException(
                        "The submission commit outcome is unknown. Check the request list before retrying.", failure);
                var cleanupErrors = new List<Exception>();
                try { await transaction.RollbackAsync(CancellationToken.None); }
                catch (Exception error) { cleanupErrors.Add(error); }
                // Storage is outside the database transaction. Attempt every cleanup even
                // after cancellation or a cleanup failure, and preserve the original error.
                foreach (var path in paths)
                {
                    try { await storage.DeleteAsync(path, CancellationToken.None); }
                    catch (Exception error) { cleanupErrors.Add(error); }
                }
                foreach (var entry in db.ChangeTracker.Entries().ToList())
                    if (!trackedBefore.Contains(entry.Entity)) entry.State = EntityState.Detached;
                if (cleanupErrors.Count > 0)
                    throw new AggregateException("Submission failed and attachment cleanup was incomplete.", [failure, .. cleanupErrors]);
                throw;
            }
        });
    }
}
