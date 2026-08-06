using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("DimensionAttributeValueCombination")]
    public class DimensionAttributeValueCombination : Entity<long>
    {
        //----------------------------------------- Core Information & Display Strings
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.DisplayValue)]
        public string DisplayValue { get; set; } = string.Empty;

        public byte[]? Hash { get; set; } // varbinary mapping supporting Nullable = YES
        public int HashVersion { get; set; }
        public long AccountStructure { get; set; }

        // Enum Properties
        public int LedgerDimensionType { get; set; } // Map to LedgerDimensionType Enum if preferred

        // ==========================================================
        // Main Core Accounting Dimensions (IDs & String Values)
        // ==========================================================
        // Basic Properties
        public long MainAccount { get; set; }
        [StringLength(FieldLengths.MainAccountValue)]
        public string MainAccountValue { get; set; } = string.Empty;

        public long Account { get; set; }
        [StringLength(FieldLengths.AccountValue)]
        public string AccountValue { get; set; } = string.Empty;

        public long BusinessUnit { get; set; }
        [StringLength(FieldLengths.BusinessUnitValue)]
        public string BusinessUnitValue { get; set; } = string.Empty;

        public long CostCenter { get; set; }
        [StringLength(FieldLengths.CostCenterValue)]
        public string CostCenterValue { get; set; } = string.Empty;

        public long Department { get; set; }
        [StringLength(FieldLengths.DepartmentValue)]
        public string DepartmentValue { get; set; } = string.Empty;

        public long Division { get; set; }
        [StringLength(FieldLengths.DivisionValue)]
        public string DivisionValue { get; set; } = string.Empty;

        public long LegalEntity { get; set; }
        [StringLength(FieldLengths.LegalEntityValue)]
        public string LegalEntityValue { get; set; } = string.Empty;

        // ==========================================================
        // Sub-Ledger & Operational Dimensions (IDs & String Values)
        // ==========================================================
        // Basic Properties
        public long Project { get; set; }
        [StringLength(FieldLengths.ProjectValue)]
        public string ProjectValue { get; set; } = string.Empty;

        public long ServiceLine { get; set; }
        [StringLength(FieldLengths.ServiceLineValue)]
        public string ServiceLineValue { get; set; } = string.Empty;

        public long Fund { get; set; }
        [StringLength(FieldLengths.FundValue)]
        public string FundValue { get; set; } = string.Empty;

        public long Program { get; set; }
        [StringLength(FieldLengths.ProgramValue)]
        public string ProgramValue { get; set; } = string.Empty;

        public long ItemGroup { get; set; }
        [StringLength(FieldLengths.ItemGroupValue)]
        public string ItemGroupValue { get; set; } = string.Empty;

        public long ProductGroup { get; set; }
        [StringLength(FieldLengths.ProductGroupValue)]
        public string ProductGroupValue { get; set; } = string.Empty;

        public long Agreement { get; set; }
        [StringLength(FieldLengths.AgreementValue)]
        public string AgreementValue { get; set; } = string.Empty;

        public long RetailChannel { get; set; }
        [StringLength(FieldLengths.RetailChannelValue)]
        public string RetailChannelValue { get; set; } = string.Empty;

        public long Terminal { get; set; }
        [StringLength(FieldLengths.TerminalValue)]
        public string TerminalValue { get; set; } = string.Empty;

        public long Worker { get; set; }
        [StringLength(FieldLengths.WorkerValue)]
        public string WorkerValue { get; set; } = string.Empty;

        public long Groups { get; set; }
        [StringLength(FieldLengths.GroupsValue)]
        public string GroupsValue { get; set; } = string.Empty;

        public long ExpenseAndIncomeCode { get; set; }
        [StringLength(FieldLengths.ExpenseAndIncomeCodeValue)]
        public string ExpenseAndIncomeCodeValue { get; set; } = string.Empty;

        public long ObjectClass { get; set; }
        [StringLength(FieldLengths.ObjectClassValue)]
        public string ObjectClassValue { get; set; } = string.Empty;

        public long Filial { get; set; }
        [StringLength(FieldLengths.FilialValue)]
        public string FilialValue { get; set; } = string.Empty;

        public long FiscalEstablishment { get; set; }
        [StringLength(FieldLengths.FiscalEstablishmentValue)]
        public string FiscalEstablishmentValue { get; set; } = string.Empty;

        public long TaxBranch { get; set; }
        [StringLength(FieldLengths.TaxBranchValue)]
        public string TaxBranchValue { get; set; } = string.Empty;

        // ==========================================================
        // Regional Localization Extensions (CN / RU)
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.CostCenter_CnValue)]
        public string CostCenter_CnValue { get; set; } = string.Empty;

        [StringLength(FieldLengths.Department_CnValue)]
        public string Department_CnValue { get; set; } = string.Empty;

        [StringLength(FieldLengths.CashFlow_CnValue)]
        public string CashFlow_CnValue { get; set; } = string.Empty;

        [StringLength(FieldLengths.Ownership_CnValue)]
        public string Ownership_CnValue { get; set; } = string.Empty;

        [StringLength(FieldLengths.SystemGeneratedAttributeFixedAssets_RuValue)]
        public string SystemGeneratedAttributeFixedAssets_RuValue { get; set; } = string.Empty;

        // ==========================================================
        // System Generated Journal Accounts
        // ==========================================================
        // Basic Properties
        public long SystemGeneratedJournalAccount { get; set; }
        [StringLength(FieldLengths.SystemGeneratedJournalAccountValue)]
        public string SystemGeneratedJournalAccountValue { get; set; } = string.Empty;

        // Enum Properties
        public int SystemGeneratedJournalAccountType { get; set; } // Map to LedgerJournalACType if matching

        // ==========================================================
        // System Generated Global Reference Contexts
        // ==========================================================
        // Basic Properties
        public long SystemGeneratedAttributeBankAccount { get; set; }
        [StringLength(FieldLengths.SystemGeneratedAttributeBankAccountValue)]
        public string SystemGeneratedAttributeBankAccountValue { get; set; } = string.Empty;

        public long SystemGeneratedAttributeCustomer { get; set; }
        [StringLength(FieldLengths.SystemGeneratedAttributeCustomerValue)]
        public string SystemGeneratedAttributeCustomerValue { get; set; } = string.Empty;

        public long SystemGeneratedAttributeVendor { get; set; }
        [StringLength(FieldLengths.SystemGeneratedAttributeVendorValue)]
        public string SystemGeneratedAttributeVendorValue { get; set; } = string.Empty;

        public long SystemGeneratedAttributeFixedAsset { get; set; }
        [StringLength(FieldLengths.SystemGeneratedAttributeFixedAssetValue)]
        public string SystemGeneratedAttributeFixedAssetValue { get; set; } = string.Empty;

        public long SystemGeneratedAttributeProject { get; set; }
        [StringLength(FieldLengths.SystemGeneratedAttributeProjectValue)]
        public string SystemGeneratedAttributeProjectValue { get; set; } = string.Empty;

        public long SystemGeneratedAttributeEmployee { get; set; }
        public long SystemGeneratedAttributeItem { get; set; }
        public long SystemGeneratedAttributeRCash { get; set; }
        [StringLength(FieldLengths.SystemGeneratedAttributeRCashValue)]
        public string SystemGeneratedAttributeRCashValue { get; set; } = string.Empty;
        public long SystemGeneratedAttributeRDeferrals { get; set; }

        // ==========================================================
        // Extra Reference Identifiers (IDs only)
        // ==========================================================
        // Basic Properties
        public long BankAccount { get; set; }
        public long Campaign { get; set; }
        public long Cargo { get; set; }
        public long Center { get; set; }
        public long Condition { get; set; }
        public long Contract { get; set; }
        public long Customer { get; set; }
        public long JobSkills { get; set; }
        public long LaborType { get; set; }
        public long Location { get; set; }
        public long Primary_ { get; set; }
        public long Purpose { get; set; }
        public long Store { get; set; }
        public long Vehicle { get; set; }

        #region Navigation Properties Row

//         [ForeignKey(nameof(MainAccount))]
//         public virtual IAX.IXApi.Modules.Finance.Entities.MainAccount? MainAccountDefinition { get; set; }

//         [ForeignKey(nameof(AccountStructure))]
//         public virtual DimensionHierarchy? AccountStructureHierarchy { get; set; }

        #endregion
    }
}

