import React from 'react';
import { MasterDetailTotals } from '@patterns/master-detail';
import type { DocumentTotalsSection } from '@patterns/document/DocumentTotalsDrawer';
import type { FreeTextInvoiceDocument } from './useFreeTextInvoiceDocument';
import type { Invoice } from './freeTextInvoiceModel';

export function buildFreeTextInvoiceTotals(scope: FreeTextInvoiceDocument, invoice: Invoice) {
  const { t, amount, display } = scope;
  const totalsContent = (
    <MasterDetailTotals
      items={[
        { id: 'net', label: t('freeTextInvoices.netAmount'), value: amount(display.netAmount) },
        { id: 'tax', label: t('freeTextInvoices.salesTax'), value: amount(display.taxAmount) },
        {
          id: 'total',
          label: t('freeTextInvoices.invoiceAmount'),
          value: `${amount(display.invoiceAmount)} ${display.currencyCode}`,
          emphasis: true,
        },
      ]}
    />
  );
  const totalsSections: DocumentTotalsSection[] = [
    {
      id: 'totals',
      title: invoice.currencyCode,
      columns: [
        {
          id: 'amounts',
          fields: [
            {
              id: 'net',
              label: t('freeTextInvoices.netAmount'),
              value: amount(invoice.netAmount),
            },
            {
              id: 'tax',
              label: t('freeTextInvoices.salesTax'),
              value: amount(invoice.taxAmount),
            },
            {
              id: 'total',
              label: t('freeTextInvoices.invoiceAmount'),
              value: amount(invoice.invoiceAmount),
              emphasized: true,
            },
          ],
        },
      ],
    },
  ];
  return { totalsContent, totalsSections };
}
