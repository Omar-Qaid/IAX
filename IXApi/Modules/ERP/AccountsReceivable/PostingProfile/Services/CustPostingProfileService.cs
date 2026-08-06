using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.ERP.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class CustPostingProfileService : ICustPostingProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustPostingProfileService> _logger;

        public CustPostingProfileService(IUnitOfWork unitOfWork, ILogger<CustPostingProfileService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CustLedgerAccounts?> ResolveAccountRuleAsync(string postingProfile, string customerAccountNum, string customerGroupId, CancellationToken ct = default)
        {
            _logger.LogInformation("[CustPostingProfileService] Resolving rule for Profile: {Profile}, Customer: {CustNum}, Group: {CustGroup}",
                postingProfile, customerAccountNum, customerGroupId);

            var repo = _unitOfWork.Repository<CustLedgerAccounts>();

            var rules = await repo.GetQueryable()
                .Where(r => r.PostingProfile == postingProfile)
                .ToListAsync(ct);

            if (!rules.Any())
            {
                _logger.LogWarning("[CustPostingProfileService] No rules found for PostingProfile: {Profile}", postingProfile);
                return null;
            }

            // Priority 1: Table (Specific Customer Account Number)
            var tableRule = rules.FirstOrDefault(r => r.AccountCode == AccountCode.Table && string.Equals(r.Num, customerAccountNum, StringComparison.OrdinalIgnoreCase));
            if (tableRule != null) return tableRule;

            // Priority 2: Group (Customer Group ID)
            var groupRule = rules.FirstOrDefault(r => r.AccountCode == AccountCode.Group && string.Equals(r.Num, customerGroupId, StringComparison.OrdinalIgnoreCase));
            if (groupRule != null) return groupRule;

            // Priority 3: All (Catch-All Fallback)
            var allRule = rules.FirstOrDefault(r => r.AccountCode == AccountCode.All);
            return allRule;
        }
    }
}
