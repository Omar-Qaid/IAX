using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;

/// <summary>Check mapped storage limits, including on providers that do not enforce nvarchar length.</summary>
public sealed class SeedModelIntegrityValidator : ISeedValidator
{
    public Task ValidateAsync(SeedContext context, CancellationToken cancellationToken)
    {
        foreach (var entry in context.Db.ChangeTracker.Entries())
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var property in entry.Properties)
                if (property.CurrentValue is string value && property.Metadata.GetMaxLength() is int max && value.Length > max)
                    throw new InvalidOperationException($"{entry.Metadata.ClrType.Name}.{property.Metadata.Name} exceeds its mapped length {max}.");
        }
        return Task.CompletedTask;
    }
}
