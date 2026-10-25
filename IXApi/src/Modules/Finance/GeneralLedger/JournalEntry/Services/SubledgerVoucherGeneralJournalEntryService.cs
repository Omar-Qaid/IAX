using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry
{
    public class SubledgerVoucherGeneralJournalEntryService : BaseService<SubledgerVoucherGeneralJournalEntry>, ISubledgerVoucherGeneralJournalEntryService
    {
        public SubledgerVoucherGeneralJournalEntryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public SubledgerVoucherGeneralJournalEntryService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }
    }
}
