using System.Threading;
using System.Threading.Tasks;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public interface ICustPostingProfileService
    {
        /// <summary>
        /// Resolves the exact posting profile account rule for a given customer & group using D365 hierarchy (Table > Group > All).
        /// </summary>
        Task<CustLedgerAccounts?> ResolveAccountRuleAsync(string postingProfile, string customerAccountNum, string customerGroupId, CancellationToken ct = default);
    }
}
