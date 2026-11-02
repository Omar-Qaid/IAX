using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// Persisted enum values are stable.
///
/// Every enum reachable from a finance entity property is stored in the database as its integer
/// value, so the value is part of the schema, not an implementation detail. Many members in
/// Enumeration.cs take their value implicitly from their position, which means inserting or
/// reordering a member silently renumbers every member after it — and silently remaps every row
/// already written with the old numbering. Nothing caught that.
///
/// This pins the whole persisted surface against a baseline. Any value that moves fails here,
/// whether it moved because of a reorder or because someone edited the number.
/// </summary>
public class PersistedEnumValueTests
{
    [Fact]
    public void Every_persisted_enum_value_matches_the_recorded_baseline()
    {
        var actual = Canonical();
        var baseline = Baseline();
        if (actual == baseline) return;

        // Written out so the baseline can be replaced wholesale once the change is confirmed
        // deliberate and no stored data carries the old numbering.
        var regenerated = Path.Combine(AppContext.BaseDirectory, "PersistedEnumValues.actual.txt");
        File.WriteAllText(regenerated, actual);

        var expectedLines = baseline.Split('\n');
        var actualLines = actual.Split('\n');
        var added = actualLines.Except(expectedLines).ToList();
        var removed = expectedLines.Except(actualLines).ToList();
        var report = new StringBuilder()
            .AppendLine("Persisted enum values changed. These numbers are stored in the database, so a")
            .AppendLine("changed value silently remaps existing rows. If the change is deliberate and no")
            .AppendLine("stored data carries the old numbering, replace Tests/PersistedEnumValues.txt with:")
            .AppendLine(regenerated)
            .AppendLine();
        Report(report, "No longer present (or renumbered away from)", removed);
        Report(report, "New (or renumbered to)", added);
        Assert.Fail(report.ToString());
    }

    /// <summary>
    /// The baseline covers what is actually stored, so this pins the rule that produced it: an enum
    /// counts when a finance entity has a property of that type. A new entity property of a new
    /// enum type widens the surface, and the test above then reports it as new.
    /// </summary>
    [Fact]
    public void The_baseline_covers_every_enum_reachable_from_a_finance_entity()
    {
        // Each line is "Namespace.Enum.Member = value", so the type is everything up to the last
        // dot before the value.
        var covered = Baseline().Split('\n')
            .Where(line => line.Length > 0)
            .Select(line => line[..line.IndexOf(" = ", StringComparison.Ordinal)])
            .Select(qualified => qualified[..qualified.LastIndexOf('.')])
            .ToHashSet();

        var missing = PersistedEnums().Select(x => x.FullName!).Where(x => !covered.Contains(x)).ToList();

        Assert.Empty(missing);
    }

    /// <summary>
    /// Every member in Enumeration.cs carries its value explicitly, so a member inserted in the
    /// middle of an enum cannot renumber the members after it. The baseline above would catch that
    /// renumbering; this stops it being possible in the first place, which matters because the
    /// baseline is only consulted when someone runs the tests before shipping a schema change.
    /// </summary>
    [Fact]
    public void Every_enum_member_declares_its_value()
    {
        var implicitMembers = new List<string>();
        var enumName = string.Empty;
        foreach (var raw in EnumerationSource().Split('\n'))
        {
            var line = raw.Trim();
            var declaration = Regex.Match(line, @"^public enum (\w+)");
            if (declaration.Success) { enumName = declaration.Groups[1].Value; continue; }
            if (enumName.Length == 0 || line.StartsWith('}')) { if (line.StartsWith('}')) enumName = string.Empty; continue; }

            var member = Regex.Replace(line, "//.*$", string.Empty).Trim().TrimEnd(',').Trim();
            if (member.Length == 0 || member.StartsWith('{') || member.StartsWith('[')) continue;
            if (!Regex.IsMatch(member, @"^\w+\s*=\s*-?\d+$")) implicitMembers.Add($"{enumName}.{member}");
        }

        Assert.True(implicitMembers.Count == 0,
            "These enum members take their value from their position, so inserting a member before "
            + "them renumbers them and remaps stored data. Write the value out:"
            + Environment.NewLine + string.Join(Environment.NewLine, implicitMembers.Take(25)));
    }

    private static string EnumerationSource()
    {
        using var stream = typeof(PersistedEnumValueTests).Assembly
            .GetManifestResourceStream("IXApi.Tests.Enumeration.cs")
            ?? throw new InvalidOperationException("Enumeration.cs is not embedded in the test assembly.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Replace("\r\n", "\n");
    }

    private static void Report(StringBuilder report, string heading, List<string> lines)
    {
        if (lines.Count == 0) return;
        report.AppendLine($"{heading} ({lines.Count}):");
        foreach (var line in lines.Take(25)) report.AppendLine($"  {line}");
        if (lines.Count > 25) report.AppendLine($"  ... and {lines.Count - 25} more");
        report.AppendLine();
    }

    private static string Canonical()
    {
        var text = new StringBuilder();
        foreach (var type in PersistedEnums().OrderBy(x => x.FullName, StringComparer.Ordinal))
            foreach (var member in Enum.GetValues(type).Cast<object>()
                .Select(value => (Name: Enum.GetName(type, value)!, Value: Convert.ToInt64(value)))
                .OrderBy(x => x.Value).ThenBy(x => x.Name, StringComparer.Ordinal))
                text.Append(type.FullName).Append('.').Append(member.Name)
                    .Append(" = ").Append(member.Value).Append('\n');
        return text.ToString();
    }

    /// <summary>Every enum a finance entity stores, including the nullable ones.</summary>
    private static IEnumerable<Type> PersistedEnums() =>
        typeof(NoYes).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && typeof(Entity<long>).IsAssignableFrom(type))
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Select(property => Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType)
            .Where(type => type.IsEnum)
            .Distinct();

    private static string Baseline()
    {
        using var stream = typeof(PersistedEnumValueTests).Assembly
            .GetManifestResourceStream("IXApi.Tests.PersistedEnumValues.txt")
            ?? throw new InvalidOperationException("The persisted enum baseline is not embedded in the test assembly.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Replace("\r\n", "\n");
    }
}
