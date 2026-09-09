import React, { useMemo, useState } from 'react';
import {



  Box,
  Button,
  Stack,
  Tab,
  Tabs,
  Typography,
} from '@mui/material';
import { MasterDetailSection } from '@patterns/master-detail/MasterDetailSection';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { useNavigate, useParams } from 'react-router-dom';
import { MasterDetailPage } from '@patterns/master-detail/MasterDetailPage';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { MOCK_SALES_ORDERS, type SalesOrderLine } from '@mocks/data/salesOrders';
import { useAppTranslation } from '@core/localization/useAppTranslation';

type DetailLine = SalesOrderLine & { site?: string; warehouse?: string; deliveryDate?: string };

export function SalesOrderDetailsPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const navigate = useNavigate();
  const { salesOrderId } = useParams<{ salesOrderId: string }>();
  const [tab, setTab] = useState('lines');
  const [selectedLineId, setSelectedLineId] = useState<string>();
  const order = salesOrderId
    ? MOCK_SALES_ORDERS.find((candidate) => candidate.id === salesOrderId)
    : MOCK_SALES_ORDERS[0];
  const columns = useMemo<ColumnDef<DetailLine>[]>(
    () => [
      { field: 'lineNumber', headerName: 'fields.line', width: 65, type: 'number' },
      { field: 'itemNumber', headerName: 'fields.item', width: 130 },
      { field: 'description', headerName: 'fields.description', minWidth: 240, flex: 1 },
      { field: 'quantity', headerName: 'fields.quantity', width: 95, type: 'number' },
      { field: 'unit', headerName: 'fields.unit', width: 75 },
      { field: 'site', headerName: t('salesOrderQuickCreate.site', 'Site'), width: 100 },
      {
        field: 'warehouse',
        headerName: t('salesOrderQuickCreate.warehouse', 'Warehouse'),
        width: 120,
      },
      { field: 'unitPrice', headerName: 'fields.unitPrice', width: 110, type: 'number' },
      { field: 'deliveryDate', headerName: 'fields.requestedDelivery', width: 140, type: 'date' },
      { field: 'lineTotal', headerName: 'fields.lineTotal', width: 120, type: 'number' },
    ],
    [t]
  );
  if (!order) return <ErrorState message={t('messages.noSalesOrders')} />;
  const amount = (value: number) =>
    `${value.toLocaleString(currentLanguage.code)} ${order.currency}`;
  const field = (label: string, value?: string) => (
    <Box key={label}>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body2">{value || '-'}</Typography>
    </Box>
  );
  const header = (
    <Box
      sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: 2 }}
    >
      {field(t('fields.customerAccount'), order.customerAccount)}
      {field(t('fields.customerName'), order.customerName)}
      {field(t('fields.invoiceAccount'))}
      {field(t('fields.customerReference'), order.customerReference)}
      {field(t('fields.currency'), order.currency)}
      {field(t('fields.paymentTerms'), order.paymentTerms)}
      {field(t('fields.deliveryMode'), order.deliveryMode)}
      {field(t('customerQuickCreate.fields.deliveryTerms'))}
      {field(t('fields.orderDate'), order.orderDate)}
      {field(t('fields.requestedDelivery'), order.requestedDeliveryDate)}
    </Box>
  );
  const selectedLine: DetailLine | undefined = order.lines.find((line) => line.id === selectedLineId) ?? order.lines[0];
  const section = (title: string, content: React.ReactNode, expanded = true) => <MasterDetailSection key={`${tab}-${title}`} title={title} defaultExpanded={expanded}>{content}</MasterDetailSection>;  return (
    <MasterDetailPage
      records={MOCK_SALES_ORDERS.map((record) => ({
        id: record.id,
        title: record.salesOrderNumber,
        subtitle: record.customerAccount,
        description: record.customerName,
      }))}
      selectedId={order.id}
      onSelect={(id) => navigate(`/accounts-receivable/sales-orders/${encodeURIComponent(id)}`)}
      title={`${order.salesOrderNumber} : ${order.customerName}`}
      subtitle={t('pages.salesOrder.title', { number: order.salesOrderNumber })}
      status={<Typography variant="body2">{t(`status.${order.status}`, order.status)}</Typography>}
      filterLabel={t('actions.filter')}
      emptyLabel={t('messages.noSalesOrders')}
      actionPane={
        <>
          <ActionPaneBackButton label={t('actions.back')} onClick={() => navigate(-1)} />
          <EnterpriseCrudActions editLabel={t('actions.edit')} newLabel={t('actions.new')} deleteLabel={t('actions.delete')} canEdit={false} canNew={false} canDelete={false} />
          {[
            'Sales order',
            'Sell',
            'Manage',
            'Pick and pack',
            'Invoice',
            'General',
            'Warehouse',
            'Transportation',
          ].map((label) => (
            <Button key={label} disabled>
              {label}
            </Button>
          ))}
        </>
      }
    >
      <Tabs
        value={tab}
        onChange={(_, value: string) => setTab(value)}
        sx={{ mb: 1.5 }}
        aria-label={t('pages.salesOrders.title')}
      >
        <Tab
          value="lines"
          label={t('fields.lines', 'Lines')}
          id="sales-lines-tab"
          aria-controls="sales-detail-panel"
        />
        <Tab
          value="header"
          label={t('fields.header', 'Header')}
          id="sales-header-tab"
          aria-controls="sales-detail-panel"
        />
      </Tabs>
      <Box role="tabpanel" id="sales-detail-panel" aria-labelledby={`sales-${tab}-tab`}>
        {section(t('salesOrder.header', 'Sales order header'), header, tab === 'header')}
        {tab === 'lines' &&
          section(
            t('salesOrder.lines', 'Sales order lines'),
            <>
              <Stack direction="row" spacing={0.5} sx={{ mb: 1 }}>
                <Button disabled size="small">
                  {t('actions.addLine', 'Add line')}
                </Button>
                <Button disabled size="small">
                  {t('actions.delete', 'Delete')}
                </Button>
              </Stack>
              <DataGrid
                key={order.id}
                rows={order.lines}
                selectionMode="single"
                onRowClick={(line) => setSelectedLineId(line.id)}
                columns={columns}
                height={340}
                hideAddRowButton
                hideToolbar
                storageKey="accounts-receivable.sales-order-lines"
              />
            </>
          )}
        {section(
          t('fields.totals', 'Totals'),
          <Stack direction="row" spacing={4} useFlexGap sx={{ flexWrap: 'wrap' }}>
            {field(t('fields.subtotal'), amount(order.subtotal))}
            {field(t('fields.discount'), amount(order.discountTotal))}
            {field(t('fields.tax'), amount(order.taxTotal))}
            {field(t('fields.total'), amount(order.orderTotal))}
          </Stack>
        )}
      </Box>
    </MasterDetailPage>
  );
}
