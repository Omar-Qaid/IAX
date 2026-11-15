import React from 'react';
import { Alert } from '@mui/material';
import { useQueries, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { OpenBalanceCurrencyPage } from '@patterns/open-balance-currency/OpenBalanceCurrencyPage';
import type { BalancePartyKind } from '@patterns/open-balance-currency/types';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '@modules/finance/accounts-receivable/routes/accountsReceivableRoutePaths';
import { partyBalanceApi } from '../api/partyBalanceApi';

function PartyBalancePage({ kind }: { kind: BalancePartyKind }): React.ReactElement {
  const { accountNum = '' } = useParams<{ accountNum: string }>();
  const company = useCompanyStore((state) => state.currentCompany);
  const { hasPermission } = useAuth();
  const { t } = useAppTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const primaryPermission =
    kind === 'customer'
      ? PERMISSIONS.CUSTOMER_TRANSACTION_VIEW
      : PERMISSIONS.VENDOR_TRANSACTION_VIEW;
  const relatedPermission =
    kind === 'customer'
      ? PERMISSIONS.VENDOR_TRANSACTION_VIEW
      : PERMISSIONS.CUSTOMER_TRANSACTION_VIEW;
  const otherKind = kind === 'customer' ? 'vendor' : 'customer';
  const permitted = hasPermission(primaryPermission);
  const balance = useQuery({
    queryKey: ['party-balance-currency', company, kind, accountNum],
    queryFn: ({ signal }) => partyBalanceApi.load(kind, accountNum, signal),
    enabled: permitted && Boolean(accountNum),
  });
  const relatedAccounts = balance.data?.relatedAccounts ?? [];
  const canReadRelated = hasPermission(relatedPermission);
  const related = useQueries({
    queries: canReadRelated
      ? relatedAccounts.map((account) => ({
          queryKey: ['party-balance-currency', company, otherKind, account],
          queryFn: ({ signal }: { signal: AbortSignal }) =>
            partyBalanceApi.load(otherKind, account, signal),
        }))
      : [],
  });
  if (!permitted) return <Alert severity="warning">{t('partyBalance.accessDenied')}</Alert>;
  if (!accountNum) return <ErrorState message={t('partyBalance.accountRequired')} />;
  if (balance.isPending) return <LoadingState />;
  if (balance.isError || !balance.data)
    return <ErrorState message={balance.error?.message} onRetry={() => void balance.refetch()} />;
  const relatedUnavailable =
    relatedAccounts.length > 0 && (!canReadRelated || related.some((query) => query.isError));
  const relatedPending = related.some((query) => query.isPending);
  const relatedBalance =
    relatedUnavailable || relatedPending
      ? null
      : related.reduce((sum, query) => sum + (query.data?.balanceMst ?? 0), 0);
  return (
    <OpenBalanceCurrencyPage
      kind={kind}
      balance={balance.data}
      relatedBalance={relatedBalance}
      relatedUnavailable={relatedUnavailable}
      refreshing={balance.isFetching || related.some((query) => query.isFetching)}
      onRefresh={() =>
        void queryClient.invalidateQueries({ queryKey: ['party-balance-currency', company] })
      }
      onOpenTransactions={kind === 'customer' ? () => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customerOpenTransactions(accountNum)) : undefined}
      onBack={() =>
        kind === 'customer'
          ? navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customer(String(balance.data.recId)))
          : navigate(-1)
      }
    />
  );
}

export function CustOpenBalanceCurrency(): React.ReactElement {
  return <PartyBalancePage kind="customer" />;
}
export function VendOpenBalanceCurrency(): React.ReactElement {
  return <PartyBalancePage kind="vendor" />;
}
