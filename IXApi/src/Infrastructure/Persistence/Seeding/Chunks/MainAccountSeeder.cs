using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    /// <summary>Seeds MainAccount chart of accounts records.</summary>
    public class MainAccountSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var mainAccounts = new[]
            {
                // ── Assets (Accounts Receivable & Cash) ──────────────────────────
                new MainAccount
                {
                    MainAccountId = "10010000",
                    Name = "Cash on Hand",
                    Type = MainAccountType.Asset,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "10010000",
                    GroupLevel01 = "Assets",
                    GroupLevel02 = "Current Assets",
                    GroupLevel03 = "Cash & Bank"
                },
                new MainAccount
                {
                    MainAccountId = "10020000",
                    Name = "Main Operating Bank Account",
                    Type = MainAccountType.Asset,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "10020000",
                    GroupLevel01 = "Assets",
                    GroupLevel02 = "Current Assets",
                    GroupLevel03 = "Cash & Bank"
                },
                new MainAccount
                {
                    MainAccountId = "11010000",
                    Name = "Accounts Receivable - General Fallback",
                    Type = MainAccountType.Asset,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "11010000",
                    GroupLevel01 = "Assets",
                    GroupLevel02 = "Current Assets",
                    GroupLevel03 = "Trade Receivables"
                },
                new MainAccount
                {
                    MainAccountId = "11020005",
                    Name = "Trade Accounts Receivable - Local",
                    Type = MainAccountType.Asset,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "11020005",
                    GroupLevel01 = "Assets",
                    GroupLevel02 = "Current Assets",
                    GroupLevel03 = "Trade Receivables"
                },
                new MainAccount
                {
                    MainAccountId = "11020010",
                    Name = "Customer Prepayments & Advanced Payments",
                    Type = MainAccountType.Asset,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "11020010",
                    GroupLevel01 = "Assets",
                    GroupLevel02 = "Current Assets",
                    GroupLevel03 = "Trade Receivables"
                },
                new MainAccount
                {
                    MainAccountId = "11020020",
                    Name = "Trade Accounts Receivable - Export",
                    Type = MainAccountType.Asset,
                    CurrencyCode = "USD",
                    ConsolidationMainAccount = "11020020",
                    GroupLevel01 = "Assets",
                    GroupLevel02 = "Current Assets",
                    GroupLevel03 = "Trade Receivables"
                },
                new MainAccount
                {
                    MainAccountId = "11020099",
                    Name = "Accounts Receivable - VIP Customers",
                    Type = MainAccountType.Asset,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "11020099",
                    GroupLevel01 = "Assets",
                    GroupLevel02 = "Current Assets",
                    GroupLevel03 = "Trade Receivables"
                },
                new MainAccount
                {
                    MainAccountId = "11030000",
                    Name = "Accounts Receivable - Government",
                    Type = MainAccountType.Asset,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "11030000",
                    GroupLevel01 = "Assets",
                    GroupLevel02 = "Current Assets",
                    GroupLevel03 = "Trade Receivables"
                },
                new MainAccount
                {
                    MainAccountId = "11040000",
                    Name = "Accounts Receivable - Trading",
                    Type = MainAccountType.Asset,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "11040000",
                    GroupLevel01 = "Assets",
                    GroupLevel02 = "Current Assets",
                    GroupLevel03 = "Trade Receivables"
                },
                new MainAccount
                {
                    MainAccountId = "11050000",
                    Name = "Accounts Receivable - Specific Customer (C00015)",
                    Type = MainAccountType.Asset,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "11050000",
                    GroupLevel01 = "Assets",
                    GroupLevel02 = "Current Assets",
                    GroupLevel03 = "Trade Receivables"
                },
                new MainAccount
                {
                    MainAccountId = "11090001",
                    Name = "Payment Clearing & Liquidity Account",
                    Type = MainAccountType.Asset,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "11090001",
                    GroupLevel01 = "Assets",
                    GroupLevel02 = "Current Assets",
                    GroupLevel03 = "Cash & Bank"
                },

                // ── Liabilities & Tax ──────────────────────────────────────────────
                new MainAccount
                {
                    MainAccountId = "20010000",
                    Name = "Accounts Payable Summary",
                    Type = MainAccountType.Liability,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "20010000",
                    GroupLevel01 = "Liabilities",
                    GroupLevel02 = "Current Liabilities",
                    GroupLevel03 = "Trade Payables"
                },
                new MainAccount
                {
                    MainAccountId = "22010005",
                    Name = "Sales Tax / VAT Prepayments Account",
                    Type = MainAccountType.Liability,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "22010005",
                    GroupLevel01 = "Liabilities",
                    GroupLevel02 = "Current Liabilities",
                    GroupLevel03 = "Tax Liabilities"
                },

                // ── Equity ─────────────────────────────────────────────────────────
                new MainAccount
                {
                    MainAccountId = "30010000",
                    Name = "Share Capital",
                    Type = MainAccountType.Equity,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "30010000",
                    GroupLevel01 = "Equity",
                    GroupLevel02 = "Owner Capital",
                    GroupLevel03 = "Capital"
                },

                // ── Revenue ────────────────────────────────────────────────────────
                new MainAccount
                {
                    MainAccountId = "40010000",
                    Name = "Sales Revenue - Local",
                    Type = MainAccountType.Revenue,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "40010000",
                    GroupLevel01 = "Revenue",
                    GroupLevel02 = "Operating Revenue",
                    GroupLevel03 = "Sales"
                },

                // ── Expenses ───────────────────────────────────────────────────────
                new MainAccount
                {
                    MainAccountId = "50010000",
                    Name = "Cost of Goods Sold (COGS)",
                    Type = MainAccountType.Expense,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "50010000",
                    GroupLevel01 = "Expenses",
                    GroupLevel02 = "Direct Costs",
                    GroupLevel03 = "COGS"
                },
                new MainAccount
                {
                    MainAccountId = "51030001",
                    Name = "Customer Cash Discounts Allowed",
                    Type = MainAccountType.Expense,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "51030001",
                    GroupLevel01 = "Expenses",
                    GroupLevel02 = "Financial Expenses",
                    GroupLevel03 = "Discounts"
                },
                new MainAccount
                {
                    MainAccountId = "51040001",
                    Name = "Bad Debt Write-Off Expense",
                    Type = MainAccountType.Expense,
                    CurrencyCode = "SAR",
                    ConsolidationMainAccount = "51040001",
                    GroupLevel01 = "Expenses",
                    GroupLevel02 = "Operating Expenses",
                    GroupLevel03 = "Bad Debt"
                },
            };

            var existingAccountIds = await db.Set<MainAccount>()
                .IgnoreQueryFilters()
                .Select(m => m.MainAccountId)
                .ToListAsync(ct);

            var accountsToAdd = mainAccounts
                .Where(m => !existingAccountIds.Contains(m.MainAccountId))
                .ToList();

            if (accountsToAdd.Any())
            {
                await db.Set<MainAccount>().AddRangeAsync(accountsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}

