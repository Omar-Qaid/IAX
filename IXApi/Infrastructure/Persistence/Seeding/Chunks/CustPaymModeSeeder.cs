using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    public class CustPaymModeSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var seeds = new[]
            {
                new CustPaymModeTable
                {
                    PaymMode = "Cash",
                    Name = "Cash Payment",
                    AccountType = LedgerJournalACType.Ledger,
                    PaymStatus = CustVendPaymStatus.None,
                    PaymentType = CustPaymentType.Other,
                    TypeOfDraft = TypeOfDraft.NoDraft,
                    PaymSumBy = 0, // Invoice
                    DiscGraceDays = 0,
                    DataAreaId = "dat",
                    FurtherPosting = NoYes.No,
                    BridgingAccountByBank = NoYes.No,
                    IsSepa = NoYes.No,
                    DirectDebit = NoYes.No
                },
                new CustPaymModeTable
                {
                    PaymMode = "Check",
                    Name = "Customer Check",
                    AccountType = LedgerJournalACType.Bank,
                    PaymStatus = CustVendPaymStatus.None,
                    PaymentType = CustPaymentType.Check,
                    TypeOfDraft = TypeOfDraft.NoDraft,
                    PaymSumBy = 0, // Invoice
                    DiscGraceDays = 0,
                    DataAreaId = "dat",
                    FurtherPosting = NoYes.Yes,
                    BridgingAccountByBank = NoYes.Yes,
                    IsSepa = NoYes.No,
                    DirectDebit = NoYes.No
                },
                new CustPaymModeTable
                {
                    PaymMode = "SPAN",
                    Name = "SPAN / MADA Debit Card",
                    AccountType = LedgerJournalACType.Bank,
                    PaymStatus = CustVendPaymStatus.None,
                    PaymentType = CustPaymentType.ElectronicPayment,
                    TypeOfDraft = TypeOfDraft.NoDraft,
                    PaymSumBy = 0, // Invoice
                    DiscGraceDays = 0,
                    DataAreaId = "dat",
                    FurtherPosting = NoYes.No,
                    BridgingAccountByBank = NoYes.No,
                    IsSepa = NoYes.No,
                    DirectDebit = NoYes.No
                },
                new CustPaymModeTable
                {
                    PaymMode = "Transfer",
                    Name = "Bank Wire Transfer (SWIFT)",
                    AccountType = LedgerJournalACType.Bank,
                    PaymStatus = CustVendPaymStatus.None,
                    PaymentType = CustPaymentType.ElectronicPayment,
                    TypeOfDraft = TypeOfDraft.NoDraft,
                    PaymSumBy = 0, // Invoice
                    DiscGraceDays = 0,
                    DataAreaId = "dat",
                    FurtherPosting = NoYes.No,
                    BridgingAccountByBank = NoYes.No,
                    IsSepa = NoYes.No,
                    DirectDebit = NoYes.No
                },
                new CustPaymModeTable
                {
                    PaymMode = "CreditCard",
                    Name = "Credit Card (Visa/MasterCard)",
                    AccountType = LedgerJournalACType.Bank,
                    PaymStatus = CustVendPaymStatus.None,
                    PaymentType = CustPaymentType.CreditCard,
                    TypeOfDraft = TypeOfDraft.NoDraft,
                    PaymSumBy = 0, // Invoice
                    DiscGraceDays = 0,
                    DataAreaId = "dat",
                    FurtherPosting = NoYes.No,
                    BridgingAccountByBank = NoYes.No,
                    IsSepa = NoYes.No,
                    DirectDebit = NoYes.No
                },
                new CustPaymModeTable
                {
                    PaymMode = "DirDebit",
                    Name = "Direct Debit Mandate",
                    AccountType = LedgerJournalACType.Bank,
                    PaymStatus = CustVendPaymStatus.Sent,
                    PaymentType = CustPaymentType.ElectronicPayment,
                    TypeOfDraft = TypeOfDraft.NoDraft,
                    PaymSumBy = 1, // Date
                    DiscGraceDays = 3,
                    DataAreaId = "dat",
                    FurtherPosting = NoYes.No,
                    BridgingAccountByBank = NoYes.No,
                    IsSepa = NoYes.Yes,
                    DirectDebit = NoYes.Yes
                },
                new CustPaymModeTable
                {
                    PaymMode = "BillOfExch",
                    Name = "Bill of Exchange",
                    AccountType = LedgerJournalACType.Ledger,
                    PaymStatus = CustVendPaymStatus.Confirmed,
                    PaymentType = CustPaymentType.BillOfExchange,
                    TypeOfDraft = TypeOfDraft.Acceptance,
                    PaymSumBy = 2, // Total
                    DiscGraceDays = 5,
                    DataAreaId = "dat",
                    FurtherPosting = NoYes.Yes,
                    BridgingAccountByBank = NoYes.No,
                    IsSepa = NoYes.No,
                    DirectDebit = NoYes.No
                },
                new CustPaymModeTable
                {
                    PaymMode = "PromNote",
                    Name = "Promissory Note",
                    AccountType = LedgerJournalACType.Ledger,
                    PaymStatus = CustVendPaymStatus.None,
                    PaymentType = CustPaymentType.Other,
                    TypeOfDraft = TypeOfDraft.Promissory,
                    PaymSumBy = 0, // Invoice
                    DiscGraceDays = 0,
                    DataAreaId = "dat",
                    FurtherPosting = NoYes.No,
                    BridgingAccountByBank = NoYes.No,
                    IsSepa = NoYes.No,
                    DirectDebit = NoYes.No
                },
                new CustPaymModeTable
                {
                    PaymMode = "L/C",
                    Name = "Letter of Credit (L/C)",
                    AccountType = LedgerJournalACType.Bank,
                    PaymStatus = CustVendPaymStatus.Confirmed,
                    PaymentType = CustPaymentType.Other,
                    TypeOfDraft = TypeOfDraft.NoDraft,
                    PaymSumBy = 0, // Invoice
                    DiscGraceDays = 0,
                    DataAreaId = "dat",
                    FurtherPosting = NoYes.Yes,
                    BridgingAccountByBank = NoYes.No,
                    IsSepa = NoYes.No,
                    DirectDebit = NoYes.No
                },

                new CustPaymModeTable
                {
                    PaymMode = "Sadad",
                    Name = "SADAD Payment Gateway",
                    AccountType = LedgerJournalACType.Bank,
                    PaymStatus = CustVendPaymStatus.Received,
                    PaymentType = CustPaymentType.ElectronicPayment,
                    TypeOfDraft = TypeOfDraft.NoDraft,
                    PaymSumBy = 0, // Invoice
                    DiscGraceDays = 0,
                    DataAreaId = "dat",
                    FurtherPosting = NoYes.No,
                    BridgingAccountByBank = NoYes.No,
                    IsSepa = NoYes.No,
                    DirectDebit = NoYes.No
                }
            };

            var existingCodes = await db.CustPaymModeTables
                .IgnoreQueryFilters()
                .Select(p => p.PaymMode)
                .ToListAsync(ct);

            var toAdd = seeds.Where(s => !existingCodes.Contains(s.PaymMode)).ToList();
            if (toAdd.Count > 0)
            {
                await db.CustPaymModeTables.AddRangeAsync(toAdd, ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}

