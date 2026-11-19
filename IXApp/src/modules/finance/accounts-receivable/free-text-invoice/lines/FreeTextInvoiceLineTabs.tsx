import React from 'react';
import { MasterDetailLineDetails } from '@patterns/master-detail';
import type { FreeTextInvoiceDocument } from '../details/useFreeTextInvoiceDocument';
import { buildFreeTextInvoiceLineFields } from './FreeTextInvoiceLineFields';

export function buildFreeTextInvoiceLineTabs(scope: FreeTextInvoiceDocument) {
  const { t, lineTab, setLineTab, selectedLine } = scope;
  const { lineDetailFields } = buildFreeTextInvoiceLineFields(scope);
  const lineDetails = (
    <MasterDetailLineDetails
      ariaLabel={t('freeTextInvoices.lineDetails')}
      tabs={[
        { value: 'general', label: t('common.general') },
        { value: 'setup', label: t('freeTextInvoices.setup') },
        { value: 'dimensions', label: t('freeTextInvoices.financialDimensions') },
      ]}
      value={lineTab}
      onChange={setLineTab}
      hasLine={Boolean(selectedLine)}
      emptyLabel={t('freeTextInvoices.selectLine')}
    >
      {selectedLine && lineDetailFields(selectedLine)}
    </MasterDetailLineDetails>
  );
  return { lineDetails };
}
