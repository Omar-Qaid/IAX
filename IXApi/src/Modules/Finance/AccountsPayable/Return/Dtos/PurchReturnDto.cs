using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Type argument for <c>BaseController&lt;PurchTable, PurchReturnDto&gt;</c>, deliberately without
/// members. A vendor return is not an entity of its own: it is a <c>PurchTable</c> row with
/// <c>PurchaseType.ReturnItem</c> and a <c>ReturnItemNum</c>, so it is read through the purchase
/// order surface. Every generic CRUD action on the return controller is <c>[NonAction]</c>, because
/// a return must be created, shipped and credited through <see cref="IPurchReturnService"/> so the
/// entitlement, inventory and ledger effects are applied - never by posting a DTO. This type is
/// therefore never serialised to a caller; adding fields to it would be dead weight.
/// </summary>
public class PurchReturnDto : EntityDto<long>
{
}
