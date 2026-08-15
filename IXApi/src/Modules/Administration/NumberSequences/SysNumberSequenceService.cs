using System.Data;
using System.Text;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using IAX.IXApi.Shared.Application.NumberSequences;
using IAX.IXApi.Shared.Domain.Entities;

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

            async Task<NextSequenceResultDto> AllocateAsync()
            {
                var tx = db.Database.CurrentTransaction == null 
                    ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken) 
                    : null;

                try
                {
                    var effectiveAreaId = tenantId ?? _currentUser.GetDataAreaId();
                    var seq = await db.Set<SysNumberSequence>()
                        .IgnoreQueryFilters()
                        .Where(s => s.NumberSequence == entityName && !s.IsDeleted &&
                            (s.DataAreaId == effectiveAreaId || s.NumberSequenceScope == null))
                        .OrderByDescending(s => s.DataAreaId == effectiveAreaId)
                        .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new InvalidOperationException($"No number sequence configured for entity '{entityName}'.");

                    ValidateAvailability(seq);
                    if (seq.Manual == 1)
                        throw new InvalidOperationException($"Number sequence '{seq.NumberSequence}' requires manual code entry.");
                    if (seq.NoIncrement == 1)
                        throw new InvalidOperationException($"Number sequence '{seq.NumberSequence}' cannot auto-allocate because NoIncrement is enabled.");

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
            }

            // The generic create pipeline already owns a retry-aware transaction.
            // Starting another execution strategy inside it triggers EF Core's
            // user-initiated transaction guard.
            return db.Database.CurrentTransaction != null
                ? await AllocateAsync()
                : await strategy.ExecuteAsync(AllocateAsync);
        }

        public async Task<NextSequenceResultDto?> PeekAsync(string entityName, string? tenantId = null, CancellationToken cancellationToken = default)
        {
            var effectiveAreaId = tenantId ?? _currentUser.GetDataAreaId();
            var seq = await _unitOfWork.Context.Set<SysNumberSequence>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(s => s.NumberSequence == entityName && !s.IsDeleted &&
                    (s.DataAreaId == effectiveAreaId || s.NumberSequenceScope == null))
                .OrderByDescending(s => s.DataAreaId == effectiveAreaId)
                .FirstOrDefaultAsync(cancellationToken);
            if (seq == null) return null;

            ValidateAvailability(seq);
            if (seq.Manual == 1) return null;
            if (seq.NoIncrement == 1)
                throw new InvalidOperationException($"Number sequence '{seq.NumberSequence}' cannot preview an incrementing unique code because NoIncrement is enabled.");

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
            var requested = nextValue ?? seq.Lowest ?? 1;
            var current = seq.NextRec ?? seq.Lowest ?? 1;
            if (requested > current && seq.AllowChangeUp != 1)
                throw new InvalidOperationException("This sequence does not allow increasing NextRec manually.");
            if (requested < current && seq.AllowChangeDown != 1)
                throw new InvalidOperationException("This sequence does not allow decreasing NextRec manually.");
            seq.NextRec = checked((int)requested);
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

        public async Task<NumberSequenceMetadataDto?> GetMetadataAsync(
            Type entityType,
            string? dataAreaId = null,
            CancellationToken cancellationToken = default)
        {
            var sequenceKey = entityType.Name;
            dataAreaId ??= _currentUser.GetDataAreaId();
            var seq = await _unitOfWork.Context.Set<SysNumberSequence>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(item => item.NumberSequence == sequenceKey && !item.IsDeleted &&
                    (item.DataAreaId == dataAreaId || item.NumberSequenceScope == null))
                .OrderByDescending(item => item.DataAreaId == dataAreaId)
                .FirstOrDefaultAsync(cancellationToken);
            if (seq == null) return null;

            var blocked = seq.Blocked == 1;
            var available = !blocked && seq.InUse != 0 && seq.IsActive && !seq.IsDeleted;
            string? preview = null;
            string? message = null;
            if (!available)
                message = $"Number sequence '{sequenceKey}' is blocked or inactive.";
            else if (seq.Manual != 1)
            {
                ValidateConfiguration(seq);
                var value = seq.NextRec ?? seq.Lowest ?? 1;
                preview = FormatCode(seq, value);
            }

            return new NumberSequenceMetadataDto
            {
                SequenceKey = sequenceKey,
                Mode = seq.Manual == 1 ? "manual" : "automatic",
                Manual = seq.Manual == 1,
                Available = available,
                Blocked = blocked,
                PreviewCode = preview,
                Scope = dataAreaId ?? seq.DataAreaId,
                Message = message
            };
        }

        public async Task PrepareCreateAsync(object entity, CancellationToken cancellationToken = default)
        {
            if (entity is not ICode coded) return;
            var dataAreaId = entity is IMultiCompany company ? company.DataAreaId : null;
            var metadata = await GetMetadataAsync(entity.GetType(), dataAreaId, cancellationToken);
            if (metadata == null) return; // Entity is not configured for number sequences.
            if (!metadata.Available) throw new InvalidOperationException(metadata.Message);

            if (metadata.Manual)
            {
                if (string.IsNullOrWhiteSpace(coded.Code))
                    throw new InvalidOperationException($"Code is required because number sequence '{metadata.SequenceKey}' is manual.");
                coded.Code = coded.Code.Trim();
                if (await CodeExistsAsync(entity.GetType(), coded.Code, dataAreaId, cancellationToken))
                    throw new InvalidOperationException($"Code '{coded.Code}' already exists for '{metadata.SequenceKey}'.");
                return;
            }

            // Never trust an automatic preview/client value. Allocate atomically now.
            coded.Code = null;
            var allocated = await NextAsync(metadata.SequenceKey, dataAreaId, cancellationToken);
            coded.Code = allocated.Code;
            if (await CodeExistsAsync(entity.GetType(), coded.Code, dataAreaId, cancellationToken))
                throw new InvalidOperationException($"Generated code '{coded.Code}' already exists for '{metadata.SequenceKey}'. Check NextRec and historical data.");
        }

        private async Task<bool> CodeExistsAsync(
            Type entityType,
            string code,
            string? dataAreaId,
            CancellationToken cancellationToken)
        {
            var db = _unitOfWork.Context;
            if (db.Model.FindEntityType(entityType)?.FindProperty(nameof(ICode.Code)) == null) return false;

            var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "entity");
            var codeProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(ICode.Code));
            System.Linq.Expressions.Expression body = System.Linq.Expressions.Expression.Equal(
                codeProperty,
                System.Linq.Expressions.Expression.Constant(code, typeof(string)));

            if (dataAreaId != null && db.Model.FindEntityType(entityType)?.FindProperty(nameof(IMultiCompany.DataAreaId)) != null)
            {
                var areaProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(IMultiCompany.DataAreaId));
                body = System.Linq.Expressions.Expression.AndAlso(body,
                    System.Linq.Expressions.Expression.Equal(areaProperty,
                        System.Linq.Expressions.Expression.Constant(dataAreaId, typeof(string))));
            }

            var predicate = System.Linq.Expressions.Expression.Lambda(body, parameter);
            var set = typeof(DbContext).GetMethods()
                .Single(method => method.Name == nameof(DbContext.Set) && method.IsGenericMethod && method.GetParameters().Length == 0)
                .MakeGenericMethod(entityType)
                .Invoke(db, null)!;
            var where = typeof(Queryable).GetMethods()
                .Single(method => method.Name == nameof(Queryable.Where) && method.GetParameters().Length == 2 &&
                    method.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2)
                .MakeGenericMethod(entityType)
                .Invoke(null, [set, predicate])!;
            var anyMethod = typeof(EntityFrameworkQueryableExtensions).GetMethods()
                .Single(method => method.Name == nameof(EntityFrameworkQueryableExtensions.AnyAsync) &&
                    method.GetParameters().Length == 2)
                .MakeGenericMethod(entityType);
            return await (Task<bool>)anyMethod.Invoke(null, [where, cancellationToken])!;
        }

        private static void ValidateAvailability(SysNumberSequence seq)
        {
            if (seq.Blocked == 1 || seq.InUse == 0 || !seq.IsActive || seq.IsDeleted)
                throw new InvalidOperationException($"Number sequence '{seq.NumberSequence}' is blocked or inactive.");
            ValidateConfiguration(seq);
        }

        private static void ValidateConfiguration(SysNumberSequence seq)
        {
            var lowest = seq.Lowest ?? 1;
            var highest = seq.Highest ?? int.MaxValue;
            var next = seq.NextRec ?? lowest;
            if (lowest > highest)
                throw new InvalidOperationException($"Number sequence '{seq.NumberSequence}' has Lowest greater than Highest.");
            if (next < lowest || next > highest)
                throw new InvalidOperationException($"Number sequence '{seq.NumberSequence}' NextRec is outside its configured range.");
            if (seq.Manual != 1)
            {
                var pattern = string.IsNullOrWhiteSpace(seq.AnnotatedFormat) ? seq.Format : seq.AnnotatedFormat;
                if (string.IsNullOrWhiteSpace(pattern) || !pattern.Contains("{SEQ}", StringComparison.Ordinal))
                    throw new InvalidOperationException($"Automatic number sequence '{seq.NumberSequence}' requires a {{SEQ}} format token.");
            }
            if (seq.FetchAhead == 1 && (seq.FetchAheadQty ?? 0) <= 0)
                throw new InvalidOperationException($"Number sequence '{seq.NumberSequence}' requires a positive FetchAheadQty.");
        }

        public static string FormatCode(SysNumberSequence seq, long value)
        {
            var now = DateTime.UtcNow;
            var formatPattern = string.IsNullOrWhiteSpace(seq.AnnotatedFormat) 
                ? (string.IsNullOrWhiteSpace(seq.Format) ? "{SEQ}" : seq.Format) 
                : seq.AnnotatedFormat;
                
            var format = seq.Format ?? string.Empty;
            var separatorIndex = format.IndexOf('#');
            var prefix = separatorIndex < 0
                ? format.TrimEnd('-', '_', ' ')
                : format[..separatorIndex].TrimEnd('-', '_', ' ');
            var padding = Math.Max(1, format.Count(character => character == '#'));
            var seqStr = value.ToString().PadLeft(padding, '0');

            var sb = new StringBuilder(formatPattern);
            sb.Replace("{PREFIX}", prefix);
            sb.Replace("{SEQ}", seqStr);
            sb.Replace("{YYYY}", now.Year.ToString("D4"));
            sb.Replace("{YY}", (now.Year % 100).ToString("D2"));
            sb.Replace("{MM}", now.Month.ToString("D2"));
            sb.Replace("{DD}", now.Day.ToString("D2"));
            return sb.ToString();
        }
    }
}

