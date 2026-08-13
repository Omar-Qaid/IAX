using System.Data;
using System.Text;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public class SysNumberSequenceService : BaseService<SysNumberSequence>, ISysNumberSequenceService
    {
        public SysNumberSequenceService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }

        public async Task<NextSequenceResultDto> NextAsync(string entityName, string? tenantId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("EntityName (NumberSequence) is required", nameof(entityName));

            var db = _unitOfWork.Context;
            var strategy = db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                var tx = db.Database.CurrentTransaction == null 
                    ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken) 
                    : null;

                try
                {
                    var seq = await db.Set<SysNumberSequence>()
                        .FirstOrDefaultAsync(s => s.NumberSequence == entityName, cancellationToken)
                        ?? throw new InvalidOperationException($"No number sequence configured for entity '{entityName}'.");

                    ApplyResetIfDue(seq);

                    if (seq.NextRec > seq.Highest)
                        throw new InvalidOperationException($"Number sequence '{seq.NumberSequence}' has exceeded Highest ({seq.Highest}).");

                    var current = seq.NextRec ?? 1;
                    seq.NextRec = current + 1;

                    await db.SaveChangesAsync(cancellationToken);
                    if (tx != null) await tx.CommitAsync(cancellationToken);

                    return new NextSequenceResultDto
                    {
                        EntityName = entityName,
                        Value = current,
                        Code = FormatCode(seq, current)
                    };
                }
                catch
                {
                    if (tx != null) await tx.RollbackAsync(cancellationToken);
                    throw;
                }
                finally
                {
                    if (tx != null) await tx.DisposeAsync();
                }
            });
        }

        public async Task<NextSequenceResultDto?> PeekAsync(string entityName, string? tenantId = null, CancellationToken cancellationToken = default)
        {
            var seq = await _repository.GetQueryable().AsNoTracking()
                .FirstOrDefaultAsync(s => s.NumberSequence == entityName, cancellationToken);
            if (seq == null) return null;

            var preview = seq;
            ApplyResetIfDue(preview); 
            return new NextSequenceResultDto
            {
                EntityName = entityName,
                Value = preview.NextRec ?? 1,
                Code = FormatCode(preview, preview.NextRec ?? 1)
            };
        }

        public async Task<SysNumberSequence> ResetAsync(int id, long? nextValue, CancellationToken cancellationToken = default)
        {
            var seq = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Sequence not found");
            seq.NextRec = (int?)(nextValue ?? seq.Lowest) ?? 1;
            seq.LatestCleanDateTime = DateTime.UtcNow;
            return await UpdateAsync(seq, cancellationToken);
        }

        private static void ApplyResetIfDue(SysNumberSequence seq)
        {
            if (seq.Cyclic != 1) return;
            var now = DateTime.UtcNow;
            var last = seq.LatestCleanDateTime ?? DateTime.MinValue;
            // Simplified reset cycle (assume daily if cyclic is true for now)
            if (last.Date != now.Date)
            {
                seq.NextRec = seq.Lowest ?? 1;
                seq.LatestCleanDateTime = now;
            }
        }

        public static string FormatCode(SysNumberSequence seq, long value)
        {
            var now = DateTime.UtcNow;
            var formatPattern = string.IsNullOrWhiteSpace(seq.AnnotatedFormat) 
                ? (string.IsNullOrWhiteSpace(seq.Format) ? "{SEQ}" : seq.Format) 
                : seq.AnnotatedFormat;
                
            var seqStr = value.ToString().PadLeft(5, '0'); // default pad

            var sb = new StringBuilder(formatPattern);
            sb.Replace("{SEQ}", seqStr);
            sb.Replace("{YYYY}", now.Year.ToString("D4"));
            sb.Replace("{YY}", (now.Year % 100).ToString("D2"));
            sb.Replace("{MM}", now.Month.ToString("D2"));
            sb.Replace("{DD}", now.Day.ToString("D2"));
            return sb.ToString();
        }
    }
}

