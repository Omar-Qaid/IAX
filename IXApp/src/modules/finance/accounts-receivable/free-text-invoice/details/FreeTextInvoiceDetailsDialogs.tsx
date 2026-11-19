import React from 'react';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import {
  DocumentTotalsDrawer,
  type DocumentTotalsSection,
} from '@patterns/document/DocumentTotalsDrawer';
import type { FreeTextInvoiceDocument } from './useFreeTextInvoiceDocument';

export function FreeTextInvoiceDetailsDialogs({
  scope,
  totalsSections,
}: {
  scope: FreeTextInvoiceDocument;
  totalsSections: DocumentTotalsSection[];
}) {
  const { t, busy, confirmation, setConfirmation, runConfirmed, totalsOpen, setTotalsOpen } = scope;
  return (
    <>
      {' '}
      <ConfirmationDialog
        open={Boolean(confirmation)}
        title={confirmation === 'post' ? t('freeTextInvoices.post') : t('actions.delete')}
        message={
          confirmation === 'post'
            ? t('freeTextInvoices.confirmPost')
            : t('freeTextInvoices.confirmDelete', 'Delete this unposted free text invoice?')
        }
        confirmLabel={confirmation === 'post' ? t('freeTextInvoices.post') : t('actions.delete')}
        loading={busy}
        onClose={() => {
          if (!busy) setConfirmation(null);
        }}
        onConfirm={() => void runConfirmed()}
      />
      <DocumentTotalsDrawer
        open={totalsOpen}
        onClose={() => setTotalsOpen(false)}
        title={t('freeTextInvoices.totals')}
        viewLabel={t('common.standardView')}
        sections={totalsSections}
      />
    </>
  );
}
