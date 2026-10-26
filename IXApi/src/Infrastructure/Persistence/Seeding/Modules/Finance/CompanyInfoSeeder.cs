using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance
{
    /// <summary>
    /// Seeds core legal entities (dat and HBMC) and root DirParty records.
    /// Category: Required. Policy: RunOnce (or Repeatable reconciliation). Scope: dat.
    /// </summary>
    public class CompanyInfoSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.Foundation.CompanyInfo";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Required;
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "dat";

        public override async Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        {
            var db = context.Db;
            var users = context.Users;

            var sysUser = users != null ? await users.FindByNameAsync("sys") : null;
            var createdBy = sysUser?.Id ?? "sys";

            // 1. Ensure DirParty for DAT
            var datParty = await db.Set<DirPartyTable>().IgnoreQueryFilters().FirstOrDefaultAsync(p => p.PartyNumber == "DAT", cancellationToken);
            if (datParty == null)
            {
                datParty = new DirPartyTable
                {
                    PartyNumber = "DAT",
                    Name = "Company accounts data",
                    NameAlias = "DAT",
                    IsActive = NoYes.Yes,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                };
                await db.Set<DirPartyTable>().AddAsync(datParty, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            // 2. Ensure DirParty for HBMC
            var hbmcParty = await db.Set<DirPartyTable>().IgnoreQueryFilters().FirstOrDefaultAsync(p => p.PartyNumber == "HBMC", cancellationToken);
            if (hbmcParty == null)
            {
                hbmcParty = new DirPartyTable
                {
                    PartyNumber = "HBMC",
                    Name = "AlHayat Building Materials Company",
                    NameAlias = "HBMC",
                    IsActive = NoYes.Yes,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "HBMC"
                };
                await db.Set<DirPartyTable>().AddAsync(hbmcParty, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            // 3. Ensure CompanyInfo for DAT
            var datCompany = await db.Set<CompanyInfo>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.DataArea == "dat", cancellationToken);
            if (datCompany == null)
            {
                datCompany = new CompanyInfo
                {
                    DataArea = "dat",
                    Name = "Company accounts data",
                    Party = datParty.RecId,
                    LanguageId = "en-us",
                    TimeZone = "(GMT-08:00) Pacific Time (US & Canada)",
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                };
                await db.Set<CompanyInfo>().AddAsync(datCompany, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            // 4. Ensure CompanyInfo for HBMC
            var hbmcCompany = await db.Set<CompanyInfo>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.DataArea == "HBMC", cancellationToken);
            if (hbmcCompany == null)
            {
                hbmcCompany = new CompanyInfo
                {
                    DataArea = "HBMC",
                    Name = "AlHayat Building Materials Company",
                    Party = hbmcParty.RecId,
                    LanguageId = "ar",
                    TimeZone = "(GMT+03:00) Riyadh",
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "HBMC"
                };
                await db.Set<CompanyInfo>().AddAsync(hbmcCompany, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            // 5. Grant legal entity claims to sys user
            if (sysUser != null && users != null)
            {
                var existingCompanyClaims = (await users.GetClaimsAsync(sysUser))
                    .Where(claim => claim.Type == CompanyContextDefaults.ClaimType)
                    .Select(claim => claim.Value)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var legalEntityIds = await db.Set<CompanyInfo>()
                    .IgnoreQueryFilters()
                    .Where(company => company.IsActive)
                    .Select(company => company.DataArea)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                foreach (var legalEntityId in legalEntityIds.Where(id => !existingCompanyClaims.Contains(id)))
                {
                    var result = await users.AddClaimAsync(
                        sysUser,
                        new Claim(CompanyContextDefaults.ClaimType, legalEntityId));

                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Could not grant legal entity '{legalEntityId}' to the sys user: " +
                            string.Join("; ", result.Errors.Select(error => error.Description)));
                    }
                }
            }
        }
    }
}
