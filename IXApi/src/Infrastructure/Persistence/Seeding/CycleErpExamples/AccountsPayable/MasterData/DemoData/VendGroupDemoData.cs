using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class VendGroupDemoData
{
    internal static VendGroup[] Create(long? defaultExchTypeId, string createdBy)
    {
            return new VendGroup[]
            {
                new VendGroup
                {
                    VendGroupCode = "DOM",
                    Name = "Domestic Trade Vendors",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "INT",
                    Name = "International Trade Vendors",
                    PaymTermId = "Monthly",
                    TaxGroupId = "EXP",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "SERVICES",
                    Name = "Service & Subcontractor Vendors",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "RAW_MAT",
                    Name = "Raw Material Suppliers",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "EQUIPMENT",
                    Name = "Capital Machinery & Equipment",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "GOV",
                    Name = "Government Authorities & Municipalities",
                    PaymTermId = "Monthly",
                    TaxGroupId = "EXEMPT",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "CONSULTANT",
                    Name = "Consulting & Professional Services",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                },
                new VendGroup
                {
                    VendGroupCode = "AFFILIATE",
                    Name = "Sister Companies & Group Affiliates",
                    PaymTermId = "Monthly",
                    TaxGroupId = "DOM",
                    AccountingCurrencyExchangeRateType = defaultExchTypeId,
                    ReportingCurrencyExchangeRateType = defaultExchTypeId,
                    DefaultDimension = null,
                    VendAccountNumSeq = null,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                    DataAreaId = "dat"
                }
            };
    }
}
