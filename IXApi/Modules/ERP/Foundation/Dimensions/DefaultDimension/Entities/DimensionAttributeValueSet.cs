using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("DimensionAttributeValueSet")]
    public class DimensionAttributeValueSet : Entity<long>
    {
        //----------------------------------------- Core Information & Hashing
        // Basic Properties
        public byte[]? Hash { get; set; } // varbinary mapping supporting Nullable = YES
        public int HashVersion { get; set; }

        // ==========================================================
        // Standard Financial Dimensions (IDs & Display Values)
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
        // Localization Dimensions (CN)
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.CostCenter_CnValue)]
        public string CostCenter_CnValue { get; set; } = string.Empty;

        [StringLength(FieldLengths.Department_CnValue)]
        public string Department_CnValue { get; set; } = string.Empty;

        [StringLength(FieldLengths.CashFlow_CnValue)]
        public string CashFlow_CnValue { get; set; } = string.Empty;

        // ==========================================================
        // Extra Operational Dimensions (IDs only)
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
        public long LegalEntity { get; set; }
        public long Location { get; set; }
        public long Primary_ { get; set; }
        public long Purpose { get; set; }
        public long Store { get; set; }
        public long Vehicle { get; set; }

        // ==========================================================
        // System Generated Reference Attributes (IDs only)
        // ==========================================================
        // Basic Properties
        public long SystemGeneratedAttributeBankAccount { get; set; }
        public long SystemGeneratedAttributeCustomer { get; set; }
        public long SystemGeneratedAttributeEmployee { get; set; }
        public long SystemGeneratedAttributeFixedAsset { get; set; }
        public long SystemGeneratedAttributeItem { get; set; }
        public long SystemGeneratedAttributeProject { get; set; }
        public long SystemGeneratedAttributeRCash { get; set; }
        public long SystemGeneratedAttributeRDeferrals { get; set; }
        public long SystemGeneratedAttributeVendor { get; set; }

        #region Navigation Properties Row

//         [ForeignKey(nameof(MainAccount))]
//         public virtual IAX.IXApi.Modules.ERP.Entities.MainAccount? MainAccountDefinition { get; set; }

        #endregion
    }
}
