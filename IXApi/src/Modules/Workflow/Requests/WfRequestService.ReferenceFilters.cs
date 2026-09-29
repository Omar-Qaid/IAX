using System.Globalization;
using System.Linq.Expressions;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Finance.Foundation.Genders;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IAX.IXApi.Modules.Workflow.Requests;

public partial class WfRequestService
{
    private const string OrganizationAssignmentFieldPrefix = "OrganizationAssignment.";
    private const string ShowroomAssignmentFieldPrefix = "ShowroomAssignment.";
    private static readonly string[] TextOperators = ["equals", "notEquals", "contains", "startsWith", "endsWith", "isEmpty", "isNotEmpty"];
    private static readonly string[] ComparableOperators = ["equals", "notEquals", "greaterThan", "greaterThanOrEqual", "lessThan", "lessThanOrEqual", "isEmpty", "isNotEmpty"];
    private static readonly string[] EqualityOperators = ["equals", "notEquals", "isEmpty", "isNotEmpty"];

    public IReadOnlyList<DynamicReferenceFilterFieldDto>? GetReferenceFilterFields(string referenceType)
        => referenceType switch
        {
            "Employee" => ReferenceFields<HcmWorker>()
                .Concat(ReferenceFields<HcmWorkerOrganizationAssignmentV1>(OrganizationAssignmentFieldPrefix))
                .OrderBy(field => field.Name)
                .ToList(),
            "Showroom" => ReferenceFields<HcmShowroom>()
                .Concat(ReferenceFields<HcmWorkerShowroomAssignment>(ShowroomAssignmentFieldPrefix))
                .OrderBy(field => field.Name)
                .ToList(),
            _ => null
        };

    private IReadOnlyList<DynamicReferenceFilterFieldDto> ReferenceFields<TEntity>(string prefix = "") where TEntity : class
    {
        var entity = _context.Model.FindEntityType(typeof(TEntity))!;
        return entity.GetProperties()
            .Where(property => !property.IsShadowProperty() && property.ClrType != typeof(byte[]))
            .Select(property =>
            {
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                var foreignKey = entity.GetForeignKeys().Any(key => key.Properties.Contains(property));
                var operators = foreignKey || type == typeof(bool) ? EqualityOperators
                    : type == typeof(string) ? TextOperators : ComparableOperators;
                return new DynamicReferenceFilterFieldDto(
                    prefix + property.Name, TypeName(type), property.IsNullable, foreignKey, operators);
            })
            .OrderBy(field => field.Name)
            .ToList();
    }

    public async Task<DynamicReferenceFilterValuePageDto?> GetReferenceFilterValuesAsync(
        string referenceType, string field, int pageNumber, int pageSize, string? search,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 10, 100);
        return (referenceType, field) switch
        {
            ("Employee", "Person") or ("Showroom", "Party") => await ReferenceValues(
                _context.Set<DirPartyTable>().AsNoTracking()
                    .OrderBy(item => item.Name ?? item.PartyNumber).ThenBy(item => item.RecId)
                    .Select(item => new ReferenceValueRow(item.RecId.ToString(), item.Name ?? item.PartyNumber, item.NameAlias)),
                pageNumber, pageSize, search, cancellationToken),
            ("Employee", "OccupationId") or ("Employee", OrganizationAssignmentFieldPrefix + "OccupationId") => await ReferenceValues(
                _context.Set<HcmOccupation>().AsNoTracking()
                    .OrderBy(item => item.Name ?? item.Code).ThenBy(item => item.RecId)
                    .Select(item => new ReferenceValueRow(item.RecId.ToString(), item.Name ?? item.Code ?? item.RecId.ToString(), item.NameAlias)),
                pageNumber, pageSize, search, cancellationToken),
            ("Employee", "GenderId") => await ReferenceValues(
                _context.Set<Gender>().AsNoTracking()
                    .OrderBy(item => item.Name ?? item.Code).ThenBy(item => item.RecId)
                    .Select(item => new ReferenceValueRow(item.RecId.ToString(), item.Name ?? item.Code ?? item.RecId.ToString(), item.NameAlias)),
                pageNumber, pageSize, search, cancellationToken),
            ("Employee", "NationalityId") => await ReferenceValues(
                _context.Set<HcmNationality>().AsNoTracking()
                    .OrderBy(item => item.Name ?? item.Code).ThenBy(item => item.RecId)
                    .Select(item => new ReferenceValueRow(item.RecId.ToString(), item.Name ?? item.Code ?? item.RecId.ToString(), item.NameAlias)),
                pageNumber, pageSize, search, cancellationToken),
            ("Employee", "UserId") => await ReferenceValues(
                _context.Set<AspNetUser>().AsNoTracking()
                    .OrderBy(item => item.UserName ?? item.Email ?? item.Id).ThenBy(item => item.Id)
                    .Select(item => new ReferenceValueRow(item.Id, item.UserName ?? item.Email ?? item.Id, null)),
                pageNumber, pageSize, search, cancellationToken),
            ("Employee", OrganizationAssignmentFieldPrefix + "DepartmentId") => await ReferenceValues(
                _context.Set<HcmDepartment>().AsNoTracking()
                    .OrderBy(item => item.Name ?? item.Code).ThenBy(item => item.RecId)
                    .Select(item => new ReferenceValueRow(item.RecId.ToString(), item.Name ?? item.Code ?? item.RecId.ToString(), item.NameAlias)),
                pageNumber, pageSize, search, cancellationToken),
            ("Employee", OrganizationAssignmentFieldPrefix + "HcmWorkerId")
                or ("Employee", OrganizationAssignmentFieldPrefix + "HcmManagerWorkerId")
                or ("Showroom", ShowroomAssignmentFieldPrefix + "HcmWorkerId") => await ReferenceValues(
                _context.Set<HcmWorker>().AsNoTracking()
                    .OrderBy(item => item.PersonnelNumber).ThenBy(item => item.RecId)
                    .Select(item => new ReferenceValueRow(item.RecId.ToString(), item.Party.Name ?? item.PersonnelNumber, item.Party.NameAlias)),
                pageNumber, pageSize, search, cancellationToken),
            ("Showroom", ShowroomAssignmentFieldPrefix + "HcmShowroomId") => await ReferenceValues(
                _context.Set<HcmShowroom>().AsNoTracking()
                    .OrderBy(item => item.PersonnelNumber).ThenBy(item => item.RecId)
                    .Select(item => new ReferenceValueRow(item.RecId.ToString(), item.PartyTable.Name ?? item.PersonnelNumber, item.PartyTable.NameAlias)),
                pageNumber, pageSize, search, cancellationToken),
            _ => null
        };
    }

    private static async Task<DynamicReferenceFilterValuePageDto> ReferenceValues(
        IQueryable<ReferenceValueRow> query, int pageNumber, int pageSize, string? search, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(item => item.Value.Contains(term) || item.Label.Contains(term)
                || item.LabelAlias != null && item.LabelAlias.Contains(term));
        }
        var total = await query.CountAsync(cancellationToken);
        var rows = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(item => new DynamicReferenceFilterValueDto(item.Value, item.Label, item.LabelAlias))
            .ToListAsync(cancellationToken);
        return new(rows, pageNumber, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)), total);
    }

    private static IQueryable<TEntity> ApplyReferenceRules<TEntity>(
        IQueryable<TEntity> query, IReadOnlyList<RuntimeReferenceFilterRule> rules) where TEntity : class
    {
        foreach (var rule in rules)
        {
            var property = typeof(TEntity).GetProperty(rule.Field);
            if (property == null || property.PropertyType == typeof(byte[])) continue;
            var parameter = Expression.Parameter(typeof(TEntity), "item");
            var member = Expression.Property(parameter, property);
            Expression? body = rule.Operator switch
            {
                "isEmpty" => IsEmptyExpression(member, property.PropertyType),
                "isNotEmpty" => Expression.Not(IsEmptyExpression(member, property.PropertyType)),
                _ => ComparisonExpression(member, property.PropertyType, rule.Operator, rule.Value)
            };
            if (body != null) query = query.Where(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
        }
        return query;
    }

    private IQueryable<HcmWorker> ApplyEmployeeReferenceRules(
        IQueryable<HcmWorker> workers,
        IReadOnlyList<RuntimeReferenceFilterRule> rules,
        DateOnly today)
    {
        var workerRules = rules
            .Where(rule => !rule.Field.StartsWith(OrganizationAssignmentFieldPrefix, StringComparison.Ordinal))
            .ToList();
        workers = ApplyReferenceRules(workers, workerRules);

        var assignmentRules = rules
            .Where(rule => rule.Field.StartsWith(OrganizationAssignmentFieldPrefix, StringComparison.Ordinal))
            .Select(rule => rule with { Field = rule.Field[OrganizationAssignmentFieldPrefix.Length..] })
            .ToList();
        if (assignmentRules.Count == 0) return workers;

        var assignments = _context.Set<HcmWorkerOrganizationAssignmentV1>().AsNoTracking()
            .Where(assignment => assignment.IsPrimary && assignment.IsActive && !assignment.IsDeleted
                && assignment.ValidFrom <= today
                && (!assignment.ValidTo.HasValue || assignment.ValidTo.Value >= today));
        assignments = ApplyReferenceRules(assignments, assignmentRules);
        var workerIds = assignments.Select(assignment => assignment.HcmWorkerId);
        return workers.Where(worker => workerIds.Contains(worker.RecId));
    }

    private IQueryable<HcmShowroom> ApplyShowroomReferenceRules(
        IQueryable<HcmShowroom> showrooms,
        IReadOnlyList<RuntimeReferenceFilterRule> rules,
        DateOnly today)
    {
        showrooms = ApplyReferenceRules(showrooms, rules
            .Where(rule => !rule.Field.StartsWith(ShowroomAssignmentFieldPrefix, StringComparison.Ordinal)).ToList());
        var assignmentRules = rules
            .Where(rule => rule.Field.StartsWith(ShowroomAssignmentFieldPrefix, StringComparison.Ordinal))
            .Select(rule => rule with { Field = rule.Field[ShowroomAssignmentFieldPrefix.Length..] }).ToList();
        if (assignmentRules.Count == 0) return showrooms;

        var assignments = _context.Set<HcmWorkerShowroomAssignment>().AsNoTracking()
            .Where(assignment => assignment.IsPrimary && assignment.IsActive && !assignment.IsDeleted
                && assignment.ValidFrom <= today
                && (!assignment.ValidTo.HasValue || assignment.ValidTo.Value >= today));
        assignments = ApplyReferenceRules(assignments, assignmentRules);
        var showroomIds = assignments.Select(assignment => assignment.HcmShowroomId);
        return showrooms.Where(showroom => showroomIds.Contains(showroom.RecId));
    }

    private static Expression IsEmptyExpression(Expression member, Type type)
    {
        if (type == typeof(string))
            return Expression.OrElse(Expression.Equal(member, Expression.Constant(null, type)), Expression.Equal(member, Expression.Constant(string.Empty)));
        return Nullable.GetUnderlyingType(type) != null
            ? Expression.Equal(member, Expression.Constant(null, type))
            : Expression.Constant(false);
    }

    private static Expression? ComparisonExpression(Expression member, Type propertyType, string op, string text)
    {
        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        object? parsed = type == typeof(string) ? text
            : type == typeof(bool) && bool.TryParse(text, out var boolean) ? boolean
            : type == typeof(DateTime) && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime) ? dateTime
            : type == typeof(DateOnly) && DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date
            : type == typeof(Guid) && Guid.TryParse(text, out var guid) ? guid
            : TryConvert(text, type);
        if (parsed == null) return null;
        var constant = Expression.Convert(Expression.Constant(parsed, type), propertyType);
        if (type == typeof(string) && op is "contains" or "startsWith" or "endsWith")
            return Expression.Call(member, type.GetMethod(op switch { "contains" => nameof(string.Contains), "startsWith" => nameof(string.StartsWith), _ => nameof(string.EndsWith) }, [typeof(string)])!, Expression.Constant(text));
        return op switch
        {
            "equals" => Expression.Equal(member, constant),
            "notEquals" => Expression.NotEqual(member, constant),
            "greaterThan" => Expression.GreaterThan(member, constant),
            "greaterThanOrEqual" => Expression.GreaterThanOrEqual(member, constant),
            "lessThan" => Expression.LessThan(member, constant),
            "lessThanOrEqual" => Expression.LessThanOrEqual(member, constant),
            _ => null
        };
    }

    private static object? TryConvert(string value, Type type)
    {
        try { return Convert.ChangeType(value, type, CultureInfo.InvariantCulture); }
        catch (FormatException) { return null; }
        catch (InvalidCastException) { return null; }
        catch (OverflowException) { return null; }
    }

    private static string TypeName(Type type) => type == typeof(string) ? "string"
        : type == typeof(bool) ? "boolean"
        : type == typeof(DateTime) || type == typeof(DateOnly) ? "date"
        : type == typeof(decimal) || type == typeof(double) || type == typeof(float) ? "decimal"
        : "integer";

    private static List<RuntimeReferenceFilterRule> ReadReferenceFilterRules(System.Text.Json.JsonElement filter)
    {
        if (!filter.TryGetProperty("rules", out var rules) || rules.ValueKind != System.Text.Json.JsonValueKind.Array) return [];
        return rules.EnumerateArray().Where(item => item.ValueKind == System.Text.Json.JsonValueKind.Object)
            .Select(item => new RuntimeReferenceFilterRule(
                GetString(item, "field") ?? string.Empty,
                GetString(item, "operator") ?? "equals",
                GetString(item, "value") ?? string.Empty))
            .Where(item => !string.IsNullOrWhiteSpace(item.Field)).ToList();
    }

    private sealed record ReferenceValueRow(string Value, string Label, string? LabelAlias);
}
