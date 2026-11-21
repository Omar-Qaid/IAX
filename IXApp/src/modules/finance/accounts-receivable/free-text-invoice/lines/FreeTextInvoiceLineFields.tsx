import React from 'react';
import { TAX_ROUTE_PATHS } from '@modules/finance/tax/routes/taxRoutePaths';
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
          masterRoute={TAX_ROUTE_PATHS.TAX_GROUPS}
          previewEntity={FOUNDATION_PREVIEWS.SALES_TAX_GROUP}
        />
        <MasterDetailField
          label={t('freeTextInvoices.itemTaxGroup')}
          value={line.taxItemGroup}
          masterRoute={TAX_ROUTE_PATHS.TAX_ITEM_GROUPS}
          previewEntity={FOUNDATION_PREVIEWS.ITEM_SALES_TAX_GROUP}
        />
      </>
    ) : (
      <>
        <MasterDetailField
          label={t('freeTextInvoices.ledgerAccount')}
          value={ledgerOf(line)}
          masterRoute="/general-ledger/main-accounts"
          previewEntity="customerLedgerDimension"
          previewKey={line.ledgerDimension || null}
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
