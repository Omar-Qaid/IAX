import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { usePermission } from '@core/permissions/usePermission';
import { queryClient } from '@core/api/queryClient';
import { PERMISSIONS } from '@core/permissions/permissions';
import { DocumentChargesPage } from '@modules/finance/foundation/components/DocumentChargesPage';
import { salesOrderListApi } from '../api/salesOrderListApi';
import { salesOrderLinesApi } from '../api/salesOrderLinesApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';

export function SalesOrderChargesPage(): React.ReactElement {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { salesOrderId } = useParams<{ salesOrderId: string }>();
  const lineId = searchParams.get('lineId');
  const { hasPermission: canEdit } = usePermission(PERMISSIONS.SALES_ORDER_UPDATE);
  const ordersQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-orders'],
    queryFn: ({ signal }) => salesOrderListApi.list(signal),
  });
  const linesQuery = useQuery({
    queryKey: ['sales-order-lines', salesOrderId],
    queryFn: ({ signal }) => salesOrderLinesApi.list(salesOrderId!, signal),
    enabled: Boolean(salesOrderId && lineId),
  });
  if (ordersQuery.isLoading || (lineId && linesQuery.isLoading)) return <LoadingState />;
  const order = (ordersQuery.data ?? []).find((candidate) => candidate.id === salesOrderId);
  if (!order) return <ErrorState message="Sales order not found." />;
  const line = lineId ? (linesQuery.data ?? []).find((candidate) => candidate.id === lineId) : undefined;
  if (lineId && !line) return <ErrorState message="Sales order line not found." />;
  return (
    <DocumentChargesPage
      documentType="sales"
      level={line ? 'line' : 'header'}
      documentRecId={line ? Number(line.id) : order.recId}
      documentNumber={line ? `${order.salesId} / ${line.lineNumber}` : order.salesId}
      documentName={line ? `${line.itemNumber} - ${line.description}` : order.customerName}
      defaultCurrencyCode={line?.currencyCode || order.currencyCode}
      defaultTaxGroup={line?.taxGroup}
      defaultTaxItemGroup={line?.taxItemGroup}
      canEdit={canEdit && order.salesStatus.toLowerCase() === 'backorder'}
      onChanged={() => queryClient.invalidateQueries({ queryKey: ['sales-order-totals', order.id] })}
      onBack={() => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrder(order.id))}
    />
  );
}
