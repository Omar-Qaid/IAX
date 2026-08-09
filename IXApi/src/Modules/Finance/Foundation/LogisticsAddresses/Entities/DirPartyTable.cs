using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("DirPartyTable")]
    public class DirPartyTable : Entity<long>
    {
        //----------------------------------------- Core Identity & Type Hierarchy
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PartyNumber)]
        public string PartyNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.NameAlias)]
        public string NameAlias { get; set; } = string.Empty;

        public long RelationType { get; set; }          // Abstract blueprint framework tracking index
        public long InstanceRelationType { get; set; }  // Resolves structural concrete child types (e.g., Person, Organization, LegalEntity)
        public long LegacyInstanceRelationType { get; set; }

        // ==========================================================
        // Primary Communication Routing Shortcuts
        // ==========================================================
        // Basic Properties
        public long? PrimaryAddressLocation { get; set; } // Direct shortcut reference pointer to LogisticsLocation
        public long? PrimaryContactPhone { get; set; }    // Direct shortcut reference pointer to LogisticsElectronicAddress
        public long? PrimaryContactFax { get; set; }
        public long? PrimaryContactEmail { get; set; }
        public long? PrimaryContactUrl { get; set; }
        public long? PrimaryContactTelex { get; set; }
        public long? PrimaryContactTwitter { get; set; }
        public long? PrimaryContactFacebook { get; set; }
        public long? PrimaryContactLinkedIn { get; set; }
        public long? CommunicatorSignIn { get; set; }

        // ==========================================================
        // Localization, Security & Privacy Directives
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.LanguageId)]
        public string LanguageId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.AddressBookNames)]
        public string AddressBookNames { get; set; } = string.Empty; // Fast aggregated string indexing security clearances

        public int? LocalizationCountryRegionCode { get; set; }

        // Enum Properties
        public NoYes? EeEnablePersonalDataReadLog { get; set; } // GDPR / privacy transaction tracing indicator
        public NoYes? EeEnableRoleChangeLog { get; set; }

        // ==========================================================
        // Person-Specific Sub-Type Demographics
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.Initials)]
        public string? Initials { get; set; }

        public int? BirthYear { get; set; }
        public int? BirthMonth { get; set; }
        public int? Birthday { get; set; }
        public int? AnniversaryYear { get; set; }
        public int? AnniversaryMonth { get; set; }
        public int? AnniversaryDay { get; set; }
        public long? NameSequence { get; set; }
        public long? PersonalTitle { get; set; }
        public long? PersonalSuffix { get; set; }

        // Enum Properties
        public Gender? Gender { get; set; }
        public MaritalStatus? MaritalStatus { get; set; }

        // ==========================================================
        // Organization-Specific Segment Parameters
        // ==========================================================
        // Basic Properties
        public int? NumberOfEmployees { get; set; }
        public long? DunsNumberRecId { get; set; } // Dun & Bradstreet international standard organization tracker

        // Enum Properties
        public Abc? Abc { get; set; } // Operational ABC segmentation ranking (A-Class, B-Class, C-Class customers)
        public OrganizationType? OrganizationType { get; set; }

        // ==========================================================
        // Operating Unit & Internal Structure Sub-Types (OM)
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.OmOperatingUnitNumber)]
        public string? OmOperatingUnitNumber { get; set; }

        // Enum Properties
        public OmOperatingUnitType? OmOperatingUnitType { get; set; } // CostCenter, Department, ValueStream, BusinessUnit

        // ==========================================================
        // Team Framework Parameters
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.TeamAdministrator)]
        public string? TeamAdministrator { get; set; }

        public long? TeamMembershipCriterion { get; set; }

        // Enum Properties
        public new NoYes? IsActive { get; set; }

        // ==========================================================
        // Company/Legal Entity Context Specific Parameters
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.DataArea)]
        public string? DataArea { get; set; } // Direct partition pointer shortcut for legal entity scopes

        public int? Key_ { get; set; }
        public DateTime? ConversionDate { get; set; }
        public long? CompanyNafCode { get; set; } // French business segment industry code map

        // Payment Processing Priority Directives
        public long? PaymInstruction1 { get; set; }
        public long? PaymInstruction2 { get; set; }
        public long? PaymInstruction3 { get; set; }
        public long? PaymInstruction4 { get; set; }

        // Enum Properties
        public NoYes? IsConsolidationCompany { get; set; }
        public NoYes? IsEliminationCompany { get; set; }
        public NoYes? PlanningCompany { get; set; }

        // ==========================================================
        // Global Regulatory & Tax Demarcations
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.OrgNumber)]
        public string? OrgNumber { get; set; }

        [StringLength(FieldLengths.CoRegNum)]
        public string? CoRegNum { get; set; }

        [StringLength(FieldLengths.CoRegNum)]
        public string? RegNum { get; set; }

        [StringLength(FieldLengths.VatNum)]
        public string? VatNum { get; set; }

        [StringLength(FieldLengths.ImportVatNum)]
        public string? ImportVatNum { get; set; }

        [StringLength(FieldLengths.Bank)]
        public string? Bank { get; set; }

        [StringLength(FieldLengths.Description)]
        public string? Description { get; set; }

        [StringLength(FieldLengths.DvrID)]
        public string? DvrID { get; set; } // German data insurance messaging destination identification code

        [StringLength(FieldLengths.RFullName)]
        public string? RFullName { get; set; }

        [StringLength(FieldLengths.CompanyRegComFr)]
        public string? CompanyRegComFr { get; set; }

        [StringLength(FieldLengths.LegalFormFr)]
        public string? LegalFormFr { get; set; }

        [StringLength(FieldLengths.PackMaterialFeeLicenseNum)]
        public string? PackMaterialFeeLicenseNum { get; set; }

        // US 1099 Regulatory Compliance Parameters
        [StringLength(FieldLengths.Tax1099RegNum)]
        public string? Tax1099RegNum { get; set; }

        [StringLength(FieldLengths.AccountOfficeRefNum)]
        public string? AccountOfficeRefNum { get; set; }

        // Enum Properties
        public NoYes? Validate1099OnEntry { get; set; }
        public NoYes? CombinedFedStateFiler { get; set; }
        public NoYes? ForeignEntityIndicator { get; set; }
        public NoYes? LastFilingIndicator { get; set; }

        // Regional Specific Parameters
        [StringLength(FieldLengths.SiaCode)]
        public string? SiaCode { get; set; }

        [StringLength(FieldLengths.SubordinateCode)]
        public string? SubordinateCode { get; set; }

        // Enum Properties
        public NoYes? Resident_W { get; set; } // Localized residency profile marker for central bank operations

        // ==========================================================
        // Internal HRM Core Mapping
        // ==========================================================
        // Basic Properties
        public long? HcmWorker { get; set; } // Links context back to HR system employee maps


     
        #region Navigation Properties Row
        [ForeignKey(nameof(PrimaryContactFax))]
        public virtual LogisticsElectronicAddress? LogisticsElectronicAddress_Fax { get; set; }

        // DirPartyTable.PrimaryContactPhone == LogisticsElectronicAddress.RecId
        [ForeignKey(nameof(PrimaryContactPhone))]
        public virtual LogisticsElectronicAddress? LogisticsElectronicAddress_Phone { get; set; }

        // DirPartyTable.PrimaryContactTelex == LogisticsElectronicAddress.RecId
        [ForeignKey(nameof(PrimaryContactTelex))]
        public virtual LogisticsElectronicAddress? LogisticsElectronicAddress_Telex { get; set; }

        // DirPartyTable.PrimaryContactFacebook == LogisticsElectronicAddress.RecId
        [ForeignKey(nameof(PrimaryContactFacebook))]
        public virtual LogisticsElectronicAddress? LogisticsElectronicAddress_Facebook { get; set; }

        // DirPartyTable.PrimaryContactTwitter == LogisticsElectronicAddress.RecId
        [ForeignKey(nameof(PrimaryContactTwitter))]
        public virtual LogisticsElectronicAddress? LogisticsElectronicAddress_Twitter { get; set; }

        // DirPartyTable.PrimaryContactLinkedIn == LogisticsElectronicAddress.RecId
        [ForeignKey(nameof(PrimaryContactLinkedIn))]
        public virtual LogisticsElectronicAddress? LogisticsElectronicAddress_LinkedIn { get; set; }

        // DirPartyTable.PrimaryContactEmail == LogisticsElectronicAddress.RecId
        [ForeignKey(nameof(PrimaryContactEmail))]
        public virtual LogisticsElectronicAddress? LogisticsElectronicAddress_Email { get; set; }


        //DirPartyTable.PrimaryContactURL == LogisticsElectronicAddress.RecId
        [ForeignKey(nameof(PrimaryContactUrl))]
        public virtual LogisticsElectronicAddress? LogisticsElectronicAddress_Url { get; set; }


        // DirPartyTable.PrimaryAddressLocation == LogisticsLocation.RecId
        [ForeignKey(nameof(PrimaryAddressLocation))]
        public virtual LogisticsLocation? LogisticsLocationTable { get; set; }

        // DirPartyTable.PrimaryAddressLocation == LogisticsPostalAddress.Location
        public virtual LogisticsPostalAddress? LogisticsPostalAddressTable { get; set; }

        #endregion
     
    }
}


