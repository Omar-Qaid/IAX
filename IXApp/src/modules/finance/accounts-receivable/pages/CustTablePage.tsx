import { localizedName } from '@shared/utilities/localizedName';
import React from 'react';
import { Typography } from '@mui/material';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailSectionConfig, DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { customerQuickCreateApi, type CustomerRecord } from '../api/customerQuickCreateApi';
import { useParams } from 'react-router-dom';
import { PartyPostalAddressPanel, PartyElectronicAddressPanel } from '@shared/components/logistics/PartyLogisticsPanels';
import { useQuery } from '@tanstack/react-query';
import type { CustomerLookupOption } from '../api/customerQuickCreateApi';
import { salesOrderLinesApi } from '../api/salesOrderLinesApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';
import { FOUNDATION_ROUTE_PATHS } from '@modules/finance/foundation/routes/foundationRoutePaths';
import { INVENTORY_ROUTE_PATHS } from '@modules/finance/inventory/routes/inventoryRoutePaths';

const includingSelected = (options: CustomerLookupOption[], selected: string): CustomerLookupOption[] =>
  selected && !options.some((option) => option.value === selected)
    ? [{ value: selected, label: selected }, ...options]
    : options;

const emptyCustomer = (): CustomerRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  party: 0,
  accountNumber: '—',
  name: '',
  nameAr: '',
  customerGroupId: '',
  currencyCode: 'SAR',
  custCategory: '',
  paymTermId: '',
  paymModeId: '',
  dlvModeId: '',
  taxGroupId: '',
  vatNum: '',
  countryRegionId: '',
  invoiceAccount: '',
  inventSiteId: '',
  inventLocationId: '',
  memo: '',
  salesPoolId: '', cashDiscBaseDays: 0, useCashDisc: 0, inclTax: 0,
  blockFloorLimitUseInChannel: 0, prepaymentValue: 0, prePayType: 0,
  status: 'active',
  createdAt: new Date().toISOString(),
});

const createCustomer = (record: CustomerRecord) => customerQuickCreateApi.create({
  name: record.name,
  nameAlias: record.nameAr,
  custGroupId: record.customerGroupId,
  currencyCode: record.currencyCode,
  custCategory: record.custCategory,
  paymTermId: record.paymTermId,
  paymModeId: record.paymModeId,
  dlvModeId: record.dlvModeId,
  taxGroupId: record.taxGroupId,
  vatNum: record.vatNum,
  countryRegionId: record.countryRegionId,
  memo: record.memo,
  invoiceAccount: record.invoiceAccount,
  inventSiteId: record.inventSiteId,
  inventLocationId: record.inventLocationId,
  salesPoolId: record.salesPoolId,
  cashDiscBaseDays: record.cashDiscBaseDays,
  useCashDisc: record.useCashDisc,
  inclTax: record.inclTax,
  blockFloorLimitUseInChannel: record.blockFloorLimitUseInChannel,
  prepaymentValue: record.prepaymentValue,
  prePayType: record.prePayType,
});

export function CustTablePage(): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const { customerId } = useParams<{ customerId: string }>();
  const lookupsQuery = useQuery({
    queryKey: ['accounts-receivable', 'customer-quick-create', 'lookups'],
    queryFn: ({ signal }) => customerQuickCreateApi.lookups(signal),
    staleTime: 5 * 60 * 1000,
  });
  const lookups = lookupsQuery.data;
  const customersQuery = useQuery({
    queryKey: ['accounts-receivable', 'customers'],
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
    staleTime: 5 * 60 * 1000,
  });
  const dimensionsQuery = useQuery({
    queryKey: ['sales-order-inventory-dimensions'],
    queryFn: ({ signal }) => salesOrderLinesApi.inventoryDimensions(signal),
    staleTime: 5 * 60 * 1000,
  });
  const choices = (options: CustomerLookupOption[] | undefined, selected: string) =>
    includingSelected(options ?? [], selected);
  const sections = ({ record, editing }: { record: CustomerRecord; editing: boolean }): DetailSectionConfig[] => [
    {
      id: 'general',
      title: t('common.general', 'General'),
      gridTemplateColumns: 'repeat(5, minmax(150px, 1fr))',
      groups: [
        { id: 'customer', title: t('customerCommands.customer', 'Customer'), fields: [
          { name: 'accountNumber', label: t('fields.account'), type: 'display' },
          { name: 'custCategory', label: t('customerQuickCreate.fields.category'), type: 'select', options: choices([
            { value: 'retail', label: t('customerQuickCreate.options.retail') },
            { value: 'wholesale', label: t('customerQuickCreate.options.wholesale') },
          ], record.custCategory) },
          { name: 'name', label: t('fields.customerName') },
          { name: 'nameAr', label: t('fields.arabicName') },
        ] },
        { id: 'classification', fields: [
          { name: 'customerGroupId', label: t('fields.customerGroup'), type: 'select', masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_GROUPS, options: choices(lookups?.customerGroups, record.customerGroupId) },
          { name: 'currencyCode', label: t('fields.currency'), type: 'select', masterRoute: FOUNDATION_ROUTE_PATHS.CURRENCIES, options: choices(lookups?.currencies, record.currencyCode) },
        ] },
        { id: 'taxPayment', title: 'Sales tax', fields: [
          { name: 'vatNum', label: t('customerQuickCreate.fields.vatNumber') },
          { name: 'taxGroupId', label: t('fields.salesTaxGroup'), type: 'select', masterRoute: FOUNDATION_ROUTE_PATHS.TAX_GROUPS, options: choices(lookups?.salesTaxGroups, record.taxGroupId) },
          { name: 'paymTermId', label: t('fields.termsOfPayment'), type: 'select', masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_TERMS, options: choices(lookups?.paymentTerms, record.paymTermId) },
          { name: 'paymModeId', label: t('customerQuickCreate.fields.paymentMethod'), type: 'select', masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_METHODS, options: choices(lookups?.paymentMethods, record.paymModeId) },
        ] },
        { id: 'organization', title: t('customerQuickCreate.options.organization'), fields: [
          { name: 'countryRegionId', label: t('customerQuickCreate.fields.country'), type: 'select', options: choices(lookups?.countryRegions, record.countryRegionId) },
          { name: 'dlvModeId', label: t('customerQuickCreate.fields.deliveryMode'), type: 'select', options: choices(lookups?.deliveryModes, record.dlvModeId) },
        ] },
        { id: 'other', title: t('common.information'), fields: [
          { name: 'memo', label: t('customerQuickCreate.fields.notes'), multiline: true, rows: 3 },
        ] },
      ],
    },
    {
      id: 'sales-order-defaults', title: 'Sales order defaults',
      gridTemplateColumns: 'repeat(5, minmax(150px, 1fr))',
      groups: [
        { id: 'salesOrder', title: 'Sales order', fields: [
          { name: 'inventSiteId', label: 'Site', type: 'select', masterRoute: INVENTORY_ROUTE_PATHS.SITES, options: choices(dimensionsQuery.data?.sites.map((site) => ({ value: site.id, label: `${site.code} - ${site.name}` })), record.inventSiteId) },
          { name: 'inventLocationId', label: 'Warehouse', type: 'select', masterRoute: INVENTORY_ROUTE_PATHS.WAREHOUSES, options: choices(dimensionsQuery.data?.warehouses.map((warehouse) => ({ value: warehouse.id, label: `${warehouse.code} - ${warehouse.name}` })), record.inventLocationId) },
        ] },
        { id: 'salesOrderPool', fields: [
          { name: 'salesPoolId', label: 'Sales order pool', type: 'select', options: choices(lookups?.salesPools, record.salesPoolId ?? '') },
          { name: 'invoiceAccount', label: 'Invoice account', type: 'select', masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMERS, options: choices(customersQuery.data?.map((customer) => ({ value: customer.accountNumber, label: `${customer.accountNumber} - ${customer.name}` })), record.invoiceAccount) },
        ] },
      ],
    },
    {
      id: 'payment-defaults', title: 'Payment defaults',
      gridTemplateColumns: 'repeat(5, minmax(150px, 1fr))',
      groups: [
        { id: 'payment', title: 'Payment', fields: [
          { name: 'paymTermId', label: 'Terms of payment', type: 'select', masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_TERMS, options: choices(lookups?.paymentTerms, record.paymTermId) },
          { name: 'paymModeId', label: 'Method of payment', type: 'select', masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_METHODS, options: choices(lookups?.paymentMethods, record.paymModeId) },
        ] },
        { id: 'paymentTerms', fields: [
          { name: 'blockFloorLimitUseInChannel', label: 'Block floor limit use in Channel', type: 'boolean' },
        ] },
        { id: 'prepayment', title: 'Prepayment', fields: [
          { name: 'prepaymentValue', label: 'Prepayment value', type: 'number' },
        ] },
      ],
    },
    {
      id: 'addresses',
      title: t('customerDetails.sections.addresses', 'Addresses'),
      minHeight: 145,
      content: <PartyPostalAddressPanel partyId={record.party} editing={editing} storageKey="accounts-receivable.customer.addresses" />,
    },
    {
      id: 'contacts',
      title: t('customerDetails.sections.contacts', 'Contact information'),
      minHeight: 145,
      content: <PartyElectronicAddressPanel partyId={record.party} editing={editing} storageKey="accounts-receivable.customer.contacts" />,
    },
    {
      id: 'miscellaneous',
      title: t('customerDetails.sections.miscellaneous', 'Miscellaneous details'),
      groups: [{ id: 'status', fields: [{ name: 'status', label: t('common.status'), type: 'display' }] }],
    },
  ];

  const config: EnterpriseListDetailsConfig<CustomerRecord> = {
    recordTableName: 'CustTable',
    dataSource: {
      type: 'remote',
      key: 'accounts-receivable-customer-details',
      load: async (signal) => {
        const records = await customerQuickCreateApi.list(signal);
        if (!customerId) return records;
        return [...records].sort((left, right) => Number(right.id === customerId) - Number(left.id === customerId));
      },
      create: createCustomer,
      update: customerQuickCreateApi.update,
      delete: customerQuickCreateApi.remove,
    },
    createRecord: emptyCustomer,
    getPrimaryText: (record) => localizedName({ name: record.name, nameAlias: record.nameAr }, isRtl),
    getSecondaryText: (record) => record.accountNumber,
    matchesSearch: (record, query) => `${record.accountNumber} ${record.name} ${record.nameAr ?? ''}`.toLocaleLowerCase().includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      accountNumber: record.accountNumber, name: record.name, nameAr: record.nameAr ?? '',
      customerGroupId: record.customerGroupId, currencyCode: record.currencyCode,
      custCategory: record.custCategory, paymTermId: record.paymTermId, paymModeId: record.paymModeId,
      dlvModeId: record.dlvModeId, taxGroupId: record.taxGroupId, vatNum: record.vatNum,
      countryRegionId: record.countryRegionId, memo: record.memo ?? '', status: record.status,
      invoiceAccount: record.invoiceAccount, inventSiteId: record.inventSiteId,
      inventLocationId: record.inventLocationId, salesPoolId: record.salesPoolId ?? '',
      cashDiscBaseDays: record.cashDiscBaseDays ?? 0,
      blockFloorLimitUseInChannel: Boolean(record.blockFloorLimitUseInChannel),
      prepaymentValue: record.prepaymentValue ?? 0,
    }),
    setValues: (record, values) => ({
      ...record,
      name: String(values.name), nameAr: String(values.nameAr), customerGroupId: String(values.customerGroupId),
      currencyCode: String(values.currencyCode), custCategory: String(values.custCategory),
      paymTermId: String(values.paymTermId), paymModeId: String(values.paymModeId),
      dlvModeId: String(values.dlvModeId), taxGroupId: String(values.taxGroupId), vatNum: String(values.vatNum),
      countryRegionId: String(values.countryRegionId), memo: String(values.memo),
      invoiceAccount: String(values.invoiceAccount ?? ''),
      inventSiteId: String(values.inventSiteId ?? ''),
      inventLocationId: String(values.inventLocationId ?? ''),
      salesPoolId: String(values.salesPoolId ?? ''),
      cashDiscBaseDays: Number(values.cashDiscBaseDays ?? 0),
      blockFloorLimitUseInChannel: Number(Boolean(values.blockFloorLimitUseInChannel)),
      prepaymentValue: Number(values.prepaymentValue ?? 0),
    }),
    headerFields: [{
      id: 'summary', label: t('pages.customers.title'), type: 'display',
      getValue: (record) => `${record.accountNumber} : ${localizedName({ name: record.name, nameAlias: record.nameAr }, isRtl)}`,
      setValue: (record) => record,
    }],
    sections,
    commands: ['customer', 'sell', 'invoice', 'collect', 'service', 'market', 'commerce', 'general', 'creditManagement', 'options'].map((id) => ({ id, label: t(`customerCommands.${id}`) })),
    permissions: { view: PERMISSIONS.CUSTOMER_VIEW, create: PERMISSIONS.CUSTOMER_CREATE, edit: PERMISSIONS.CUSTOMER_UPDATE, delete: PERMISSIONS.CUSTOMER_DELETE },
    validate: (record) => ({
      ...(!record.name.trim() ? { name: t('validation.required', { field: t('fields.customerName') }) } : {}),
      ...(!record.customerGroupId.trim() ? { customerGroupId: t('validation.required', { field: t('fields.customerGroup') }) } : {}),
    }),
    advancedFilter: {
      title: t('filters.title'),
      fieldLabel: t('fields.account'),
      fields: [
        { id: 'accountNumber', label: t('fields.account'), getValue: (record) => record.accountNumber },
        { id: 'name', label: t('fields.customerName'), getValue: (record) => record.name },
        { id: 'nameAr', label: t('fields.arabicName'), getValue: (record) => record.nameAr ?? '' },
        { id: 'customerGroupId', label: t('fields.customerGroup'), getValue: (record) => record.customerGroupId },
        { id: 'currencyCode', label: t('fields.currency'), getValue: (record) => record.currencyCode },
        { id: 'countryRegionId', label: t('customerQuickCreate.fields.country'), getValue: (record) => record.countryRegionId },
      ],
      getValue: (record) => record.accountNumber,
      matches: (record, value) => `${record.accountNumber} ${record.name} ${record.nameAr ?? ''} ${record.customerGroupId} ${record.currencyCode} ${record.countryRegionId}`.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
    relatedInformation: {
      title: t('relatedInformation.title'),
      sections: (record) => [
        {
          id: 'primaryAddress',
          label: t('relatedInformation.primaryAddress'),
          defaultExpanded: true,
          content: <RelatedValue value={record?.countryRegionId} empty={t('customerDetails.noAddresses', 'No addresses are registered for this customer.')} />,
        },
        { id: 'recentActivity', label: t('relatedInformation.recentActivity') },
        { id: 'relationships', label: t('relatedInformation.relationships') },
        {
          id: 'statistics',
          label: t('relatedInformation.statistics'),
          content: <RelatedValue value={record ? `${record.currencyCode} · ${record.customerGroupId}` : ''} empty={t('relatedInformation.selectCustomer')} />,
        },
        { id: 'creditStatistics', label: t('relatedInformation.creditStatistics') },
        { id: 'contacts', label: t('relatedInformation.contacts') },
        { id: 'recurringInvoice', label: t('relatedInformation.recurringInvoice') },
        { id: 'classificationBalances', label: t('relatedInformation.classificationBalances') },
        { id: 'insuranceGuarantees', label: t('relatedInformation.insuranceGuarantees') },
      ],
    },
    showInformation: true,
    presentation: { mode: 'list', listWidth: 232, listMinWidth: 200, listMaxWidth: 360, listResizable: true, compactRecordHeader: true, recordHeaderMinHeight: 42 },
  };

  return <ListDetailsPage variant="enterprise" title={t('pages.customers.title')} config={config} />;
}

function RelatedValue({ value, empty }: { value?: string | null; empty: string }): React.ReactElement {
  return <Typography sx={{ fontSize: '0.75rem', color: value ? 'text.primary' : 'text.secondary', whiteSpace: 'pre-line' }}>{value || empty}</Typography>;
}
