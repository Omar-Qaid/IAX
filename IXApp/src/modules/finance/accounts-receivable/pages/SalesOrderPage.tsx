import React, { useMemo } from 'react';
import { Stack, Typography } from '@mui/material';
import { useParams } from 'react-router-dom';
import { DocumentPage } from '@patterns/document/DocumentPage';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { MOCK_SALES_ORDERS, type SalesOrderLine } from '@mocks/data/salesOrders';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export function SalesOrderPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const { salesOrderId } = useParams<{ salesOrderId: string }>();
  const order = salesOrderId
    ? MOCK_SALES_ORDERS.find((candidate) => candidate.id === salesOrderId)
    : MOCK_SALES_ORDERS[0];
  const columns = useMemo<ColumnDef<SalesOrderLine>[]>(
    () => [
      { field: 'lineNumber', headerName: 'fields.line', width: 75, type: 'number' },
      { field: 'itemNumber', headerName: 'fields.item', width: 120 },
      { field: 'description', headerName: 'fields.description', minWidth: 260, flex: 1 },
      {
        field: 'quantity',
        headerName: 'fields.quantity',
        width: 100,
        type: 'number',
        align: 'right',
      },
      {
        field: 'unitPrice',
        headerName: 'fields.unitPrice',
        width: 120,
        type: 'number',
        align: 'right',
      },
      {
        field: 'lineTotal',
        headerName: 'fields.lineTotal',
        width: 130,
        type: 'number',
        align: 'right',
      },
    ],
    []
  );

  if (!order) return <ErrorState message={t('messages.noSalesOrders')} />;

  const formatAmount = (value: number) =>
    value.toLocaleString(currentLanguage.code === 'ar' ? 'ar-SA' : 'en-US');

  return (
    <DocumentPage
      title={t('pages.salesOrder.title', { number: order.salesOrderNumber })}
      subtitle={`${order.customerAccount} · ${order.customerName}`}
      statusBadge={order.status}
      headerContent={
        <Stack
          direction={{ xs: 'column', md: 'row' }}
          spacing={3}
          useFlexGap
          sx={{ flexWrap: 'wrap' }}
        >
          <Typography variant="body2">
            {t('fields.orderDate')}: {order.orderDate}
          </Typography>
          <Typography variant="body2">
            {t('fields.requestedDelivery')}: {order.requestedDeliveryDate}
          </Typography>
          <Typography variant="body2">
            {t('fields.paymentTerms')}: {order.paymentTerms}
          </Typography>
          <Typography variant="body2">
            {t('fields.deliveryMode')}: {order.deliveryMode}
          </Typography>
        </Stack>
      }
      linesContent={
        <DataGrid
          rows={order.lines}
          columns={columns}
          height={360}
          hideAddRowButton
          hideToolbar
          storageKey="accounts-receivable.sales-order-lines"
        />
      }
      totalsContent={
        <Stack spacing={0.75}>
          <Typography variant="body2">
            {t('fields.subtotal')}: {formatAmount(order.subtotal)} {order.currency}
          </Typography>
          <Typography variant="body2">
            {t('fields.discount')}: {formatAmount(order.discountTotal)} {order.currency}
          </Typography>
          <Typography variant="body2">
            {t('fields.tax')}: {formatAmount(order.taxTotal)} {order.currency}
          </Typography>
          <Typography variant="subtitle2">
            {t('fields.total')}: {formatAmount(order.orderTotal)} {order.currency}
          </Typography>
        </Stack>
      }
    />
  );
}
