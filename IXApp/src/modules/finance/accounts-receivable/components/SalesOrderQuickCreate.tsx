import { localizedName } from '@shared/utilities/localizedName';
import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { FastTabsDrawer } from '@patterns/drawer-fast-tabs';
import type { FastTabSection, FastTabValue } from '@patterns/dialog-fast-tabs/FastTabsDialog';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { customerQuickCreateApi } from '../api/customerQuickCreateApi';
import { salesOrderListApi, type SalesOrderListRecord } from '../api/salesOrderListApi';
import { salesOrderLinesApi } from '../api/salesOrderLinesApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';
import { FOUNDATION_ROUTE_PATHS } from '@modules/finance/foundation/routes/foundationRoutePaths';
import { INVENTORY_ROUTE_PATHS } from '@modules/finance/inventory/routes/inventoryRoutePaths';

interface SalesOrderQuickCreateProps {
  open: boolean;
  onClose: () => void;
  onSave: (order: SalesOrderListRecord) => void | Promise<void>;
}

const initialValues = (): Record<string, FastTabValue> => ({
  salesId: 'Automatic',
  customerAccount: '',
  oneTimeCustomer: 'false',
  customerName: '',
  contact: '',
  deliveryName: '',
  address: '',
  deliveryAddress: '',
  deliveryPostalAddressId: '',
  taxGroupId: '',
  customerReference: '',
  invoiceAccount: '',
  currencyCode: '',
  paymentTerms: '',
  paymentMethod: '',
  salesName: '',
  salesGroup: '',
  inventSiteId: '',
  inventLocationId: '',
  customerRequisitionNumber: '',
  intercompany: 'false',
  intercompanyCompanyId: '',
  requestedReceiptDate: new Date().toISOString().slice(0, 10),
  requestedShipDate: new Date().toISOString().slice(0, 10),
  deliveryDateControlType: '0',
  confirmDates: 'false',
  deliveryMode: '',
  deliveryTerms: '',
});

export function SalesOrderQuickCreate({
  open,
  onClose,
  onSave,
}: SalesOrderQuickCreateProps): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const customersQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-order-quick-create', 'customers'],
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
    enabled: open,
    refetchOnMount: 'always',
  });
  const customers = React.useMemo(() => customersQuery.data ?? [], [customersQuery.data]);
  const [selectedAccount, setSelectedAccount] = React.useState('');
  React.useEffect(() => { if (!open) setSelectedAccount(''); }, [open]);
  const customerDefaultsQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-order-quick-create', 'customer-defaults', selectedAccount],
    queryFn: ({ signal }) => customerQuickCreateApi.salesOrderDefaults(selectedAccount, signal),
    enabled: open && Boolean(selectedAccount),
  });
  const primaryAddress = customerDefaultsQuery.data?.address ?? '';
  const customerAddresses = customerDefaultsQuery.data?.addresses ?? [];
  const primaryDeliveryAddress = customerAddresses[0];
  const orderContacts = useMemo(() => (customerDefaultsQuery.data?.contacts ?? []).filter(
    (contact) => contact.type === 'Email' || contact.type === 'Phone'
  ), [customerDefaultsQuery.data]);
  const primaryContact = orderContacts.find((contact) => contact.primary) ?? orderContacts[0];
  const deliveryLookupsQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-order-quick-create', 'delivery-lookups'],
    queryFn: ({ signal }) => customerQuickCreateApi.lookups(signal),
    enabled: open,
    staleTime: 5 * 60 * 1000,
  });
  const dimensionsQuery = useQuery({
    queryKey: ['sales-order-inventory-dimensions'],
    queryFn: ({ signal }) => salesOrderLinesApi.inventoryDimensions(signal),
    enabled: open,
    staleTime: 5 * 60 * 1000,
  });
  const customerFor = React.useCallback(
    (values: Record<string, FastTabValue>) =>
      customers.find((customer) => customer.accountNumber === String(values.customerAccount)),
    [customers]
  );
  const sections = useMemo<FastTabSection[]>(
    () => [
      {
        id: 'customer',
        title: t('fields.customer', 'Customer'),
        fields: [
          {
            name: 'customerAccount',
            masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMERS,
            label: t('fields.customerAccount'),
            type: 'select',
            required: true,
            options: customers.map((customer) => ({
              value: customer.accountNumber,
              label: `${customer.accountNumber} - ${localizedName({ name: customer.name, nameAlias: customer.nameAr }, isRtl)}`,
            })),
          },
          {
            name: 'oneTimeCustomer',
            label: t('salesOrderQuickCreate.oneTimeCustomer', 'One-time customer'),
            type: 'select',
            options: [
              { value: 'false', label: t('common.no', 'No') },
              { value: 'true', label: t('common.yes', 'Yes') },
            ],
          },
          {
            name: 'customerName',
            label: t('fields.customerName'),
            disabled: true,
            valueGetter: (values) =>
              localizedName(
                { name: customerFor(values)?.name, nameAlias: customerFor(values)?.nameAr },
                isRtl
              ),
          },
          {
            name: 'contact',
            label: t('relatedInformation.contacts', 'Contact'),
            type: 'select',
            valueGetter: (values) => values.contact || primaryContact?.number || '',
            options: orderContacts.map((contact) => ({
              value: contact.number,
              label: `${contact.type}: ${contact.number}`,
            })),
          },
          {
            name: 'deliveryName',
            label: t('salesOrderQuickCreate.deliveryName', 'Delivery name'),
            type: 'multiline',
            rows: 3,
          },
          {
            name: 'address',
            label: t('salesOrderQuickCreate.address', 'Address'),
            type: 'multiline',
            rows: 3,
            disabled: true,
            valueGetter: () => primaryAddress,
          },
          {
            name: 'deliveryAddress',
            label: t('salesOrderQuickCreate.deliveryAddress', 'Delivery address'),
            type: 'select',
            valueGetter: (values) => values.deliveryAddress || primaryDeliveryAddress?.id || '',
            options: customerAddresses.map((address) => ({ value: address.id, label: address.address })),
          },
        ],
      },
      {
        id: 'general',
        title: t('common.general', 'General'),
        fields: [
          { name: 'salesId', label: t('fields.salesOrderNumber'), disabled: true },
          {
            name: 'currencyCode',
            masterRoute: FOUNDATION_ROUTE_PATHS.CURRENCIES,
            label: t('fields.currency'),
            type: 'select',
            required: true,
            valueGetter: (values) => values.currencyCode || customerFor(values)?.currencyCode || '',
            options: [
              ...new Set(customers.map((customer) => customer.currencyCode).filter(Boolean)),
            ].map((currency) => ({ value: currency, label: currency })),
          },
          {
            name: 'invoiceAccount',
            masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMERS,
            label: t('fields.invoiceAccount'),
            type: 'select',
            required: true,
            valueGetter: (values) =>
              values.invoiceAccount ||
              customerFor(values)?.invoiceAccount ||
              customerFor(values)?.accountNumber ||
              '',
            options: customers.map((customer) => ({
              value: customer.accountNumber,
              label: `${customer.accountNumber} - ${localizedName({ name: customer.name, nameAlias: customer.nameAr }, isRtl)}`,
            })),
          },
          {
            name: 'inventSiteId',
            masterRoute: INVENTORY_ROUTE_PATHS.SITES,
            label: t('salesOrderQuickCreate.site', 'Site'),
            type: 'select',
            valueGetter: (values) => values.inventSiteId || customerFor(values)?.inventSiteId || '',
            options: (dimensionsQuery.data?.sites ?? []).map((site) => ({
              value: site.id,
              label: `${site.code} - ${site.name}`,
            })),
          },
          {
            name: 'inventLocationId',
            masterRoute: INVENTORY_ROUTE_PATHS.WAREHOUSES,
            label: t('salesOrderQuickCreate.warehouse', 'Warehouse'),
            type: 'select',
            valueGetter: (values) => {
              const customer = customerFor(values);
              const selectedSite = String(values.inventSiteId || customer?.inventSiteId || '');
              return (
                values.inventLocationId ||
                (selectedSite === customer?.inventSiteId ? customer.inventLocationId : '') ||
                ''
              );
            },
            optionsGetter: (values) => {
              const selectedSite = String(
                values.inventSiteId || customerFor(values)?.inventSiteId || ''
              );
              return (dimensionsQuery.data?.warehouses ?? [])
                .filter((warehouse) => selectedSite && warehouse.siteId === selectedSite)
                .map((warehouse) => ({
                  value: warehouse.id,
                  label: `${warehouse.code} - ${warehouse.name}`,
                }));
            },
          },
          {
            name: 'salesName',
            label: t('fields.name'),
            valueGetter: (values) => values.salesName || customerFor(values)?.name || '',
          },
          {
            name: 'intercompany',
            label: t('salesOrderQuickCreate.intercompany', 'Intercompany'),
            type: 'select',
            options: [
              { value: 'false', label: t('common.no', 'No') },
              { value: 'true', label: t('common.yes', 'Yes') },
            ],
          },
          {
            name: 'paymentTerms',
            masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_TERMS,
            label: t('fields.termsOfPayment'),
            type: 'select',
            valueGetter: (values) => values.paymentTerms || customerFor(values)?.paymTermId || '',
            options: deliveryLookupsQuery.data?.paymentTerms ?? [],
          },
          {
            name: 'paymentMethod',
            masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_METHODS,
            label: t('customerQuickCreate.fields.paymentMethod'),
            type: 'select',
            valueGetter: (values) => values.paymentMethod || customerFor(values)?.paymModeId || '',
            options: deliveryLookupsQuery.data?.paymentMethods ?? [],
          },
          {
            name: 'taxGroupId',
            masterRoute: FOUNDATION_ROUTE_PATHS.TAX_GROUPS,
            label: t('fields.salesTaxGroup', 'Sales tax group'),
            type: 'select',
            valueGetter: (values) => values.taxGroupId || customerFor(values)?.taxGroupId || '',
            options: deliveryLookupsQuery.data?.salesTaxGroups ?? [],
          },
          { name: 'salesGroup', label: t('salesOrderQuickCreate.salesGroup', 'Sales group') },
          { name: 'intercompanyCompanyId', label: t('fields.company', 'Company') },
          {
            name: 'customerRequisitionNumber',
            label: t('salesOrderQuickCreate.customerRequisition', 'Customer requisition'),
          },
          { name: 'customerReference', label: t('fields.customerReference', 'Customer reference') },
        ],
      },
      {
        id: 'delivery',
        title: t('salesOrderQuickCreate.delivery', 'Delivery'),
        fields: [
          {
            name: 'requestedReceiptDate',
            label: t('salesOrderQuickCreate.requestedReceiptDate', 'Requested receipt date'),
            type: 'date',
            required: true,
          },
          {
            name: 'requestedShipDate',
            label: t('salesOrderQuickCreate.requestedShipDate', 'Requested ship date'),
            type: 'date',
            required: true,
          },
          {
            name: 'deliveryDateControlType',
            label: t('salesOrderQuickCreate.deliveryDateControl', 'Delivery date control'),
            type: 'select',
            options: [
              { value: '0', label: t('common.none') },
              { value: '1', label: t('salesOrderQuickCreate.salesLeadTime', 'Sales lead time') },
              { value: '2', label: t('salesOrderQuickCreate.atp', 'ATP') },
              { value: '3', label: t('salesOrderQuickCreate.ctp', 'CTP') },
            ],
          },
          {
            name: 'confirmDates',
            label: t('salesOrderQuickCreate.confirmDates', 'Confirm dates'),
            type: 'select',
            options: [
              { value: 'false', label: t('common.no', 'No') },
              { value: 'true', label: t('common.yes', 'Yes') },
            ],
          },
          {
            name: 'deliveryMode',
            label: t('fields.deliveryMode'),
            type: 'select',
            valueGetter: (values) => values.deliveryMode || customerFor(values)?.dlvModeId || '',
            options: deliveryLookupsQuery.data?.deliveryModes ?? [],
          },
          {
            name: 'deliveryTerms',
            label: t('customerQuickCreate.fields.deliveryTerms'),
            type: 'select',
            options: deliveryLookupsQuery.data?.deliveryTerms ?? [],
          },
        ],
      },
    ],
    [customerFor, customers, orderContacts, deliveryLookupsQuery.data, dimensionsQuery.data, primaryAddress, primaryContact, primaryDeliveryAddress, customerAddresses, t, isRtl]
  );

  return (
    <FastTabsDrawer
      open={open}
      resetKey={open ? 'open' : 'closed'}
      title={t('salesOrderQuickCreate.title', 'Create sales order')}
      viewLabel={t('common.standardView')}
      sections={sections}
      loading={
        customersQuery.isLoading || deliveryLookupsQuery.isLoading || dimensionsQuery.isLoading ||
        customerDefaultsQuery.isFetching
      }
      loadError={
        customersQuery.error instanceof Error
          ? customersQuery.error.message
          : deliveryLookupsQuery.error instanceof Error
            ? deliveryLookupsQuery.error.message
            : dimensionsQuery.error instanceof Error
              ? dimensionsQuery.error.message
              : customerDefaultsQuery.error instanceof Error
                ? customerDefaultsQuery.error.message
                : null
      }
      initialValues={initialValues}
      onFieldChange={(name, value) => {
        if (name === 'inventSiteId') return { inventLocationId: '' };
        if (name !== 'customerAccount') return;
        setSelectedAccount(String(value));
        const customer = customers.find((candidate) => candidate.accountNumber === String(value));
        if (!customer) return {
          contact: '', deliveryName: '', invoiceAccount: '', currencyCode: '',
          paymentTerms: '', paymentMethod: '', salesName: '', inventSiteId: '',
          inventLocationId: '', deliveryMode: '', taxGroupId: '', deliveryAddress: '',
        };
        return {
          customerName: customer.name,
          contact: '',
          deliveryName: customer.name,
          address: '',
          deliveryAddress: '',
          taxGroupId: customer.taxGroupId,
          invoiceAccount: customer.invoiceAccount || customer.accountNumber,
          currencyCode: customer.currencyCode,
          paymentTerms: customer.paymTermId ?? '',
          paymentMethod: customer.paymModeId ?? '',
          salesName: customer.name,
          inventSiteId: customer.inventSiteId,
          inventLocationId: customer.inventLocationId,
          deliveryMode: customer.dlvModeId,
        };
      }}
      validate={(values) => {
        const errors: Record<string, string> = {};
        if (!String(values.customerAccount ?? '').trim())
          errors.customerAccount = t('validation.required', { field: t('fields.customerAccount') });
        if (!String(values.currencyCode || customerFor(values)?.currencyCode || '').trim())
          errors.currencyCode = t('validation.required', { field: t('fields.currency') });
        if (
          !String(
            values.invoiceAccount ||
              customerFor(values)?.invoiceAccount ||
              customerFor(values)?.accountNumber ||
              ''
          ).trim()
        )
          errors.invoiceAccount = t('validation.required', { field: t('fields.invoiceAccount') });
        if (!String(values.requestedReceiptDate ?? '').trim())
          errors.requestedReceiptDate = t('validation.required', {
            field: t('salesOrderQuickCreate.requestedReceiptDate', 'Requested receipt date'),
          });
        if (!String(values.requestedShipDate ?? '').trim())
          errors.requestedShipDate = t('validation.required', {
            field: t('salesOrderQuickCreate.requestedShipDate', 'Requested ship date'),
          });
        return errors;
      }}
      onSubmit={async (values) => {
        const customer = customers.find(
          (candidate) => candidate.accountNumber === String(values.customerAccount)
        );
        const inventSiteId = String(values.inventSiteId || customer?.inventSiteId || '');
        const requestedWarehouse = String(values.inventLocationId || '');
        const customerWarehouse =
          inventSiteId === customer?.inventSiteId ? String(customer.inventLocationId || '') : '';
        const inventLocationId = requestedWarehouse || customerWarehouse;
        const warehouseBelongsToSite =
          !inventLocationId ||
          dimensionsQuery.data?.warehouses.some(
            (warehouse) =>
              warehouse.id === inventLocationId && warehouse.siteId === inventSiteId
          );
        const contact = String(values.contact || primaryContact?.number || '');
        const selectedContact = orderContacts.find((item) => item.number === contact);
        const order = await salesOrderListApi.create({
          customerAccount: String(values.customerAccount),
          oneTimeCustomer: String(values.oneTimeCustomer) === 'true',
          contact,
          contactType: selectedContact?.type,
          deliveryPostalAddressId: String(values.deliveryAddress || primaryDeliveryAddress?.id || ''),
          taxGroupId: String(values.taxGroupId || customer?.taxGroupId || ''),
          deliveryName: String(values.deliveryName ?? '') || customer?.name,
          customerReference: String(values.customerReference ?? ''),
          invoiceAccount: String(
            values.invoiceAccount || customer?.invoiceAccount || customer?.accountNumber || ''
          ),
          currencyCode: String(values.currencyCode || customer?.currencyCode || ''),
          paymentTerms: String(values.paymentTerms ?? ''),
          paymentMethod: String(values.paymentMethod ?? ''),
          salesName: String(values.salesName || customer?.name || ''),
          salesGroup: String(values.salesGroup ?? ''),
          inventSiteId,
          inventLocationId: warehouseBelongsToSite ? inventLocationId : '',
          customerRequisitionNumber: String(values.customerRequisitionNumber ?? ''),
          intercompany: String(values.intercompany) === 'true',
          intercompanyCompanyId: String(values.intercompanyCompanyId ?? ''),
          requestedReceiptDate: String(values.requestedReceiptDate),
          requestedShipDate: String(values.requestedShipDate),
          deliveryDateControlType: Number(values.deliveryDateControlType),
          confirmDates: String(values.confirmDates) === 'true',
          deliveryMode: String(values.deliveryMode || customer?.dlvModeId || ''),
          deliveryTerms: String(values.deliveryTerms ?? ''),
        });
        await onSave(order);
      }}
      saveLabel={t('actions.save', 'Save')}
      cancelLabel={t('actions.cancel')}
      closeLabel={t('actions.close')}
      helpLabel={t('common.help')}
      onCancel={onClose}
    />
  );
}
