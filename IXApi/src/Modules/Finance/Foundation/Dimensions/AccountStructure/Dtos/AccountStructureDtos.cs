namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;

/// <summary>
/// One segment of an account structure: the dimension it constrains and the values it allows,
/// written as the account structure page writes them — semicolon separated, with <c>..</c> for a
/// range, <c>*</c> for any non-blank value and <c>""</c> to allow blank. Use the dimension name
/// <c>MainAccount</c> for the main account segment, which every structure must have.
/// </summary>
public sealed record AccountStructureSegmentDto(string Dimension, string AllowedValues);

/// <summary>A structure to create as a draft, with its segments in order.</summary>
public sealed record AccountStructureCreateDto(
    string Name,
    string? Description,
    IReadOnlyList<AccountStructureSegmentDto> Segments);

public sealed record AccountStructureSegmentResultDto(
    int Level,
    string Dimension,
    string AllowedValues,
    bool AllowsBlank);

public sealed record AccountStructureDto(
    long RecId,
    string Name,
    string Description,
    bool IsDraft,
    bool IsAssignedToLedger,
    IReadOnlyList<AccountStructureSegmentResultDto> Segments);
