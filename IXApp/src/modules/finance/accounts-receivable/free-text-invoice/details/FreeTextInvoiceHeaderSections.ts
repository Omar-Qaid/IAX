import type {
  DetailFieldConfig,
  DetailSectionConfig,
  DetailValues,
} from '@patterns/list-details/types';
import type { Invoice } from './freeTextInvoiceModel';
import type { useFreeTextInvoiceDocument } from './useFreeTextInvoiceDocument';

type HeaderScope = {
  t: ReturnType<typeof useFreeTextInvoiceDocument>['t'];
  fields: Record<string, DetailFieldConfig>;
  values: DetailValues;
  display: Invoice;
  customerName: string;
};

export function buildFreeTextInvoiceHeaderSections({
  t,
  fields,
  values,
  display,
  customerName,
}: HeaderScope) {
  const group = (id: string, title: string, groupFields: DetailFieldConfig[]) => ({
    id,
    title: title.toUpperCase(),
    fields: groupFields,
  });
  // Lines tab: compact header FastTab (one row of field groups), like sales order.
  const compactHeader: DetailSectionConfig = {
    id: 'invoice-header',
    title: t('freeTextInvoices.header'),
    visualVariant: 'legalEntity',
    columns: 5,
    groups: [
      group('customer', t('fields.customer'), [fields.customerAccount, fields.customerName]),
      group('invoice', t('freeTextInvoices.invoice'), [fields.invoiceDate, fields.dueDate]),
      group('references', t('fields.description'), [fields.invoiceAccount, fields.description]),
      group('sales-tax', t('freeTextInvoices.salesTax'), [fields.currencyCode, fields.taxGroup]),
      group('payment', t('freeTextInvoices.payment'), [fields.paymentTerms, fields.paymentMethod]),
    ],
  };
  // Header tab: the full header as FastTabs with key values in the heading.
  const headerSections: DetailSectionConfig[] = [
    {
      id: 'header-general',
      title: t('common.general'),
      visualVariant: 'legalEntity',
      columns: 4,
      summaryItems: [
        {
          id: 'invoiceId',
          label: t('freeTextInvoices.invoice'),
          value: values.invoiceId,
          accent: true,
        },
        {
          id: 'customerAccount',
          label: t('fields.customerAccount'),
          value: display.customerAccount,
          accent: true,
        },
        { id: 'customerName', label: t('fields.customerName'), value: customerName },
      ],
      groups: [
        group('customer', t('fields.customer'), [
          fields.customerAccount,
          fields.customerName,
          fields.invoiceAccount,
        ]),
        group('invoice', t('freeTextInvoices.invoice'), [
          fields.invoiceId,
          fields.invoiceDate,
          fields.dueDate,
        ]),
        group('description', t('fields.description'), [fields.description]),
        group('status', t('common.status'), [fields.posted]),
      ],
    },
    {
      id: 'header-setup',
      title: t('freeTextInvoices.setup'),
      visualVariant: 'legalEntity',
      columns: 4,
      summaryItems: [
        {
          id: 'currencyCode',
          label: t('fields.currency'),
          value: display.currencyCode,
          accent: true,
        },
        { id: 'taxGroup', label: t('fields.salesTaxGroup'), value: display.taxGroup, accent: true },
      ],
      groups: [
        group('currency', t('fields.currency'), [fields.currencyCode]),
        group('sales-tax', t('freeTextInvoices.salesTax'), [fields.taxGroup]),
        group('posting', t('freeTextInvoices.posting'), [fields.postingProfile]),
      ],
    },
    {
      id: 'header-payment',
      title: t('freeTextInvoices.payment'),
      visualVariant: 'legalEntity',
      columns: 4,
      summaryItems: [
        {
          id: 'paymentTerms',
          label: t('freeTextInvoices.paymentTerms'),
          value: display.paymentTerms,
          accent: true,
        },
      ],
      groups: [
        group('payment', t('freeTextInvoices.payment'), [
          fields.paymentTerms,
          fields.paymentMethod,
        ]),
      ],
    },
  ];
  return { compactHeader, headerSections };
}
