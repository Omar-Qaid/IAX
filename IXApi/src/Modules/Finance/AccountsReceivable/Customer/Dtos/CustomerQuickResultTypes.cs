using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;

public enum CustomerQuickUpdateStatus { CustomerNotFound, PartyNotFound, Updated }
public sealed record CustomerQuickUpdateResult(CustomerQuickUpdateStatus Status, CustTable? Customer = null, DirPartyTable? Party = null);
