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
    [ScopedService]
    public class SysNumberSequenceService : BaseService<SysNumberSequence>, ISysNumberSequenceService
    {
        public SysNumberSequenceService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }

        public async Task<NextSequenceResultDto> NextAsync(string entityName, string? tenantId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("EntityName is required", nameof(entityName));

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
                        .FirstOrDefaultAsync(s => s.EntityName == entityName && s.TenantId == tenantId, cancellationToken)
                        ?? throw new InvalidOperationException($"No number sequence configured for entity '{entityName}'.");

                    ApplyResetIfDue(seq);

                    if (seq.NextValue > seq.LargestValue)
                        throw new InvalidOperationException($"Number sequence '{seq.Code}' has exceeded LargestValue ({seq.LargestValue}).");

                    var current = seq.NextValue;
                    seq.NextValue = current + seq.Step;

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
                .FirstOrDefaultAsync(s => s.EntityName == entityName && s.TenantId == tenantId, cancellationToken);
            if (seq == null) return null;

            var preview = seq;
            ApplyResetIfDue(preview); // logical preview only — not persisted
            return new NextSequenceResultDto
            {
                EntityName = entityName,
                Value = preview.NextValue,
                Code = FormatCode(preview, preview.NextValue)
            };
        }

        public async Task<SysNumberSequence> ResetAsync(int id, long? nextValue, CancellationToken cancellationToken = default)
        {
            var seq = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Sequence not found");
            seq.NextValue = nextValue ?? seq.SmallestValue;
            seq.LastResetAt = DateTime.UtcNow;
            return await UpdateAsync(seq, cancellationToken);
        }

        private static void ApplyResetIfDue(SysNumberSequence seq)
        {
            if (seq.ResetCycle == SequenceResetCycle.Never) return;
            var now = DateTime.UtcNow;
            var last = seq.LastResetAt ?? DateTime.MinValue;
            bool due = seq.ResetCycle switch
            {
                SequenceResetCycle.Yearly => last.Year != now.Year,
                SequenceResetCycle.Monthly => last.Year != now.Year || last.Month != now.Month,
                SequenceResetCycle.Daily => last.Date != now.Date,
                _ => false
            };
            if (due)
            {
                seq.NextValue = seq.SmallestValue;
                seq.LastResetAt = now;
            }
        }

        /// <summary>
        /// Formats the code from the pattern. Supported tokens:
        ///   {PREFIX} {SUFFIX} {SEQ} {YYYY} {YY} {MM} {DD}
        /// {SEQ} is left-padded with zeros up to PaddingLength.
        /// </summary>
        public static string FormatCode(SysNumberSequence seq, long value)
        {
            var now = DateTime.UtcNow;
            var seqStr = seq.PaddingLength > 0
                ? value.ToString().PadLeft(seq.PaddingLength, '0')
                : value.ToString();

            var sb = new StringBuilder(seq.FormatPattern);
            sb.Replace("{PREFIX}", seq.Prefix ?? string.Empty);
            sb.Replace("{SUFFIX}", seq.Suffix ?? string.Empty);
            sb.Replace("{SEQ}", seqStr);
            sb.Replace("{YYYY}", now.Year.ToString("D4"));
            sb.Replace("{YY}", (now.Year % 100).ToString("D2"));
            sb.Replace("{MM}", now.Month.ToString("D2"));
            sb.Replace("{DD}", now.Day.ToString("D2"));
            return sb.ToString();
        }
    }
}
