using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Tests;

/// <summary>
/// A permissive account structure for test fixtures that post.
/// </summary>
/// <remarks>
/// <para>
/// Posting now requires an active account structure assigned to the ledger — a company without one
/// is refused rather than left unvalidated. Every hand-built fixture that posts therefore has to
/// model that setup, which is six rows across five tables plus the types in its model. This helper
/// holds both so no fixture carries its own copy, and so the shape stays in one place if the
/// account-structure tables change.
/// </para>
/// <para>
/// The structure allows <b>any non-blank main account</b> and constrains no further segment, so a
/// fixture gets past validation without having to invent financial dimension values. A test that
/// is about the structure rules themselves should build its own through
/// <c>AccountStructureBuilder</c> instead; this is the "do not get in my way" setup for fixtures
/// whose subject is something else.
/// </para>
/// </remarks>
internal static class TestAccountStructures
{
    /// <summary>
    /// The entity types a fixture must include in its model for <see cref="AssignPermissive"/> to
    /// work, on top of <c>Ledger</c>, which every posting fixture already has.
    /// </summary>
    public static readonly Type[] Types =
    [
        typeof(DimensionHierarchy), typeof(LedgerStructure), typeof(DimensionHierarchyLevel),
        typeof(DimensionConstraintTree), typeof(DimensionConstraintNode),
        typeof(DimensionConstraintNodeCriteria), typeof(DimensionAttribute),
        // Validation reads each combination's segment values through LedgerDimensionResolver, so
        // the value-group chain has to be in the model even when a fixture never populates it:
        // the resolver falls back to the combination's own per-dimension columns.
        typeof(DimensionAttributeValueGroupCombination), typeof(DimensionAttributeLevelValue),
        typeof(DimensionAttributeValue)
    ];

    /// <summary>
    /// Assigns an active structure to <paramref name="ledgerRecId"/> that allows every main
    /// account. Record ids start high so they cannot collide with a fixture's own rows.
    /// </summary>
    public static void AssignPermissive(DbContext db, string area, long ledgerRecId)
    {
        const long hierarchy = 900_001;
        const long tree = 900_002;
        const long level = 900_003;
        const long node = 900_004;

        db.Add(new DimensionHierarchy
        {
            RecId = hierarchy, DataAreaId = area, Name = "TEST-ALL",
            StructureType = DimensionHierarchyStructureType.AccountStructure, IsDraft = NoYes.No
        });
        db.Add(new LedgerStructure
        {
            RecId = 900_005, DataAreaId = area, Ledger = ledgerRecId, DimensionHierarchy = hierarchy
        });
        db.Add(new DimensionConstraintTree
        {
            RecId = tree, DataAreaId = area, DimensionHierarchy = hierarchy
        });
        // DimensionAttribute 0 is the main account segment.
        db.Add(new DimensionHierarchyLevel
        {
            RecId = level, DataAreaId = area, DimensionHierarchy = hierarchy, Level = 1, DimensionAttribute = 0
        });
        db.Add(new DimensionConstraintNode
        {
            RecId = node, DataAreaId = area, DimensionConstraintTree = tree,
            DimensionHierarchyLevel = level, ParentConstraintNode = 0, Ordinal = 1,
            Name = "TEST-ALL", IsOptional = NoYes.No
        });
        // "*" allows every non-blank value, so no main account is turned away.
        db.Add(new DimensionConstraintNodeCriteria
        {
            RecId = 900_006, DataAreaId = area, DimensionConstraintNode = node, Ordinal = 1,
            WildCardString = "*"
        });
    }
}
