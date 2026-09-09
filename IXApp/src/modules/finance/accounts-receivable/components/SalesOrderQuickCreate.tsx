import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { FastTabsDrawer } from '@patterns/drawer-fast-tabs';
import type { FastTabSection, FastTabValue } from '@patterns/dialog-fast-tabs/FastTabsDialog';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { customerQuickCreateApi } from '../api/customerQuickCreateApi';
import { salesOrderListApi, type SalesOrderListRecord } from '../api/salesOrderListApi';

interface SalesOrderQuickCreateProps {
  open: boolean;
  onClose: () => void;
  onSave: (order: SalesOrderListRecord) => void | Promise<void>;
}

const initialValues = (): Record<string, FastTabValue> => ({
  salesId: 'Automatic',
  customerAccount: '',
  oneTimeCustomer: 'false',
  searchBy: 'keyword',
  searchFor: '',
  customerName: '',
  contact: '',
  deliveryName: '',
  address: '',
  deliveryAddress: '',
  customerReference: '',
  invoiceAccount: '',
  currencyCode: '',
  paymentTerms: '',
  paymentMethod: '',
  orderType: 'sales',
  salesName: '',
  salesGroup: '',
  inventSiteId: '',
  inventLocationId: '',
  customerRequisitionNumber: '',
  salesAgreementId: '',
  intercompany: 'false',
  intercompanyCompanyId: '',
  requestedReceiptDate: new Date().toISOString().slice(0, 10),
  requestedShipDate: new Date().toISOString().slice(0, 10),
  shippingTimeZone: '(GMT+03:00) Kuwait, Riyadh',
  deliveryDateControlType: '0',
  confirmDates: 'false',
  deliveryMode: '',
  deliveryTerms: '',
});

export function SalesOrderQuickCreate({ open, onClose, onSave }: SalesOrderQuickCreateProps): React.ReactElement {
  const { t } = useAppTranslation();
  const customersQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-order-quick-create', 'customers'],
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
    enabled: open,
    staleTime: 5 * 60 * 1000,
  });
  const customers = customersQuery.data ?? [];
  const deliveryLookupsQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-order-quick-create', 'delivery-lookups'],
    queryFn: ({ signal }) => customerQuickCreateApi.lookups(signal),
    enabled: open,
    staleTime: 5 * 60 * 1000,
  });
  const customerFor = (values: Record<string, FastTabValue>) =>
    customers.find((customer) => customer.accountNumber === String(values.customerAccount));
  const sections = useMemo<FastTabSection[]>(() => [
    {
      id: 'customer',
      title: t('fields.customer', 'Customer'),
      fields: [
        { name: 'customerAccount', label: t('fields.customerAccount'), type: 'select', required: true, options: customers.map((customer) => ({ value: customer.accountNumber, label: `${customer.accountNumber} - ${customer.name}` })) },
        { name: 'oneTimeCustomer', label: t('salesOrderQuickCreate.oneTimeCustomer', 'One-time customer'), type: 'select', options: [{ value: 'false', label: t('common.no', 'No') }, { value: 'true', label: t('common.yes', 'Yes') }] },
        { name: 'searchBy', label: t('fields.searchBy'), type: 'select', options: [{ value: 'keyword', label: t('common.keyword', 'Keyword') }, { value: 'account', label: t('fields.customerAccount') }, { value: 'name', label: t('fields.customerName') }] },
        { name: 'searchFor', label: t('salesOrderQuickCreate.searchFor', 'Search for') },
        { name: 'customerName', label: t('fields.customerName'), disabled: true, valueGetter: (values) => customers.find((customer) => customer.accountNumber === String(values.customerAccount))?.name ?? '' },
        { name: 'contact', label: t('relatedInformation.contacts', 'Contact'), type: 'select', optionsGetter: (values) => {
          const customer = customers.find((candidate) => candidate.accountNumber === String(values.customerAccount));
          return [customer?.phone, customer?.email].filter((value): value is string => Boolean(value)).map((value) => ({ value, label: value }));
        } },
        { name: 'deliveryName', label: t('salesOrderQuickCreate.deliveryName', 'Delivery name'), type: 'multiline', rows: 3 },
        { name: 'address', label: t('salesOrderQuickCreate.address', 'Address'), type: 'multiline', rows: 3, disabled: true, valueGetter: (values) => customers.find((customer) => customer.accountNumber === String(values.customerAccount))?.countryRegionId ?? '' },
        { name: 'deliveryAddress', label: t('salesOrderQuickCreate.deliveryAddress', 'Delivery address'), disabled: true },
      ],
    },
    {
      id: 'general',
      title: t('common.general', 'General'),
      fields: [
        { name: 'salesId', label: t('fields.salesOrderNumber'), disabled: true },
        { name: 'currencyCode', label: t('fields.currency'), type: 'select', required: true, valueGetter: (values) => values.currencyCode || customerFor(values)?.currencyCode || '', options: [...new Set(customers.map((customer) => customer.currencyCode).filter(Boolean))].map((currency) => ({ value: currency, label: currency })) },
        { name: 'invoiceAccount', label: t('fields.invoiceAccount'), type: 'select', required: true, valueGetter: (values) => values.invoiceAccount || customerFor(values)?.invoiceAccount || customerFor(values)?.accountNumber || '', options: customers.map((customer) => ({ value: customer.accountNumber, label: `${customer.accountNumber} - ${customer.name}` })) },
        { name: 'inventSiteId', label: t('salesOrderQuickCreate.site', 'Site'), valueGetter: (values) => values.inventSiteId || customerFor(values)?.inventSiteId || '' },
        { name: 'orderType', label: t('salesOrderQuickCreate.orderType', 'Order type'), type: 'select', options: [{ value: 'sales', label: t('salesOrderQuickCreate.salesOrder', 'Sales order') }] },
        { name: 'inventLocationId', label: t('salesOrderQuickCreate.warehouse', 'Warehouse'), valueGetter: (values) => values.inventLocationId || customerFor(values)?.inventLocationId || '' },
        { name: 'salesName', label: t('fields.name'), valueGetter: (values) => values.salesName || customerFor(values)?.name || '' },
        { name: 'intercompany', label: t('salesOrderQuickCreate.intercompany', 'Intercompany'), type: 'select', options: [{ value: 'false', label: t('common.no', 'No') }, { value: 'true', label: t('common.yes', 'Yes') }] },
        { name: 'paymentTerms', label: t('fields.termsOfPayment'), type: 'select', options: deliveryLookupsQuery.data?.paymentTerms ?? [] },
        { name: 'paymentMethod', label: t('customerQuickCreate.fields.paymentMethod'), type: 'select', options: deliveryLookupsQuery.data?.paymentMethods ?? [] },
        { name: 'salesGroup', label: t('salesOrderQuickCreate.salesGroup', 'Sales group') },
        { name: 'intercompanyCompanyId', label: t('fields.company', 'Company') },
        { name: 'customerRequisitionNumber', label: t('salesOrderQuickCreate.customerRequisition', 'Customer requisition') },
        { name: 'customerReference', label: t('fields.customerReference', 'Customer reference') },
        { name: 'salesAgreementId', label: t('salesOrderQuickCreate.salesAgreementId', 'Sales agreement ID'), disabled: true },
      ],
    },
    {
      id: 'delivery',
      title: t('salesOrderQuickCreate.delivery', 'Delivery'),
      fields: [
        { name: 'requestedReceiptDate', label: t('salesOrderQuickCreate.requestedReceiptDate', 'Requested receipt date'), type: 'date', required: true },
        { name: 'shippingTimeZone', label: t('salesOrderQuickCreate.shippingTimeZone', 'Shipping location time zone'), disabled: true },
        { name: 'requestedShipDate', label: t('salesOrderQuickCreate.requestedShipDate', 'Requested ship date'), type: 'date', required: true },
        { name: 'deliveryDateControlType', label: t('salesOrderQuickCreate.deliveryDateControl', 'Delivery date control'), type: 'select', options: [{ value: '0', label: t('common.none') }, { value: '1', label: t('salesOrderQuickCreate.salesLeadTime', 'Sales lead time') }, { value: '2', label: t('salesOrderQuickCreate.atp', 'ATP') }, { value: '3', label: t('salesOrderQuickCreate.ctp', 'CTP') }] },
        { name: 'confirmDates', label: t('salesOrderQuickCreate.confirmDates', 'Confirm dates'), type: 'select', options: [{ value: 'false', label: t('common.no', 'No') }, { value: 'true', label: t('common.yes', 'Yes') }] },
        { name: 'deliveryMode', label: t('fields.deliveryMode'), type: 'select', valueGetter: (values) => values.deliveryMode || customerFor(values)?.dlvModeId || '', options: deliveryLookupsQuery.data?.deliveryModes ?? [] },
        { name: 'deliveryTerms', label: t('customerQuickCreate.fields.deliveryTerms'), type: 'select', options: deliveryLookupsQuery.data?.deliveryTerms ?? [] },
      ],
    },
  ], [customers, deliveryLookupsQuery.data, t]);

  return (
    <FastTabsDrawer
      open={open}
      resetKey={open ? 'open' : 'closed'}
      title={t('salesOrderQuickCreate.title', 'Create sales order')}
      viewLabel={t('common.standardView')}
      sections={sections}
      loading={customersQuery.isLoading || deliveryLookupsQuery.isLoading}
      loadError={customersQuery.error instanceof Error ? customersQuery.error.message : deliveryLookupsQuery.error instanceof Error ? deliveryLookupsQuery.error.message : null}
      initialValues={initialValues}
      onFieldChange={(name, value) => {
        if (name !== 'customerAccount') return;
        const customer = customers.find((candidate) => candidate.accountNumber === String(value));
        if (!customer) return;
        return {
          customerName: customer.name,
          contact: customer.phone ?? customer.email ?? '',
          deliveryName: customer.name,
          address: customer.countryRegionId,
          deliveryAddress: customer.countryRegionId,
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
        if (!String(values.invoiceAccount || customerFor(values)?.invoiceAccount || customerFor(values)?.accountNumber || '').trim())
          errors.invoiceAccount = t('validation.required', { field: t('fields.invoiceAccount') });
        if (!String(values.requestedReceiptDate ?? '').trim())
          errors.requestedReceiptDate = t('validation.required', { field: t('salesOrderQuickCreate.requestedReceiptDate', 'Requested receipt date') });
        if (!String(values.requestedShipDate ?? '').trim())
          errors.requestedShipDate = t('validation.required', { field: t('salesOrderQuickCreate.requestedShipDate', 'Requested ship date') });
        return errors;
      }}
      onSubmit={async (values) => {
        const customer = customers.find((candidate) => candidate.accountNumber === String(values.customerAccount));
        const order = await salesOrderListApi.create({
          customerAccount: String(values.customerAccount),
          oneTimeCustomer: String(values.oneTimeCustomer) === 'true',
          contact: String(values.contact ?? ''),
          deliveryName: String(values.deliveryName ?? '') || customer?.name,
          customerReference: String(values.customerReference ?? ''),
          invoiceAccount: String(values.invoiceAccount || customer?.invoiceAccount || customer?.accountNumber || ''),
          currencyCode: String(values.currencyCode || customer?.currencyCode || ''),
          paymentTerms: String(values.paymentTerms ?? ''),
          paymentMethod: String(values.paymentMethod ?? ''),
          salesName: String(values.salesName || customer?.name || ''),
          salesGroup: String(values.salesGroup ?? ''),
          inventSiteId: String(values.inventSiteId || customer?.inventSiteId || ''),
          inventLocationId: String(values.inventLocationId || customer?.inventLocationId || ''),
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
