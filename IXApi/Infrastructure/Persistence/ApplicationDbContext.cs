

using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Administration.Settings;


using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Impersonation;
using IAX.IXApi.Modules.Organization.Departments;
using IAX.IXApi.Modules.Organization.Employees;
using IAX.IXApi.Modules.Organization.Genders;
using IAX.IXApi.Modules.Organization.Nationalities;
using IAX.IXApi.Modules.Organization.Occupations;
using IAX.IXApi.Modules.Finance.AccountsReceivable;





using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.Categories;
using IAX.IXApi.Modules.Workflow.Priorities;
using IAX.IXApi.Modules.Workflow.Steps;
using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Operators;
using IAX.IXApi.Modules.Workflow.Variables;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Transitions;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Execution;
using IAX.IXApi.Modules.Organization.Announcements;
using IAX.IXApi.Modules.Organization.Attachments;
using IAX.IXApi.Modules.Organization.EmployeeManagers;
using IAX.IXApi.Modules.Organization.ManagementLevels;
using IAX.IXApi.Modules.Organization.Showrooms;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using IAX.IXApi.Infrastructure.Persistence.ModelBuilding;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup;
using IAX.IXApi.Modules.Organization.Features.OrgEmployeeCategory;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.GeneralLedger;

namespace IAX.IXApi.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<
        AspNetUser,   
        AspNetRole,      
        string,           
        AspNetUserClaim, 
        AspNetUserRole,  
        AspNetUserLogin,  
        AspNetRoleClaim, 
        AspNetUserToken  
      >
     {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor? httpContextAccessor = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetDataAreaId()
        {
            var context = _httpContextAccessor?.HttpContext;
            if (context == null) return "dat";

            if (context.Request.Headers.TryGetValue("X-Company", out var headerValue) && !string.IsNullOrWhiteSpace(headerValue))
                return headerValue.ToString();
            if (context.Request.Headers.TryGetValue("X-DataAreaId", out var headerValue2) && !string.IsNullOrWhiteSpace(headerValue2))
                return headerValue2.ToString();

            var user = context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var companyClaim = user.FindFirst("Company") ?? user.FindFirst("DataAreaId");
                if (companyClaim != null && !string.IsNullOrWhiteSpace(companyClaim.Value))
                    return companyClaim.Value;
            }

            return "dat";
        }


        #region Global
        public DbSet<SysDataSeedLog> SysDataSeedLogs => Set<SysDataSeedLog>();
        public DbSet<SysAuditLog> SysAuditLogs => Set<SysAuditLog>();
        public DbSet<SysExceptionLog> SysExceptionLogs => Set<SysExceptionLog>();
        public DbSet<SysNumberSequence> SysNumberSequences => Set<SysNumberSequence>();
        public DbSet<SysSettings> SysSettings => Set<SysSettings>();
        public DbSet<SysUserSettings> SysUserSettings => Set<SysUserSettings>();
        #endregion

        #region Notifications
        public DbSet<SysNotification> SysNotifications => Set<SysNotification>();
        public DbSet<SysNotificationRecipient> SysNotificationRecipients => Set<SysNotificationRecipient>();
        public DbSet<SysNotificationTemplate> SysNotificationTemplates => Set<SysNotificationTemplate>();
        public DbSet<SysScheduledNotification> SysScheduledNotifications => Set<SysScheduledNotification>();
        public DbSet<SysNotificationAuditLog> SysNotificationAuditLogs => Set<SysNotificationAuditLog>();
        public DbSet<SysNotificationPreference> SysNotificationPreferences => Set<SysNotificationPreference>();
        #endregion

        #region Background Jobs
        public DbSet<SysBackgroundJob> SysBackgroundJobs => Set<SysBackgroundJob>();
        public DbSet<SysBackgroundJobExecution> SysBackgroundJobExecutions => Set<SysBackgroundJobExecution>();
        #endregion

        #region Chat
        public DbSet<IAX.IXApi.Modules.Communication.Chat.Entities.SysChatMessage> SysChatMessages => Set<IAX.IXApi.Modules.Communication.Chat.Entities.SysChatMessage>();
        public DbSet<IAX.IXApi.Modules.Communication.Chat.Entities.SysChatReadState> SysChatReadStates => Set<IAX.IXApi.Modules.Communication.Chat.Entities.SysChatReadState>();
        #endregion

        #region Identity Users
        public DbSet<AspNetUser> AspNetUser => Set<AspNetUser>();
        public DbSet<AspNetRole> AspNetRole => Set<AspNetRole>();
        public DbSet<AspNetUserClaim> AspNetUserClaims => Set<AspNetUserClaim>();
        public DbSet<AspNetUserRole> AspNetUserRole => Set<AspNetUserRole>();
        public DbSet<AspNetUserLogin> AspNetUserLogins => Set<AspNetUserLogin>();
        public DbSet<AspNetRoleClaim> AspNetRoleClaims => Set<AspNetRoleClaim>();
        public DbSet<AspNetUserToken> AspNetUserTokens => Set<AspNetUserToken>();


        #endregion

      
        #region ًWorkflow
        public DbSet<WfDataType> WfDataTypes => Set<WfDataType>();
        public DbSet<WfPriority> WfPriorities => Set<WfPriority>();
        public DbSet<WfCategory> WfCategories => Set<WfCategory>();
        public DbSet<WfActivityType> WfActivityTypes => Set<WfActivityType>();
        public DbSet<WfControl> WfControls => Set<WfControl>();
        public DbSet<WfOperator> WfOperators => Set<WfOperator>();
        public DbSet<WfRequest> WfRequests => Set<WfRequest>();
        public DbSet<WfProcess> WfProcesses => Set<WfProcess>();
        public DbSet<WfUsersProcess> WfUsersProcesses => Set<WfUsersProcess>();
        public DbSet<WfStep> WfSteps => Set<WfStep>();
        public DbSet<WfVariable> WfVariables => Set<WfVariable>();
        public DbSet<WfRequestControl> WfRequestControls => Set<WfRequestControl>();
        public DbSet<WfRequestControlsValidation> WfRequestControlsValidations => Set<WfRequestControlsValidation>();
        public DbSet<WfRequestDetail> WfRequestDetails => Set<WfRequestDetail>();
        public DbSet<WfActivity> WfActivities => Set<WfActivity>();
        public DbSet<WfActivityControl> WfActivityControls => Set<WfActivityControl>();
        public DbSet<WfActivityControlsValidation> WfActivityControlsValidations => Set<WfActivityControlsValidation>();
        public DbSet<WfRequestControlsOption> WfRequestControlsOptions => Set<WfRequestControlsOption>();
        public DbSet<WfActivityControlsOption> WfActivityControlsOptions => Set<WfActivityControlsOption>();
        public DbSet<WfTransition> WfTransitions => Set<WfTransition>();
        public DbSet<WfPerformer> WfPerformers => Set<WfPerformer>();
        public DbSet<WfPerformerUsers> WfPerformerUsers => Set<WfPerformerUsers>();
        public DbSet<WfActivityMappingVariable> WfActivityMappingVariables => Set<WfActivityMappingVariable>();
        public DbSet<WfProcessData> WfProcessData => Set<WfProcessData>();
        public DbSet<WfProcessVariable> WfProcessVariables => Set<WfProcessVariable>();
        public DbSet<WfRequestMappingVariable> WfRequestMappingVariables => Set<WfRequestMappingVariable>();
        public DbSet<WfRequestVariable> WfRequestVariables => Set<WfRequestVariable>();
        public DbSet<WfAssignment> WfAssignments => Set<WfAssignment>();
        #endregion

        #region Organization
        public DbSet<OrgDepartment> Departments => Set<OrgDepartment>();
        public DbSet<HcmWorker> HcmWorkers => Set<HcmWorker>();
        public DbSet<OrgGender> Genders => Set<OrgGender>();
        public DbSet<OrgNationality> Nationalities => Set<OrgNationality>();
        public DbSet<OrgOccupation> Occupations => Set<OrgOccupation>();
        public DbSet<OrgAnnouncement> Announcements => Set<OrgAnnouncement>();
        public DbSet<OrgAttachment> Attachments => Set<OrgAttachment>();
        public DbSet<OrgAttachmentDetail> AttachmentDetails => Set<OrgAttachmentDetail>();
        public DbSet<OrgShowroom> Showrooms => Set<OrgShowroom>();
        public DbSet<OrgManagementLevel> OrgManagementLevels => Set<OrgManagementLevel>();
        public DbSet<OrgEmployeeManager> OrgEmployeeManagers => Set<OrgEmployeeManager>();
        #endregion

        #region RBAC Permissions
        public DbSet<AppPermission> AspNetPermissions => Set<AppPermission>();
        public DbSet<AppRolePermission> AspNetRolePermissions => Set<AppRolePermission>();
        #endregion

        #region Identity Groups & Categories
        public DbSet<OrgEmployeeGroup> OrgEmployeeGroups => Set<OrgEmployeeGroup>();
        public DbSet<OrgEmployeeGroupDetail> OrgEmployeeGroupDetails => Set<OrgEmployeeGroupDetail>();
        public DbSet<OrgEmployeeCategory> OrgEmployeeCategories => Set<OrgEmployeeCategory>();
        public DbSet<OrgEmployeeCategoryGroup> OrgEmployeeCategoryGroups => Set<OrgEmployeeCategoryGroup>();
        #endregion

         #region Accounts
        public DbSet<CustGroup> CustGroups => Set<CustGroup>();
        public DbSet<CustTable> CustTables => Set<CustTable>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<ExchangeRateType> ExchangeRateTypes => Set<ExchangeRateType>();
        public DbSet<ExchangeRateCurrencyPair> ExchangeRateCurrencyPairs => Set<ExchangeRateCurrencyPair>();
        public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
        public DbSet<DlvTerm> DlvTerms => Set<DlvTerm>();
        public DbSet<DlvMode> DlvModes => Set<DlvMode>();
        public DbSet<PaymTerm> PaymTerms => Set<PaymTerm>();
        public DbSet<PaymSched> PaymScheds => Set<PaymSched>();
        public DbSet<PaymSchedLine> PaymSchedLines => Set<PaymSchedLine>();
        #endregion
        #region AccountsReceivable
        public DbSet<SalesTable> SalesTables => Set<SalesTable>();
        public DbSet<SalesQuotationTable> SalesQuotationTables => Set<SalesQuotationTable>();
        public DbSet<SalesQuotationLine> SalesQuotationLines => Set<SalesQuotationLine>();
        public DbSet<CustInvoiceJour> CustInvoiceJours => Set<CustInvoiceJour>();
        public DbSet<SalesLine> SalesLines => Set<SalesLine>();
        public DbSet<CustInvoiceTrans> CustInvoiceTrans => Set<CustInvoiceTrans>();
        public DbSet<SpecTrans> SpecTrans => Set<SpecTrans>();
        public DbSet<CustTrans> CustTrans => Set<CustTrans>();
        public DbSet<CustSettlement> CustSettlements => Set<CustSettlement>();
        public DbSet<LedgerJournalTable> LedgerJournalTables => Set<LedgerJournalTable>();
        public DbSet<LedgerJournalTrans> LedgerJournalTrans => Set<LedgerJournalTrans>();
        public DbSet<CustPackingSlipJour> CustPackingSlipJours => Set<CustPackingSlipJour>();
        public DbSet<CustPackingSlipTrans> CustPackingSlipTrans => Set<CustPackingSlipTrans>();

        public DbSet<SalesPool> SalesPools => Set<SalesPool>();
     
        public DbSet<ContactPerson> ContactPerson => Set<ContactPerson>();

        
        
        // CUST* extended entities
        public DbSet<CustTransOpen> CustTransOpens => Set<CustTransOpen>();
        public DbSet<CustConfirmJour> CustConfirmJours => Set<CustConfirmJour>();
        public DbSet<CustConfirmTrans> CustConfirmTrans => Set<CustConfirmTrans>();
        public DbSet<CustQuotationJour> CustQuotationJours => Set<CustQuotationJour>();
        public DbSet<CustQuotationTrans> CustQuotationTrans => Set<CustQuotationTrans>();
        public DbSet<CustInvoiceTable> CustInvoiceTables => Set<CustInvoiceTable>();
        public DbSet<CustInvoiceLine> CustInvoiceLines => Set<CustInvoiceLine>();
        public DbSet<CustLedger> CustLedgers => Set<CustLedger>();
        public DbSet<CustLedgerAccounts> CustLedgerAccounts => Set<CustLedgerAccounts>();
        public DbSet<CustPaymModeTable> CustPaymModeTables => Set<CustPaymModeTable>();
        #endregion
        #region InventoryManagement
        public DbSet<InventTable> InventTables => Set<InventTable>();
        public DbSet<InventItemGroup> InventItemGroups => Set<InventItemGroup>();
        public DbSet<InventItemBarcode> InventItemBarcodes => Set<InventItemBarcode>();
        public DbSet<InventItemPrice> InventItemPrices => Set<InventItemPrice>();
        public DbSet<InventTrans> InventTrans => Set<InventTrans>();
        public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
        public DbSet<InventTransOrigin> InventTransOrigins => Set<InventTransOrigin>();
        public DbSet<InventSum> InventSums => Set<InventSum>();
        public DbSet<InventDim> InventDims => Set<InventDim>();
        public DbSet<InventSettlement> InventSettlements => Set<InventSettlement>();
        public DbSet<InventClosing> InventClosings => Set<InventClosing>();
        public DbSet<InventJournalTable> InventJournalTables => Set<InventJournalTable>();
        public DbSet<InventJournalTrans> InventJournalTrans => Set<InventJournalTrans>();
        public DbSet<InventSite> InventSites => Set<InventSite>();
        public DbSet<InventLocation> InventLocations => Set<InventLocation>();
        public DbSet<InventBatch> InventBatches => Set<InventBatch>();
        


        // INVENT* extended entities
        public DbSet<InventCountJour> InventCountJours => Set<InventCountJour>();
        public DbSet<InventJournalName> InventJournalNames => Set<InventJournalName>();
        public DbSet<InventItemLocation> InventItemLocations => Set<InventItemLocation>();
        public DbSet<InventTableModule> InventTableModules => Set<InventTableModule>();
        #endregion

        #region GeneralLedger
        public DbSet<MainAccount> MainAccounts => Set<MainAccount>();
        public DbSet<GeneralJournalEntry> GeneralJournalEntries => Set<GeneralJournalEntry>();
        public DbSet<GeneralJournalAccountEntry> GeneralJournalAccountEntries => Set<GeneralJournalAccountEntry>();
        public DbSet<FiscalCalendar> FiscalCalendars => Set<FiscalCalendar>();
        public DbSet<FiscalCalendarYear> FiscalCalendarYears => Set<FiscalCalendarYear>();
        public DbSet<FiscalCalendarPeriod> FiscalCalendarPeriods => Set<FiscalCalendarPeriod>();
        
        // Ledger Fiscal Calendars
        public DbSet<LedgerFiscalCalendarYear> LedgerFiscalCalendarYears => Set<LedgerFiscalCalendarYear>();
        public DbSet<LedgerFiscalCalendarPeriod> LedgerFiscalCalendarPeriods => Set<LedgerFiscalCalendarPeriod>();
        
        public DbSet<LogisticsAddressState> LogisticsAddressStates => Set<LogisticsAddressState>();
        public DbSet<LogisticsAddressCountryRegion> LogisticsAddressCountryRegions => Set<LogisticsAddressCountryRegion>();
        public DbSet<LogisticsAddressCounty> LogisticsAddressCounties => Set<LogisticsAddressCounty>();
        public DbSet<LogisticsAddressCity> LogisticsAddressCities => Set<LogisticsAddressCity>();
        
        public DbSet<TaxGroupData> TaxGroupDatas => Set<TaxGroupData>();
        public DbSet<InventPosting> InventPostings => Set<InventPosting>();
        public DbSet< MarkupTable> MarkupTables => Set< MarkupTable>();
        public DbSet< MarkupTrans> MarkupTrans => Set< MarkupTrans>();

        public DbSet<Ledger> Ledgers => Set<Ledger>();
        public DbSet<BankGroup> BankGroups => Set<BankGroup>();
        public DbSet<BankAccountTable> BankAccountTables => Set<BankAccountTable>();
        public DbSet<CompanyInfo> CompanyInfos => Set<CompanyInfo>();
        public DbSet<DirPartyTable> DirPartyTables => Set<DirPartyTable>();
        public DbSet<DirPartyLocation> DirPartyLocations => Set<DirPartyLocation>();
        public DbSet<LogisticsLocation> LogisticsLocations => Set<LogisticsLocation>();
        public DbSet<LogisticsPostalAddress> LogisticsPostalAddresses => Set<LogisticsPostalAddress>();
        public DbSet<LogisticsElectronicAddress> LogisticsElectronicAddresses => Set<LogisticsElectronicAddress>();
        public DbSet<LogisticsLocationRole> LogisticsLocationRoles => Set<LogisticsLocationRole>();
        public DbSet<DirPartyLocationRole> DirPartyLocationRoles => Set<DirPartyLocationRole>();


        // LEDGER* extended entities
        public DbSet<LedgerChartOfAccounts> LedgerChartOfAccounts => Set<LedgerChartOfAccounts>();
        public DbSet<LedgerJournalName> LedgerJournalNames => Set<LedgerJournalName>();

        // Tax GL entities
        public DbSet<TaxData> TaxData => Set<TaxData>();
        public DbSet<TaxTable> TaxTables => Set<TaxTable>();
        public DbSet<TaxGroupHeading> TaxGroupHeadings => Set<TaxGroupHeading>();
        public DbSet<TaxOnItem> TaxOnItems => Set<TaxOnItem>();
        public DbSet<TaxJournalTrans> TaxJournalTrans => Set<TaxJournalTrans>();
        public DbSet<TaxTrans> TaxTrans => Set<TaxTrans>();
        #endregion
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            // Suppress the pending model changes warning since we cannot generate migrations for the external ERP database
            optionsBuilder.ConfigureWarnings(warnings => 
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            // Default precision for every decimal column, preventing silent truncation
            // (EF's default is decimal(18,2)). Per-property HasPrecision still overrides this.
            configurationBuilder.Properties<decimal>().HavePrecision(18, 4);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            modelBuilder.ApplyMissingERPMappings();

            // Disable cascade delete globally
            // This prevents SQL Server error 1785 (multiple cascade paths / cycles)
            // D365FO manages cascades in application logic, not at the SQL constraint level.
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            SetGlobalQueryFilters(modelBuilder);
        }

        private void SetGlobalQueryFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Query filters can only be defined on the root of an inheritance hierarchy (TPH).
                if (entityType.BaseType != null)
                    continue;

                Expression? filterExpression = null;
                var parameter = Expression.Parameter(entityType.ClrType, "e");

                // 1. Soft Delete Filter
                var isDeletedProp = entityType.FindProperty("IsDeleted");
                if (isDeletedProp != null && isDeletedProp.ClrType == typeof(bool))
                {
                    filterExpression = Expression.Equal(
                        Expression.Property(parameter, "IsDeleted"),
                        Expression.Constant(false)
                    );
                }

                // 2. Multi-Company Filter
                if (typeof(IMultiCompany).IsAssignableFrom(entityType.ClrType))
                {
                    var typeName = entityType.ClrType.Name;
                    var excludeFromCompanyFilter = new HashSet<string>
                    {
                        "LogisticsElectronicAddress",
                        "LogisticsPostalAddress",
                        "DirPartyLocation",
                        "LogisticsLocation",
                        "DirPartyTable",
                        "CompanyInfo"
                    };

                    if (!excludeFromCompanyFilter.Contains(typeName))
                    {
                        var companyFilter = Expression.Equal(
                            Expression.Property(parameter, "DataAreaId"),
                            Expression.Call(Expression.Constant(this), typeof(ApplicationDbContext).GetMethod(nameof(GetDataAreaId))!)
                        );

                        filterExpression = filterExpression == null
                            ? companyFilter
                            : Expression.AndAlso(filterExpression, companyFilter);
                    }
                }

                if (filterExpression != null)
                {
                    var lambda = Expression.Lambda(filterExpression, parameter);
                    entityType.SetQueryFilter(lambda);
                }
            }
        }
    }
}



