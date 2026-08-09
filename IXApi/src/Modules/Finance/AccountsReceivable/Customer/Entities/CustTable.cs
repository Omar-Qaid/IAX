using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [Table("CustTable")]
    public class CustTable : Entity<long>
    {
        //-----------------------------------------Customer Identification
        [StringLength(FieldLengths.AccountNum)]
        public string AccountNum { get; set; } = string.Empty;

        [StringLength(FieldLengths.CustCategory)]
        public string CustCategory { get; set; } = string.Empty;

        [StringLength(FieldLengths.CustGroupId)]
        public string CustGroupId { get; set; } = string.Empty;

        public long Party { get; set; }

        //-----------------------------------------Sales Configuration
        [StringLength(FieldLengths.SalesPoolId)]
        public string SalesPoolId { get; set; } = string.Empty;

        [StringLength(FieldLengths.InvoiceAccount)]
        public string InvoiceAccount { get; set; } = string.Empty;

        [StringLength(FieldLengths.VendAccount)]
        public string VendAccount { get; set; } = string.Empty;

        //-----------------------------------------Financial Configuration
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(32,6)")]
        public decimal CreditMax { get; set; }

        public NoYes MandatoryCreditLimit { get; set; }

        public CustVendorBlocked Blocked { get; set; }

        public long DefaultDimension { get; set; }

        //-----------------------------------------Payment Configuration
        [StringLength(FieldLengths.PaymModeId)]
        public string PaymModeId { get; set; } = string.Empty;

        [StringLength(FieldLengths.PaymTermId)]
        public string PaymTermId { get; set; } = string.Empty;

        public int CashDiscBaseDays { get; set; }

        public int UseCashDisc { get; set; }

        //-----------------------------------------Tax Configuration
        [StringLength(FieldLengths.TaxGroupId)]
        public string TaxGroupId { get; set; } = string.Empty;

        [StringLength(FieldLengths.VatNum)]
        public string VatNum { get; set; } = string.Empty;

        public NoYes VatFileAttachment { get; set; }

        public long VatNumRecId { get; set; }

        public int VatNumTableType { get; set; }

        public int OverrideSalesTax { get; set; }

        public int InclTax { get; set; }

        //-----------------------------------------Warehouse & Logistics
        [StringLength(FieldLengths.InventSiteId)]
        public string InventSiteId { get; set; } = string.Empty;

        [StringLength(FieldLengths.InventLocationId)]
        public string InventLocationId { get; set; } = string.Empty;

        [StringLength(FieldLengths.DlvModeId)]
        public string DlvModeId { get; set; } = string.Empty;

        //-----------------------------------------Address Information
        [StringLength(FieldLengths.CountryRegionId)]
        public string CountryRegionId { get; set; } = string.Empty;

        [StringLength(FieldLengths.StateId)]
        public string StateId { get; set; } = string.Empty;

        [StringLength(FieldLengths.PartyCountry)]
        public string PartyCountry { get; set; } = string.Empty;

        [StringLength(FieldLengths.PartyState)]
        public string PartyState { get; set; } = string.Empty;

        public int InvoiceAddress { get; set; }

        //-----------------------------------------Prepayment
        [Column(TypeName = "decimal(32,6)")]
        public decimal PrepaymentValue { get; set; }

        public int PrePayType { get; set; }

        public DateTime ExpiryDate { get; set; }

        //-----------------------------------------Statement Configuration
        public int AccountStatement { get; set; }

        public int AccStmtSign { get; set; }

        public int CollectionLetterCode { get; set; }

        public int GiroTypeAccountStatement { get; set; }

        public int GiroTypeCollectionLetter { get; set; }

        //-----------------------------------------Credit Card Verification
        public int CreditCardAddressVerification { get; set; }

        public int CreditCardAddressVerificationLevel { get; set; }

        public int CreditCardAddressVerificationVoid { get; set; }

        public int CreditCardCvc { get; set; }

        //-----------------------------------------Credit Management
        public decimal CredManCustCreditMaxAlt { get; set; }

        public DateTime CredManCustomerSince { get; set; }

        public int CredManCustUnlimitedCredit { get; set; }

        public DateTime CredManEligibleCreditLimitDate { get; set; }

        public decimal CredManEligibleCreditMax { get; set; }

        public int CredManExclude { get; set; }

        public DateTime CredManLastReviewDate { get; set; }

        public DateTime CredManNextSchedReviewDate { get; set; }

        public int CredManTitleHeld { get; set; }

        public int CredManWithAgency { get; set; }

        [StringLength(FieldLengths.CredManNotes)]
        public string? CredManNotes { get; set; }

        [StringLength(FieldLengths.CredManAccountStatusId)]
        public string CredManAccountStatusId { get; set; } = string.Empty;

        public DateTime CredManBusinessStarted { get; set; }

        public DateTime CredManCreditLimitDate { get; set; }

        public DateTime CredManCreditLimitExpiryDate { get; set; }

        //-----------------------------------------Collections & Settlement
        public int CustExcludeCollectionFee { get; set; }

        public int CustExcludeInterestCharges { get; set; }

        public long CustTradingPartnerCode { get; set; }

        public long CustWriteOffRefRecId { get; set; }

        public long DefaultDirectDebitMandate { get; set; }

        //-----------------------------------------Electronic Documents
        public int DocValid { get; set; }

        public int EInvoice { get; set; }

        public int EInvoiceAttachment { get; set; }

        //-----------------------------------------Shipping & Export
        public int EntryCertificateRequiredW { get; set; }

        public int ExpressBillOfLading { get; set; }

        public int FedNonFedIndicator { get; set; }

        //-----------------------------------------Forecast & Planning
        public int ForecastDmpInclude { get; set; }

        //-----------------------------------------Giro Configuration
        public int GiroType { get; set; }

        public int GiroTypeFreeTextInvoice { get; set; }

        public int GiroTypeInterestNote { get; set; }

        public int GiroTypeProjInvoice { get; set; }

        //-----------------------------------------Government & Compliance
        public long CompanyNafCode { get; set; }

        public int Government { get; set; }

        public int Cr { get; set; }

        public DateTime CrDate { get; set; }

        public DateTime CrEndDate { get; set; }

        public DateTime CrParentEndDate { get; set; }

        //-----------------------------------------Intercompany Configuration
        public int InterCompanyAllowIndirectCreation { get; set; }

        public int InterCompanyAutoCreateOrders { get; set; }

        public int InterCompanyDirectDelivery { get; set; }

        //-----------------------------------------System Configuration
        public int IsExternallyMaintained { get; set; }

        public int WorkflowState { get; set; }

        public DateTime ValidatedFrom { get; set; }

        public DateTime ValidatedTo { get; set; }

        //-----------------------------------------Banking & Payment Integration
        public long BankCustPaymIdTable { get; set; }

        public long LvPaymTransCodes { get; set; }

        //-----------------------------------------Customer Contacts
        public long MainContactWorker { get; set; }

        //-----------------------------------------Business Classification
        public int MainHolding { get; set; }

        public int JointVenture { get; set; }

        public int OneTimeCustomer { get; set; }

        //-----------------------------------------Address & Location
        public int NationalAddress { get; set; }

        public int SiteSketch { get; set; }

        //-----------------------------------------Trade & Shipping
        public int BlockFloorLimitUseInChannel { get; set; }

        public int PdsFreightAccrued { get; set; }

        public int ShipCarrierBlindShipment { get; set; }

        public int ShipCarrierFuelSurcharge { get; set; }

        //-----------------------------------------Import & Export Compliance
        public int IssueOwnEntryCertificateW { get; set; }

        //-----------------------------------------Quality Management
        public int QmsCustomerCheckItem { get; set; }

        public int QmsPrintCustSpecificCertOfAnalysis { get; set; }

        //-----------------------------------------RFID Configuration
        public int RfidCaseTagging { get; set; }

        public int RfidItemTagging { get; set; }

        public int RfidPalletTagging { get; set; }

        //-----------------------------------------Purchasing Configuration
        public int UsePurchRequest { get; set; }

        //-----------------------------------------System & Administration
        public int OpeningAccFile { get; set; }

        public int OwnerIdCopy { get; set; }

        public int RecipSign { get; set; }

        public int RevRecDisableInterCompany { get; set; }

        public int Stamp { get; set; }

        //-----------------------------------------Notes
        [StringLength(FieldLengths.Memo)]
        public string? Memo { get; set; }

        #region Navigation Properties Row     
        #endregion

        #region Navigation Properties List
        #endregion
    }
}

