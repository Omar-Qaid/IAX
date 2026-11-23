using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;

/// <summary>
/// Validate what the application resolves before it can allocate a document number or run a job: one
/// settings row in a currency the company knows, a usable sequence for every generated code, and a
/// recurring job per required worker. The sequence rules are the ones the number sequence service
/// itself enforces at allocation time, checked here instead of at the first failed document.
/// </summary>
public sealed class AdministrationIntegrityValidator : ISeedValidator
{
    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        var settings = await c.Db.Set<SysSettings>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => !x.IsDeleted).ToListAsync(ct);
        if (settings.Count != 1)
            throw new InvalidOperationException($"Expected exactly one application settings row; found {settings.Count}.");
        if (string.IsNullOrWhiteSpace(settings[0].AppName) || string.IsNullOrWhiteSpace(settings[0].DefaultLanguage)
            || string.IsNullOrWhiteSpace(settings[0].TimeZone) || string.IsNullOrWhiteSpace(settings[0].DateFormat))
            throw new InvalidOperationException("The application settings row is missing a required default.");
        if (settings[0].DecimalPlaces < 0 || settings[0].PaginationSize <= 0 || settings[0].MaxUploadSize <= 0)
            throw new InvalidOperationException("The application settings row carries a non-usable limit.");
        await c.RequireAsync<Currency>(x => x.CurrencyCode == settings[0].Currency, ct);

        var sequences = await c.Db.Set<SysNumberSequence>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => !x.IsDeleted).ToListAsync(ct);
        if (sequences.Select(x => x.NumberSequence).Distinct(StringComparer.OrdinalIgnoreCase).Count() != sequences.Count)
            throw new InvalidOperationException("Two number sequences share a key; SysNumberSequence.NumberSequence is unique across companies.");
        var byKey = sequences.ToDictionary(x => x.NumberSequence, StringComparer.OrdinalIgnoreCase);
        foreach (var definition in AdministrationNumberSequenceCatalog.Definitions)
        {
            if (!byKey.TryGetValue(definition.Key, out var sequence))
                throw new InvalidOperationException($"No number sequence was seeded for {definition.Key}; its code cannot be allocated.");
            if (sequence.InUse == 0 || sequence.Blocked == 1 || !sequence.IsActive)
                throw new InvalidOperationException($"Number sequence {definition.Key} is blocked or out of use and cannot allocate a code.");
            if (string.IsNullOrWhiteSpace(sequence.Format) || string.IsNullOrWhiteSpace(sequence.AnnotatedFormat))
                throw new InvalidOperationException($"Number sequence {definition.Key} has no format.");
            var lowest = sequence.Lowest ?? 1;
            var highest = sequence.Highest ?? int.MaxValue;
            var next = sequence.NextRec ?? lowest;
            if (lowest > highest)
                throw new InvalidOperationException($"Number sequence {definition.Key} has Lowest greater than Highest.");
            if (next < lowest || next > highest)
                throw new InvalidOperationException($"Number sequence {definition.Key} has NextRec outside its configured range.");
            // The service resets a cyclic sequence once a calendar day, which re-issues a code that
            // already exists when the format carries a year or a month instead of a day.
            if (sequence.Cyclic == 1 && AdministrationNumberSequenceCatalog.CarriesPeriod(sequence.AnnotatedFormat))
                throw new InvalidOperationException($"Number sequence {definition.Key} resets daily although its format carries a longer period; it would re-issue an existing code.");
        }

        var jobs = await c.Db.Set<SysBackgroundJob>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => !x.IsDeleted).ToListAsync(ct);
        if (jobs.Select(x => x.Caption).Distinct(StringComparer.OrdinalIgnoreCase).Count() != jobs.Count)
            throw new InvalidOperationException("Two background jobs share a caption; SysBackgroundJob.Caption is unique across companies.");
        foreach (var job in jobs.Where(x => x.ScheduleType == SysJobScheduleType.Recurring && x.IsEnabled))
        {
            if (job.RecurrenceData == null || job.RecurrenceData.Length == 0)
                throw new InvalidOperationException($"Enabled recurring job {job.Caption} carries no recurrence.");
            if (!int.TryParse(System.Text.Encoding.UTF8.GetString(job.RecurrenceData), out var seconds) || seconds <= 0)
                throw new InvalidOperationException($"Enabled recurring job {job.Caption} carries an unreadable recurrence interval.");
            if (string.IsNullOrWhiteSpace(job.JobKey))
                throw new InvalidOperationException($"Enabled job {job.Caption} resolves no handler.");
        }

        var tasks = await c.Db.Set<SysBackgroundJobTask>().IgnoreQueryFilters().AsNoTracking()
            .Select(x => new { x.RecId, x.JobId, x.JobKey }).ToListAsync(ct);
        var jobIds = jobs.Select(x => x.RecId).ToHashSet();
        foreach (var task in tasks)
        {
            if (!jobIds.Contains(task.JobId))
                throw new InvalidOperationException($"Batch task {task.RecId} belongs to missing job {task.JobId}.");
            if (string.IsNullOrWhiteSpace(task.JobKey))
                throw new InvalidOperationException($"Batch task {task.RecId} resolves no handler.");
        }
    }
}
