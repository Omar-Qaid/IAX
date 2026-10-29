using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTransPosting;

/// <summary>One inventory posting record with the origin it posted for.</summary>
public sealed record InventTransPostingDto(
    long RecId,
    long InventTransOrigin,
    string InventTransId,
    string ItemId,
    InventTransPostingType InventTransPostingType,
    NoYes IsPosted,
    long LedgerDimension,
    long OffsetLedgerDimension,
    LedgerPostingType PostingType,
    LedgerPostingType PostingTypeOffset,
    string Voucher,
    DateTime TransDate);
