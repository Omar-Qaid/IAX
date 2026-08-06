namespace IAX.IXApi.Modules.ERP.Common
{
    public enum EvenOdd
    {
        All = 0,
        Even = 1,
        Odd = 2
    }
    public enum CustCollectionLetterCode
    {
        None = 0,
        CollectionLetter1 = 1,
        CollectionLetter2 = 2,
        CollectionLetter3 = 3,
        CollectionLetter4 = 4,
        Collection = 5,
        All = 6,
        CollectionPerCust = 7
    }
    public enum NetCurrent
    {
        Net = 0,
        CurrentMth = 1,
        CurrentQuart = 2,
        CurrentYear = 3,
        CurrentWeek = 4,
        COD = 5,
        CutOffDate = 6
    }
    public enum SalesQuotationCreationMethod
    {
        SCM = 0,
        Dynamics365Sales = 1
    }
    public enum SalesQuotationStatus
    {
        Created = 0,
        Sent = 1,
        Confirmed = 2,
        Lost = 3,
        Cancelled = 4,
        Reset = 5,
        Modified = 6,
        Submitted = 7,
        Approved = 8,
        Revised = 9
    }
    public enum ProjTransType
    {
        None = 0,
        Revenue = 1,
        Hour = 2,
        Cost = 3,
        Item = 4,
        OnAccount = 5,
        WIP = 6,
        IndirectComponent = 7,
        Retainage = 8
    }
    public enum QuotationType
    {
        Sales = 1,
        Project=2
    }
    public enum CustPaymentType
    {
        Other = 0,
        ElectronicPayment = 1,
        Check = 2,
        BillOfExchange = 3,
        CreditCard = 4
    }
    public enum CustVendPaymStatus
    {
        None = 0,
        Sent = 1,
        Received = 2,
        Confirmed = 3,
        Rejected = 4,
        Delete = 5,
        SentDelete = 6,
        Deleted = 7
    }
    public enum CustVendorBlocked
    {
        No = 0,
        Invoicing = 1,
        All = 2
    }
    public enum InventModuleType : byte
    {
        Inventory = 0,
        Purchase = 1,
        Sales = 2
    }
    public enum DocumentStatus
    {
        None = 0,
        Quotation = 1,
        PurchaseOrder = 2,
        Confirmation = 3,
        PickingList = 4,
        PackingSlip = 5,
        ReceiptsList = 6,
        Invoice = 7,
        ApproveJournal = 8,
        ProjectInvoice = 9,
        ProjectPackingSlip = 10,
        CRMQuotation = 11,
        Lost = 12,
        Cancelled = 13,
        FreeTextInvoice = 14,
        RFQ = 15,
        RFQAccept = 16,
        RFQReject = 17,
        PurchReq = 18,
        RFQReSend = 19
    }

    public enum SalesType
    {
        Journal = 0,
        DEL_Quotation = 1,
        Subscription = 2,
        Sales = 3,
        ReturnItem = 4,
        DEL_Blanket = 5,
        ItemReq = 6,
        Prepayment = 7
    }

    public enum SalesStatus
    {
        None = 0,
        Backorder = 1,
        Delivered = 2,
        Invoiced = 3,
        Canceled = 4
    }
    public enum ReturnStatusHeader
    {
        None = 0,
        Created = 1,
        Open = 2,
        Canceled = 3,
        Closed = 4
    }
    public enum AddressType : int
    {
        Delivery = 0,
        Invoice = 1,
        Other = 2
    }

    public enum CustTransType : byte
    {
        None = 0,
        SalesTable = 1,
        Invoice = 2,
        Payment = 3,
        Interest = 4,
        CollectionLetter = 5,
        Project = 6,
        ReturnItem = 7,
        Adjustment = 8
    }
    public enum RoundOffType
    {
        Ordinary = 0,

        RoundDown = 1,

        RoundUp = 2
    }
    public enum InventAccountType
    {
        PurchPackingSlip = 0,
        PurchPackingSlipOffsetAccount,
        PurchReceipt,
        PurchConsump,
        PurchDisc,

        InventStdProfit,
        InventStdLoss,

        InventIssue,
        InventLoss,
        InventReceipt,
        InventProfit,

        ProdPicklist,
        ProdPicklistOffsetAccount,

        ProdReportFinished,
        ProdReportFinishedOffsetAccount,

        ProdIssue,
        ProdIssueOffsetAccount,

        ProdReceipt,
        ProdReceiptOffsetAccount,

        SalesPackingSlip,
        SalesPackingSlipOffsetAccount,

        SalesIssue,
        SalesConsump,
        SalesRevenue,
        SalesDisc,

        SalesCommission,
        SalesCommissionOffsetAccount,

        PurchStdProfit,
        PurchStdLoss,
        PurchStdOffsetAccount,

        SalesPckSlpRevenue,
        SalesPckSlpRevenueOffsetAccount,

        PurchCharge,
        PurchStockVariation,

        PurchPackingSlipPurchaseOffsetAccount,
        PurchPackingSlipTax,

        DEL_PurchPackingSlipPurchase,

        SalesPackingSlipTax,

        InventStdCostRevaluation,

        PurchStdCostPurchasePriceVariance,
        ProdStdCostProductionVariance,

        InventInterUnitPayable,
        InventInterUnitReceivable,

        InventStdCostChangeVariance,

        ProdStdCostLotSizeVariance,
        ProdStdCostQuantityVariance,
        ProdStdCostSubstitutionVariance,

        InventStdCostRoundingVariance,

        PurchReceiptFixedAsset,
        InventIssueFixedAsset,

        TransferIssue_IN,
        TransferReceipt_IN,

        TransferProfit_IN,
        TransferLoss_IN,

        TransferGoodsTransit_IN,
        TransferScrap_IN,

        ProdLeanWIPServiceReceipt,
        ProdLeanWIPServiceClearing,

        PurchAdvance,
        PurchExpense,

        InventMovingAveragePriceDifference,
        InventMovingAverageCostRevaluation,

        DEL_PmfCoByProduct,
        DEL_PmfCoByProductOffsetAccount,

        PdsCWLoss,
        PdsCWProfit,

        InventRoundingLoss_RU,
        InventRoundingProfit_RU,

        SalesGoodsInRoute_RU,
        SalesGoodsInRouteOffsetAccount_RU,

        TransferInterim_IN,

        SalesReturn,
        SalesCancel,
        SalesAdvance
    }
    public enum LedgerPostingType
    {
        None = 0,

        ExchRateGain,
        ExchRateLoss,
        InterCompany,
        Tax,
        VATRoundOff,
        Allocation,
        InvestmentDuty,
        Liquidity,
        MSTDiffSecond,
        ErrorAccount,
        MSTDiff,
        YearResult,
        Closing,
        LedgerJournal,
        CashDiscount,
        ConsolidateDiffBalance,
        PaymentFee,
        TaxReport,
        TransferOpeningClosing,
        Bank,
        ConversionProfit,
        ConversionLoss,
        TaxWithhold,
        ConsolidateDiffProfitLoss,

        IndirectEstimatedAbsorptionOffset,
        IndirectAbsorption,
        IndirectAbsorptionOffset,

        FreeTextInvoice,

        ConversionReportingLoss,
        ConversionReportingProfit,

        CustBalance,
        CustRevenue,
        CustInterest,
        CustCashDisc,
        CustCollectionLetterFee,
        CustInterestFee,
        CustInvoiceDisc,
        CustPayment,
        CustReimbursement,
        CustSettlement,

        VendBalance,
        VendPurchLedger,
        VendOffsetAccount,
        VendInterest,
        VendCashDisc,
        VendPayment,
        VendInvoiceDisc,
        VendSettlement,

        CrossCompanySettlement,

        InventIssueFixedAsset,

        SalesRevenue,
        SalesConsump,
        SalesDisc,
        SalesCash,
        SalesFreight,
        SalesFee,
        SalesPostage,
        SalesRoundOff,
        SalesPackingSlip,
        SalesOffsetAccountPackingSlip,
        SalesIssue,
        SalesCommission,

        PdsCWProfit,

        PurchConsump,
        PurchDisc,
        PurchCash,
        PurchFreight,
        PurchFee,
        PurchPostage,
        PurchOffsetAccount,
        PurchaseInvoiceRoundOff,

        PurchMarkupFreight,
        PurchMarkupCustoms,
        PurchMarkupInsurance,

        PurchPckSlp,
        PurchOffsetAccountPckSlp,

        PurchReceipt,

        PurchStdProfit,
        PurchStdLoss,
        PurchStdOffsetAccount,

        InventReceipt,
        InventIssue,
        InventProfit,
        InventLoss,
        InventStdProfit,
        InventStdLoss,

        Opening_ES,

        PurchReq,
        PurchOrder,
        APInvoice,

        Budget,

        PurchOrderYearEnd,

        InflationAdjustment_MX,

        ProdReportFinished,
        ProdReportFinishedOffsetAccount,

        ProdIssue,
        ProdIssueOffsetAccount,

        ProdReceipt,
        ProdReceiptOffsetAccount,

        ProdPicklistOffsetAccount,
        ProdPicklist,

        ProdWIPValuation
    }
    public enum PostingType
    {
        CustomerSummary = 0,            // AR control
        SalesRevenue = 1,
        VatPayable = 2,                 // Output VAT
        Cogs = 3,
        InventoryPhysical = 4,
        DeferredCogs = 5,
        InventoryAdjustmentOffset = 6,
        AccountsPayable = 7,            // AP control
        AccruedPurchases = 8,           // Received not invoiced
        InputVat = 9,                   // Recoverable purchase VAT
        Bank = 10,                      // Cash / bank settlement account
        ChargeExpense = 11,             // Purchase charges (freight, handling...)
        ChargeIncome = 12               // Sales charges billed to customers
    }
    public enum MainAccountType : int
    {
        Asset = 1,
        Liability = 2,
        Equity = 3,
        Revenue = 4,
        Expense = 5
    }

    public enum MainAccountPostingType : int
    {
        None = 0,
        Customer = 1,
        Vendor = 2,
        Bank = 3,
        SalesTax = 4,
        Inventory = 5,
        FixedAsset = 6,
        Project = 7
    }
    public enum LedgerAccountType : byte
    {
        Customer = 1,
        Revenue = 2,
        Tax = 3,
        Inventory = 4,
        CostOfGoods = 5
    }
    public enum LedgerTransType
    {
        None = 0,
        Transfer = 1,
        Sales = 2,
        Purch = 3,
        Invent = 4,
        Production = 5,
        Project = 6,
        Interest = 7,
        Cust = 8,
        ExchAdjustment = 9,
        SummedUp = 10,
        Payroll = 11,
        FixedAssets = 12,
        CollectionLetter = 13,
        Vend = 14,
        Payment = 15,
        Tax = 16,
        Bank = 17,
        Conversion = 18,
        BillOfExchange = 19,
        PromissoryNote = 20,
        Cost = 21,
        Work = 22,
        Fee = 23,
        Settlement = 24,
        Allocation = 25,
        Elimination = 26,
        CashDiscount = 27,
        OverUnder = 28,
        PennyDifference = 29,
        CrossCompanySettlement = 30,
        PurchReq = 31,

        PurchAdvanceApplication = 32,

        ConversionReporting = 33,

        RDeferrals = 34,

        PdsRebateCreditNote = 35,

        PdsRebatePassToAp = 36,

        WriteOff = 37,

        GeneralJournal = 38,

        MCRUnderpayWriteOff = 39,

        CustVendNetting = 40,

        SalesPrepayment = 41
    }
    public enum LedgerJournalACType
    {
        Ledger = 0,
        Cust = 1,
        Vend = 2,
        Project = 3,
        FixedAssets = 4,
        Bank = 5,
        FixedAssetsRu = 6,
        EmployeeRu = 7,
        RDeferrals = 8,
        RCash = 9
    }
    public enum LedgerJournalType
    {
        Daily = 0,
        CustPayment = 1,
        VendDisbursement = 2
    }

    public enum LedgerJournalCategory
    {
        SalesInvoice = 1,
        CustomerPayment = 2,
        General = 3
    }
    public enum FiscalYearStatus : byte
    {
        Open = 0,
        Closed = 1
    }

    /// <summary>Nature of a fiscal period (Dynamics AX period type).</summary>
    public enum FiscalPeriodType : byte
    {
        Operating = 0,
        Opening = 1,
        Closing = 2,
        Adjustment = 3
    }

    /// <summary>Status of a fiscal period.</summary>
    public enum FiscalPeriodStatus : byte
    {
        Open = 0,
        OnHold = 1,
        Closed = 2,
        YearClosed = 3
    }

    public enum FiscalCalendarType : byte
    {
        Regular = 0,
        Fiscal = 1,
        Tax = 2
    }
    public enum InventJournalType : byte
    {
        Movement = 0,
        Adjustment = 1,
        Counting = 2
    }
    public enum NoYes
    {
        No  = 0,
        Yes = 1
    }

    /// <summary>Represents the physical data-lifecycle state of a record row (SalesTable.SysDataStateCode, SalesLine.SysDataStateCode, etc.).</summary>
    // SalesTable: SysDataStateCode
    // SalesLine:  SysDataStateCode
    public enum SysDataStateCode
    {
        Normal   = 0,
        Archived = 1
    }

    /// <summary>Source that created the sales order or line (SalesTable.SystemEntrySource, SalesLine.SystemEntrySource).</summary>
    // SalesTable: SystemEntrySource
    // SalesLine:  SystemEntrySource
    public enum SalesSystemEntrySource
    {
        None       = 0,
        Online     = 1,
        CallCenter = 2
    }

    public enum InventTransType : byte
    {
        None = 0,
        Receipt = 1,
        Issue = 2
    }


    public enum StatusIssue : byte
    {
        None = 0,
        Quotation = 1,
        Ordered = 2,
        Picked = 3,
        Deducted = 4,
        Sold = 5,
        Released = 6
    }
 
    public enum StatusReceipt : byte
    {
        None = 0,
        Ordered = 1,
        Arrived = 2,
        Registered = 3,
        Received = 4,
        Purchased = 5
    }
    /// <summary>Inventory reference type — shared across all inventory transactions (SalesLine.InventRefType, CustInvoiceTrans.InventRefType, CustPackingSlipTrans.InventRefType).</summary>
    // SalesLine: InventRefType
    public enum InventRefType : byte
    {
        None = 0,
        PurchaseOrder = 1,
        SalesTable = 2,
        InventoryJournal = 3,
        TransferOrder = 4,
        ProductionOrder = 5,
        InventoryAdjustment = 6,
        PhysicalinventoryCount = 7,
        ReturnOrder = 8
    }

    // ── Sales Order Header (SalesTable) ───────────────────────────────────────

    /// <summary>Controls when and how delivery dates are calculated (SalesTable.DeliveryDateControlType, SalesLine.DeliveryDateControlType).</summary>
    // SalesTable: DeliveryDateControlType
    // SalesLine:  DeliveryDateControlType
    public enum SalesDlvDateControlType
    {
        None          = 0,
        SalesLeadTime = 1,
        ATP           = 2,
        CTP           = 3
    }

    /// <summary>Default reservation method applied when lines are added (SalesTable.Reservation, SalesLine.Reservation).</summary>
    // SalesTable: Reservation
    // SalesLine:  Reservation
    public enum SalesAutoReservation
    {
        None         = 0,
        Standard     = 1,
        SameLocation = 2,
        SameWH       = 3,
        SameSite     = 4
    }

    /// <summary>Indicates whether the order has been released to the warehouse (SalesTable.ReleaseStatus).</summary>
    // SalesTable: ReleaseStatus
    public enum SalesReleaseStatus
    {
        NotSpecified = 0,
        Available    = 1,
        Released     = 2,
        Released_All = 3
    }

    /// <summary>EU sales list reporting classification (SalesTable.ListCode, CustInvoiceJour.ListCode, CustPackingSlipJour.ListCode).</summary>
    // SalesTable: ListCode
    public enum SalesListCode
    {
        None  = 0,
        EU    = 1,
        NotEU = 2,
        Both  = 3
    }

    /// <summary>Voucher settlement approach used during automatic settlement (SalesTable.SettleVoucher).</summary>
    // SalesTable: SettleVoucher
    public enum CustVendSettleVoucher
    {
        PostedVoucher = 0,
        Automatic     = 1,
        Selection     = 2
    }

    /// <summary>Direction of an intercompany relationship on the sales order (SalesTable.IntercompanyOrigin, SalesLine.IntercompanyOrigin).</summary>
    // SalesTable: IntercompanyOrigin
    // SalesLine:  IntercompanyOrigin
    public enum SalesIntercompanyOrigin
    {
        None     = 0,
        Outbound = 1,
        Inbound  = 2
    }

    /// <summary>Giro payment slip type printed on the invoice (SalesTable.GiroType, CustInvoiceJour.GiroType).</summary>
    // SalesTable: GiroType
    public enum SalesGiroType
    {
        None        = 0,
        BG          = 1,
        PlusgiroFI  = 2,
        PlusgiroSE  = 3
    }

    /// <summary>How the freight slip ID is generated on the packing slip (SalesTable.FreightSlipType, CustPackingSlipJour.FreightSlipType).</summary>
    // SalesTable: FreightSlipType
    public enum SalesFreightSlipType
    {
        None          = 0,
        InvoiceNumber = 1,
        PackingSlipId = 2,
        Blank         = 3
    }

    /// <summary>Type of bank guarantee or letter of credit attached to the order (SalesTable.BankDocumentType).</summary>
    // SalesTable: BankDocumentType
    public enum BankDocumentType
    {
        None               = 0,
        LetterOfCredit     = 1,
        LetterOfGuarantee  = 2
    }

    /// <summary>Module level at which automatic customer account statements are grouped (SalesTable.AutoSummaryModuleType).</summary>
    // SalesTable: AutoSummaryModuleType
    public enum CustAutoSummaryModule
    {
        None      = 0,
        SalesOrder = 1,
        Invoice   = 2
    }

    /// <summary>Master-planning demand coverage status (SalesTable.CovStatus).</summary>
    // SalesTable: CovStatus
    public enum CovStatus
    {
        None        = 0,
        Planned     = 1,
        Transferred = 2
    }

    /// <summary>Carrier delivery service type (SalesTable.ShipCarrierDlvType, SalesLine.ShipCarrierDlvType).</summary>
    // SalesTable: ShipCarrierDlvType
    // SalesLine:  ShipCarrierDlvType
    public enum WHSShipCarrierDlvType
    {
        None   = 0,
        Ground = 1,
        Air    = 2,
        Ocean  = 3,
        Other  = 4
    }

    /// <summary>Exception raised by Distributed Order Management processing (SalesTable.DomExceptionType, SalesLine.DomExceptionType).</summary>
    // SalesTable: DomExceptionType
    // SalesLine:  DomExceptionType
    public enum SalesDOMExceptionType
    {
        None               = 0,
        Ignored            = 1,
        ItemNotFound       = 2,
        SiteNotFound       = 3,
        OrderCannotFulfill = 4
    }

    /// <summary>Capable-to-promise full-run status from master planning (SalesTable.MpsFullRunCtpStatus, SalesLine.MpsFullRunCtpStatus).</summary>
    // SalesTable: MpsFullRunCtpStatus
    // SalesLine:  MpsFullRunCtpStatus
    public enum ReqFullCTPStatus
    {
        None     = 0,
        Complete = 1,
        Failed   = 2
    }

    /// <summary>Specifies how e-invoice line detail is included on electronic invoices (SalesTable.EInvoiceLineSpec, CustInvoiceJour.EInvoiceLineSpecific).</summary>
    // SalesTable: EInvoiceLineSpec
    public enum EInvoiceLineSpecification
    {
        None            = 0,
        LineSpec        = 1,
        LineSpecTaxSpec = 2
    }

    /// <summary>Credit-card pre-authorization failure reason (SalesTable.CreditCardAuthorizationError).</summary>
    // SalesTable: CreditCardAuthorizationError
    public enum SalesCreditCardAuthorizationError
    {
        None                       = 0,
        GeneralError               = 1,
        Declined                   = 2,
        CardExpired                = 3,
        InsufficientFunds          = 4,
        InvalidCardNumber          = 5
    }

    /// <summary>Physical tagging policy for cases on a sales order (SalesTable.CaseTagging, SalesLine.CaseTagging).</summary>
    // SalesTable: CaseTagging
    // SalesLine:  CaseTagging
    public enum WHSCaseTaggingPolicy
    {
        None         = 0,
        MandatoryAll = 1,
        OptionalAll  = 2
    }

    /// <summary>Physical tagging policy for individual items on a sales order (SalesTable.ItemTagging, SalesLine.ItemTagging).</summary>
    // SalesTable: ItemTagging
    // SalesLine:  ItemTagging
    public enum WHSItemTaggingPolicy
    {
        None         = 0,
        MandatoryAll = 1,
        OptionalAll  = 2
    }

    /// <summary>Physical tagging policy for pallets on a sales order (SalesTable.PalletTagging, SalesLine.PalletTagging).</summary>
    // SalesTable: PalletTagging
    // SalesLine:  PalletTagging
    public enum WHSPalletTaggingPolicy
    {
        None         = 0,
        MandatoryAll = 1,
        OptionalAll  = 2
    }

    // ── Sales Order Line (SalesLine) ──────────────────────────────────────────

    /// <summary>Whether a sales line is blocked from processing (SalesLine.Blocked).</summary>
    // SalesLine: Blocked
    public enum SalesLineBlocked
    {
        No  = 0,
        Yes = 1
    }

    /// <summary>Delivery mode overriding the header — Direct Delivery bypasses the warehouse (SalesLine.DeliveryType, SalesLine.LineDeliveryType).</summary>
    // SalesLine: DeliveryType, LineDeliveryType
    public enum SalesDeliveryType
    {
        None           = 0,
        Pickup         = 1,
        DirectDelivery = 2
    }

    /// <summary>Role a line plays in a revenue-recognition bundle (SalesLine.BundleLineType).</summary>
    // SalesLine: BundleLineType
    public enum RevRecBundleLineType
    {
        NotBundle  = 0,
        Bundle     = 1,
        Component  = 2
    }

    /// <summary>How the sales order was originally created (SalesLine.SalesSalesOrderCreationMethod).</summary>
    // SalesLine: SalesSalesOrderCreationMethod
    public enum SalesOrderCreationMethod
    {
        Manual    = 0,
        Automatic = 1
    }

    /// <summary>Whether the demand originated from a direct or indirect sourcing flow (SalesLine.SourcingOrigin).</summary>
    // SalesLine: SourcingOrigin
    public enum SalesLineSourcingOrigin
    {
        None     = 0,
        Direct   = 1,
        Indirect = 2
    }

    /// <summary>Type of service represented by a non-stocked sales line (SalesLine.ServiceLineType).</summary>
    // SalesLine: ServiceLineType
    public enum SalesServiceLineType
    {
        None        = 0,
        Vendor      = 1,
        Consignment = 2
    }

    /// <summary>Granularity at which soft reservation is blocked by availability rules (SalesLine.SoftReserveBlockLevel).</summary>
    // SalesLine: SoftReserveBlockLevel
    public enum WHSSoftReserveBlockLevel
    {
        None        = 0,
        ItemAndSite = 1,
        ItemSiteWH  = 2,
        ItemDimAll  = 3
    }

    /// <summary>Method used to allocate parent revenue across sub-billing child lines (SalesLine.SubBillRevenueSplitAllocationMethod, CustInvoiceTrans.SubBillRevenueSplitAllocationMethod).</summary>
    // SalesLine: SubBillRevenueSplitAllocationMethod
    public enum SubBillRevenueSplitAllocationMethod
    {
        EqualAmount = 0,
        Percentage  = 1
    }

    // ── Invoice and Packing Slip ───────────────────────────────────────────────

    /// <summary>Posting lifecycle state of an invoice or packing slip journal (CustInvoiceJour.PostedState, CustPackingSlipJour.PostedState).</summary>
    // CustInvoiceJour:    PostedState
    // CustPackingSlipJour: PostedState
    public enum CustPostedState
    {
        NotPosted  = 0,
        Posted     = 1,
        Corrected  = 2,
        Canceled   = 3
    }

    /// <summary>Identifies who is responsible for freight costs on the bill of lading (CustPackingSlipJour.BolFreightedBy).</summary>
    // CustPackingSlipJour: BolFreightedBy
    public enum BOLFreightedBy
    {
        None      = 0,
        Carrier   = 1,
        Shipper   = 2,
        Consignee = 3
    }

    // ==== InventoryManagement additions ====

    /// <summary>Classifies a released product as a physical item, a service, or a bill of materials (InventTable.ItemType).</summary>
    // InventTable: ItemType
    public enum ItemType
    {
        Item    = 0,
        Service = 1,
        BOM     = 2
    }

    // ==== AccountsReceivable additions ====

    /// <summary>Main account selection scope used by posting-account setup tables (CustLedgerAccounts.AccountCode).</summary>
    // CustLedgerAccounts: AccountCode
    public enum AccountCode
    {
        Table = 0,
        Group = 1,
        All   = 2
    }

    // ==== GeneralLedger additions ====

    /// <summary>D365FO base enum LedgerPostingLayer: which posting layer a ledger entry/journal targets.</summary>
    // GeneralJournalEntry.PostingLayer, LedgerJournalTable.CurrentOperationsTax, LedgerJournalName.CurrentOperationsTax
    public enum LedgerPostingLayer
    {
        Current = 0,
        Operations = 1,
        Tax = 2,
        OperationsTax = 3
    }

    /// <summary>D365FO base enum DetailSummary: whether a journal posts detail lines or a summarized total.</summary>
    // LedgerJournalName.DetailSummary, LedgerJournalTable.DetailSummaryPosting
    public enum DetailSummary
    {
        Detail = 0,
        Summary = 1
    }

    // ==== InventoryManagement additions (2nd pass) ====

    /// <summary>Cycle counting frequency policy (InventCountGroup.CountCode).</summary>
    // InventCountGroup: CountCode
    public enum InventCountCodePer
    {
        Manual  = 0,
        Period  = 1,
        Minimum = 2,
        Zero    = 3
    }

    /// <summary>ABC classification used for value/revenue/contribution-margin/tie-up analysis (InventTable.AbcContributionMargin, AbcRevenue, AbcTieUp, AbcValue).</summary>
    // InventTable: AbcContributionMargin, AbcRevenue, AbcTieUp, AbcValue
    public enum InventAbc
    {
        A = 0,
        B = 1,
        C = 2
    }

    // ==== AccountsReceivable additions (2nd pass) ====

    /// <summary>D365FO base enum WorkflowApprovalStatus: lifecycle status of a document submitted to a workflow (CustInvoiceTable.WorkflowApprovalStatus).</summary>
    // CustInvoiceTable: WorkflowApprovalStatus
    public enum WorkflowApprovalStatus
    {
        NotSubmitted = 0,
        Submitted = 1,
        InReview = 2,
        Approved = 3,
        Rejected = 4,
        Cancelled = 5
    }

    // ==== AccountsReceivable additions (4th pass) ====

    /// <summary>D365FO base enum SalesQuotationType: whether a sales quotation is for a specific item or a total amount (SalesQuotationTable.QuotationType, SalesQuotationLine.QuotationType, CustQuotationJour.QuotationType).</summary>
    // SalesQuotationTable: QuotationType
    // SalesQuotationLine:  QuotationType
    // CustQuotationJour:   QuotationType
    public enum SalesQuotationType
    {
        Item  = 0,
        Total = 1
    }

    /// <summary>D365FO base enum QuotationStatus: lifecycle status of a sales quotation (SalesQuotationTable.QuotationStatus, SalesQuotationLine.QuotationStatus).</summary>
    // SalesQuotationTable: QuotationStatus
    // SalesQuotationLine:  QuotationStatus
    public enum QuotationStatus
    {
        InProcess = 0,
        Sent      = 1,
        Confirmed = 2,
        Lost      = 3,
        Canceled  = 4,
        Revised   = 5,
        Expired   = 6
    }

    /// <summary>D365FO base enum TypeOfDraft: bill-of-exchange draft instrument type on a customer payment mode (CustPaymModeTable.TypeOfDraft).</summary>
    // CustPaymModeTable: TypeOfDraft
    public enum TypeOfDraft
    {
        NoDraft = 0,
        NoAcceptance = 1,
        Acceptance = 2,
        Promissory = 3,
        BankAcceptance = 4
    }

    /// <summary>D365FO base enum BillOfExchangeStatus: processing status of a bill-of-exchange transaction (CustTrans.BillOfExchangeStatus).</summary>
    // CustTrans: BillOfExchangeStatus
    public enum BillOfExchangeStatus
    {
        None        = 0,
        Created     = 1,
        Discounted  = 2,
        Collected   = 3,
        Rejected    = 4,
        Cancelled   = 5
    }

    // ==== Shared additions (5th pass) ====

    /// <summary>D365FO base enum FreightChargeTerm: who is responsible for freight charges on a delivery term (DlvTerm.FreightChargeTerm).</summary>
    // DlvTerm: FreightChargeTerm
    public enum FreightChargeTerm
    {
        ThirdParty = 0,
        Prepaid = 1,
        Collect = 2
    }

    /// <summary>D365FO base enum Sensitivity: confidentiality classification of a contact record (ContactPerson.Sensitivity).</summary>
    // ContactPerson: Sensitivity
    public enum Sensitivity
    {
        Normal = 0,
        Personal = 1,
        Private = 2,
        Confidential = 3
    }

    /// <summary>D365FO base enum InventProfileType: reservation/delivery inventory profile type on a customer account (CustTable.InventProfileType).</summary>
    // CustTable: InventProfileType
    public enum InventProfileType
    {
        None = 0,
        Fixed = 1,
        Period = 2
    }

    public enum BankAccountStatus
    {
        Active = 0,
        Inactive = 1,
        Closed = 2
    }
    public enum VendorContactRole
    {
        None = 0,
        Sales = 1,
        Escalations = 2,
        Primary = 3
    }
    public enum UseAltItemId
    {
        No = 0,
        Yes = 1,
        Always = 2
    }
    public enum UnitOfMeasureConversionRounding
    {
        None = 0,
        RoundUp = 1,
        RoundDown = 2,
        Nearest = 3
    }
    public enum UnitOfMeasureClass
    {
        Quantity = 0,
        Length = 1,
        Area = 2,
        Volume = 3,
        Mass = 4,
        Time = 5,
        Temperature = 6,
        Electricity = 7
    }
    public enum TransactionType
    {
        None = 0,
        Payment = 1,
        Deposit = 2,
        Fee = 3,
        Interest = 4,
        Tax = 5
    }
    public enum Timezone
    {
        GMT = 0,
        UTC = 1,
        Local = 2
    }
    public enum TaxWriteSelection
    {
        TaxCode = 0,
        TaxGroup = 1
    }
    public enum TaxType_W
    {
        Standard = 0,
        Reduced = 1,
        Zero = 2,
        Exempt = 3
    }
    public enum TaxReconcileAmountOrigin
    {
        Calculated = 0,
        Actual = 1
    }
    public enum TaxPrintDetail
    {
        Summary = 0,
        Detail = 1
    }
    public enum TaxPointBase
    {
        Delivery = 0,
        Invoice = 1,
        Payment = 2
    }
    public enum TaxPeriodUnit
    {
        Day = 0,
        Month = 1,
        Quarter = 2,
        Year = 3
    }
    public enum TaxOrigin
    {
        Percentage = 0,
        AmountPerUnit = 1,
        PercentageOfNet = 2,
        PercentageOfGross = 3
    }
    public enum TaxObligationCompany
    {
        None = 0,
        Company = 1,
        Vendor = 2
    }
    public enum TaxLimitBase
    {
        None = 0,
        Invoice = 1,
        Line = 2
    }
    public enum TaxHideAmountFields
    {
        No = 0,
        Yes = 1
    }
    public enum TaxGroupSource
    {
        None = 0,
        Customer = 1,
        Vendor = 2,
        Project = 3
    }
    public enum TaxGroupSetup
    {
        None = 0,
        Standard = 1,
        Custom = 2
    }
    public enum TaxGroupRounding
    {
        None = 0,
        RoundUp = 1,
        RoundDown = 2
    }
    public enum TaxDirectionControl
    {
        None = 0,
        Standard = 1,
        Custom = 2
    }
    public enum TaxDirection
    {
        Incoming = 0,
        Outgoing = 1
    }
    public enum TaxCountryRegionType
    {
        Domestic = 0,
        EU = 1,
        Foreign = 2
    }
    public enum TaxCalcMethod
    {
        Line = 0,
        Total = 1
    }
    public enum TaxBookTypeJournal
    {
        Standard = 0,
        Special = 1
    }
    public enum TaxBase
    {
        Net = 0,
        Gross = 1
    }
    public enum TaxAccountType
    {
        Standard = 0,
        Deferred = 1
    }
    public enum TableGroupAll
    {
        Table = 0,
        Group = 1,
        All = 2
    }
    public enum SystemOfUnits
    {
        Metric = 0,
        Imperial = 1
    }
    public enum SettleVoucher
    {
        PostedVoucher = 0,
        Automatic = 1,
        Selection = 2
    }
    public enum SalesPriceModelBasic
    {
        PurchPrice = 0,
        StandardCost = 1
    }
    public enum RoundingType
    {
        None = 0,
        RoundUp = 1,
        RoundDown = 2
    }
    public enum RevRecRevenueType
    {
        None = 0,
        Revenue = 1,
        Deferred = 2
    }
    public enum RevRecLedgerPostingType
    {
        Standard = 0,
        Deferred = 1
    }
    public enum RevRecDeferredType
    {
        None = 0,
        Deferred = 1
    }
    public enum RetailInventJournalPosAdjustmentType
    {
        Standard = 0,
        Pos = 1
    }
    public enum RCashPayTransType
    {
        None = 0,
        Receipt = 1,
        Payment = 2
    }
    public enum RCashDocRepresType
    {
        Standard = 0,
        Representative = 1
    }
    public enum QmsDispensingControl
    {
        None = 0,
        Controlled = 1
    }
    public enum QmsAuthorizedPersonnel
    {
        No = 0,
        Yes = 1
    }
    public enum PurchLedgerPosting
    {
        None = 0,
        Standard = 1
    }
    public enum ProfitSet
    {
        Standard = 0,
        Percentage = 1
    }
    public enum ProdFlushingPrincip
    {
        Start = 0,
        Finish = 1,
        Manual = 2
    }
    public enum PriceModel
    {
        Standard = 0,
        Custom = 1
    }
    public enum PreferredPriceType
    {
        SalesPrice = 0,
        CostPrice = 1
    }
    public enum PmfProductType
    {
        None = 0,
        Product = 1,
        CoProduct = 2,
        ByProduct = 3
    }
    public enum PickingListBatchExpirationDateValidationRule
    {
        None = 0,
        Validate = 1
    }
    public enum PeriodUnit
    {
        Day = 0,
        Week = 1,
        Month = 2,
        Year = 3
    }
    public enum PdsPotencyAttribRecording
    {
        None = 0,
        Record = 1
    }
    public enum PdsPickCriteria
    {
        None = 0,
        ExpiryDate = 1
    }
    public enum PaymSchedTaxDist
    {
        None = 0,
        Proportionate = 1
    }
    public enum PaymSchedMiscChargeDist
    {
        None = 0,
        Proportionate = 1
    }
    public enum PaymSchedAllocateMethod
    {
        Equal = 0,
        Amount = 1
    }
    public enum OrganizationType
    {
        None = 0,
        Company = 1,
        LegalEntity = 2
    }
    public enum OmOperatingUnitType
    {
        None = 0,
        Department = 1,
        CostCenter = 2,
        ValueStream = 3,
        BusinessUnit = 4
    }
    public enum ModuleInventPurchSales
    {
        None = 0,
        Inventory = 1,
        Purchase = 2,
        Sales = 3
    }
    public enum MatchingPolicy
    {
        None = 0,
        TwoWay = 1,
        ThreeWay = 2
    }
    public enum MarkupModuleType
    {
        None = 0,
        Customer = 1,
        Vendor = 2,
        Sales = 3
    }
    public enum MarkupModuleCategory
    {
        None = 0,
        Category = 1
    }
    public enum MarkupCategory
    {
        None = 0,
        Category = 1
    }
    public enum MaritalStatus
    {
        None = 0,
        Single = 1,
        Married = 2,
        Divorced = 3,
        Widowed = 4
    }
    public enum MainAccountCloseType
    {
        Standard = 0,
        Result = 1
    }
    public enum ListCode
    {
        None = 0,
        EU = 1,
        NonEU = 2
    }
    public enum LedgerJournalVoucherChange
    {
        None = 0,
        Change = 1
    }
    public enum LedgerJournalTransStatus
    {
        None = 0,
        Approved = 1,
        Rejected = 2
    }
    public enum LedgerJournalFeePosting
    {
        None = 0,
        Post = 1
    }
    public enum JournalVoucherDraw
    {
        None = 0,
        Draw = 1
    }
    public enum JournalVoucherChange
    {
        None = 0,
        Change = 1
    }
    public enum ItmCostArea
    {
        Standard = 0,
        Custom = 1
    }
    public enum ItemReservation
    {
        None = 0,
        Automatic = 1
    }
    public enum ItemProdReservation
    {
        None = 0,
        Automatic = 1
    }
    public enum InventTransOpen
    {
        No = 0,
        Yes = 1
    }
    public enum InventTransChildType
    {
        None = 0,
        Child = 1
    }
    public enum InventSettleType
    {
        None = 0,
        FinancialClose = 1,
        RecalculationAdjustment = 2
    }
    public enum InventPostingCostCode
    {
        None = 0,
        StandardCost = 1
    }
    public enum InventModel
    {
        FIFO = 0,
        LIFO = 1,
        LIFO_Date = 2,
        WeightedAvg = 3,
        WeightedAvg_Date = 4,
        StandardCost = 5,
        MovingAverage = 6
    }
    public enum InventJournalOriginType
    {
        Standard = 0,
        Migration = 1
    }
    public enum InventCountCode
    {
        Manual = 0,
        Period = 1,
        Minimum = 2,
        Zero = 3
    }
    public enum InventCostStatus
    {
        None = 0,
        Finished = 1,
        Running = 2,
        Error = 3
    }
    public enum InventAdjustmentType
    {
        None = 0,
        Adjustment = 1,
        Closing = 2
    }
    public enum InventAdjustmentSpec
    {
        Total = 0,
        Item = 1,
        ItemGroup = 2
    }
    public enum IntracomVatDueDate_W
    {
        None = 0,
        PostingDate = 1,
        DocumentDate = 2
    }
    public enum InternalCodeSymbol
    {
        None = 0,
        Symbol = 1
    }
    public enum Gender
    {
        None = 0,
        Male = 1,
        Female = 2,
        NonSpecific = 3
    }
    public enum FurtherPostingType
    {
        Standard = 0,
        Custom = 1
    }
    public enum FreqCode
    {
        Standard = 0,
        Custom = 1
    }
    public enum EuSalesListType
    {
        NotSpecified = 0,
        Item = 1,
        Service = 2,
        Triangulated = 3
    }
    public enum ElectronicAddressType
    {
        Phone = 0,
        Email = 1,
        URL = 2,
        Telex = 3,
        Fax = 4,
        InstantMessage = 5
    }
    public enum DetailSummaryPosting
    {
        Detail = 0,
        Summary = 1
    }
    public enum DebitCreditProposal
    {
        None = 0,
        Debit = 1,
        Credit = 2
    }
    public enum DebitCreditDemand
    {
        None = 0,
        Demand = 1
    }
    public enum DebitCreditCheck
    {
        None = 0,
        Check = 1
    }
    public enum CustVendNegInstProtestReason
    {
        None = 0,
        Reason = 1
    }
    public enum CustVendNegInstProtestProcess
    {
        None = 0,
        Process = 1
    }
    public enum CurrentToOperationsPostingLayer
    {
        Current = 0,
        Operations = 1,
        Tax = 2
    }
    public enum CountingStatusRegistrationPolicy
    {
        Standard = 0,
        Custom = 1
    }
    public enum CostingType
    {
        StandardCost = 0,
        PlannedCost = 1
    }
    public enum ContactSensitivity
    {
        Normal = 0,
        Personal = 1,
        Private = 2,
        Confidential = 3
    }
    public enum BomWhsReleasePolicy
    {
        Standard = 0,
        Custom = 1
    }
    public enum BatchMergedDateCalculationMethod
    {
        Standard = 0,
        Custom = 1
    }
    public enum BasePricePurchase
    {
        Standard = 0,
        Custom = 1
    }
    public enum BankTransSummarizationCriteria
    {
        Standard = 0,
        Custom = 1
    }
    public enum BankRevalDimensionSetting
    {
        Standard = 0,
        Custom = 1
    }
    public enum BankRemittanceType
    {
        Standard = 0,
        Custom = 1
    }
    public enum BankNsfFeeMarkupGroupModule
    {
        Standard = 0,
        Custom = 1
    }
    public enum BankCustPaymFeePost
    {
        Standard = 0,
        Custom = 1
    }
    public enum BankCodeType
    {
        Standard = 0,
        Custom = 1
    }
    public enum AssetTransType
    {
        Standard = 0,
        Custom = 1
    }
    public enum AssetLeaseStatus
    {
        Standard = 0,
        Custom = 1
    }
    public enum AssetLeasePostingTypes
    {
        Standard = 0,
        Custom = 1
    }
    public enum Agz_Ksa_DebitNoteType
    {
        Standard = 0,
        Custom = 1
    }
    public enum ABCValue
    {
        None = 0,
        A = 1,
        B = 2,
        C = 3
    }
    public enum ABCTieUp
    {
        None = 0,
        A = 1,
        B = 2,
        C = 3
    }
    public enum ABCRevenue
    {
        None = 0,
        A = 1,
        B = 2,
        C = 3
    }
    public enum ABCContributionMargin
    {
        None = 0,
        A = 1,
        B = 2,
        C = 3
    }
    public enum Abc
    {
        None = 0,
        A = 1,
        B = 2,
        C = 3
    }

    public enum ExchangeRateDisplayFactor
    {
        Undefined = 0,
        One = 1,
        Ten = 10,
        Hundred = 100,
        Thousand = 1000,
        TenThousand = 10000
    }
    public enum DimensionAttributeType
    {
        Standard = 0,
        Custom = 1
    }
    public enum DimensionBackingEntityType
    {
        Standard = 0,
        Custom = 1
    }
    public enum DimensionBackingEntityPerCompanyType
    {
        Standard = 0,
        Custom = 1
    }
    public enum QuotationLineCreationMethod
    {
        Standard = 0,
        Copy = 1
    }
    public enum QuotationOwnership
    {
        Company = 0,
        Customer = 1
    }
    public enum QuotationHeaderCreationMethod
    {
        Standard = 0,
        Copy = 1
    }
    public enum CustPaymentStatus
    {
        None = 0,
        Sent = 1,
        Approved = 2,
        Rejected = 3
    }
    public enum CustSettlementStatus
    {
        None = 0,
        Settled = 1
    }
    public enum UnitOfMeasureRounding
    {
        Nearest = 0,
        Up = 1,
        Down = 2
    }

    public enum InventLocationType
    {
        Standard = 0,
        Quarantine = 1,
        Transit = 2,
        Vendor = 3
    }
    public enum LoadReleaseReservationPolicy
    {
        FulfillRate = 0,
        PermitReleaseWithUnderdelivery = 1
    }
    public enum ReleaseToWarehouseRule
    {
        AllowPartialReservation = 0,
        RequireFullReservation = 1
    }
    public enum ReleaseRuleFailureOption
    {
        FailRelease = 0,
        ReleasePartial = 1
    }
    public enum WhsRawMaterialPolicy
    {
        RequireFullReservation = 0,
        AllowPartialReservation = 1
    }
    public enum RafPostingMethod
    {
        FlushingPrinciple = 0,
        Always = 1,
        Never = 2
    }

    public enum PaymMethod
    {
        Net = 0,
        CurrentMonth = 1,
        COD = 2,
        CutOffDate = 3
    }

    public enum CreditCardPaymentType
    {
        None = 0,
        Authorize = 1,
        Payment = 2,
        Refund = 3
    }

    public enum DimensionHierarchyStructureType
    {
        AccountStructure = 0,
        AdvancedRuleStructure = 1
    }

    public enum DimensionHierarchyFocusState
    {
        None = 0,
        Focus = 1,
        Active = 2
    }

    public enum TaxReportLayout
    {
        Default = 0,
        German = 1,
        English = 2,
        US = 3,
        Nordic = 4,
        Spanish = 5,
        Italian = 6,
        French = 7,
        Belgian = 8,
        Dutch = 9,
        Austrian = 10
    }

    public enum TaxRoundOffType
    {
        Ordinary = 0,
        RoundDown = 1,
        RoundUp = 2
    }

    public enum ItmVendType
    {
        None = 0,
        Broker = 1,
        ShippingCompany = 2,
        CustomsAgent = 3
    }

    public enum AccrueSalesTaxType
    {
        None = 0,
        Invoice = 1,
        Payment = 2
    }

    public enum CisStatus
    {
        None = 0,
        Gross = 1,
        Standard = 2,
        Higher = 3
    }

    public enum CustVendBlocked
    {
        No = 0,
        Invoicing = 1,
        All = 2,
        Payment = 3,
        Requisition = 4
    }

    public enum PurchAmountPurchaseOrder
    {
        None = 0,
        Invoice = 1,
        PackingSlip = 2
    }

    public enum Tax1099NameChoice
    {
        First = 0,
        Second = 1
    }

    public enum TaxIdType
    {
        Unknown = 0,
        EIN = 1,
        SSN = 2,
        ITIN = 3,
        ATIN = 4
    }

    public enum TaxVendorChargeTaxToleranceValidation
    {
        None = 0,
        Warning = 1,
        Error = 2
    }

    public enum VatNumTableType
    {
        None = 0,
        VAT = 1,
        TaxID = 2
    }

    public enum VendTableUseCashDisc
    {
        Normal = 0,
        Always = 1,
        Never = 2
    }

    public enum VendVendorCollaborationType
    {
        Disabled = 0,
        AutoPrompt = 1,
        Active = 2
    }

    public enum WorkflowState
    {
        NotSubmitted = 0,
        Submitted = 1,
        Pending = 2,
        Approved = 3,
        Rejected = 4,
        Completed = 5,
        Cancelled = 6
    }
}
