import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { useParams, useSearchParams } from 'react-router-dom';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { freeTextInvoiceQueryKeys } from '../../queries/accountsReceivableQueryKeys';
import { freeTextInvoiceApi } from '../api/freeTextInvoiceApi';
import { FreeTextInvoiceDetails } from '../details/FreeTextInvoiceDetails';
import { asInvoice, newInvoice } from '../details/freeTextInvoiceModel';
/**
 * Free text invoice details on the master-detail pattern, laid out like sales
 * order: Lines tab (header FastTab, lines, line details, totals) and Header tab
 * (General / Setup / Payment FastTabs), ribbon, record list and status.
 */
export function CustFreeInvoiceDetailsPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  const { invoiceRecId } = useParams<{ invoiceRecId: string }>();
  const [params] = useSearchParams();
  const isNew = !invoiceRecId || invoiceRecId === 'new';
  const { t } = useAppTranslation();
  const query = useQuery({
    queryKey: freeTextInvoiceQueryKeys.invoice(company, invoiceRecId),
    queryFn: ({ signal }) => freeTextInvoiceApi.get(Number(invoiceRecId), signal),
    enabled: !isNew && /^\d+$/.test(invoiceRecId ?? ''),
  });
  const accountContext = params.get('customerAccount') ?? '';
  const blank = React.useMemo(() => newInvoice(accountContext), [accountContext]);
  if (!isNew && query.isLoading) return <LoadingState message={t('common.loading')} />;
  if (!isNew && (!query.data || query.error))
    return (
      <ErrorState
        title={t('errors.generic')}
        message={
          query.error?.message ?? t('freeTextInvoices.notFound', 'Free text invoice was not found.')
        }
        onRetry={() => void query.refetch()}
      />
    );
  return (
    <FreeTextInvoiceDetails
      key={`${company}:${invoiceRecId ?? 'new'}`}
      company={company}
      invoice={isNew ? blank : asInvoice(query.data!)}
      isNew={isNew}
      startEditing={params.get('edit') === 'true'}
    />
  );
}
