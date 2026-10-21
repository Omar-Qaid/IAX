using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.Finance
{
    public static class FinanceModule
    {
        public static IServiceCollection AddFinanceModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<GeneralLedger.FiscalCalendar.ILedgerFiscalCalendarPeriodService, GeneralLedger.FiscalCalendar.LedgerFiscalCalendarPeriodService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarService, GeneralLedger.FiscalCalendar.FiscalCalendarService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarYearService, GeneralLedger.FiscalCalendar.FiscalCalendarYearService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarPeriodService, GeneralLedger.FiscalCalendar.FiscalCalendarPeriodService>();
            services.AddScoped<AccountsReceivable.PostingProfile.Interfaces.ICustPostingProfileService, AccountsReceivable.PostingProfile.Services.CustPostingProfileService>();
            services.AddScoped<AccountsReceivable.PriceDisc.IPriceDiscJournalPostingService, AccountsReceivable.PriceDisc.PriceDiscJournalPostingService>();
            services.AddScoped<AccountsReceivable.Confirm.ISalesOrderConfirmationService, AccountsReceivable.Confirm.SalesOrderConfirmationService>();
            services.AddScoped<AccountsReceivable.PackingSlip.IPackingSlipJournalService, AccountsReceivable.PackingSlip.PackingSlipJournalService>();
            services.AddScoped<FluentValidation.IValidator<AccountsReceivable.Confirm.PostConfirmationRequest>, AccountsReceivable.Confirm.SalesOrderConfirmationValidator>();
            services.AddScoped<AccountsReceivable.Customer.ICustomerService, AccountsReceivable.Customer.CustomerService>();
            services.AddScoped<AccountsReceivable.Customer.ICustomerGroupService, AccountsReceivable.Customer.CustomerGroupService>();
            services.AddScoped<AccountsReceivable.ISalesPoolService, AccountsReceivable.SalesPoolService>();
            services.AddScoped<AccountsReceivable.PaymMode.ICustPaymModeService, AccountsReceivable.PaymMode.CustPaymModeService>();
            services.AddScoped<AccountsReceivable.PostingProfile.ICustLedgerService, AccountsReceivable.PostingProfile.CustLedgerService>();
            services.AddScoped<AccountsReceivable.PostingProfile.ICustLedgerAccountsService, AccountsReceivable.PostingProfile.CustLedgerAccountsService>();
            services.AddScoped<AccountsPayable.IVendorService, AccountsPayable.VendorService>();
            services.AddScoped<AccountsPayable.IVendorGroupService, AccountsPayable.VendorGroupService>();
            services.AddScoped<AccountsReceivable.Customer.ICustomerQuickService, AccountsReceivable.Customer.CustomerQuickService>();
            services.AddScoped<AccountsReceivable.SalesOrder.Interfaces.IInventDimensionResolver, AccountsReceivable.SalesOrder.Services.InventDimensionResolver>();
            services.AddScoped<AccountsReceivable.SalesOrder.Interfaces.ISalesInventoryNumberService, AccountsReceivable.SalesOrder.Services.SalesInventoryNumberService>();
            services.AddScoped<AccountsReceivable.SalesOrder.Interfaces.ISalesInventoryDemandService, AccountsReceivable.SalesOrder.Services.SalesInventoryDemandService>();
            services.AddScoped<AccountsReceivable.ISalesOrderCopyService, AccountsReceivable.SalesOrderCopyService>();
            services.AddScoped<FluentValidation.IValidator<AccountsReceivable.SalesOrderCopyRequest>, AccountsReceivable.SalesOrderCopyValidator>();
            services.AddScoped<AccountsReceivable.ISalesOrderLookupService, AccountsReceivable.SalesOrderLookupService>();
            services.AddScoped<AccountsReceivable.ISalesOrderReadService, AccountsReceivable.SalesOrderReadService>();
            services.AddScoped<AccountsReceivable.ISalesOrderCancellationService, AccountsReceivable.SalesOrderCancellationService>();
            services.AddScoped<AccountsReceivable.ISalesOrderDeliveryAddressService, AccountsReceivable.SalesOrderDeliveryAddressService>();
            services.AddScoped<AccountsReceivable.SalesOrderValidationService>();
            services.AddScoped<AccountsReceivable.ISalesOrderHeaderService, AccountsReceivable.SalesOrderHeaderService>();
            services.AddScoped<AccountsReceivable.ISalesOrderQuickCreateService, AccountsReceivable.SalesOrderQuickCreateService>();
            services.AddScoped<AccountsReceivable.ISalesOrderDiscountService, AccountsReceivable.SalesOrderDiscountService>();
            services.AddScoped<AccountsReceivable.ISalesOrderLineService, AccountsReceivable.SalesOrderLineService>();
            services.AddScoped<AccountsReceivable.SalesUnitConversionService>();
            services.AddScoped<AccountsReceivable.PriceDisc.IPriceDiscTableService, AccountsReceivable.PriceDisc.PriceDiscTableService>();
            services.AddScoped<AccountsReceivable.PriceDisc.IPriceDiscGroupService, AccountsReceivable.PriceDisc.PriceDiscGroupService>();
            services.AddScoped<AccountsReceivable.PriceDisc.IPriceDiscAdmTableService, AccountsReceivable.PriceDisc.PriceDiscAdmTableService>();
            services.AddScoped<AccountsReceivable.PriceDisc.IPriceDiscAdmTransService, AccountsReceivable.PriceDisc.PriceDiscAdmTransService>();
            services.AddScoped<AccountsReceivable.PriceDisc.IPriceDiscAdmNameService, AccountsReceivable.PriceDisc.PriceDiscAdmNameService>();
            services.AddScoped<AccountsReceivable.ICustParametersService, AccountsReceivable.CustParametersService>();

            services.AddScoped<Foundation.Markup.IMarkupChargeCalculator, Foundation.Markup.MarkupChargeCalculator>();
            services.AddScoped<Foundation.Markup.IMarkupTransCommandService, Foundation.Markup.MarkupTransCommandService>();
            services.AddScoped<Foundation.Markup.IMarkupTransService, Foundation.Markup.MarkupTransService>();
            services.AddScoped<FluentValidation.IValidator<Foundation.Markup.MarkupTransDto>, Foundation.Markup.MarkupTransValidator>();
            services.AddScoped<Foundation.Markup.IMarkupTableService, Foundation.Markup.MarkupTableService>();
            services.AddScoped<Foundation.Tax.ITaxTableService, Foundation.Tax.TaxTableService>();
            services.AddScoped<Foundation.Tax.ITaxGroupService, Foundation.Tax.TaxGroupService>();
            services.AddScoped<Foundation.Tax.ITaxItemGroupService, Foundation.Tax.TaxItemGroupService>();
            services.AddScoped<Foundation.Tax.ITaxPeriodHeadCrudService, Foundation.Tax.TaxPeriodHeadCrudService>();
            services.AddScoped<Foundation.Tax.ITaxLedgerAccountGroupService, Foundation.Tax.TaxLedgerAccountGroupService>();
            services.AddScoped<Foundation.Tax.ITaxExemptCodeService, Foundation.Tax.TaxExemptCodeService>();
            services.AddScoped<Foundation.Tax.ITaxGroupQueryService, Foundation.Tax.TaxGroupQueryService>();
            services.AddScoped<Foundation.Tax.ITaxGroupLineService, Foundation.Tax.TaxGroupLineService>();
            services.AddScoped<Foundation.Tax.ITaxGroupCommandService, Foundation.Tax.TaxGroupCommandService>();
            services.AddScoped<Foundation.Tax.ITaxItemGroupQueryService, Foundation.Tax.TaxItemGroupQueryService>();
            services.AddScoped<Foundation.Tax.ITaxItemGroupLineService, Foundation.Tax.TaxItemGroupLineService>();
            services.AddScoped<Foundation.Tax.ITaxItemGroupCommandService, Foundation.Tax.TaxItemGroupCommandService>();
            services.AddScoped<Foundation.Tax.ITaxCodeRateService, Foundation.Tax.TaxCodeRateService>();
            services.AddScoped<Foundation.Tax.ITaxPeriodService, Foundation.Tax.TaxPeriodService>();
            services.AddScoped<Foundation.Tax.ITaxAuthorityAddressService, Foundation.Tax.TaxAuthorityAddressService>();
            services.AddScoped<Inventory.IInventSiteService, Inventory.InventSiteService>();
            services.AddScoped<Inventory.IReleasedProductListService, Inventory.ReleasedProductListService>();
            services.AddScoped<Inventory.IInventItemBarcodeService, Inventory.InventItemBarcodeService>();
            services.AddScoped<Inventory.ProductCategories.IProductCategoryService, Inventory.ProductCategories.ProductCategoryService>();
            services.AddScoped<Inventory.IEcoResProductService, Inventory.EcoResProductService>();
            services.AddScoped<FluentValidation.IValidator<Inventory.EcoResProductInputDto>, Inventory.EcoResProductInputValidator>();
            services.AddScoped<Inventory.IStorageDimensionGroupService, Inventory.StorageDimensionGroupService>();
            services.AddScoped<Inventory.ITrackingDimensionGroupService, Inventory.TrackingDimensionGroupService>();
            services.AddScoped<FluentValidation.IValidator<Inventory.StorageDimensionGroupInputDto>, Inventory.StorageDimensionGroupInputValidator>();
            services.AddScoped<FluentValidation.IValidator<Inventory.TrackingDimensionGroupInputDto>, Inventory.TrackingDimensionGroupInputValidator>();
            services.AddScoped<Inventory.IInventModelGroupService, Inventory.InventModelGroupService>();
            services.AddScoped<Inventory.IInventItemGroupService, Inventory.InventItemGroupService>();
            services.AddScoped<Inventory.IInventPostingService, Inventory.InventPostingService>();
            services.AddScoped<FluentValidation.IValidator<Inventory.InventSiteUpdateValidation>, Inventory.InventSiteUpdateValidator>();
            services.AddScoped<Inventory.IInventLocationService, Inventory.InventLocationService>();
            services.AddScoped<Inventory.IInventTransService, Inventory.InventTransService>();
            services.AddScoped<Inventory.IInventOnHandService, Inventory.InventOnHandService>();
            services.AddScoped<Inventory.IUnitOfMeasureService, Inventory.UnitOfMeasureService>();
            services.AddScoped<Inventory.IInventParametersService, Inventory.InventParametersService>();
            services.AddScoped<Inventory.IInventJournalNameService, Inventory.InventJournalNameService>();
            services.AddScoped<Inventory.IInventJournalService, Inventory.InventJournalService>();
            services.AddScoped<Inventory.IInventJournalPostingService, Inventory.InventJournalPostingService>();
            services.AddScoped<Inventory.IWMSLocationService, Inventory.WMSLocationService>();
            services.AddScoped<FluentValidation.IValidator<Inventory.InventJournalNameInputDto>, Inventory.InventJournalNameInputValidator>();
            services.AddScoped<FluentValidation.IValidator<Inventory.InventLocationUpdateValidation>, Inventory.InventLocationUpdateValidator>();

            services.AddScoped<Foundation.LogisticsAddresses.IAddressBookQueryService, Foundation.LogisticsAddresses.AddressBookQueryService>();

            // Explicit Finance registrations
            services.AddScoped<Foundation.Currency.ICurrencyService, Foundation.Currency.CurrencyService>();
            services.AddScoped<Foundation.Currency.IExchangeRateTypeService, Foundation.Currency.ExchangeRateTypeService>();
            services.AddScoped<Foundation.Currency.IExchangeRateCurrencyPairService, Foundation.Currency.ExchangeRateCurrencyPairService>();
            services.AddScoped<Foundation.Currency.IExchangeRateService, Foundation.Currency.ExchangeRateService>();
            services.AddScoped<Foundation.DeliveryModes.IDlvModeService, Foundation.DeliveryModes.DlvModeService>();
            services.AddScoped<Foundation.DeliveryTerms.IDlvTermService, Foundation.DeliveryTerms.DlvTermService>();
            services.AddScoped<Foundation.LegalEntities.ICompanyInfoService, Foundation.LegalEntities.CompanyInfoService>();
            services.AddScoped<Foundation.LogisticsAddresses.IElectronicAddressService, Foundation.LogisticsAddresses.ElectronicAddressService>();
            services.AddScoped<Foundation.LogisticsAddresses.IGlobalAddressBookService, Foundation.LogisticsAddresses.GlobalAddressBookService>();
            services.AddScoped<Foundation.LogisticsAddresses.ILocationService, Foundation.LogisticsAddresses.LocationService>();
            services.AddScoped<Foundation.LogisticsAddresses.IPartyLocationService, Foundation.LogisticsAddresses.PartyLocationService>();
            services.AddScoped<Foundation.LogisticsAddresses.IPartyService, Foundation.LogisticsAddresses.PartyService>();
            services.AddScoped<Foundation.LogisticsAddresses.IPostalAddressService, Foundation.LogisticsAddresses.PostalAddressService>();
            services.AddScoped<Foundation.PaymentSchedules.IPaymSchedLineService, Foundation.PaymentSchedules.PaymSchedLineService>();
            services.AddScoped<Foundation.PaymentSchedules.IPaymSchedService, Foundation.PaymentSchedules.PaymSchedService>();
            services.AddScoped<Foundation.PaymentTerms.IPaymTermService, Foundation.PaymentTerms.PaymTermService>();
            services.AddScoped<Foundation.Genders.IGenderService, Foundation.Genders.GenderService>();
            services.AddScoped<Foundation.Nationalities.IHcmNationalityService, Foundation.Nationalities.HcmNationalityService>();
            services.AddScoped<Foundation.Occupations.IHcmOccupationService, Foundation.Occupations.HcmOccupationService>();
            services.AddScoped<Foundation.Departments.IHcmDepartmentService, Foundation.Departments.HcmDepartmentService>();
            services.AddScoped<Foundation.HcmShowrooms.IHcmShowroomService, Foundation.HcmShowrooms.HcmShowroomService>();
            services.AddScoped<Foundation.HcmWorkers.IHcmWorkerService, Foundation.HcmWorkers.HcmWorkerService>();
            services.AddScoped<Foundation.Structure.OrganizationStructureService>();
            services.AddScoped<Foundation.Structure.IOrganizationStructureService>(provider => provider.GetRequiredService<Foundation.Structure.OrganizationStructureService>());
            services.AddScoped<IAX.IXApi.Shared.Application.Organization.IOrganizationDirectory,
                Foundation.Structure.OrganizationStructureService>();
            return services;
        }
    }
}

