# Sales order cycle schema inventory

Source: `ApplicationDbContextModelSnapshot.cs` in this checkout, including the uncommitted three-table migration from the preceding task. This is the IXApi EF model, not a live database inspection.
D365 reference fields and types come only from the user's pasted material and are illustrative, not a complete or version-specific D365 schema. The attachment uses X++ base types for eight tables and published CDM data formats for MarkupAutoTable/MarkupAutoLine; these are labeled as supplied, not converted into SQL precision. `RecId` maps to IXApi's `RECID` SQL column.
SQL types shown below are the model's configured column types. Custom fields, migrations not applied to a database, and D365 extensions require live metadata verification.
Official published references for selected tables: [SalesLine](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/salesandmarketing/worksheetline/salesline), [MarkupAutoTable](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/procurementandsourcing/group/markupautotable), [MarkupAutoLine](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/procurementandsourcing/group/markupautoline), [InventTransOriginSalesLine](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/transaction/inventtransoriginsalesline).

| Group | Tables in cycle | IXApi model | Missing in IXApi model |
| --- | ---: | ---: | ---: |
| Setup: customer and addresses | 5 | 5 | 0 |
| Setup: item and inventory | 10 | 6 | 4 |
| Setup: pricing and charges | 7 | 7 | 0 |
| Setup: tax and terms | 10 | 9 | 1 |
| Order and inventory | 7 | 7 | 0 |
| Confirmation and documents | 8 | 4 | 4 |

## Setup: customer and addresses

### CustTable

IXApi entity: `IAX.IXApi.Modules.Finance.AccountsReceivable.CustTable`. 123 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `AccStmtSign` | `AccStmtSign` | `int` | `int` | — |
| `AccountNum` | `AccountNum` | `string` | `nvarchar(20)` | — |
| `AccountStatement` | `AccountStatement` | `int` | `int` | — |
| `BankCustPaymIdTable` | `BankCustPaymIdTable` | `long` | `bigint` | — |
| `BlockFloorLimitUseInChannel` | `BlockFloorLimitUseInChannel` | `int` | `int` | — |
| `Blocked` | `Blocked` | `int` | `int` | — |
| `CashDiscBaseDays` | `CashDiscBaseDays` | `int` | `int` | — |
| `CollectionLetterCode` | `CollectionLetterCode` | `int` | `int` | — |
| `CompanyNafCode` | `CompanyNafCode` | `long` | `bigint` | — |
| `CountryRegionId` | `CountryRegionId` | `string` | `nvarchar(10)` | — |
| `Cr` | `Cr` | `int` | `int` | — |
| `CrDate` | `CrDate` | `DateTime` | `datetime2` | — |
| `CrEndDate` | `CrEndDate` | `DateTime` | `datetime2` | — |
| `CrParentEndDate` | `CrParentEndDate` | `DateTime` | `datetime2` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `CredManAccountStatusId` | `CredManAccountStatusId` | `string` | `nvarchar(100)` | — |
| `CredManBusinessStarted` | `CredManBusinessStarted` | `DateTime` | `datetime2` | — |
| `CredManCreditLimitDate` | `CredManCreditLimitDate` | `DateTime` | `datetime2` | — |
| `CredManCreditLimitExpiryDate` | `CredManCreditLimitExpiryDate` | `DateTime` | `datetime2` | — |
| `CredManCustCreditMaxAlt` | `CredManCustCreditMaxAlt` | `decimal` | `decimal(18,4)` | — |
| `CredManCustUnlimitedCredit` | `CredManCustUnlimitedCredit` | `int` | `int` | — |
| `CredManCustomerSince` | `CredManCustomerSince` | `DateTime` | `datetime2` | — |
| `CredManEligibleCreditLimitDate` | `CredManEligibleCreditLimitDate` | `DateTime` | `datetime2` | — |
| `CredManEligibleCreditMax` | `CredManEligibleCreditMax` | `decimal` | `decimal(18,4)` | — |
| `CredManExclude` | `CredManExclude` | `int` | `int` | — |
| `CredManLastReviewDate` | `CredManLastReviewDate` | `DateTime` | `datetime2` | — |
| `CredManNextSchedReviewDate` | `CredManNextSchedReviewDate` | `DateTime` | `datetime2` | — |
| `CredManNotes` | `CredManNotes` | `string` | `nvarchar(max)` | — |
| `CredManTitleHeld` | `CredManTitleHeld` | `int` | `int` | — |
| `CredManWithAgency` | `CredManWithAgency` | `int` | `int` | — |
| `CreditCardAddressVerification` | `CreditCardAddressVerification` | `int` | `int` | — |
| `CreditCardAddressVerificationLevel` | `CreditCardAddressVerificationLevel` | `int` | `int` | — |
| `CreditCardAddressVerificationVoid` | `CreditCardAddressVerificationVoid` | `int` | `int` | — |
| `CreditCardCvc` | `CreditCardCvc` | `int` | `int` | — |
| `CreditMax` | `CreditMax` | `decimal` | `decimal(32,6)` | — |
| `CurrencyCode` | `CurrencyCode` | `string` | `nvarchar(3)` | — |
| `CustCategory` | `CustCategory` | `string` | `nvarchar(10)` | — |
| `CustExcludeCollectionFee` | `CustExcludeCollectionFee` | `int` | `int` | — |
| `CustExcludeInterestCharges` | `CustExcludeInterestCharges` | `int` | `int` | — |
| `CustGroupId` | `CustGroupId` | `string` | `nvarchar(20)` | — |
| `CustTradingPartnerCode` | `CustTradingPartnerCode` | `long` | `bigint` | — |
| `CustWriteOffRefRecId` | `CustWriteOffRefRecId` | `long` | `bigint` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(10)` | — |
| `DefaultDimension` | `DefaultDimension` | `long` | `bigint` | — |
| `DefaultDirectDebitMandate` | `DefaultDirectDebitMandate` | `long` | `bigint` | — |
| `DlvModeId` | `DlvModeId` | `string` | `nvarchar(10)` | — |
| `DocValid` | `DocValid` | `int` | `int` | — |
| `EInvoice` | `EInvoice` | `int` | `int` | — |
| `EInvoiceAttachment` | `EInvoiceAttachment` | `int` | `int` | — |
| `EntryCertificateRequiredW` | `EntryCertificateRequiredW` | `int` | `int` | — |
| `ExpiryDate` | `ExpiryDate` | `DateTime` | `datetime2` | — |
| `ExpressBillOfLading` | `ExpressBillOfLading` | `int` | `int` | — |
| `FedNonFedIndicator` | `FedNonFedIndicator` | `int` | `int` | — |
| `ForecastDmpInclude` | `ForecastDmpInclude` | `int` | `int` | — |
| `GiroType` | `GiroType` | `int` | `int` | — |
| `GiroTypeAccountStatement` | `GiroTypeAccountStatement` | `int` | `int` | — |
| `GiroTypeCollectionLetter` | `GiroTypeCollectionLetter` | `int` | `int` | — |
| `GiroTypeFreeTextInvoice` | `GiroTypeFreeTextInvoice` | `int` | `int` | — |
| `GiroTypeInterestNote` | `GiroTypeInterestNote` | `int` | `int` | — |
| `GiroTypeProjInvoice` | `GiroTypeProjInvoice` | `int` | `int` | — |
| `Government` | `Government` | `int` | `int` | — |
| `InclTax` | `InclTax` | `int` | `int` | — |
| `InterCompanyAllowIndirectCreation` | `InterCompanyAllowIndirectCreation` | `int` | `int` | — |
| `InterCompanyAutoCreateOrders` | `InterCompanyAutoCreateOrders` | `int` | `int` | — |
| `InterCompanyDirectDelivery` | `InterCompanyDirectDelivery` | `int` | `int` | — |
| `InventLocationId` | `InventLocationId` | `string` | `nvarchar(10)` | — |
| `InventSiteId` | `InventSiteId` | `string` | `nvarchar(10)` | — |
| `InvoiceAccount` | `InvoiceAccount` | `string` | `nvarchar(20)` | — |
| `InvoiceAddress` | `InvoiceAddress` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsExternallyMaintained` | `IsExternallyMaintained` | `int` | `int` | — |
| `IssueOwnEntryCertificateW` | `IssueOwnEntryCertificateW` | `int` | `int` | — |
| `JointVenture` | `JointVenture` | `int` | `int` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LvPaymTransCodes` | `LvPaymTransCodes` | `long` | `bigint` | — |
| `MainContactWorker` | `MainContactWorker` | `long` | `bigint` | — |
| `MainHolding` | `MainHolding` | `int` | `int` | — |
| `MandatoryCreditLimit` | `MandatoryCreditLimit` | `int` | `int` | — |
| `Memo` | `Memo` | `string` | `nvarchar(max)` | — |
| `NationalAddress` | `NationalAddress` | `int` | `int` | — |
| `OneTimeCustomer` | `OneTimeCustomer` | `int` | `int` | — |
| `OpeningAccFile` | `OpeningAccFile` | `int` | `int` | — |
| `OverrideSalesTax` | `OverrideSalesTax` | `int` | `int` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `OwnerIdCopy` | `OwnerIdCopy` | `int` | `int` | — |
| `Party` | `Party` | `long` | `bigint` | — |
| `PartyCountry` | `PartyCountry` | `string` | `nvarchar(10)` | — |
| `PartyState` | `PartyState` | `string` | `nvarchar(30)` | — |
| `PaymModeId` | `PaymModeId` | `string` | `nvarchar(10)` | — |
| `PaymTermId` | `PaymTermId` | `string` | `nvarchar(100)` | — |
| `PdsFreightAccrued` | `PdsFreightAccrued` | `int` | `int` | — |
| `PrePayType` | `PrePayType` | `int` | `int` | — |
| `PrepaymentValue` | `PrepaymentValue` | `decimal` | `decimal(32,6)` | — |
| `QmsCustomerCheckItem` | `QmsCustomerCheckItem` | `int` | `int` | — |
| `QmsPrintCustSpecificCertOfAnalysis` | `QmsPrintCustSpecificCertOfAnalysis` | `int` | `int` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RecipSign` | `RecipSign` | `int` | `int` | — |
| `RevRecDisableInterCompany` | `RevRecDisableInterCompany` | `int` | `int` | — |
| `RfidCaseTagging` | `RfidCaseTagging` | `int` | `int` | — |
| `RfidItemTagging` | `RfidItemTagging` | `int` | `int` | — |
| `RfidPalletTagging` | `RfidPalletTagging` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SalesPoolId` | `SalesPoolId` | `string` | `nvarchar(10)` | — |
| `ShipCarrierBlindShipment` | `ShipCarrierBlindShipment` | `int` | `int` | — |
| `ShipCarrierFuelSurcharge` | `ShipCarrierFuelSurcharge` | `int` | `int` | — |
| `SiteSketch` | `SiteSketch` | `int` | `int` | — |
| `Stamp` | `Stamp` | `int` | `int` | — |
| `StateId` | `StateId` | `string` | `nvarchar(30)` | — |
| `TaxGroupId` | `TaxGroupId` | `string` | `nvarchar(10)` | — |
| `UseCashDisc` | `UseCashDisc` | `int` | `int` | — |
| `UsePurchRequest` | `UsePurchRequest` | `int` | `int` | — |
| `ValidatedFrom` | `ValidatedFrom` | `DateTime` | `datetime2` | — |
| `ValidatedTo` | `ValidatedTo` | `DateTime` | `datetime2` | — |
| `VatFileAttachment` | `VatFileAttachment` | `int` | `int` | — |
| `VatNum` | `VatNum` | `string` | `nvarchar(20)` | — |
| `VatNumRecId` | `VatNumRecId` | `long` | `bigint` | — |
| `VatNumTableType` | `VatNumTableType` | `int` | `int` | — |
| `VendAccount` | `VendAccount` | `string` | `nvarchar(20)` | — |
| `WorkflowState` | `WorkflowState` | `int` | `int` | — |

### DirPartyTable

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.DirPartyTable`. 85 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `Abc` | `Abc` | `int?` | `int` | — |
| `AccountOfficeRefNum` | `AccountOfficeRefNum` | `string` | `nvarchar(13)` | — |
| `AddressBookNames` | `AddressBookNames` | `string` | `nvarchar(1000)` | — |
| `AnniversaryDay` | `AnniversaryDay` | `int?` | `int` | — |
| `AnniversaryMonth` | `AnniversaryMonth` | `int?` | `int` | — |
| `AnniversaryYear` | `AnniversaryYear` | `int?` | `int` | — |
| `Bank` | `Bank` | `string` | `nvarchar(10)` | — |
| `BirthMonth` | `BirthMonth` | `int?` | `int` | — |
| `BirthYear` | `BirthYear` | `int?` | `int` | — |
| `Birthday` | `Birthday` | `int?` | `int` | — |
| `CoRegNum` | `CoRegNum` | `string` | `nvarchar(25)` | — |
| `CombinedFedStateFiler` | `CombinedFedStateFiler` | `int?` | `int` | — |
| `CommunicatorSignIn` | `CommunicatorSignIn` | `long?` | `bigint` | — |
| `CompanyNafCode` | `CompanyNafCode` | `long?` | `bigint` | — |
| `CompanyRegComFr` | `CompanyRegComFr` | `string` | `nvarchar(30)` | — |
| `ConversionDate` | `ConversionDate` | `DateTime?` | `datetime2` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataArea` | `DataArea` | `string` | `nvarchar(4)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `Description` | `Description` | `string` | `nvarchar(60)` | — |
| `DunsNumberRecId` | `DunsNumberRecId` | `long?` | `bigint` | — |
| `DvrID` | `DvrID` | `string` | `nvarchar(20)` | — |
| `EeEnablePersonalDataReadLog` | `EeEnablePersonalDataReadLog` | `int?` | `int` | — |
| `EeEnableRoleChangeLog` | `EeEnableRoleChangeLog` | `int?` | `int` | — |
| `ForeignEntityIndicator` | `ForeignEntityIndicator` | `int?` | `int` | — |
| `Gender` | `Gender` | `int?` | `int` | — |
| `HcmWorker` | `HcmWorker` | `long?` | `bigint` | — |
| `ImportVatNum` | `ImportVatNum` | `string` | `nvarchar(20)` | — |
| `Initials` | `Initials` | `string` | `nvarchar(10)` | — |
| `InstanceRelationType` | `InstanceRelationType` | `long` | `bigint` | — |
| `IsActive` | `IsActive` | `int?` | `int` | — |
| `IsConsolidationCompany` | `IsConsolidationCompany` | `int?` | `int` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsEliminationCompany` | `IsEliminationCompany` | `int?` | `int` | — |
| `Key_` | `Key_` | `int?` | `int` | — |
| `LanguageId` | `LanguageId` | `string` | `nvarchar(7)` | — |
| `LastFilingIndicator` | `LastFilingIndicator` | `int?` | `int` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LegacyInstanceRelationType` | `LegacyInstanceRelationType` | `long` | `bigint` | — |
| `LegalFormFr` | `LegalFormFr` | `string` | `nvarchar(100)` | — |
| `LocalizationCountryRegionCode` | `LocalizationCountryRegionCode` | `int?` | `int` | — |
| `MaritalStatus` | `MaritalStatus` | `int?` | `int` | — |
| `Name` | `Name` | `string` | `nvarchar(60)` | — |
| `NameAlias` | `NameAlias` | `string` | `nvarchar(60)` | — |
| `NameSequence` | `NameSequence` | `long?` | `bigint` | — |
| `NumberOfEmployees` | `NumberOfEmployees` | `int?` | `int` | — |
| `OmOperatingUnitNumber` | `OmOperatingUnitNumber` | `string` | `nvarchar(30)` | — |
| `OmOperatingUnitType` | `OmOperatingUnitType` | `int?` | `int` | — |
| `OrgNumber` | `OrgNumber` | `string` | `nvarchar(25)` | — |
| `OrganizationType` | `OrganizationType` | `int?` | `int` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PackMaterialFeeLicenseNum` | `PackMaterialFeeLicenseNum` | `string` | `nvarchar(20)` | — |
| `PartyNumber` | `PartyNumber` | `string` | `nvarchar(20)` | — |
| `PaymInstruction1` | `PaymInstruction1` | `long?` | `bigint` | — |
| `PaymInstruction2` | `PaymInstruction2` | `long?` | `bigint` | — |
| `PaymInstruction3` | `PaymInstruction3` | `long?` | `bigint` | — |
| `PaymInstruction4` | `PaymInstruction4` | `long?` | `bigint` | — |
| `PersonalSuffix` | `PersonalSuffix` | `long?` | `bigint` | — |
| `PersonalTitle` | `PersonalTitle` | `long?` | `bigint` | — |
| `PlanningCompany` | `PlanningCompany` | `int?` | `int` | — |
| `PrimaryAddressLocation` | `PrimaryAddressLocation` | `long?` | `bigint` | — |
| `PrimaryContactEmail` | `PrimaryContactEmail` | `long?` | `bigint` | — |
| `PrimaryContactFacebook` | `PrimaryContactFacebook` | `long?` | `bigint` | — |
| `PrimaryContactFax` | `PrimaryContactFax` | `long?` | `bigint` | — |
| `PrimaryContactLinkedIn` | `PrimaryContactLinkedIn` | `long?` | `bigint` | — |
| `PrimaryContactPhone` | `PrimaryContactPhone` | `long?` | `bigint` | — |
| `PrimaryContactTelex` | `PrimaryContactTelex` | `long?` | `bigint` | — |
| `PrimaryContactTwitter` | `PrimaryContactTwitter` | `long?` | `bigint` | — |
| `PrimaryContactUrl` | `PrimaryContactUrl` | `long?` | `bigint` | — |
| `RFullName` | `RFullName` | `string` | `nvarchar(150)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RegNum` | `RegNum` | `string` | `nvarchar(25)` | — |
| `RelationType` | `RelationType` | `long` | `bigint` | — |
| `Resident_W` | `Resident_W` | `int?` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SiaCode` | `SiaCode` | `string` | `nvarchar(5)` | — |
| `SubordinateCode` | `SubordinateCode` | `string` | `nvarchar(5)` | — |
| `Tax1099RegNum` | `Tax1099RegNum` | `string` | `nvarchar(11)` | — |
| `TeamAdministrator` | `TeamAdministrator` | `string` | `nvarchar(20)` | — |
| `TeamMembershipCriterion` | `TeamMembershipCriterion` | `long?` | `bigint` | — |
| `Validate1099OnEntry` | `Validate1099OnEntry` | `int?` | `int` | — |
| `VatNum` | `VatNum` | `string` | `nvarchar(20)` | — |

### DirPartyLocation

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.DirPartyLocation`. 25 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `AssignmentDate` | `AssignmentDate` | `DateTime` | `datetime2` | — |
| `AssignmentDateTzId` | `AssignmentDateTzId` | `int` | `int` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsLocationOwner` | `IsLocationOwner` | `int` | `int` | — |
| `IsPostalAddress` | `IsPostalAddress` | `int` | `int` | — |
| `IsPrimary` | `IsPrimary` | `int` | `int` | — |
| `IsPrimaryTaxRegistration` | `IsPrimaryTaxRegistration` | `int` | `int` | — |
| `IsPrivate` | `IsPrivate` | `int` | `int` | — |
| `IsRoleBusiness` | `IsRoleBusiness` | `int` | `int` | — |
| `IsRoleDelivery` | `IsRoleDelivery` | `int` | `int` | — |
| `IsRoleHome` | `IsRoleHome` | `int` | `int` | — |
| `IsRoleInvoice` | `IsRoleInvoice` | `int` | `int` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `Location` | `Location` | `long` | `bigint` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `Party` | `Party` | `long` | `bigint` | — |
| `PostalAddressRoles` | `PostalAddressRoles` | `string` | `nvarchar(1000)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |

### LogisticsLocation

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.LogisticsLocation`. 16 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `Description` | `Description` | `string` | `nvarchar(60)` | — |
| `DunsNumberRecId` | `DunsNumberRecId` | `long` | `bigint` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsPostalAddress` | `IsPostalAddress` | `int` | `int` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LocationId` | `LocationId` | `string` | `nvarchar(40)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `ParentLocation` | `ParentLocation` | `long` | `bigint` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |

### LogisticsPostalAddress

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.LogisticsPostalAddress`. 38 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `Address` | `Address` | `string` | `nvarchar(250)` | — |
| `BuildingCompliment` | `BuildingCompliment` | `string` | `nvarchar(60)` | — |
| `ChannelReferenceId` | `ChannelReferenceId` | `string` | `nvarchar(38)` | — |
| `City` | `City` | `string` | `nvarchar(60)` | — |
| `CityRecId` | `CityRecId` | `long` | `bigint` | — |
| `CountryRegionId` | `CountryRegionId` | `string` | `nvarchar(10)` | — |
| `County` | `County` | `string` | `nvarchar(30)` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `District` | `District` | `long` | `bigint` | — |
| `DistrictName` | `DistrictName` | `string` | `nvarchar(60)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsPrivate` | `IsPrivate` | `int` | `int` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `Latitude` | `Latitude` | `decimal` | `decimal(18,4)` | — |
| `LocalityRecId` | `LocalityRecId` | `long` | `bigint` | — |
| `Location` | `Location` | `long` | `bigint` | — |
| `Longitude` | `Longitude` | `decimal` | `decimal(18,4)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PostBox` | `PostBox` | `string` | `nvarchar(20)` | — |
| `PrivateForParty` | `PrivateForParty` | `long?` | `bigint` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SettlementRecId` | `SettlementRecId` | `long` | `bigint` | — |
| `State` | `State` | `string` | `nvarchar(30)` | — |
| `Street` | `Street` | `string` | `nvarchar(250)` | — |
| `StreetNumber` | `StreetNumber` | `string` | `nvarchar(20)` | — |
| `TimeZone` | `TimeZone` | `int` | `int` | — |
| `ValidFrom` | `ValidFrom` | `DateTime` | `datetime2` | — |
| `ValidFromTzId` | `ValidFromTzId` | `int` | `int` | — |
| `ValidTo` | `ValidTo` | `DateTime` | `datetime2` | — |
| `ValidToTzId` | `ValidToTzId` | `int` | `int` | — |
| `ZipCode` | `ZipCode` | `string` | `nvarchar(10)` | — |
| `ZipCodeRecId` | `ZipCodeRecId` | `long` | `bigint` | — |

## Setup: item and inventory

### InventTable

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.InventTable`. 120 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `ABCContributionMargin` | `ABCContributionMargin` | `int` | `int` | — |
| `ABCRevenue` | `ABCRevenue` | `int` | `int` | — |
| `ABCTieUp` | `ABCTieUp` | `int` | `int` | — |
| `ABCValue` | `ABCValue` | `int` | `int` | — |
| `AutoReportFinished` | `AutoReportFinished` | `int` | `int` | — |
| `BatchMergedDateCalculationMethod` | `BatchMergedDateCalculationMethod` | `int` | `int` | — |
| `BatchNumGroupId` | `BatchNumGroupId` | `string` | `nvarchar(10)` | — |
| `BomCalcGroupId` | `BomCalcGroupId` | `string` | `nvarchar(10)` | — |
| `BomLevel` | `BomLevel` | `int` | `int` | — |
| `BomManualReceipt` | `BomManualReceipt` | `int` | `int` | — |
| `BomUnitId` | `BomUnitId` | `string` | `nvarchar(10)` | — |
| `BomWhsReleasePolicy` | `BomWhsReleasePolicy` | `int` | `int` | — |
| `Bundle` | `Bundle` | `int` | `int` | — |
| `CommissionGroupId` | `CommissionGroupId` | `string` | `nvarchar(10)` | — |
| `CooDualUseProduct` | `CooDualUseProduct` | `int` | `int` | — |
| `CostBomLevel` | `CostBomLevel` | `int` | `int` | — |
| `CostGroupId` | `CostGroupId` | `string` | `nvarchar(10)` | — |
| `CostModel` | `CostModel` | `int` | `int` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `DefaultDimension` | `DefaultDimension` | `long` | `bigint` | — |
| `Density` | `Density` | `decimal` | `decimal(18,4)` | — |
| `Depth` | `Depth` | `decimal` | `decimal(18,4)` | — |
| `FiscalLifoAvoidCalc` | `FiscalLifoAvoidCalc` | `int` | `int` | — |
| `FiscalLifoNormalValue` | `FiscalLifoNormalValue` | `decimal` | `decimal(18,4)` | — |
| `FiscalLifoNormalValueCalc` | `FiscalLifoNormalValueCalc` | `int` | `int` | — |
| `ForecastDmpInclude` | `ForecastDmpInclude` | `int` | `int` | — |
| `GrossDepth` | `GrossDepth` | `decimal` | `decimal(18,4)` | — |
| `GrossHeight` | `GrossHeight` | `decimal` | `decimal(18,4)` | — |
| `GrossWidth` | `GrossWidth` | `decimal` | `decimal(18,4)` | — |
| `Height` | `Height` | `decimal` | `decimal(18,4)` | — |
| `HmimIndicator` | `HmimIndicator` | `int` | `int` | — |
| `IntrastatChargePerKg` | `IntrastatChargePerKg` | `decimal` | `decimal(18,4)` | — |
| `IntrastatCommodity` | `IntrastatCommodity` | `long` | `bigint` | — |
| `IntrastatExclude` | `IntrastatExclude` | `int` | `int` | — |
| `InventFiscalLifoGroup` | `InventFiscalLifoGroup` | `long` | `bigint` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsExclusiveHbmc` | `IsExclusiveHbmc` | `int` | `int` | — |
| `ItemBuyerGroupId` | `ItemBuyerGroupId` | `string` | `nvarchar(10)` | — |
| `ItemDimCostPrice` | `ItemDimCostPrice` | `int` | `int` | — |
| `ItemId` | `ItemId` | `string` | `nvarchar(20)` | — |
| `ItemType` | `ItemType` | `int` | `int` | — |
| `ItmArrivalGroupId` | `ItmArrivalGroupId` | `string` | `nvarchar(10)` | — |
| `ItmCostTransferGroupId` | `ItmCostTransferGroupId` | `string` | `nvarchar(10)` | — |
| `ItmCostTypeGroupId` | `ItmCostTypeGroupId` | `string` | `nvarchar(100)` | — |
| `ItmOverUnderToleranceGroupId` | `ItmOverUnderToleranceGroupId` | `string` | `nvarchar(10)` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `MarketLowestPrice` | `MarketLowestPrice` | `decimal` | `decimal(18,4)` | — |
| `MatchingPolicy` | `MatchingPolicy` | `int` | `int` | — |
| `MinimumPalletQuantity` | `MinimumPalletQuantity` | `decimal` | `decimal(18,4)` | — |
| `Name` | `Name` | `string` | `nvarchar(60)` | — |
| `NameAlias` | `NameAlias` | `string` | `nvarchar(60)` | — |
| `NetWeight` | `NetWeight` | `decimal` | `decimal(18,4)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PdsBaseAttributeID` | `PdsBaseAttributeID` | `string` | `nvarchar(20)` | — |
| `PdsBestBefore` | `PdsBestBefore` | `int` | `int` | — |
| `PdsPotencyAttribRecording` | `PdsPotencyAttribRecording` | `int` | `int` | — |
| `PdsShelfAdvice` | `PdsShelfAdvice` | `int` | `int` | — |
| `PdsShelfLife` | `PdsShelfLife` | `int` | `int` | — |
| `PdsTargetFactor` | `PdsTargetFactor` | `decimal` | `decimal(18,4)` | — |
| `PdsVendorCheckItem` | `PdsVendorCheckItem` | `int` | `int` | — |
| `PdscwWmsMinimumPalletQty` | `PdscwWmsMinimumPalletQty` | `decimal` | `decimal(18,4)` | — |
| `PdscwWmsQtyPerLayer` | `PdscwWmsQtyPerLayer` | `decimal` | `decimal(18,4)` | — |
| `PdscwWmsStandardPalletQty` | `PdscwWmsStandardPalletQty` | `decimal` | `decimal(18,4)` | — |
| `Phantom` | `Phantom` | `int` | `int` | — |
| `PmfPlanningItemId` | `PmfPlanningItemId` | `string` | `nvarchar(20)` | — |
| `PmfProductType` | `PmfProductType` | `int` | `int` | — |
| `PmfYieldPct` | `PmfYieldPct` | `decimal` | `decimal(18,4)` | — |
| `PrimaryVendorID` | `PrimaryVendorID` | `string` | `nvarchar(20)` | — |
| `ProdFlushingPrincip` | `ProdFlushingPrincip` | `int` | `int` | — |
| `ProdGroupId` | `ProdGroupId` | `string` | `nvarchar(10)` | — |
| `ProdOriginId` | `ProdOriginId` | `string` | `nvarchar(40)` | — |
| `Product` | `Product` | `long` | `bigint` | — |
| `ProjCategoryId` | `ProjCategoryId` | `string` | `nvarchar(30)` | — |
| `PurchModel` | `PurchModel` | `int` | `int` | — |
| `QmsAuthorizedPersonnel` | `QmsAuthorizedPersonnel` | `int` | `int` | — |
| `QmsCustomerCheckItem` | `QmsCustomerCheckItem` | `int` | `int` | — |
| `QmsDispensingControl` | `QmsDispensingControl` | `int` | `int` | — |
| `QmsOverDispensePct` | `QmsOverDispensePct` | `decimal` | `decimal(18,4)` | — |
| `QmsUnderDispensePct` | `QmsUnderDispensePct` | `decimal` | `decimal(18,4)` | — |
| `QtyPerLayer` | `QtyPerLayer` | `decimal` | `decimal(18,4)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `ReqGroupId` | `ReqGroupId` | `string` | `nvarchar(10)` | — |
| `RevRecBundle` | `RevRecBundle` | `int` | `int` | — |
| `RevRecDefaultRevenueRecognitionSchedule` | `RevRecDefaultRevenueRecognitionSchedule` | `string` | `nvarchar(10)` | — |
| `RevRecExcludeFromCarveOut` | `RevRecExcludeFromCarveOut` | `int` | `int` | — |
| `RevRecMedianPrice` | `RevRecMedianPrice` | `int` | `int` | — |
| `RevRecMedianPriceMaximumTolerance` | `RevRecMedianPriceMaximumTolerance` | `decimal` | `decimal(18,4)` | — |
| `RevRecMedianPriceMinimumTolerance` | `RevRecMedianPriceMinimumTolerance` | `decimal` | `decimal(18,4)` | — |
| `RevRecRevenueRecognitionEnabled` | `RevRecRevenueRecognitionEnabled` | `int` | `int` | — |
| `RevRecRevenueType` | `RevRecRevenueType` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SalesContributionRatio` | `SalesContributionRatio` | `decimal` | `decimal(18,4)` | — |
| `SalesModel` | `SalesModel` | `int` | `int` | — |
| `SalesPercentMarkup` | `SalesPercentMarkup` | `decimal` | `decimal(18,4)` | — |
| `SalesPriceModelBasic` | `SalesPriceModelBasic` | `int` | `int` | — |
| `ScrapConst` | `ScrapConst` | `decimal` | `decimal(18,4)` | — |
| `ScrapVar` | `ScrapVar` | `decimal` | `decimal(18,4)` | — |
| `SerialNumGroupId` | `SerialNumGroupId` | `string` | `nvarchar(10)` | — |
| `Sku` | `Sku` | `string` | `nvarchar(40)` | — |
| `SortCode` | `SortCode` | `int` | `int` | — |
| `StandardConfigId` | `StandardConfigId` | `string` | `nvarchar(50)` | — |
| `StandardInventColorId` | `StandardInventColorId` | `string` | `nvarchar(10)` | — |
| `StandardInventSizeId` | `StandardInventSizeId` | `string` | `nvarchar(10)` | — |
| `StandardInventStyleId` | `StandardInventStyleId` | `string` | `nvarchar(10)` | — |
| `StandardPalletQuantity` | `StandardPalletQuantity` | `decimal` | `decimal(18,4)` | — |
| `StatisticsFactor` | `StatisticsFactor` | `decimal` | `decimal(18,4)` | — |
| `TaraWeight` | `TaraWeight` | `decimal` | `decimal(18,4)` | — |
| `TaxPackagingQty` | `TaxPackagingQty` | `decimal` | `decimal(18,4)` | — |
| `TaxRateType` | `TaxRateType` | `long` | `bigint` | — |
| `UnitVolume` | `UnitVolume` | `decimal` | `decimal(18,4)` | — |
| `UseAltItemId` | `UseAltItemId` | `int` | `int` | — |
| `Width` | `Width` | `decimal` | `decimal(18,4)` | — |
| `WmsArrivalHandlingTime` | `WmsArrivalHandlingTime` | `int` | `int` | — |
| `WmsPalletTypeIdId` | `WmsPalletTypeIdId` | `string` | `nvarchar(15)` | — |
| `WmsPickingQtyTime` | `WmsPickingQtyTime` | `int` | `int` | — |

### InventTableModule

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.InventTableModule`. 31 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `AllocateMarkup` | `AllocateMarkup` | `int` | `int` | — |
| `BasePricePurchase` | `BasePricePurchase` | `int` | `int` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `EndDisc` | `EndDisc` | `int` | `int` | — |
| `IntercompanyBlocked` | `IntercompanyBlocked` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `ItemId` | `ItemId` | `string` | `nvarchar(20)` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LineDisc` | `LineDisc` | `string` | `nvarchar(10)` | — |
| `Markup` | `Markup` | `decimal` | `decimal(18,4)` | — |
| `MarkupGroupId` | `MarkupGroupId` | `string` | `nvarchar(10)` | — |
| `ModuleType` | `ModuleType` | `int` | `int` | — |
| `OverDeliveryPct` | `OverDeliveryPct` | `decimal` | `decimal(18,4)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PdsPricingPrecision` | `PdsPricingPrecision` | `int` | `int` | — |
| `Price` | `Price` | `decimal` | `decimal(18,4)` | — |
| `PriceDate` | `PriceDate` | `DateTime` | `datetime2` | — |
| `PriceQty` | `PriceQty` | `decimal` | `decimal(18,4)` | — |
| `PriceUnit` | `PriceUnit` | `decimal` | `decimal(18,4)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RetailInventoryAvailabilityBuffer` | `RetailInventoryAvailabilityBuffer` | `decimal` | `decimal(18,4)` | — |
| `RetailInventoryAvailabilityLevelProfile` | `RetailInventoryAvailabilityLevelProfile` | `string` | `nvarchar(10)` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `TaxItemGroupId` | `TaxItemGroupId` | `string` | `nvarchar(10)` | — |
| `UnderDeliveryPct` | `UnderDeliveryPct` | `decimal` | `decimal(18,4)` | — |
| `UnitId` | `UnitId` | `string` | `nvarchar(10)` | — |

### EcoResProduct

**No IXApi EF table mapping found in the current snapshot.** Class file exists: `src/Modules/Finance/Inventory/InventTable/Entities/EcoResProduct.cs`.

### InventDimCombination

**No IXApi EF table mapping found in the current snapshot.** No matching class file found.

### InventDim

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.InventDim`. 29 mapped columns.

D365 fields listed in the attachment but absent in IXApi: none.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | `int64` |
| `ConfigId` | `ConfigId` | `string` | `nvarchar(50)` | `str` |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | `str` |
| `InventBatchId` | `InventBatchId` | `string` | `nvarchar(20)` | `str` |
| `InventColorId` | `InventColorId` | `string` | `nvarchar(10)` | `str` |
| `InventDimId` | `InventDimId` | `string` | `nvarchar(100)` | `str` |
| `InventDimension10` | `InventDimension10` | `decimal` | `decimal(18,4)` | — |
| `InventDimension9` | `InventDimension9` | `DateTime` | `datetime2` | — |
| `InventDimension9TzId` | `InventDimension9TzId` | `int` | `int` | — |
| `InventLocationId` | `InventLocationId` | `string` | `nvarchar(10)` | `str` |
| `InventSerialId` | `InventSerialId` | `string` | `nvarchar(20)` | `str` |
| `InventSiteId` | `InventSiteId` | `string` | `nvarchar(10)` | `str` |
| `InventSizeId` | `InventSizeId` | `string` | `nvarchar(10)` | `str` |
| `InventStatusId` | `InventStatusId` | `string` | `nvarchar(10)` | — |
| `InventStyleId` | `InventStyleId` | `string` | `nvarchar(10)` | `str` |
| `InventVersionId` | `InventVersionId` | `string` | `nvarchar(10)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LicensePlateId` | `LicensePlateId` | `string` | `nvarchar(25)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `Sha1HashHex` | `Sha1HashHex` | `string` | `nvarchar(40)` | — |
| `Sha3HashHex` | `Sha3HashHex` | `string` | `nvarchar(96)` | — |
| `WmsLocationId` | `WmsLocationId` | `string` | `nvarchar(10)` | — |

### InventItemSalesSetup

**No IXApi EF table mapping found in the current snapshot.** No matching class file found.

### InventSite

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.InventSite`. 18 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `DefaultDimension` | `DefaultDimension` | `long` | `bigint` | — |
| `DefaultInventStatusID` | `DefaultInventStatusID` | `string` | `nvarchar(10)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsReceivingWarehouseOverrideAllowed` | `IsReceivingWarehouseOverrideAllowed` | `int` | `int` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `Name` | `Name` | `string` | `nvarchar(60)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SiteId` | `SiteId` | `string` | `nvarchar(10)` | — |
| `TaxBranchRefRecId` | `TaxBranchRefRecId` | `long` | `bigint` | — |
| `TimeZone` | `TimeZone` | `int` | `int` | — |

### InventLocation

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.InventLocation`. 72 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `AllowLaborStandards` | `AllowLaborStandards` | `int` | `int` | — |
| `AllowMarkingReservationRemoval` | `AllowMarkingReservationRemoval` | `int` | `int` | — |
| `AutoUpdateShipment` | `AutoUpdateShipment` | `int` | `int` | — |
| `ConsolidateShipAtRtw` | `ConsolidateShipAtRtw` | `int` | `int` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `CycleCountAllowPalletMove` | `CycleCountAllowPalletMove` | `int` | `int` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `DecrementLoadLine` | `DecrementLoadLine` | `int` | `int` | — |
| `DefaultKanbanFinishedGoodsLocation` | `DefaultKanbanFinishedGoodsLocation` | `string` | `nvarchar(10)` | — |
| `DefaultProductionFinishGoodsLocation` | `DefaultProductionFinishGoodsLocation` | `string` | `nvarchar(10)` | — |
| `DefaultProductionInputLocation` | `DefaultProductionInputLocation` | `string` | `nvarchar(10)` | — |
| `DefaultReturnCreditOnlyLocation` | `DefaultReturnCreditOnlyLocation` | `string` | `nvarchar(10)` | — |
| `DefaultStatusID` | `DefaultStatusID` | `string` | `nvarchar(10)` | — |
| `EnableExternalWarehouse` | `EnableExternalWarehouse` | `int` | `int` | — |
| `EnableQualityManagement` | `EnableQualityManagement` | `int` | `int` | — |
| `FshStore` | `FshStore` | `int` | `int` | — |
| `InventLocationId` | `InventLocationId` | `string` | `nvarchar(10)` | — |
| `InventLocationIdQuarantine` | `InventLocationIdQuarantine` | `string` | `nvarchar(10)` | — |
| `InventLocationIdReqMain` | `InventLocationIdReqMain` | `string` | `nvarchar(10)` | — |
| `InventLocationIdTransit` | `InventLocationIdTransit` | `string` | `nvarchar(10)` | — |
| `InventLocationLevel` | `InventLocationLevel` | `int` | `int` | — |
| `InventLocationType` | `InventLocationType` | `int` | `int` | — |
| `InventSiteId` | `InventSiteId` | `string` | `nvarchar(10)` | — |
| `InventUseDefaultProductionLocationForFormulaBom` | `InventUseDefaultProductionLocationForFormulaBom` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `ItmInventLocationIdGit` | `ItmInventLocationIdGit` | `string` | `nvarchar(10)` | — |
| `ItmInventLocationIdUnder` | `ItmInventLocationIdUnder` | `string` | `nvarchar(10)` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LoadReleaseReservationPolicy` | `LoadReleaseReservationPolicy` | `int` | `int` | — |
| `Manual` | `Manual` | `int` | `int` | — |
| `MaxPickingRouteTime` | `MaxPickingRouteTime` | `int` | `int` | — |
| `MaxPickingRouteVolume` | `MaxPickingRouteVolume` | `decimal` | `decimal(18,4)` | — |
| `Name` | `Name` | `string` | `nvarchar(60)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PickingLineTime` | `PickingLineTime` | `int` | `int` | — |
| `PrintBolBeforeShipConfirm` | `PrintBolBeforeShipConfirm` | `int` | `int` | — |
| `ProdReserveOnlyWhse` | `ProdReserveOnlyWhse` | `int` | `int` | — |
| `RafPostingMethod` | `RafPostingMethod` | `int` | `int` | — |
| `RboDefaultWmsLocationID` | `RboDefaultWmsLocationID` | `string` | `nvarchar(10)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RejectOrderFulfillment` | `RejectOrderFulfillment` | `string` | `nvarchar(10)` | — |
| `ReleaseRuleFailureOption` | `ReleaseRuleFailureOption` | `int` | `int` | — |
| `ReleaseToWarehouseRule` | `ReleaseToWarehouseRule` | `int` | `int` | — |
| `RemoveInventBlockingOnStatusChange` | `RemoveInventBlockingOnStatusChange` | `int` | `int` | — |
| `ReqRefill` | `ReqRefill` | `int` | `int` | — |
| `ReserveAtLoadPost` | `ReserveAtLoadPost` | `int` | `int` | — |
| `RetailInventNegFinancial` | `RetailInventNegFinancial` | `int` | `int` | — |
| `RetailInventNegPhysical` | `RetailInventNegPhysical` | `int` | `int` | — |
| `RetailWeightEx1` | `RetailWeightEx1` | `decimal` | `decimal(18,4)` | — |
| `RetailWmsLocationIDDefaultReturn` | `RetailWmsLocationIDDefaultReturn` | `string` | `nvarchar(10)` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `UniqueCheckDigits` | `UniqueCheckDigits` | `int` | `int` | — |
| `UseWmsOrders` | `UseWmsOrders` | `int` | `int` | — |
| `VendAccount` | `VendAccount` | `string` | `nvarchar(20)` | — |
| `WarehouseAutoReleaseReservation` | `WarehouseAutoReleaseReservation` | `int` | `int` | — |
| `WhsEnabled` | `WhsEnabled` | `int` | `int` | — |
| `WhsProdOrderBackflushMustUseReservedQty` | `WhsProdOrderBackflushMustUseReservedQty` | `int` | `int` | — |
| `WhsRawMaterialPolicy` | `WhsRawMaterialPolicy` | `int` | `int` | — |
| `WmsAisleNameActive` | `WmsAisleNameActive` | `int` | `int` | — |
| `WmsLevelFormat` | `WmsLevelFormat` | `string` | `nvarchar(10)` | — |
| `WmsLevelNameActive` | `WmsLevelNameActive` | `int` | `int` | — |
| `WmsLocationIdDefaultIssue` | `WmsLocationIdDefaultIssue` | `string` | `nvarchar(10)` | — |
| `WmsLocationIdDefaultReceipt` | `WmsLocationIdDefaultReceipt` | `string` | `nvarchar(10)` | — |
| `WmsPositionFormat` | `WmsPositionFormat` | `string` | `nvarchar(10)` | — |
| `WmsPositionNameActive` | `WmsPositionNameActive` | `int` | `int` | — |
| `WmsRackFormat` | `WmsRackFormat` | `string` | `nvarchar(10)` | — |
| `WmsRackNameActive` | `WmsRackNameActive` | `int` | `int` | — |
| `WorkflowApproval` | `WorkflowApproval` | `int` | `int` | — |

### UnitOfMeasure

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.UnitOfMeasure`. 15 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `DecimalPrecision` | `DecimalPrecision` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `Symbol` | `Symbol` | `string` | `nvarchar(5)` | — |
| `SystemOfUnits` | `SystemOfUnits` | `int` | `int` | — |
| `UnitOfMeasureClass` | `UnitOfMeasureClass` | `int` | `int` | — |

### UnitOfMeasureConversion

**No IXApi EF table mapping found in the current snapshot.** Class file exists: `src/Modules/Finance/Inventory/UOM/Entities/UnitOfMeasureConversion.cs`.

## Setup: pricing and charges

### PriceDiscAdmTable

IXApi entity: `IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDiscAdmTable`. 22 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `DefaultRelation` | `DefaultRelation` | `int` | `int` | — |
| `ExportCurrentPrice` | `ExportCurrentPrice` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `JournalName` | `JournalName` | `string` | `nvarchar(10)` | — |
| `JournalNum` | `JournalNum` | `string` | `nvarchar(100)` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LockedForDeletion` | `LockedForDeletion` | `int` | `int` | — |
| `Name` | `Name` | `string` | `nvarchar(150)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `Posted` | `Posted` | `int` | `int` | — |
| `PostedDate` | `PostedDate` | `DateTime?` | `datetime2` | — |
| `PriceApplyAdjustment` | `PriceApplyAdjustment` | `int` | `int` | — |
| `PriceComponentCombination` | `PriceComponentCombination` | `long` | `bigint` | — |
| `PriceGroup` | `PriceGroup` | `string` | `nvarchar(10)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |

### PriceDiscAdmTrans

IXApi entity: `IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDiscAdmTrans`. 57 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `AccountCode` | `AccountCode` | `int` | `int` | — |
| `AccountRelation` | `AccountRelation` | `string` | `nvarchar(20)` | — |
| `Agreement` | `Agreement` | `string` | `nvarchar(10)` | — |
| `AgreementHeaderExt_Ru` | `AgreementHeaderExt_Ru` | `long` | `bigint` | — |
| `AllocateMarkup` | `AllocateMarkup` | `int` | `int` | — |
| `Amount` | `Amount` | `decimal` | `decimal(32, 6)` | — |
| `CalendarDays` | `CalendarDays` | `int` | `int` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `Currency` | `Currency` | `string` | `nvarchar(3)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `DeliveryTime` | `DeliveryTime` | `int` | `int` | — |
| `DifferentFromPosted` | `DifferentFromPosted` | `int` | `int` | — |
| `DisregardLeadTime` | `DisregardLeadTime` | `int` | `int` | — |
| `FromDate` | `FromDate` | `DateTime` | `datetime2` | — |
| `GenericCurrency` | `GenericCurrency` | `int` | `int` | — |
| `InventBaileeFreeDays_Ru` | `InventBaileeFreeDays_Ru` | `int` | `int` | — |
| `InventDimId` | `InventDimId` | `string` | `nvarchar(100)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsGupTradeAgreement` | `IsGupTradeAgreement` | `int` | `int` | — |
| `ItemCode` | `ItemCode` | `int` | `int` | — |
| `ItemRelation` | `ItemRelation` | `string` | `nvarchar(20)` | — |
| `JournalNum` | `JournalNum` | `string` | `nvarchar(100)` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LineNum` | `LineNum` | `decimal` | `decimal(32, 16)` | — |
| `Log` | `Log` | `string` | `nvarchar(255)` | — |
| `Markup` | `Markup` | `decimal` | `decimal(32, 6)` | — |
| `MaximumRetailPrice_In` | `MaximumRetailPrice_In` | `decimal` | `decimal(32, 6)` | — |
| `Module` | `Module` | `int` | `int` | — |
| `MustBeDeleted` | `MustBeDeleted` | `int` | `int` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `Partition` | `Partition` | `long` | `bigint` | — |
| `PdsCalculationId` | `PdsCalculationId` | `string` | `nvarchar(10)` | — |
| `Percent1` | `Percent1` | `decimal` | `decimal(32, 6)` | — |
| `Percent2` | `Percent2` | `decimal` | `decimal(32, 6)` | — |
| `PriceApplyAdjustment` | `PriceApplyAdjustment` | `int` | `int` | — |
| `PriceComponentCombination` | `PriceComponentCombination` | `long` | `bigint` | — |
| `PriceDiscTableRef` | `PriceDiscTableRef` | `long` | `bigint` | — |
| `PriceGroup` | `PriceGroup` | `string` | `nvarchar(10)` | — |
| `PriceUnit` | `PriceUnit` | `decimal` | `decimal(32, 12)` | — |
| `PricingAttributesHeaderAreMatched` | `PricingAttributesHeaderAreMatched` | `int` | `int` | — |
| `PricingAttributesLineAreMatched` | `PricingAttributesLineAreMatched` | `int` | `int` | — |
| `PricingRuleHeader` | `PricingRuleHeader` | `long` | `bigint` | — |
| `PricingRuleLine` | `PricingRuleLine` | `long` | `bigint` | — |
| `QuantityAmountFrom` | `QuantityAmountFrom` | `decimal` | `decimal(32, 6)` | — |
| `QuantityAmountTo` | `QuantityAmountTo` | `decimal` | `decimal(32, 6)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `Relation` | `Relation` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SearchAgain` | `SearchAgain` | `int` | `int` | — |
| `SubBillFlatTierPrice` | `SubBillFlatTierPrice` | `decimal` | `decimal(32, 6)` | — |
| `ToDate` | `ToDate` | `DateTime` | `datetime2` | — |
| `UnitAppliesToAll` | `UnitAppliesToAll` | `int` | `int` | — |
| `UnitId` | `UnitId` | `string` | `nvarchar(10)` | — |

### PriceDiscTable

IXApi entity: `IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDiscTable`. 54 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `AccountCode` | `AccountCode` | `int` | `int` | — |
| `AccountRelation` | `AccountRelation` | `string` | `nvarchar(20)` | — |
| `Agreement` | `Agreement` | `string` | `nvarchar(10)` | — |
| `AgreementHeaderExt_Ru` | `AgreementHeaderExt_Ru` | `long` | `bigint` | — |
| `AllocateMarkup` | `AllocateMarkup` | `int` | `int` | — |
| `Amount` | `Amount` | `decimal` | `decimal(32, 6)` | — |
| `ApplicabilityId` | `ApplicabilityId` | `long` | `bigint` | — |
| `CalendarDays` | `CalendarDays` | `int` | `int` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `Currency` | `Currency` | `string` | `nvarchar(3)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `DeliveryTime` | `DeliveryTime` | `int` | `int` | — |
| `DisregardLeadTime` | `DisregardLeadTime` | `int` | `int` | — |
| `FromDate` | `FromDate` | `DateTime` | `datetime2` | — |
| `GenericCurrency` | `GenericCurrency` | `int` | `int` | — |
| `InventBaileeFreeDays_Ru` | `InventBaileeFreeDays_Ru` | `int` | `int` | — |
| `InventDimId` | `InventDimId` | `string` | `nvarchar(100)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsGupTradeAgreement` | `IsGupTradeAgreement` | `int` | `int` | — |
| `ItemCode` | `ItemCode` | `int` | `int` | — |
| `ItemRelation` | `ItemRelation` | `string` | `nvarchar(20)` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `Markup` | `Markup` | `decimal` | `decimal(32, 6)` | — |
| `MaximumRetailPrice_In` | `MaximumRetailPrice_In` | `decimal` | `decimal(32, 6)` | — |
| `McrFixedAmountCur` | `McrFixedAmountCur` | `decimal` | `decimal(32, 6)` | — |
| `McrMerchandisingEventId` | `McrMerchandisingEventId` | `string` | `nvarchar(10)` | — |
| `McrPriceDiscGroupType` | `McrPriceDiscGroupType` | `int` | `int` | — |
| `Module` | `Module` | `int` | `int` | — |
| `OriginalPriceDiscAdmTransRecId` | `OriginalPriceDiscAdmTransRecId` | `long` | `bigint` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `Partition` | `Partition` | `long` | `bigint` | — |
| `PdsCalculationId` | `PdsCalculationId` | `string` | `nvarchar(10)` | — |
| `Percent1` | `Percent1` | `decimal` | `decimal(32, 6)` | — |
| `Percent2` | `Percent2` | `decimal` | `decimal(32, 6)` | — |
| `PriceApplyAdjustment` | `PriceApplyAdjustment` | `int` | `int` | — |
| `PriceComponentCombination` | `PriceComponentCombination` | `long` | `bigint` | — |
| `PriceGroup` | `PriceGroup` | `string` | `nvarchar(10)` | — |
| `PriceUnit` | `PriceUnit` | `decimal` | `decimal(32, 12)` | — |
| `PricingRuleHeader` | `PricingRuleHeader` | `long` | `bigint` | — |
| `PricingRuleLine` | `PricingRuleLine` | `long` | `bigint` | — |
| `QuantityAmountFrom` | `QuantityAmountFrom` | `decimal` | `decimal(32, 6)` | — |
| `QuantityAmountTo` | `QuantityAmountTo` | `decimal` | `decimal(32, 6)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `Relation` | `Relation` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SearchAgain` | `SearchAgain` | `int` | `int` | — |
| `SubBillFlatTierPrice` | `SubBillFlatTierPrice` | `decimal` | `decimal(32, 6)` | — |
| `ToDate` | `ToDate` | `DateTime` | `datetime2` | — |
| `UnitAppliesToAll` | `UnitAppliesToAll` | `int` | `int` | — |
| `UnitId` | `UnitId` | `string` | `nvarchar(10)` | — |

### PriceDiscGroup

IXApi entity: `IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDiscGroup`. 21 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `GroupId` | `GroupId` | `string` | `nvarchar(10)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `McrPriceDiscGroupType` | `McrPriceDiscGroupType` | `int` | `int` | — |
| `Module` | `Module` | `int` | `int` | — |
| `Name` | `Name` | `string` | `nvarchar(150)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `Partition` | `Partition` | `long` | `bigint` | — |
| `PriceGroupAttributeEnable` | `PriceGroupAttributeEnable` | `int` | `int` | — |
| `PricingRuleRecId` | `PricingRuleRecId` | `long` | `bigint` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RetailCheckSalesPriceStatus` | `RetailCheckSalesPriceStatus` | `int` | `int` | — |
| `RetailPricingPriorityNumber` | `RetailPricingPriorityNumber` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `Type` | `Type` | `int` | `int` | — |

### MarkupTable

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.MarkupTable`. 31 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `CustPosting` | `CustPosting` | `int` | `int` | — |
| `CustType` | `CustType` | `int` | `int` | — |
| `CustomerLedgerDimension` | `CustomerLedgerDimension` | `long?` | `bigint` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `IncludeIntoIntrastatInvoiceValue` | `IncludeIntoIntrastatInvoiceValue` | `int` | `int` | — |
| `IncludeIntoIntrastatStatisticalValue` | `IncludeIntoIntrastatStatisticalValue` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsShipping` | `IsShipping` | `int` | `int` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `MarkupCode` | `MarkupCode` | `string` | `nvarchar(10)` | — |
| `MaxAmount` | `MaxAmount` | `decimal` | `decimal(18,2)` | — |
| `McrBrokerContractFee` | `McrBrokerContractFee` | `int` | `int` | — |
| `McrProrate` | `McrProrate` | `int` | `int` | — |
| `ModuleType` | `ModuleType` | `int` | `int` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `Refundable` | `Refundable` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `TaxItemGroup` | `TaxItemGroup` | `string` | `nvarchar(10)` | — |
| `TaxRateType` | `TaxRateType` | `long` | `bigint` | — |
| `TaxWithholdItemGroup` | `TaxWithholdItemGroup` | `long` | `bigint` | — |
| `Txt` | `Txt` | `string` | `nvarchar(150)` | — |
| `UseInMatching` | `UseInMatching` | `int` | `int` | — |
| `VendPosting` | `VendPosting` | `int` | `int` | — |
| `VendType` | `VendType` | `int` | `int` | — |
| `VendorLedgerDimension` | `VendorLedgerDimension` | `long?` | `bigint` | — |

### MarkupAutoTable

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.MarkupAutoTable`. 28 mapped columns.

D365 fields listed in the attachment but absent in IXApi: none.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | `int64` |
| `AccountCode` | `AccountCode` | `int` | `int` | `int32` |
| `AccountRelation` | `AccountRelation` | `string` | `nvarchar(20)` | `string` |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | `string` |
| `Description` | `Description` | `string` | `nvarchar(150)` | `string` |
| `DlvModeCode` | `DlvModeCode` | `int` | `int` | `int32` |
| `DlvModeRelation` | `DlvModeRelation` | `string` | `nvarchar(10)` | `string` |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `ItemCode` | `ItemCode` | `int` | `int` | `int32` |
| `ItemRelation` | `ItemRelation` | `string` | `nvarchar(20)` | `string` |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `MarkupReturn` | `MarkupReturn` | `int` | `int` | `int32` |
| `ModuleCategory` | `ModuleCategory` | `int` | `int` | `int32` |
| `ModuleType` | `ModuleType` | `int` | `int` | `int32` |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RetailAdvancedChargesDeliveryProrate` | `RetailAdvancedChargesDeliveryProrate` | `int` | `int` | `int32` |
| `RetailChannelCode` | `RetailChannelCode` | `int` | `int` | `int32` |
| `RetailChannelRelation` | `RetailChannelRelation` | `string` | `nvarchar(100)` | `string` |
| `RetailConcessionFee` | `RetailConcessionFee` | `int` | `int` | `int32` |
| `RetailConcessionFeeLegacy` | `RetailConcessionFeeLegacy` | `int` | `int` | `int32` |
| `ReturnRelation` | `ReturnRelation` | `string` | `nvarchar(100)` | `string` |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SHA256Hash` | `SHA256Hash` | `string` | `nvarchar(64)` | `string` |

### MarkupAutoLine

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.MarkupAutoLine`. 33 mapped columns.

D365 fields listed in the attachment but absent in IXApi: none.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | `int64` |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `CurrencyCode` | `CurrencyCode` | `string` | `nvarchar(3)` | `string` |
| `CustomsAssessableValue_IN` | `CustomsAssessableValue_IN` | `int` | `int` | `int32` |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | `string` |
| `FromAmount` | `FromAmount` | `decimal` | `decimal(18,4)` | `decimal` |
| `InventLocationId` | `InventLocationId` | `string` | `nvarchar(10)` | `string` |
| `InventSiteId` | `InventSiteId` | `string` | `nvarchar(10)` | `string` |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `Keep` | `Keep` | `int` | `int` | `int32` |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LineNum` | `LineNum` | `decimal` | `decimal(18,4)` | `decimal` |
| `MCRReturnMarkup` | `MCRReturnMarkup` | `int` | `int` | `int32` |
| `MarkupCategory` | `MarkupCategory` | `int` | `int` | `int32` |
| `MarkupCode` | `MarkupCode` | `string` | `nvarchar(10)` | `string` |
| `MarkupCurrencyCode` | `MarkupCurrencyCode` | `string` | `nvarchar(3)` | `string` |
| `ModuleCategory` | `ModuleCategory` | `int` | `int` | `int32` |
| `ModuleType` | `ModuleType` | `int` | `int` | `int32` |
| `NotionalCharges_IN` | `NotionalCharges_IN` | `int` | `int` | `int32` |
| `NotionalPct_IN` | `NotionalPct_IN` | `decimal` | `decimal(18,4)` | `decimal` |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `TableRecId` | `TableRecId` | `long` | `bigint` | `int64` |
| `TableTableId` | `TableTableId` | `int` | `int` | `int32` |
| `TaxGroup` | `TaxGroup` | `string` | `nvarchar(10)` | `string` |
| `TaxItemGroup` | `TaxItemGroup` | `string` | `nvarchar(10)` | `string` |
| `ToAmount` | `ToAmount` | `decimal` | `decimal(18,4)` | `decimal` |
| `Txt` | `Txt` | `string` | `nvarchar(150)` | `string` |
| `Value` | `Value` | `decimal` | `decimal(18,4)` | `decimal` |

## Setup: tax and terms

### TaxGroupHeading

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.TaxGroupHeading`. 25 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `EuTrade_W` | `EuTrade_W` | `int` | `int` | — |
| `FillSalesDate_W` | `FillSalesDate_W` | `int` | `int` | — |
| `FillVatDueDateBasedOn` | `FillVatDueDateBasedOn` | `int` | `int` | — |
| `FillVatDueDatePeriod` | `FillVatDueDatePeriod` | `int` | `int` | — |
| `FillVatDueDatePeriodNumber` | `FillVatDueDatePeriodNumber` | `int` | `int` | — |
| `FillVatDueDate_W` | `FillVatDueDate_W` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `MandatorySalesDate_W` | `MandatorySalesDate_W` | `int` | `int` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `Source` | `Source` | `int` | `int` | — |
| `TaxGroup` | `TaxGroup` | `string` | `nvarchar(10)` | — |
| `TaxGroupName` | `TaxGroupName` | `string` | `nvarchar(60)` | — |
| `TaxGroupRounding` | `TaxGroupRounding` | `int` | `int` | — |
| `TaxGroupSetup` | `TaxGroupSetup` | `int` | `int` | — |
| `TaxPrintDetail` | `TaxPrintDetail` | `int` | `int` | — |
| `TaxReverseOnCashDisc` | `TaxReverseOnCashDisc` | `int` | `int` | — |

### TaxGroupData

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.TaxGroupData`. 18 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `ExemptTax` | `ExemptTax` | `int` | `int` | — |
| `IntracomVat` | `IntracomVat` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `ReverseCharge_W` | `ReverseCharge_W` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `TaxCode` | `TaxCode` | `string` | `nvarchar(10)` | — |
| `TaxExemptCode` | `TaxExemptCode` | `string` | `nvarchar(10)` | — |
| `TaxGroup` | `TaxGroup` | `string` | `nvarchar(10)` | — |
| `UseTax` | `UseTax` | `int` | `int` | — |

### TaxItemGroupHeading

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.TaxItemGroupHeading`. 15 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `EuSalesListType` | `EuSalesListType` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `Name` | `Name` | `string` | `nvarchar(60)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `Source` | `Source` | `int` | `int` | — |
| `TaxItemGroup` | `TaxItemGroup` | `string` | `nvarchar(10)` | — |

### TaxOnItem

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.TaxOnItem`. 14 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `TaxCode` | `TaxCode` | `string` | `nvarchar(10)` | — |
| `TaxExemptCode` | `TaxExemptCode` | `string` | `nvarchar(10)` | — |
| `TaxItemGroup` | `TaxItemGroup` | `string` | `nvarchar(10)` | — |

### TaxTable

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.TaxTable`. 60 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `ExcludeFromInvoice` | `ExcludeFromInvoice` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `NegativeTax` | `NegativeTax` | `int` | `int` | — |
| `NotEuSalesList` | `NotEuSalesList` | `int` | `int` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PaymentTaxCode` | `PaymentTaxCode` | `string` | `nvarchar(10)` | — |
| `PrintCode` | `PrintCode` | `string` | `nvarchar(10)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `ReconcileAmountOrigin` | `ReconcileAmountOrigin` | `int` | `int` | — |
| `RepFieldBaseIncoming` | `RepFieldBaseIncoming` | `int` | `int` | — |
| `RepFieldBaseIncomingCreditNote` | `RepFieldBaseIncomingCreditNote` | `int` | `int` | — |
| `RepFieldBaseOutgoing` | `RepFieldBaseOutgoing` | `int` | `int` | — |
| `RepFieldBaseOutgoingCreditNote` | `RepFieldBaseOutgoingCreditNote` | `int` | `int` | — |
| `RepFieldBaseUseTax` | `RepFieldBaseUseTax` | `int` | `int` | — |
| `RepFieldBaseUseTaxCreditNote` | `RepFieldBaseUseTaxCreditNote` | `int` | `int` | — |
| `RepFieldBaseUseTaxOffset` | `RepFieldBaseUseTaxOffset` | `int` | `int` | — |
| `RepFieldBaseUseTaxOffsetCreditNote` | `RepFieldBaseUseTaxOffsetCreditNote` | `int` | `int` | — |
| `RepFieldTaxFreeBuy` | `RepFieldTaxFreeBuy` | `int` | `int` | — |
| `RepFieldTaxFreeBuyCreditNote` | `RepFieldTaxFreeBuyCreditNote` | `int` | `int` | — |
| `RepFieldTaxFreeSales` | `RepFieldTaxFreeSales` | `int` | `int` | — |
| `RepFieldTaxFreeSalesCreditNote` | `RepFieldTaxFreeSalesCreditNote` | `int` | `int` | — |
| `RepFieldTaxIncoming` | `RepFieldTaxIncoming` | `int` | `int` | — |
| `RepFieldTaxIncomingCreditNote` | `RepFieldTaxIncomingCreditNote` | `int` | `int` | — |
| `RepFieldTaxOutgoing` | `RepFieldTaxOutgoing` | `int` | `int` | — |
| `RepFieldTaxOutgoingCreditNote` | `RepFieldTaxOutgoingCreditNote` | `int` | `int` | — |
| `RepFieldUseTax` | `RepFieldUseTax` | `int` | `int` | — |
| `RepFieldUseTaxCreditNote` | `RepFieldUseTaxCreditNote` | `int` | `int` | — |
| `RepFieldUseTaxOffset` | `RepFieldUseTaxOffset` | `int` | `int` | — |
| `RepFieldUseTaxOffsetCreditNote` | `RepFieldUseTaxOffsetCreditNote` | `int` | `int` | — |
| `RoundDeductibleFirst` | `RoundDeductibleFirst` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `Source` | `Source` | `int` | `int` | — |
| `TaxAccountGroup` | `TaxAccountGroup` | `string` | `nvarchar(10)` | — |
| `TaxAllowLineDiscountOnTaxPerUnit` | `TaxAllowLineDiscountOnTaxPerUnit` | `int` | `int` | — |
| `TaxBase` | `TaxBase` | `int` | `int` | — |
| `TaxCalcMethod` | `TaxCalcMethod` | `int` | `int` | — |
| `TaxCode` | `TaxCode` | `string` | `nvarchar(10)` | — |
| `TaxCountryRegionType` | `TaxCountryRegionType` | `int` | `int` | — |
| `TaxCurrencyCode` | `TaxCurrencyCode` | `string` | `nvarchar(3)` | — |
| `TaxIncludeInTax` | `TaxIncludeInTax` | `int` | `int` | — |
| `TaxJurisdictionCode` | `TaxJurisdictionCode` | `string` | `nvarchar(10)` | — |
| `TaxLimitBase` | `TaxLimitBase` | `int` | `int` | — |
| `TaxName` | `TaxName` | `string` | `nvarchar(30)` | — |
| `TaxOnTax` | `TaxOnTax` | `string` | `nvarchar(10)` | — |
| `TaxPackagingTax` | `TaxPackagingTax` | `int` | `int` | — |
| `TaxPeriod` | `TaxPeriod` | `string` | `nvarchar(10)` | — |
| `TaxPurchaseTax` | `TaxPurchaseTax` | `int` | `int` | — |
| `TaxRoundOff` | `TaxRoundOff` | `decimal` | `decimal(18,4)` | — |
| `TaxRoundOffType` | `TaxRoundOffType` | `int` | `int` | — |
| `TaxType_W` | `TaxType_W` | `int` | `int` | — |
| `TaxUnit` | `TaxUnit` | `string` | `nvarchar(5)` | — |
| `TaxWriteSelection` | `TaxWriteSelection` | `int` | `int` | — |
| `UnrealizedTax` | `UnrealizedTax` | `int` | `int` | — |

### TaxData

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.TaxData`. 19 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `TaxCode` | `TaxCode` | `string` | `nvarchar(10)` | — |
| `TaxFromDate` | `TaxFromDate` | `DateTime` | `datetime2` | — |
| `TaxLimitMax` | `TaxLimitMax` | `decimal` | `decimal(18,4)` | — |
| `TaxLimitMin` | `TaxLimitMin` | `decimal` | `decimal(18,4)` | — |
| `TaxSubstitutionMarkupValue` | `TaxSubstitutionMarkupValue` | `decimal` | `decimal(18,4)` | — |
| `TaxToDate` | `TaxToDate` | `DateTime` | `datetime2` | — |
| `TaxValue` | `TaxValue` | `decimal` | `decimal(18,4)` | — |
| `VatExemptPct` | `VatExemptPct` | `decimal` | `decimal(18,4)` | — |

### PaymTerm

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.PaymTerm`. 31 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `AdditionalMonths` | `AdditionalMonths` | `int` | `int` | — |
| `Cash` | `Cash` | `int` | `int` | — |
| `CashLedgerDimension` | `CashLedgerDimension` | `long` | `bigint` | — |
| `CfmPaymentRequestTypePayment` | `CfmPaymentRequestTypePayment` | `long` | `bigint` | — |
| `CfmPaymentRequestTypePrepayment` | `CfmPaymentRequestTypePrepayment` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `CreditCardCreditCheck` | `CreditCardCreditCheck` | `int` | `int` | — |
| `CreditCardPaymentType` | `CreditCardPaymentType` | `int` | `int` | — |
| `CustomerUpdateDueDate` | `CustomerUpdateDueDate` | `int` | `int` | — |
| `CutOffDay` | `CutOffDay` | `int` | `int` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `Description` | `Description` | `string` | `nvarchar(60)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `NumOfDays` | `NumOfDays` | `int` | `int` | — |
| `NumOfMonths` | `NumOfMonths` | `int` | `int` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PaymDayId` | `PaymDayId` | `string` | `nvarchar(10)` | — |
| `PaymMethod` | `PaymMethod` | `int` | `int` | — |
| `PaymSched` | `PaymSched` | `string` | `nvarchar(30)` | — |
| `PaymTermId` | `PaymTermId` | `string` | `nvarchar(100)` | — |
| `PostOffsettingAr` | `PostOffsettingAr` | `int` | `int` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `ShipCarrierAncillaryCharge` | `ShipCarrierAncillaryCharge` | `int` | `int` | — |
| `ShipCarrierCertifiedCheck` | `ShipCarrierCertifiedCheck` | `int` | `int` | — |
| `VendorUpdateDueDate` | `VendorUpdateDueDate` | `int` | `int` | — |

### DlvTerm

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.DlvTerm`. 18 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `Code` | `Code` | `string` | `nvarchar(10)` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `FreightChargeTerm` | `FreightChargeTerm` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `ItmGoodsInTransitControl` | `ItmGoodsInTransitControl` | `int` | `int` | — |
| `ItmPortMandatory` | `ItmPortMandatory` | `int` | `int` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `ShipCarrierFreeMinimum` | `ShipCarrierFreeMinimum` | `decimal` | `decimal(18,4)` | — |
| `TaxLocationRole` | `TaxLocationRole` | `int` | `int` | — |
| `Txt` | `Txt` | `string` | `nvarchar(150)` | — |

### DlvMode

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.DlvMode`. 18 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `Code` | `Code` | `string` | `nvarchar(10)` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `DisplayOrder` | `DisplayOrder` | `int` | `int` | — |
| `DomPriority` | `DomPriority` | `long` | `bigint` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `MarkupGroup` | `MarkupGroup` | `string` | `nvarchar(10)` | — |
| `McrExpedite` | `McrExpedite` | `string` | `nvarchar(10)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `ShipCarrierDlvType` | `ShipCarrierDlvType` | `int` | `int` | — |
| `Txt` | `Txt` | `string` | `nvarchar(150)` | — |

### SalesParameters

**No IXApi EF table mapping found in the current snapshot.** No matching class file found.

## Order and inventory

### SalesTable

IXApi entity: `IAX.IXApi.Modules.Finance.AccountsReceivable.SalesTable`. 191 mapped columns.

D365 fields listed in the attachment but absent in IXApi: none.

Naming differences: D365 `Payment` → IXApi `PaymTerm`, D365 `TaxGroup` → IXApi `TaxGroupId`.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | `int64` |
| `AccountingDistributionTemplate` | `AccountingDistributionTemplate` | `long` | `bigint` | — |
| `AddressRefRecId` | `AddressRefRecId` | `long` | `bigint` | — |
| `AddressRefTableId` | `AddressRefTableId` | `int` | `int` | — |
| `AsohOrderClass` | `AsohOrderClass` | `string` | `nvarchar(10)` | — |
| `AutoSummaryModuleType` | `AutoSummaryModuleType` | `int` | `int` | — |
| `BankDocumentType` | `BankDocumentType` | `int` | `int` | — |
| `CaseTagging` | `CaseTagging` | `int` | `int` | — |
| `CashDisc` | `CashDisc` | `string` | `nvarchar(10)` | — |
| `CashDiscBaseDate` | `CashDiscBaseDate` | `DateTime` | `datetime2` | — |
| `CashDiscBaseDays` | `CashDiscBaseDays` | `int` | `int` | — |
| `CashDiscPercent` | `CashDiscPercent` | `decimal` | `decimal(18,4)` | — |
| `CommissionGroup` | `CommissionGroup` | `string` | `nvarchar(10)` | — |
| `ContactPersonId` | `ContactPersonId` | `string` | `nvarchar(20)` | — |
| `CountyOrigDest` | `CountyOrigDest` | `string` | `nvarchar(30)` | — |
| `CovStatus` | `CovStatus` | `int` | `int` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `CreatedTransactionId` | `CreatedTransactionId` | `long` | `bigint` | — |
| `CredManExcludeSalesOrder` | `CredManExcludeSalesOrder` | `bool` | `bit` | — |
| `CredManId` | `CredManId` | `string` | `nvarchar(100)` | — |
| `CredManInCreditControl` | `CredManInCreditControl` | `bool` | `bit` | — |
| `CredManRejected` | `CredManRejected` | `bool` | `bit` | — |
| `CredManReleasedFromCreditControl` | `CredManReleasedFromCreditControl` | `bool` | `bit` | — |
| `CreditCardApprovalAmount` | `CreditCardApprovalAmount` | `decimal` | `decimal(18,4)` | — |
| `CreditCardAuthorization` | `CreditCardAuthorization` | `string` | `nvarchar(10)` | — |
| `CreditCardAuthorizationError` | `CreditCardAuthorizationError` | `int` | `int` | — |
| `CreditCardCustRefId` | `CreditCardCustRefId` | `long` | `bigint` | — |
| `CreditNoteReasonCode` | `CreditNoteReasonCode` | `long` | `bigint` | — |
| `CurrencyCode` | `CurrencyCode` | `string` | `nvarchar(3)` | `str` |
| `CustAccount` | `CustAccount` | `string` | `nvarchar(20)` | `str` |
| `CustGroup` | `CustGroup` | `string` | `nvarchar(20)` | — |
| `CustInvoiceId` | `CustInvoiceId` | `string` | `nvarchar(100)` | — |
| `CustRequisitionNum` | `CustRequisitionNum` | `string` | `nvarchar(60)` | — |
| `CustomerRef` | `CustomerRef` | `string` | `nvarchar(100)` | `str` |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(10)` | `str` |
| `Deadline` | `Deadline` | `DateTime` | `datetime2` | — |
| `DefaultDimension` | `DefaultDimension` | `long` | `bigint` | `int64` |
| `DeliveryDate` | `DeliveryDate` | `DateTime` | `datetime2` | — |
| `DeliveryDateControlType` | `DeliveryDateControlType` | `int` | `int` | — |
| `DeliveryName` | `DeliveryName` | `string` | `nvarchar(250)` | `str` |
| `DeliveryPostalAddress` | `DeliveryPostalAddress` | `long` | `bigint` | `int64` |
| `DirectDebitMandate` | `DirectDebitMandate` | `long` | `bigint` | — |
| `DiscPercent` | `DiscPercent` | `decimal` | `decimal(18,4)` | — |
| `DiscTotal` | `DiscTotal` | `decimal` | `decimal(18,4)` | — |
| `DlvMode` | `DlvMode` | `string` | `nvarchar(10)` | `str` |
| `DlvReason` | `DlvReason` | `string` | `nvarchar(10)` | — |
| `DlvTerm` | `DlvTerm` | `string` | `nvarchar(10)` | `str` |
| `DocumentStatus` | `DocumentStatus` | `int` | `int` | `enum` |
| `DomExceptionType` | `DomExceptionType` | `int` | `int` | — |
| `DomIgnore` | `DomIgnore` | `bool` | `bit` | — |
| `DomIterations` | `DomIterations` | `int` | `int` | — |
| `DomProcessed` | `DomProcessed` | `bool` | `bit` | — |
| `DomProcessedDateTime` | `DomProcessedDateTime` | `DateTime` | `datetime2` | — |
| `DomProcessedDateTimeTZID` | `DomProcessedDateTimeTZID` | `int` | `int` | — |
| `EInvoiceAccountCode` | `EInvoiceAccountCode` | `string` | `nvarchar(10)` | — |
| `EInvoiceLineSpec` | `EInvoiceLineSpec` | `int` | `int` | — |
| `Email` | `Email` | `string` | `nvarchar(80)` | — |
| `EndDisc` | `EndDisc` | `string` | `nvarchar(10)` | `real` |
| `EnterpriseNumber` | `EnterpriseNumber` | `string` | `nvarchar(20)` | — |
| `Estimate` | `Estimate` | `decimal` | `decimal(18,4)` | — |
| `ExportReason` | `ExportReason` | `string` | `nvarchar(10)` | — |
| `FinTag` | `FinTag` | `long` | `bigint` | — |
| `FixedDueDate` | `FixedDueDate` | `DateTime` | `datetime2` | — |
| `FixedExchRate` | `FixedExchRate` | `decimal` | `decimal(18,4)` | — |
| `FreightSlipType` | `FreightSlipType` | `int` | `int` | — |
| `FreightZone` | `FreightZone` | `string` | `nvarchar(10)` | — |
| `FundingSource` | `FundingSource` | `long` | `bigint` | — |
| `GiroType` | `GiroType` | `int` | `int` | — |
| `GupDelayPricingCalculation` | `GupDelayPricingCalculation` | `int` | `int` | — |
| `GupSkipPricingCalculation` | `GupSkipPricingCalculation` | `int` | `int` | — |
| `InclTax` | `InclTax` | `bool` | `bit` | — |
| `IntercompanyAllowIndirectCreation` | `IntercompanyAllowIndirectCreation` | `bool` | `bit` | — |
| `IntercompanyAllowIndirectCreationOrig` | `IntercompanyAllowIndirectCreationOrig` | `bool` | `bit` | — |
| `IntercompanyAutoCreateOrders` | `IntercompanyAutoCreateOrders` | `bool` | `bit` | — |
| `IntercompanyCompanyId` | `IntercompanyCompanyId` | `string` | `nvarchar(4)` | — |
| `IntercompanyDirectDelivery` | `IntercompanyDirectDelivery` | `bool` | `bit` | — |
| `IntercompanyDirectDeliveryOrig` | `IntercompanyDirectDeliveryOrig` | `bool` | `bit` | — |
| `IntercompanyOrder` | `IntercompanyOrder` | `bool` | `bit` | — |
| `IntercompanyOrigin` | `IntercompanyOrigin` | `int` | `int` | — |
| `IntercompanyOriginalCustAccount` | `IntercompanyOriginalCustAccount` | `string` | `nvarchar(20)` | — |
| `IntercompanyOriginalSalesId` | `IntercompanyOriginalSalesId` | `string` | `nvarchar(100)` | — |
| `IntercompanyPurchId` | `IntercompanyPurchId` | `string` | `nvarchar(100)` | — |
| `InventLocationId` | `InventLocationId` | `string` | `nvarchar(50)` | `str` |
| `InventSiteId` | `InventSiteId` | `string` | `nvarchar(50)` | `str` |
| `InvoiceAccount` | `InvoiceAccount` | `string` | `nvarchar(20)` | `str` |
| `InvoiceType` | `InvoiceType` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `ItemTagging` | `ItemTagging` | `int` | `int` | — |
| `LanguageId` | `LanguageId` | `string` | `nvarchar(7)` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LineDisc` | `LineDisc` | `string` | `nvarchar(10)` | — |
| `ListCode` | `ListCode` | `int` | `int` | — |
| `ManualEntryChangePolicy` | `ManualEntryChangePolicy` | `long` | `bigint` | — |
| `MarkupGroup` | `MarkupGroup` | `string` | `nvarchar(10)` | — |
| `MatchingAgreement` | `MatchingAgreement` | `long` | `bigint` | — |
| `McrOrderStopped` | `McrOrderStopped` | `bool` | `bit` | — |
| `ModifiedTransactionId` | `ModifiedTransactionId` | `long` | `bigint` | — |
| `MpsExcludeSalesOrder` | `MpsExcludeSalesOrder` | `bool` | `bit` | — |
| `MpsFullRunCtpStatus` | `MpsFullRunCtpStatus` | `int` | `int` | — |
| `MultiLineDisc` | `MultiLineDisc` | `string` | `nvarchar(10)` | — |
| `Notes` | `Notes` | `string` | `nvarchar(max)` | — |
| `NumberSequenceGroup` | `NumberSequenceGroup` | `string` | `nvarchar(10)` | — |
| `OneTimeCustomer` | `OneTimeCustomer` | `int` | `int` | — |
| `OrderDate` | `OrderDate` | `DateTime` | `datetime2` | — |
| `OverrideSalesTax` | `OverrideSalesTax` | `int` | `int` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PalletTagging` | `PalletTagging` | `int` | `int` | — |
| `PaymMode` | `PaymMode` | `string` | `nvarchar(10)` | `str` |
| `PaymSpec` | `PaymSpec` | `string` | `nvarchar(10)` | — |
| `PaymTerm` | `PaymTerm` | `string` | `nvarchar(100)` | `str` (`Payment`) |
| `PaymentSched` | `PaymentSched` | `string` | `nvarchar(30)` | — |
| `PdsBatchAttribAutoRes` | `PdsBatchAttribAutoRes` | `int` | `int` | — |
| `PdsCustRebateGroupId` | `PdsCustRebateGroupId` | `string` | `nvarchar(10)` | — |
| `PdsRebateProgramTmaGroup` | `PdsRebateProgramTmaGroup` | `string` | `nvarchar(10)` | — |
| `Phone` | `Phone` | `string` | `nvarchar(20)` | — |
| `Port` | `Port` | `string` | `nvarchar(10)` | — |
| `PostingProfile` | `PostingProfile` | `string` | `nvarchar(10)` | — |
| `PriceGroupId` | `PriceGroupId` | `string` | `nvarchar(10)` | — |
| `ProjId` | `ProjId` | `string` | `nvarchar(20)` | — |
| `PurchOrderFormNum` | `PurchOrderFormNum` | `string` | `nvarchar(20)` | `str` |
| `QuotationId` | `QuotationId` | `string` | `nvarchar(100)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `ReceiptDateConfirmed` | `ReceiptDateConfirmed` | `DateTime` | `datetime2` | `date` |
| `ReceiptDateRequested` | `ReceiptDateRequested` | `DateTime` | `datetime2` | `date` |
| `ReleaseStatus` | `ReleaseStatus` | `int` | `int` | — |
| `ReportingCurrencyFixedExchRate` | `ReportingCurrencyFixedExchRate` | `decimal` | `decimal(18,4)` | — |
| `Reservation` | `Reservation` | `int` | `int` | — |
| `RetailChannelTable` | `RetailChannelTable` | `long` | `bigint` | — |
| `ReturnDeadline` | `ReturnDeadline` | `DateTime` | `datetime2` | — |
| `ReturnItemNum` | `ReturnItemNum` | `string` | `nvarchar(20)` | — |
| `ReturnNotes` | `ReturnNotes` | `string` | `nvarchar(max)` | — |
| `ReturnReasonCodeId` | `ReturnReasonCodeId` | `string` | `nvarchar(10)` | — |
| `ReturnReplacementCreated` | `ReturnReplacementCreated` | `bool` | `bit` | — |
| `ReturnReplacementId` | `ReturnReplacementId` | `string` | `nvarchar(100)` | — |
| `ReturnStatus` | `ReturnStatus` | `int` | `int` | — |
| `RevRecContractEndDate` | `RevRecContractEndDate` | `DateTime` | `datetime2` | — |
| `RevRecContractStartDate` | `RevRecContractStartDate` | `DateTime` | `datetime2` | — |
| `RevRecFollowOriginalPricingMethod` | `RevRecFollowOriginalPricingMethod` | `bool` | `bit` | — |
| `RevRecLatestReverseJournal` | `RevRecLatestReverseJournal` | `long` | `bigint` | — |
| `RevRecMultipleSoReallocation` | `RevRecMultipleSoReallocation` | `bool` | `bit` | — |
| `RevRecReallocationId` | `RevRecReallocationId` | `string` | `nvarchar(20)` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SalesGroup` | `SalesGroup` | `string` | `nvarchar(10)` | — |
| `SalesId` | `SalesId` | `string` | `nvarchar(20)` | `str` |
| `SalesName` | `SalesName` | `string` | `nvarchar(60)` | `str` |
| `SalesNameAlias` | `SalesNameAlias` | `string` | `nvarchar(60)` | — |
| `SalesOriginId` | `SalesOriginId` | `string` | `nvarchar(10)` | — |
| `SalesPoolId` | `SalesPoolId` | `string` | `nvarchar(10)` | — |
| `SalesStatus` | `SalesStatus` | `int` | `int` | `enum` |
| `SalesType` | `SalesType` | `string` | `nvarchar(50)` | `enum` |
| `SalesUnitId` | `SalesUnitId` | `string` | `nvarchar(10)` | — |
| `ServiceCodeRefRecId` | `ServiceCodeRefRecId` | `long` | `bigint` | — |
| `SettleVoucher` | `SettleVoucher` | `int` | `int` | — |
| `ShipCarrierAccount` | `ShipCarrierAccount` | `string` | `nvarchar(10)` | — |
| `ShipCarrierAccountCode` | `ShipCarrierAccountCode` | `string` | `nvarchar(10)` | — |
| `ShipCarrierBlindShipment` | `ShipCarrierBlindShipment` | `int` | `int` | — |
| `ShipCarrierDeliveryContact` | `ShipCarrierDeliveryContact` | `string` | `nvarchar(60)` | — |
| `ShipCarrierDlvType` | `ShipCarrierDlvType` | `int` | `int` | — |
| `ShipCarrierId` | `ShipCarrierId` | `string` | `nvarchar(10)` | — |
| `ShipCarrierName` | `ShipCarrierName` | `string` | `nvarchar(60)` | — |
| `ShipCarrierPostalAddress` | `ShipCarrierPostalAddress` | `long` | `bigint` | — |
| `ShipCarrierResidential` | `ShipCarrierResidential` | `bool` | `bit` | — |
| `ShippingDateConfirmed` | `ShippingDateConfirmed` | `DateTime` | `datetime2` | `date` |
| `ShippingDateRequested` | `ShippingDateRequested` | `DateTime` | `datetime2` | `date` |
| `SmmCampaignId` | `SmmCampaignId` | `string` | `nvarchar(10)` | — |
| `SmmSalesAmountTotal` | `SmmSalesAmountTotal` | `decimal` | `decimal(18,4)` | — |
| `SourceDocumentHeader` | `SourceDocumentHeader` | `long` | `bigint` | — |
| `StatProcId` | `StatProcId` | `string` | `nvarchar(10)` | — |
| `SubBillBillToName` | `SubBillBillToName` | `string` | `nvarchar(60)` | — |
| `SubBillBillToPostalAddress` | `SubBillBillToPostalAddress` | `long` | `bigint` | — |
| `SubBillCreatedFromSb` | `SubBillCreatedFromSb` | `int` | `int` | — |
| `SubBillSuppressChild` | `SubBillSuppressChild` | `int` | `int` | — |
| `SysDataStateCode` | `SysDataStateCode` | `int` | `int` | — |
| `SystemEntryChangePolicy` | `SystemEntryChangePolicy` | `long` | `bigint` | — |
| `SystemEntrySource` | `SystemEntrySource` | `int` | `int` | — |
| `TamDeductionId` | `TamDeductionId` | `string` | `nvarchar(20)` | — |
| `TamRebateReference` | `TamRebateReference` | `string` | `nvarchar(20)` | — |
| `TaxGroupId` | `TaxGroupId` | `string` | `nvarchar(10)` | `str` (`TaxGroup`) |
| `TaxId` | `TaxId` | `long` | `bigint` | — |
| `TransactionCode` | `TransactionCode` | `string` | `nvarchar(10)` | — |
| `Transport` | `Transport` | `string` | `nvarchar(10)` | — |
| `TransportationDocument` | `TransportationDocument` | `long` | `bigint` | — |
| `Url` | `Url` | `string` | `nvarchar(255)` | — |
| `VatNum` | `VatNum` | `string` | `nvarchar(20)` | — |
| `VatNumRecId` | `VatNumRecId` | `long` | `bigint` | — |
| `VatNumTableType` | `VatNumTableType` | `int` | `int` | — |
| `WorkerSalesResponsible` | `WorkerSalesResponsible` | `long` | `bigint` | — |
| `WorkerSalesTaker` | `WorkerSalesTaker` | `long` | `bigint` | — |

### SalesLine

IXApi entity: `IAX.IXApi.Modules.Finance.AccountsReceivable.SalesLine`. 146 mapped columns.

D365 fields listed in the attachment but absent in IXApi: none.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | `int64` |
| `AccountingDistributionTemplate` | `AccountingDistributionTemplate` | `long` | `bigint` | — |
| `AddressRefRecId` | `AddressRefRecId` | `long` | `bigint` | — |
| `AddressRefTableId` | `AddressRefTableId` | `int` | `int` | — |
| `AgreementSkipAutoLink` | `AgreementSkipAutoLink` | `int` | `int` | — |
| `Blocked` | `Blocked` | `int` | `int` | — |
| `BundleLineStatus` | `BundleLineStatus` | `int` | `int` | — |
| `BundleLineType` | `BundleLineType` | `int` | `int` | — |
| `CaseTagging` | `CaseTagging` | `int` | `int` | — |
| `Complete` | `Complete` | `int` | `int` | — |
| `ConfirmedDlv` | `ConfirmedDlv` | `DateTime` | `datetime2` | — |
| `CostPrice` | `CostPrice` | `decimal` | `decimal(18,4)` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `CreditNoteReasonCode` | `CreditNoteReasonCode` | `long` | `bigint` | — |
| `CurrencyCode` | `CurrencyCode` | `string` | `nvarchar(3)` | `str` |
| `CustAccount` | `CustAccount` | `string` | `nvarchar(20)` | `str` |
| `CustGroupId` | `CustGroupId` | `string` | `nvarchar(20)` | — |
| `CustomerLineNum` | `CustomerLineNum` | `int` | `int` | — |
| `CustomerRef` | `CustomerRef` | `string` | `nvarchar(100)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | `str` |
| `DefaultDimension` | `DefaultDimension` | `long` | `bigint` | `int64` |
| `DeliveryDateControlType` | `DeliveryDateControlType` | `int` | `int` | — |
| `DeliveryName` | `DeliveryName` | `string` | `nvarchar(60)` | `str` |
| `DeliveryPostalAddress` | `DeliveryPostalAddress` | `long` | `bigint` | `int64` |
| `DeliveryType` | `DeliveryType` | `int` | `int` | — |
| `DlvMode` | `DlvMode` | `string` | `nvarchar(10)` | — |
| `DlvTerm` | `DlvTerm` | `string` | `nvarchar(10)` | — |
| `DomExceptionType` | `DomExceptionType` | `int` | `int` | — |
| `EInvoiceAccountCode` | `EInvoiceAccountCode` | `string` | `nvarchar(10)` | — |
| `ExpectedRetQty` | `ExpectedRetQty` | `decimal` | `decimal(18,4)` | — |
| `FinTag` | `FinTag` | `long` | `bigint` | — |
| `IntercompanyOrigin` | `IntercompanyOrigin` | `int` | `int` | — |
| `IntrastatCommodity` | `IntrastatCommodity` | `long` | `bigint` | — |
| `InventDeliverNow` | `InventDeliverNow` | `decimal` | `decimal(18,4)` | — |
| `InventDimId` | `InventDimId` | `string` | `nvarchar(100)` | `str` |
| `InventRefId` | `InventRefId` | `string` | `nvarchar(100)` | — |
| `InventRefTransId` | `InventRefTransId` | `string` | `nvarchar(100)` | — |
| `InventRefType` | `InventRefType` | `byte` | `tinyint` | — |
| `InventTransId` | `InventTransId` | `string` | `nvarchar(100)` | `str` |
| `InventTransIdReturn` | `InventTransIdReturn` | `string` | `nvarchar(100)` | — |
| `InventoryServiceAutoOffset` | `InventoryServiceAutoOffset` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsSoftReservedExternally` | `IsSoftReservedExternally` | `int` | `int` | — |
| `ItemId` | `ItemId` | `string` | `nvarchar(20)` | `str` |
| `ItemReplaced` | `ItemReplaced` | `int` | `int` | — |
| `ItemTagging` | `ItemTagging` | `int` | `int` | — |
| `KittingSkipUpdateHelper` | `KittingSkipUpdateHelper` | `int` | `int` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LedgerDimension` | `LedgerDimension` | `long` | `bigint` | — |
| `LineAmount` | `LineAmount` | `decimal` | `decimal(18,4)` | `real` |
| `LineDeliveryType` | `LineDeliveryType` | `int` | `int` | — |
| `LineDisc` | `LineDisc` | `decimal` | `decimal(18,4)` | `real` |
| `LineNum` | `LineNum` | `decimal` | `decimal(18,4)` | `real` |
| `LinePercent` | `LinePercent` | `decimal` | `decimal(18,4)` | `real` |
| `ManualEntryChangePolicy` | `ManualEntryChangePolicy` | `long` | `bigint` | — |
| `MatchingAgreementLine` | `MatchingAgreementLine` | `long` | `bigint` | — |
| `McrMarginPercent` | `McrMarginPercent` | `decimal` | `decimal(18,4)` | — |
| `MpsExcludeSalesLine` | `MpsExcludeSalesLine` | `int` | `int` | — |
| `MpsFullRunCtpStatus` | `MpsFullRunCtpStatus` | `int` | `int` | — |
| `MultiLnDisc` | `MultiLnDisc` | `decimal` | `decimal(18,4)` | `real` |
| `MultiLnPercent` | `MultiLnPercent` | `decimal` | `decimal(18,4)` | `real` |
| `Name` | `Name` | `string` | `nvarchar(60)` | `str` |
| `OverDeliveryPct` | `OverDeliveryPct` | `decimal` | `decimal(18,4)` | — |
| `OverrideSalesTax` | `OverrideSalesTax` | `int` | `int` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PackingUnit` | `PackingUnit` | `string` | `nvarchar(10)` | — |
| `PackingUnitQty` | `PackingUnitQty` | `decimal` | `decimal(18,4)` | — |
| `PalletTagging` | `PalletTagging` | `int` | `int` | — |
| `PdsBatchAttribAutoRes` | `PdsBatchAttribAutoRes` | `int` | `int` | — |
| `PdsCwExpectedRetQty` | `PdsCwExpectedRetQty` | `decimal` | `decimal(18,4)` | — |
| `PdsCwInventDeliverNow` | `PdsCwInventDeliverNow` | `decimal` | `decimal(18,4)` | — |
| `PdsCwQty` | `PdsCwQty` | `decimal` | `decimal(18,4)` | — |
| `PdsCwRemainInventFinancial` | `PdsCwRemainInventFinancial` | `decimal` | `decimal(18,4)` | — |
| `PdsCwRemainInventPhysical` | `PdsCwRemainInventPhysical` | `decimal` | `decimal(18,4)` | — |
| `PdsExcludeFromRebate` | `PdsExcludeFromRebate` | `int` | `int` | — |
| `PdsSameLot` | `PdsSameLot` | `int` | `int` | — |
| `PdsSameLotOverride` | `PdsSameLotOverride` | `int` | `int` | — |
| `PlanningPriority` | `PlanningPriority` | `decimal` | `decimal(18,4)` | — |
| `PriceUnit` | `PriceUnit` | `decimal` | `decimal(18,4)` | `real` |
| `ProjFundingSource` | `ProjFundingSource` | `long` | `bigint` | — |
| `PsaProjProposalInventQty` | `PsaProjProposalInventQty` | `decimal` | `decimal(18,4)` | — |
| `PsaProjProposalQty` | `PsaProjProposalQty` | `decimal` | `decimal(18,4)` | — |
| `PurchOrderFormNum` | `PurchOrderFormNum` | `string` | `nvarchar(20)` | — |
| `QtyOrdered` | `QtyOrdered` | `decimal` | `decimal(18,4)` | `real` |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `ReceiptDateConfirmed` | `ReceiptDateConfirmed` | `DateTime` | `datetime2` | `date` |
| `ReceiptDateRequested` | `ReceiptDateRequested` | `DateTime` | `datetime2` | `date` |
| `RefReturnInvoiceTransW` | `RefReturnInvoiceTransW` | `long` | `bigint` | — |
| `RemainInventFinancial` | `RemainInventFinancial` | `decimal` | `decimal(18,4)` | `real` |
| `RemainInventPhysical` | `RemainInventPhysical` | `decimal` | `decimal(18,4)` | `real` |
| `RemainSalesFinancial` | `RemainSalesFinancial` | `decimal` | `decimal(18,4)` | `real` |
| `RemainSalesPhysical` | `RemainSalesPhysical` | `decimal` | `decimal(18,4)` | `real` |
| `Reservation` | `Reservation` | `int` | `int` | — |
| `ReturnAllowReservation` | `ReturnAllowReservation` | `int` | `int` | — |
| `ReturnArrivalDate` | `ReturnArrivalDate` | `DateTime` | `datetime2` | — |
| `ReturnClosedDate` | `ReturnClosedDate` | `DateTime` | `datetime2` | — |
| `ReturnDeadline` | `ReturnDeadline` | `DateTime` | `datetime2` | — |
| `ReturnDispositionCodeId` | `ReturnDispositionCodeId` | `string` | `nvarchar(10)` | — |
| `ReturnStatus` | `ReturnStatus` | `int` | `int` | — |
| `RevRecBundle` | `RevRecBundle` | `int` | `int` | — |
| `RevRecBundleNetAmount` | `RevRecBundleNetAmount` | `decimal` | `decimal(18,4)` | — |
| `RevRecBundleQty` | `RevRecBundleQty` | `decimal` | `decimal(18,4)` | — |
| `RevRecBundleQtyOrdered` | `RevRecBundleQtyOrdered` | `decimal` | `decimal(18,4)` | — |
| `RevRecBundleRatio` | `RevRecBundleRatio` | `decimal` | `decimal(18,4)` | — |
| `RevRecBundleSalesPrice` | `RevRecBundleSalesPrice` | `decimal` | `decimal(18,4)` | — |
| `RevRecBundleSalesStatus` | `RevRecBundleSalesStatus` | `int` | `int` | — |
| `RevRecContractEndDate` | `RevRecContractEndDate` | `DateTime` | `datetime2` | — |
| `RevRecContractStartDate` | `RevRecContractStartDate` | `DateTime` | `datetime2` | — |
| `RevRecIsBundleComponent` | `RevRecIsBundleComponent` | `int` | `int` | — |
| `RevRecOccurrences` | `RevRecOccurrences` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SalesCategory` | `SalesCategory` | `long` | `bigint` | `int64` |
| `SalesDeliverNow` | `SalesDeliverNow` | `decimal` | `decimal(18,4)` | — |
| `SalesGroup` | `SalesGroup` | `string` | `nvarchar(10)` | — |
| `SalesId` | `SalesId` | `string` | `nvarchar(20)` | `str` |
| `SalesMarkup` | `SalesMarkup` | `decimal` | `decimal(18,4)` | — |
| `SalesPrice` | `SalesPrice` | `decimal` | `decimal(18,4)` | `real` |
| `SalesQty` | `SalesQty` | `decimal` | `decimal(18,4)` | `real` |
| `SalesSalesOrderCreationMethod` | `SalesSalesOrderCreationMethod` | `int` | `int` | — |
| `SalesStatus` | `SalesStatus` | `int` | `int` | `enum` |
| `SalesType` | `SalesType` | `int` | `int` | `enum` |
| `SalesUnit` | `SalesUnit` | `string` | `nvarchar(10)` | `str` |
| `Scrap` | `Scrap` | `int` | `int` | — |
| `ServiceLineType` | `ServiceLineType` | `int` | `int` | — |
| `ShipCarrierDlvType` | `ShipCarrierDlvType` | `int` | `int` | — |
| `ShipCarrierPostalAddress` | `ShipCarrierPostalAddress` | `long` | `bigint` | — |
| `ShippingDateConfirmed` | `ShippingDateConfirmed` | `DateTime` | `datetime2` | `date` |
| `ShippingDateRequested` | `ShippingDateRequested` | `DateTime` | `datetime2` | `date` |
| `SoftReserveBlockLevel` | `SoftReserveBlockLevel` | `int` | `int` | — |
| `SourceDocumentLine` | `SourceDocumentLine` | `long` | `bigint` | — |
| `SourcingOrigin` | `SourcingOrigin` | `int` | `int` | — |
| `StAtTriangularDeal` | `StAtTriangularDeal` | `int` | `int` | — |
| `StockedProduct` | `StockedProduct` | `int` | `int` | — |
| `SysDataStateCode` | `SysDataStateCode` | `int` | `int` | — |
| `SystemEntryChangePolicy` | `SystemEntryChangePolicy` | `long` | `bigint` | — |
| `SystemEntrySource` | `SystemEntrySource` | `int` | `int` | — |
| `TamRebateExcludeRebateManagement` | `TamRebateExcludeRebateManagement` | `int` | `int` | — |
| `TaxAutoGenerated` | `TaxAutoGenerated` | `int` | `int` | — |
| `TaxGroup` | `TaxGroup` | `string` | `nvarchar(10)` | `str` |
| `TaxId` | `TaxId` | `long` | `bigint` | — |
| `TaxItemGroup` | `TaxItemGroup` | `string` | `nvarchar(10)` | `str` |
| `UnbilledRevenueCredit` | `UnbilledRevenueCredit` | `int` | `int` | — |
| `UnderDeliveryPct` | `UnderDeliveryPct` | `decimal` | `decimal(18,4)` | — |

### InventTransOrigin

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.InventTransOrigin`. 19 mapped columns.

D365 fields listed in the attachment but absent in IXApi: none.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | `int64` |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | `str` |
| `InventTransId` | `InventTransId` | `string` | `nvarchar(100)` | `str` |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsExcludedFromInventoryValue` | `IsExcludedFromInventoryValue` | `int` | `int` | — |
| `ItemId` | `ItemId` | `string` | `nvarchar(20)` | `str` |
| `ItemInventDimId` | `ItemInventDimId` | `string` | `nvarchar(100)` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `Party` | `Party` | `long` | `bigint` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `ReferenceCategory` | `ReferenceCategory` | `byte` | `tinyint` | `enum` |
| `ReferenceId` | `ReferenceId` | `string` | `nvarchar(100)` | `str` |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SysDataStateCode` | `SysDataStateCode` | `int` | `int` | — |

### InventTransOriginSalesLine

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.InventTransOriginSalesLine`. 14 mapped columns.

D365 fields listed in the attachment but absent in IXApi: none.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | `int64` |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `InventTransOrigin` | `InventTransOrigin` | `long` | `bigint` | `int64` |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SalesLineDataAreaId` | `SalesLineDataAreaId` | `string` | `nvarchar(4)` | `str` |
| `SalesLineInventTransId` | `SalesLineInventTransId` | `string` | `nvarchar(100)` | `str` |

### InventTrans

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.InventTrans`. 56 mapped columns.

D365 fields listed in the attachment but absent in IXApi: none.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | `int64` |
| `CostAmountAdjustment` | `CostAmountAdjustment` | `decimal` | `decimal(18,4)` | `real` |
| `CostAmountOperations` | `CostAmountOperations` | `decimal` | `decimal(18,4)` | — |
| `CostAmountPhysical` | `CostAmountPhysical` | `decimal` | `decimal(18,4)` | `real` |
| `CostAmountPosted` | `CostAmountPosted` | `decimal` | `decimal(18,4)` | `real` |
| `CostAmountSettled` | `CostAmountSettled` | `decimal` | `decimal(18,4)` | — |
| `CostAmountStd` | `CostAmountStd` | `decimal` | `decimal(18,4)` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `CurrencyCode` | `CurrencyCode` | `string` | `nvarchar(3)` | `str` |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | `str` |
| `DateClosed` | `DateClosed` | `DateTime` | `datetime2` | — |
| `DateExpected` | `DateExpected` | `DateTime` | `datetime2` | `date` |
| `DateFinancial` | `DateFinancial` | `DateTime` | `datetime2` | `date` |
| `DateInvent` | `DateInvent` | `DateTime` | `datetime2` | — |
| `DatePhysical` | `DatePhysical` | `DateTime` | `datetime2` | `date` |
| `DateStatus` | `DateStatus` | `DateTime` | `datetime2` | `date` |
| `IntercompanyInventDimTransferred` | `IntercompanyInventDimTransferred` | `int` | `int` | — |
| `InventDimFixed` | `InventDimFixed` | `int` | `int` | — |
| `InventDimId` | `InventDimId` | `string` | `nvarchar(100)` | `str` |
| `InventTransOrigin` | `InventTransOrigin` | `long` | `bigint` | `int64` |
| `InvoiceId` | `InvoiceId` | `string` | `nvarchar(100)` | `str` |
| `InvoiceReturned` | `InvoiceReturned` | `int` | `int` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `ItemId` | `ItemId` | `string` | `nvarchar(20)` | `str` |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `MarkingRefInventTransOrigin` | `MarkingRefInventTransOrigin` | `long` | `bigint` | — |
| `NonFinancialTransferInventClosing` | `NonFinancialTransferInventClosing` | `long` | `bigint` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PackingSlipId` | `PackingSlipId` | `string` | `nvarchar(100)` | `str` |
| `PackingSlipReturned` | `PackingSlipReturned` | `int` | `int` | — |
| `PdscwQty` | `PdscwQty` | `decimal` | `decimal(18,4)` | — |
| `PdscwSettled` | `PdscwSettled` | `decimal` | `decimal(18,4)` | — |
| `PickingRouteId` | `PickingRouteId` | `string` | `nvarchar(20)` | — |
| `ProjAdjustRefId` | `ProjAdjustRefId` | `string` | `nvarchar(100)` | — |
| `ProjCategoryId` | `ProjCategoryId` | `string` | `nvarchar(30)` | — |
| `ProjId` | `ProjId` | `string` | `nvarchar(20)` | — |
| `Qty` | `Qty` | `decimal` | `decimal(18,4)` | `real` |
| `QtySettled` | `QtySettled` | `decimal` | `decimal(18,4)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `ReturnInventTransOrigin` | `ReturnInventTransOrigin` | `long` | `bigint` | — |
| `RevenueAmountPhysical` | `RevenueAmountPhysical` | `decimal` | `decimal(18,4)` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `ShippingDateConfirmed` | `ShippingDateConfirmed` | `DateTime` | `datetime2` | — |
| `ShippingDateRequested` | `ShippingDateRequested` | `DateTime` | `datetime2` | — |
| `StatusIssue` | `StatusIssue` | `byte` | `tinyint` | `enum` |
| `StatusReceipt` | `StatusReceipt` | `byte` | `tinyint` | `enum` |
| `TaxAmountPhysical` | `TaxAmountPhysical` | `decimal` | `decimal(18,4)` | — |
| `TimeExpected` | `TimeExpected` | `int` | `int` | — |
| `TransChildRefId` | `TransChildRefId` | `string` | `nvarchar(100)` | — |
| `TransChildType` | `TransChildType` | `int` | `int` | — |
| `ValueOpen` | `ValueOpen` | `int` | `int` | — |
| `Voucher` | `Voucher` | `string` | `nvarchar(100)` | `str` |
| `VoucherPhysical` | `VoucherPhysical` | `string` | `nvarchar(100)` | `str` |

### InventSum

IXApi entity: `IAX.IXApi.Modules.Finance.Entities.InventSum`. 65 mapped columns.

D365 fields listed in the attachment but absent in IXApi: none.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | `int64` |
| `Arrived` | `Arrived` | `decimal` | `decimal(18,4)` | `real` |
| `AvailOrdered` | `AvailOrdered` | `decimal` | `decimal(18,4)` | `real` |
| `AvailPhysical` | `AvailPhysical` | `decimal` | `decimal(18,4)` | `real` |
| `Closed` | `Closed` | `int` | `int` | `enum` |
| `ClosedQty` | `ClosedQty` | `int` | `int` | — |
| `ConfigId` | `ConfigId` | `string` | `nvarchar(50)` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | `str` |
| `Deducted` | `Deducted` | `decimal` | `decimal(18,4)` | `real` |
| `InventBatchId` | `InventBatchId` | `string` | `nvarchar(20)` | — |
| `InventColorId` | `InventColorId` | `string` | `nvarchar(10)` | — |
| `InventDimId` | `InventDimId` | `string` | `nvarchar(100)` | `str` |
| `InventDimension10` | `InventDimension10` | `decimal` | `decimal(18,4)` | — |
| `InventDimension9` | `InventDimension9` | `DateTime` | `datetime2` | — |
| `InventDimension9TzId` | `InventDimension9TzId` | `int` | `int` | — |
| `InventLocationId` | `InventLocationId` | `string` | `nvarchar(10)` | — |
| `InventSerialId` | `InventSerialId` | `string` | `nvarchar(20)` | — |
| `InventSiteId` | `InventSiteId` | `string` | `nvarchar(10)` | — |
| `InventSizeId` | `InventSizeId` | `string` | `nvarchar(10)` | — |
| `InventStatusId` | `InventStatusId` | `string` | `nvarchar(10)` | — |
| `InventStyleId` | `InventStyleId` | `string` | `nvarchar(10)` | — |
| `InventVersionId` | `InventVersionId` | `string` | `nvarchar(10)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsExcludedFromInventoryValue` | `IsExcludedFromInventoryValue` | `int` | `int` | — |
| `ItemId` | `ItemId` | `string` | `nvarchar(20)` | `str` |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LastUpdDateExpected` | `LastUpdDateExpected` | `DateTime` | `datetime2` | — |
| `LastUpdDatePhysical` | `LastUpdDatePhysical` | `DateTime` | `datetime2` | — |
| `LicensePlateId` | `LicensePlateId` | `string` | `nvarchar(25)` | — |
| `OnOrder` | `OnOrder` | `decimal` | `decimal(18,4)` | `real` |
| `Ordered` | `Ordered` | `decimal` | `decimal(18,4)` | `real` |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PdscwArrived` | `PdscwArrived` | `decimal` | `decimal(18,4)` | — |
| `PdscwAvailOrdered` | `PdscwAvailOrdered` | `decimal` | `decimal(18,4)` | — |
| `PdscwAvailPhysical` | `PdscwAvailPhysical` | `decimal` | `decimal(18,4)` | — |
| `PdscwDeducted` | `PdscwDeducted` | `decimal` | `decimal(18,4)` | — |
| `PdscwOnOrder` | `PdscwOnOrder` | `decimal` | `decimal(18,4)` | — |
| `PdscwOrdered` | `PdscwOrdered` | `decimal` | `decimal(18,4)` | — |
| `PdscwPhysicalInvent` | `PdscwPhysicalInvent` | `decimal` | `decimal(18,4)` | — |
| `PdscwPicked` | `PdscwPicked` | `decimal` | `decimal(18,4)` | — |
| `PdscwPostedQty` | `PdscwPostedQty` | `decimal` | `decimal(18,4)` | — |
| `PdscwQuotationIssue` | `PdscwQuotationIssue` | `decimal` | `decimal(18,4)` | — |
| `PdscwQuotationReceipt` | `PdscwQuotationReceipt` | `decimal` | `decimal(18,4)` | — |
| `PdscwReceived` | `PdscwReceived` | `decimal` | `decimal(18,4)` | — |
| `PdscwRegistered` | `PdscwRegistered` | `decimal` | `decimal(18,4)` | — |
| `PdscwReservOrdered` | `PdscwReservOrdered` | `decimal` | `decimal(18,4)` | — |
| `PdscwReservPhysical` | `PdscwReservPhysical` | `decimal` | `decimal(18,4)` | — |
| `PhysicalInvent` | `PhysicalInvent` | `decimal` | `decimal(18,4)` | `real` |
| `PhysicalValue` | `PhysicalValue` | `decimal` | `decimal(18,4)` | — |
| `Picked` | `Picked` | `decimal` | `decimal(18,4)` | `real` |
| `PostedQty` | `PostedQty` | `decimal` | `decimal(18,4)` | `real` |
| `PostedValue` | `PostedValue` | `decimal` | `decimal(18,4)` | — |
| `QuotationIssue` | `QuotationIssue` | `decimal` | `decimal(18,4)` | — |
| `QuotationReceipt` | `QuotationReceipt` | `decimal` | `decimal(18,4)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `Received` | `Received` | `decimal` | `decimal(18,4)` | `real` |
| `Registered` | `Registered` | `decimal` | `decimal(18,4)` | `real` |
| `ReservOrdered` | `ReservOrdered` | `decimal` | `decimal(18,4)` | `real` |
| `ReservPhysical` | `ReservPhysical` | `decimal` | `decimal(18,4)` | `real` |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `WmsLocationId` | `WmsLocationId` | `string` | `nvarchar(10)` | — |

### MarkupTrans

IXApi entity: `IAX.IXApi.Modules.Finance.AccountsReceivable.MarkupTrans`. 74 mapped columns.

D365 fields listed in the attachment but absent in IXApi: none.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | `int64` |
| `CalculatedAmount` | `CalculatedAmount` | `decimal` | `decimal(18,4)` | — |
| `CalculatedAmountMst_W` | `CalculatedAmountMst_W` | `decimal` | `decimal(18,4)` | — |
| `CalculatedProratedAmount` | `CalculatedProratedAmount` | `decimal` | `decimal(18,4)` | — |
| `CorrectedMarkupTrans` | `CorrectedMarkupTrans` | `long` | `bigint` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `CurrencyCode` | `CurrencyCode` | `string` | `nvarchar(3)` | `str` |
| `CustInvoiceLineIdRef` | `CustInvoiceLineIdRef` | `long` | `bigint` | — |
| `CustInvoiceLineTemplate` | `CustInvoiceLineTemplate` | `long` | `bigint` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | `str` |
| `DocumentStatus` | `DocumentStatus` | `int` | `int` | — |
| `FromAmount` | `FromAmount` | `decimal` | `decimal(18,4)` | — |
| `IntercompanyMarkupUseValue` | `IntercompanyMarkupUseValue` | `int` | `int` | — |
| `IntercompanyMarkupValue` | `IntercompanyMarkupValue` | `decimal` | `decimal(18,4)` | — |
| `IntercompanyRefRecId` | `IntercompanyRefRecId` | `long` | `bigint` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsAdvancedLineProrated` | `IsAdvancedLineProrated` | `int` | `int` | — |
| `IsAutoCharge` | `IsAutoCharge` | `int` | `int` | — |
| `IsCompound` | `IsCompound` | `int` | `int` | — |
| `IsDeleted` | `IsDeleted` | `int` | `int` | — |
| `IsModified` | `IsModified` | `int` | `int` | — |
| `IsOverriddenLine` | `IsOverriddenLine` | `int` | `int` | — |
| `IsOverriddenProratedLine` | `IsOverriddenProratedLine` | `int` | `int` | — |
| `IsTieredCharge` | `IsTieredCharge` | `int` | `int` | — |
| `ItemBasePriceRecId` | `ItemBasePriceRecId` | `long` | `bigint` | — |
| `Keep` | `Keep` | `int` | `int` | `enum` |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LineNum` | `LineNum` | `decimal` | `decimal(18,4)` | `real` |
| `MarkupAutoLineRecId` | `MarkupAutoLineRecId` | `long` | `bigint` | — |
| `MarkupAutoTableRecId` | `MarkupAutoTableRecId` | `long` | `bigint` | — |
| `MarkupCategory` | `MarkupCategory` | `int` | `int` | `enum` |
| `MarkupCode` | `MarkupCode` | `string` | `nvarchar(10)` | `str` |
| `McrBrokerContractFee` | `McrBrokerContractFee` | `int` | `int` | — |
| `McrCouponMarkup` | `McrCouponMarkup` | `int` | `int` | — |
| `McrInstallmentEligible` | `McrInstallmentEligible` | `int` | `int` | — |
| `McrMarkupTransCreatedBy` | `McrMarkupTransCreatedBy` | `int` | `int` | — |
| `McrMiscChargeOverride` | `McrMiscChargeOverride` | `int` | `int` | — |
| `McrOriginalMiscChargeValue` | `McrOriginalMiscChargeValue` | `decimal` | `decimal(18,4)` | — |
| `McrSavedRecId` | `McrSavedRecId` | `long` | `bigint` | — |
| `McrSavedTableId` | `McrSavedTableId` | `int` | `int` | — |
| `ModuleCategory` | `ModuleCategory` | `int` | `int` | — |
| `ModuleType` | `ModuleType` | `int` | `int` | — |
| `OrigRecId` | `OrigRecId` | `long` | `bigint` | — |
| `OrigTableId` | `OrigTableId` | `int` | `int` | — |
| `OverrideSalesTax` | `OverrideSalesTax` | `int` | `int` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `Position` | `Position` | `int` | `int` | — |
| `Posted` | `Posted` | `decimal` | `decimal(18,4)` | — |
| `PreviousValue` | `PreviousValue` | `decimal` | `decimal(18,4)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RetailShippingPromotionDiscount` | `RetailShippingPromotionDiscount` | `decimal` | `decimal(18,4)` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `Sequence` | `Sequence` | `int` | `int` | — |
| `SourceDocumentLine` | `SourceDocumentLine` | `long` | `bigint` | — |
| `TaxAmount` | `TaxAmount` | `decimal` | `decimal(18,4)` | — |
| `TaxAmountMst_W` | `TaxAmountMst_W` | `decimal` | `decimal(18,4)` | — |
| `TaxAutoGenerated` | `TaxAutoGenerated` | `int` | `int` | — |
| `TaxExemptPriceInclusiveOriginalPrice` | `TaxExemptPriceInclusiveOriginalPrice` | `decimal` | `decimal(18,4)` | — |
| `TaxExemptPriceInclusiveReductionAmount` | `TaxExemptPriceInclusiveReductionAmount` | `decimal` | `decimal(18,4)` | — |
| `TaxGroup` | `TaxGroup` | `string` | `nvarchar(10)` | `str` |
| `TaxItemGroup` | `TaxItemGroup` | `string` | `nvarchar(10)` | `str` |
| `TaxWithholdItemGroup` | `TaxWithholdItemGroup` | `long` | `bigint` | — |
| `ToAmount` | `ToAmount` | `decimal` | `decimal(18,4)` | — |
| `TransDate` | `TransDate` | `DateTime` | `datetime2` | — |
| `TransRecId` | `TransRecId` | `long` | `bigint` | `int64` |
| `TransTableId` | `TransTableId` | `int` | `int` | `int` |
| `Txt` | `Txt` | `string` | `nvarchar(150)` | `str` |
| `Value` | `Value` | `decimal` | `decimal(18,4)` | `real` |
| `VendInvoiceLineTemplate` | `VendInvoiceLineTemplate` | `long` | `bigint` | — |
| `VendInvoiceTableMarkupTrans` | `VendInvoiceTableMarkupTrans` | `long` | `bigint` | — |
| `VendInvoiceTemplate` | `VendInvoiceTemplate` | `long` | `bigint` | — |
| `Voucher` | `Voucher` | `string` | `nvarchar(100)` | — |

## Confirmation and documents

### SalesParmUpdate

**No IXApi EF table mapping found in the current snapshot.** No matching class file found.

### SalesParmTable

**No IXApi EF table mapping found in the current snapshot.** No matching class file found.

### SalesParmLine

**No IXApi EF table mapping found in the current snapshot.** No matching class file found.

### CustConfirmJour

IXApi entity: `IAX.IXApi.Modules.Finance.AccountsReceivable.CustConfirmJour`. 49 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `CashDiscCode` | `CashDiscCode` | `string` | `nvarchar(10)` | — |
| `CashDiscPercent` | `CashDiscPercent` | `decimal` | `decimal(18,4)` | — |
| `ConfirmAmount` | `ConfirmAmount` | `decimal` | `decimal(18,4)` | — |
| `ConfirmDate` | `ConfirmDate` | `DateTime` | `datetime2` | — |
| `ConfirmDocNum` | `ConfirmDocNum` | `string` | `nvarchar(100)` | — |
| `ConfirmId` | `ConfirmId` | `string` | `nvarchar(20)` | — |
| `CostValue` | `CostValue` | `decimal` | `decimal(18,4)` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `CurrencyCode` | `CurrencyCode` | `string` | `nvarchar(3)` | — |
| `CustGroup` | `CustGroup` | `string` | `nvarchar(20)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `Deadline` | `Deadline` | `DateTime` | `datetime2` | — |
| `DefaultDimension` | `DefaultDimension` | `long` | `bigint` | — |
| `DeliveryName` | `DeliveryName` | `string` | `nvarchar(60)` | — |
| `DeliveryPostalAddress` | `DeliveryPostalAddress` | `long` | `bigint` | — |
| `DlvMode` | `DlvMode` | `string` | `nvarchar(10)` | — |
| `DlvTerm` | `DlvTerm` | `string` | `nvarchar(10)` | — |
| `EndDisc` | `EndDisc` | `decimal` | `decimal(18,4)` | — |
| `ExchRate` | `ExchRate` | `decimal` | `decimal(18,4)` | — |
| `ExchRateSecondary` | `ExchRateSecondary` | `decimal` | `decimal(18,4)` | — |
| `FixedDueDate` | `FixedDueDate` | `DateTime` | `datetime2` | — |
| `InclTax` | `InclTax` | `int` | `int` | — |
| `IntercompanyPosted` | `IntercompanyPosted` | `int` | `int` | — |
| `InvoiceAccount` | `InvoiceAccount` | `string` | `nvarchar(20)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LanguageId` | `LanguageId` | `string` | `nvarchar(7)` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `OrderAccount` | `OrderAccount` | `string` | `nvarchar(20)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `ParmId` | `ParmId` | `string` | `nvarchar(100)` | — |
| `Payment` | `Payment` | `string` | `nvarchar(100)` | — |
| `Qty` | `Qty` | `decimal` | `decimal(18,4)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RoundOff` | `RoundOff` | `decimal` | `decimal(18,4)` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SalesBalance` | `SalesBalance` | `decimal` | `decimal(18,4)` | — |
| `SalesId` | `SalesId` | `string` | `nvarchar(20)` | — |
| `SubBillSuppressChildItems` | `SubBillSuppressChildItems` | `int` | `int` | — |
| `SumLineDisc` | `SumLineDisc` | `decimal` | `decimal(18,4)` | — |
| `SumMarkup` | `SumMarkup` | `decimal` | `decimal(18,4)` | — |
| `SumTax` | `SumTax` | `decimal` | `decimal(18,4)` | — |
| `Triangulation` | `Triangulation` | `int` | `int` | — |
| `Volume` | `Volume` | `decimal` | `decimal(18,4)` | — |
| `Weight` | `Weight` | `decimal` | `decimal(18,4)` | — |
| `WorkerSalesTaker` | `WorkerSalesTaker` | `long` | `bigint` | — |

### CustConfirmTrans

IXApi entity: `IAX.IXApi.Modules.Finance.AccountsReceivable.CustConfirmTrans`. 48 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `ConfirmDate` | `ConfirmDate` | `DateTime` | `datetime2` | — |
| `ConfirmId` | `ConfirmId` | `string` | `nvarchar(20)` | — |
| `CreatedAt` | `CreatedAt` | `DateTime?` | `datetime2` | — |
| `CreatedBy` | `CreatedBy` | `string` | `nvarchar(max)` | — |
| `CurrencyCode` | `CurrencyCode` | `string` | `nvarchar(3)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `DefaultDimension` | `DefaultDimension` | `long` | `bigint` | — |
| `DiscAmount` | `DiscAmount` | `decimal` | `decimal(18,4)` | — |
| `DiscPercent` | `DiscPercent` | `decimal` | `decimal(18,4)` | — |
| `DlvDate` | `DlvDate` | `DateTime` | `datetime2` | — |
| `DlvTerm` | `DlvTerm` | `string` | `nvarchar(10)` | — |
| `InventDimId` | `InventDimId` | `string` | `nvarchar(100)` | — |
| `InventQty` | `InventQty` | `decimal` | `decimal(18,4)` | — |
| `InventTransId` | `InventTransId` | `string` | `nvarchar(100)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `ItemId` | `ItemId` | `string` | `nvarchar(20)` | — |
| `LastModifiedAt` | `LastModifiedAt` | `DateTime?` | `datetime2` | — |
| `LastModifiedBy` | `LastModifiedBy` | `string` | `nvarchar(max)` | — |
| `LineAmount` | `LineAmount` | `decimal` | `decimal(18,4)` | — |
| `LineAmountTax` | `LineAmountTax` | `decimal` | `decimal(18,4)` | — |
| `LineDisc` | `LineDisc` | `decimal` | `decimal(18,4)` | — |
| `LineHeader` | `LineHeader` | `string` | `nvarchar(150)` | — |
| `LineNum` | `LineNum` | `decimal` | `decimal(18,4)` | — |
| `LinePercent` | `LinePercent` | `decimal` | `decimal(18,4)` | — |
| `MultiLnDisc` | `MultiLnDisc` | `decimal` | `decimal(18,4)` | — |
| `MultiLnPercent` | `MultiLnPercent` | `decimal` | `decimal(18,4)` | — |
| `Name` | `Name` | `string` | `nvarchar(60)` | — |
| `OrigSalesId` | `OrigSalesId` | `string` | `nvarchar(20)` | — |
| `OverrideSalesTax` | `OverrideSalesTax` | `int` | `int` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `PdsCwQty` | `PdsCwQty` | `decimal` | `decimal(18,4)` | — |
| `PriceUnit` | `PriceUnit` | `decimal` | `decimal(18,4)` | — |
| `Qty` | `Qty` | `decimal` | `decimal(18,4)` | — |
| `RecVersion` | `RecVersion` | `int` | `int` | — |
| `RowVersion` | `RowVersion` | `byte[]` | `rowversion` | — |
| `SalesCategory` | `SalesCategory` | `long` | `bigint` | — |
| `SalesGroup` | `SalesGroup` | `string` | `nvarchar(10)` | — |
| `SalesId` | `SalesId` | `string` | `nvarchar(20)` | — |
| `SalesMarkup` | `SalesMarkup` | `decimal` | `decimal(18,4)` | — |
| `SalesPrice` | `SalesPrice` | `decimal` | `decimal(18,4)` | — |
| `SalesUnit` | `SalesUnit` | `string` | `nvarchar(10)` | — |
| `StockedProduct` | `StockedProduct` | `int` | `int` | — |
| `TaxAmount` | `TaxAmount` | `decimal` | `decimal(18,4)` | — |
| `TaxGroup` | `TaxGroup` | `string` | `nvarchar(10)` | — |
| `TaxItemGroup` | `TaxItemGroup` | `string` | `nvarchar(10)` | — |
| `TaxWriteCode` | `TaxWriteCode` | `string` | `nvarchar(10)` | — |

### CustConfirmSalesLink

**No IXApi EF table mapping found in the current snapshot.** No matching class file found.

### DocuRef

IXApi entity: `IAX.IXApi.Modules.Organization.DocumentManagement.Entities.DocuRef`. 34 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `ActualCompanyId` | `ACTUALCOMPANYID` | `string` | `nvarchar(8)` | — |
| `Author` | `AUTHOR` | `long` | `bigint` | — |
| `ContactPersonId` | `CONTACTPERSONID` | `string` | `nvarchar(40)` | — |
| `CreatedAt` | `CREATEDDATETIME` | `DateTime` | `datetime` | — |
| `CreatedBy` | `CREATEDBY` | `string` | `nvarchar(40)` | — |
| `DataAreaId` | `DATAAREAID` | `string` | `nvarchar(8)` | — |
| `DefaultAttachment` | `DEFAULTATTACHMENT` | `int` | `int` | — |
| `DocumentId` | `DOCUMENTID` | `Guid` | `uniqueidentifier` | — |
| `EncyclopediaItemId` | `ENCYCLOPEDIAITEMID` | `string` | `nvarchar(20)` | — |
| `EngChgEngineeringDocument` | `ENGCHGENGINEERINGDOCUMENT` | `long` | `bigint` | — |
| `EngChgEngineeringReference` | `ENGCHGENGINEERINGREFERENCE` | `string` | `nvarchar(72)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `IsEnabledForVirtualEntitySync` | `ISENABLEDFORVIRTUALENTITYSYNC` | `int` | `int` | — |
| `IsJustification` | `ISJUSTIFICATION` | `int` | `int` | — |
| `LastModifiedAt` | `MODIFIEDDATETIME` | `DateTime` | `datetime` | — |
| `LastModifiedBy` | `MODIFIEDBY` | `string` | `nvarchar(40)` | — |
| `Name` | `NAME` | `string` | `nvarchar(120)` | — |
| `Notes` | `NOTES` | `string` | `nvarchar(max)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `Partition` | `PARTITION` | `long` | `bigint` | — |
| `Party` | `PARTY` | `long` | `bigint` | — |
| `RecVersion` | `RECVERSION` | `int` | `int` | — |
| `RefCompanyId` | `REFCOMPANYID` | `string` | `nvarchar(8)` | — |
| `RefRecId` | `REFRECID` | `long` | `bigint` | — |
| `RefTableId` | `REFTABLEID` | `int` | `int` | — |
| `Restriction` | `RESTRICTION` | `int` | `int` | — |
| `RowVersion` | `SYSROWVERSION` | `byte[]` | `rowversion` | — |
| `SmmEmailEntryId` | `SMMEMAILENTRYID` | `string` | `nvarchar(510)` | — |
| `SmmEmailStoreId` | `SMMEMAILSTOREID` | `string` | `nvarchar(600)` | — |
| `SmmTable` | `SMMTABLE` | `int` | `int` | — |
| `TypeId` | `TYPEID` | `string` | `nvarchar(40)` | — |
| `ValueRecId` | `VALUERECID` | `long` | `bigint` | — |

### DocuValue

IXApi entity: `IAX.IXApi.Modules.Organization.DocumentManagement.Entities.DocuValue`. 24 mapped columns.

| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |
| --- | --- | --- | --- | --- |
| `RecId` | `RECID` | `long` | `bigint` | — |
| `AccessInformation` | `ACCESSINFORMATION` | `string` | `nvarchar(2520)` | — |
| `CreatedAt` | `CREATEDDATETIME` | `DateTime` | `datetime` | — |
| `CreatedBy` | `CREATEDBY` | `string` | `nvarchar(40)` | — |
| `DataAreaId` | `DataAreaId` | `string` | `nvarchar(4)` | — |
| `DocumentHashNumber` | `DOCUMENTHASHNUMBER` | `string` | `nvarchar(256)` | — |
| `File` | `FILE_` | `byte[]` | `varbinary(max)` | — |
| `FileId` | `FILEID` | `Guid` | `uniqueidentifier` | — |
| `FileName` | `FILENAME` | `string` | `nvarchar(518)` | — |
| `FileType` | `FILETYPE` | `string` | `nvarchar(518)` | — |
| `IsActive` | `IsActive` | `bool` | `bit` | — |
| `IsDeleted` | `IsDeleted` | `bool` | `bit` | — |
| `LastModifiedAt` | `MODIFIEDDATETIME` | `DateTime` | `datetime` | — |
| `LastModifiedBy` | `MODIFIEDBY` | `string` | `nvarchar(40)` | — |
| `McrDocuSubject` | `MCRDOCUSUBJECT` | `string` | `nvarchar(max)` | — |
| `Name` | `NAME` | `string` | `nvarchar(120)` | — |
| `OriginalFileName` | `ORIGINALFILENAME` | `string` | `nvarchar(518)` | — |
| `OwnerAccountId` | `OwnerAccountId` | `string` | `nvarchar(max)` | — |
| `Partition` | `PARTITION` | `long` | `bigint` | — |
| `Path` | `PATH` | `string` | `nvarchar(max)` | — |
| `RecVersion` | `RECVERSION` | `int` | `int` | — |
| `RowVersion` | `SYSROWVERSION` | `byte[]` | `rowversion` | — |
| `StorageProviderId` | `STORAGEPROVIDERID` | `int` | `int` | — |
| `Type` | `TYPE` | `int` | `int` | — |
