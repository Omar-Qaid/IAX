import React from 'react';
import { MasterDetailField } from '@patterns/master-detail';
import { FOUNDATION_RECORD_PREVIEWS as FOUNDATION_PREVIEWS } from '../../../foundation/previews/foundationRecordPreviews';
import type { FreeTextInvoiceDocument } from '../details/useFreeTextInvoiceDocument';
import type { Line } from './freeTextInvoiceLine';

export function buildFreeTextInvoiceLineFields(scope: FreeTextInvoiceDocument) {
  const { t, lineTab, amount, editing, ledgerText } = scope;
  const ledgerOf = (line: Line) =>
    ledgerText(line.ledgerDimension) ||
    line.ledgerDisplay ||
    (line.ledgerDimension ? String(line.ledgerDimension) : '');
  const lineDetailFields = (line: Line) =>
    lineTab === 'general' ? (
      <>
        <MasterDetailField label={t('freeTextInvoices.invoiceText')} value={line.description} />
        <MasterDetailField label={t('fields.quantity')} value={line.quantity} numeric />
        <MasterDetailField label={t('fields.unitPrice')} value={amount(line.unitPrice)} numeric />
        <MasterDetailField
          label={t('freeTextInvoices.amount')}
          value={editing ? '—' : amount(line.netAmount)}
          numeric
        />
      </>
    ) : lineTab === 'setup' ? (
      <>
        <MasterDetailField
          label={t('fields.salesTaxGroup')}
          value={line.taxGroup}
          previewEntity={FOUNDATION_PREVIEWS.SALES_TAX_GROUP}
        />
        <MasterDetailField
          label={t('freeTextInvoices.itemTaxGroup')}
          value={line.taxItemGroup}
          previewEntity={FOUNDATION_PREVIEWS.ITEM_SALES_TAX_GROUP}
        />
      </>
    ) : (
      <>
        <MasterDetailField
          label={t('freeTextInvoices.ledgerAccount')}
          value={ledgerOf(line)}
          lookup
        />
        <MasterDetailField
          label={t('freeTextInvoices.defaultDimension')}
          value={line.defaultDimension || ''}
        />
      </>
    );
  return { lineDetailFields };
}
