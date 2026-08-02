import React, { useMemo } from 'react';
import { SetupPage } from '@patterns/setup/SetupPage';
import type { SetupNavigationItem, SetupSectionConfig, SetupValues } from '@patterns/setup/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';

const INITIAL_VALUES: SetupValues = {
  mandatorySettlement: true, preventInvoiceDateEditing: true, preventBelowCost: true, validateSettlementWorker: true,
  applyCustomerHeaderDimensions: true, preventFinancialModification: true, requireOpenInvoiceSettlement: true,
  preventDueDateEditing: true, checkTaxAmount: true, validateSettlementDepartment: true, applyBankHeaderDimensions: true,
  allowInvoiceBeforePackingSlip: false, mandatorySettlementVoucher: false, checkCostAllocateDiscount: true,
  validateSettlementCostCenter: true, roleException: '8950FE08-DCDB-4267-A53D-...', validateSiteLocation: true,
  allowDuplicateVat: false, validateWorkerDimension: true, validateSettlementProject: false, applyBankLineDimensions: false,
  validateHeaderLineDimensions: true, invoiceTaxExemptRequirement: 'none', minimumReimbursement: 0,
  mandatoryTaxGroup: true, defaultTransactionFilter: 'all', assignResponsibleEmployee: false,
  preventRegistrationCopy: false, oneTimeCustomerAccount: '', taxExemptRequirement: 'none', suppressDepreciation: false,
  orderType: 'salesOrder', salesOrderPool: '', salesOrigin: '', autoBatchReservation: false,
  invoiceTimeZone: 'legalEntity', freeTextInvoiceTimeZone: 'legalEntity', validityPeriod: 0,
  reservation: 'manual', enterprisePortalOrigin: '', salesDocumentTimeZone: 'user', packingSlipTimeZone: 'legalEntity',
  deferDirectDelivery: false,
};

export function CustParametersPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const navigationItems = useMemo<SetupNavigationItem[]>(() => [
    'general', 'updates', 'project', 'summaryUpdate', 'shipments', 'ledgerSalesTax', 'settlement', 'directDebit',
    'creditCard', 'creditManagement', 'collections', 'collectionsAutomation', 'deductions', 'prices',
    'electronicDocuments', 'inventoryDimensions', 'rebateProgram',
  ].map((id) => ({ id, label: t(`customerParameters.navigation.${id}`) })), [t]);

  const yesNo = [
    { value: 'none', label: t('common.none') },
    { value: 'all', label: t('common.all', 'All') },
  ];
  const sections = useMemo<SetupSectionConfig[]>(() => [
    {
      id: 'general', title: t('customerParameters.sections.customized'), fields: [
        ...['mandatorySettlement', 'preventInvoiceDateEditing', 'preventBelowCost', 'validateSettlementWorker',
        'applyCustomerHeaderDimensions', 'preventFinancialModification', 'requireOpenInvoiceSettlement',
        'preventDueDateEditing', 'checkTaxAmount', 'validateSettlementDepartment', 'applyBankHeaderDimensions',
        'allowInvoiceBeforePackingSlip', 'mandatorySettlementVoucher', 'checkCostAllocateDiscount',
        'validateSettlementCostCenter',
      ].map((name) => ({ name, label: t(`customerParameters.fields.${name}`), type: 'boolean' as const })),
        { name: 'roleException', label: t('customerParameters.fields.roleException'), type: 'text' as const },
        ...['validateSiteLocation', 'allowDuplicateVat', 'validateWorkerDimension', 'validateSettlementProject', 'applyBankLineDimensions', 'validateHeaderLineDimensions'].map((name) => ({ name, label: t(`customerParameters.fields.${name}`), type: 'boolean' as const })),
      ],
    },
    {
      id: 'customer', title: t('customerParameters.sections.customer'), fields: [
        { name: 'invoiceTaxExemptRequirement', label: t('customerParameters.fields.invoiceTaxExemptRequirement'), type: 'select', options: yesNo },
        { name: 'minimumReimbursement', label: t('customerParameters.fields.minimumReimbursement'), type: 'number' },
        { name: 'mandatoryTaxGroup', label: t('customerParameters.fields.mandatoryTaxGroup'), type: 'boolean' },
        { name: 'defaultTransactionFilter', label: t('customerParameters.fields.defaultTransactionFilter'), type: 'select', options: yesNo },
        { name: 'assignResponsibleEmployee', label: t('customerParameters.fields.assignResponsibleEmployee'), type: 'boolean' },
        { name: 'preventRegistrationCopy', label: t('customerParameters.fields.preventRegistrationCopy'), type: 'boolean' },
        { name: 'oneTimeCustomerAccount', label: t('customerParameters.fields.oneTimeCustomerAccount'), type: 'select', options: [{ value: '', label: '' }] },
        { name: 'taxExemptRequirement', label: t('customerParameters.fields.taxExemptRequirement'), type: 'select', options: yesNo },
        { name: 'suppressDepreciation', label: t('customerParameters.fields.suppressDepreciation'), type: 'boolean' },
      ],
    },
    {
      id: 'salesDefaults', title: t('customerParameters.sections.salesDefaults'), fields: [
        { name: 'orderType', label: t('customerParameters.fields.orderType'), type: 'select', options: [{ value: 'salesOrder', label: t('customerParameters.options.salesOrder') }] },
        { name: 'salesOrderPool', label: t('customerParameters.fields.salesOrderPool'), type: 'select', options: [{ value: '', label: '' }] },
        { name: 'salesOrigin', label: t('customerParameters.fields.salesOrigin'), type: 'select', options: [{ value: '', label: '' }] },
        { name: 'autoBatchReservation', label: t('customerParameters.fields.autoBatchReservation'), type: 'boolean' },
        { name: 'invoiceTimeZone', label: t('customerParameters.fields.invoiceTimeZone'), type: 'select', options: [{ value: 'legalEntity', label: t('customerParameters.options.legalEntity') }] },
        { name: 'freeTextInvoiceTimeZone', label: t('customerParameters.fields.freeTextInvoiceTimeZone'), type: 'select', options: [{ value: 'legalEntity', label: t('customerParameters.options.legalEntity') }] },
        { name: 'validityPeriod', label: t('customerParameters.fields.validityPeriod'), type: 'number' },
        { name: 'reservation', label: t('customerParameters.fields.reservation'), type: 'select', options: [{ value: 'manual', label: t('customerParameters.options.manual') }] },
        { name: 'enterprisePortalOrigin', label: t('customerParameters.fields.enterprisePortalOrigin'), type: 'select', options: [{ value: '', label: '' }] },
        { name: 'salesDocumentTimeZone', label: t('customerParameters.fields.salesDocumentTimeZone'), type: 'select', options: [{ value: 'user', label: t('customerParameters.options.user') }] },
        { name: 'packingSlipTimeZone', label: t('customerParameters.fields.packingSlipTimeZone'), type: 'select', options: [{ value: 'legalEntity', label: t('customerParameters.options.legalEntity') }] },
        { name: 'deferDirectDelivery', label: t('customerParameters.fields.deferDirectDelivery'), type: 'boolean' },
      ],
    },
  ], [t]);

  return <SetupPage title={t('pages.customerParameters.title')} viewLabel={t('pages.customerParameters.standardView')} navigationItems={navigationItems} sections={sections} initialValues={INITIAL_VALUES} saveLabel={t('actions.save')} optionsLabel={t('customerCommands.options')} yesLabel={t('common.yes', 'Yes')} noLabel={t('common.no', 'No')} savedMessage={t('customerParameters.saved')} />;
}
