import React, { useMemo, useState } from 'react';
import { Box, Tab, Tabs } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { salesOrderListApi } from '../../sales-order/api/salesOrderListApi';
import {
  salesOrderConfirmationApi,
  type SalesOrderConfirmationLine,
  type SalesOrderConfirmationSummary,
} from '../api/salesOrderConfirmationApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../../routes/accountsReceivableRoutePaths';
import { salesOrderQueryKeys } from '../../queries/accountsReceivableQueryKeys';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';

type GridRow = {
  id: string;
  salesOrderId: string;
  journal?: SalesOrderConfirmationSummary;
  line?: SalesOrderConfirmationLine;
};
const amount = (value: number | undefined) =>
  (value ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const date = (value: string) => (value ? new Date(value).toLocaleDateString() : '');
const overviewColumns: ColumnDef<GridRow>[] = [
  {
    field: 'salesId',
    headerName: 'custConfirmJournal.columns.salesId',
    width: 180,
    valueGetter: ({ row }) => row.journal?.salesId,
  },
  {
    field: 'confirmDate',
    headerName: 'custConfirmJournal.columns.confirmDate',
    width: 140,
    valueGetter: ({ row }) => date(row.journal?.confirmDate ?? ''),
  },
  {
    field: 'confirmId',
    headerName: 'custConfirmJournal.columns.confirmId',
    width: 180,
    valueGetter: ({ row }) => row.journal?.confirmId,
  },
  {
    field: 'currencyCode',
    headerName: 'custConfirmJournal.columns.currencyCode',
    width: 120,
    valueGetter: ({ row }) => row.journal?.currencyCode,
  },
  {
    field: 'confirmAmount',
    headerName: 'custConfirmJournal.columns.confirmAmount',
    width: 150,
    align: 'right',
    valueGetter: ({ row }) => row.journal?.confirmAmount,
    renderCell: ({ row }) => amount(row.journal?.confirmAmount),
  },
];
const lineColumns: ColumnDef<GridRow>[] = [
  {
    field: 'salesId',
    headerName: 'custConfirmJournal.columns.salesId',
    width: 170,
    valueGetter: ({ row }) => row.salesOrderId,
  },
  {
    field: 'itemId',
    headerName: 'custConfirmJournal.columns.itemId',
    width: 150,
    valueGetter: ({ row }) => row.line?.itemId,
  },
  {
    field: 'salesCategory',
    headerName: 'custConfirmJournal.columns.salesCategory',
    width: 150,
    valueGetter: ({ row }) => row.line?.salesCategory || '',
  },
  {
    field: 'name',
    headerName: 'custConfirmJournal.columns.name',
    minWidth: 220,
    flex: 1,
    valueGetter: ({ row }) => row.line?.name,
  },
  {
    field: 'inventLocationId',
    headerName: 'custConfirmJournal.columns.inventLocationId',
    width: 150,
    valueGetter: ({ row }) => row.line?.inventLocationId,
  },
  {
    field: 'wmsLocationId',
    headerName: 'custConfirmJournal.columns.wmsLocationId',
    width: 130,
    valueGetter: ({ row }) => row.line?.wmsLocationId,
  },
  {
    field: 'pdsCwQty',
    headerName: 'custConfirmJournal.columns.pdsCwQty',
    width: 130,
    align: 'right',
    renderCell: ({ row }) => amount(row.line?.pdsCwQty),
  },
  {
    field: 'qty',
    headerName: 'custConfirmJournal.columns.qty',
    width: 120,
    align: 'right',
    renderCell: ({ row }) => amount(row.line?.qty),
  },
  {
    field: 'salesPrice',
    headerName: 'custConfirmJournal.columns.salesPrice',
    width: 120,
    align: 'right',
    renderCell: ({ row }) => amount(row.line?.salesPrice),
  },
  {
    field: 'discPercent',
    headerName: 'custConfirmJournal.columns.discPercent',
    width: 150,
    align: 'right',
    renderCell: ({ row }) => amount(row.line?.discPercent || row.line?.linePercent),
  },
  {
    field: 'lineDisc',
    headerName: 'custConfirmJournal.columns.lineDisc',
    width: 120,
    align: 'right',
    renderCell: ({ row }) => amount(row.line?.lineDisc),
  },
  {
    field: 'lineAmount',
    headerName: 'custConfirmJournal.columns.lineAmount',
    width: 130,
    align: 'right',
    renderCell: ({ row }) => amount(row.line?.lineAmount),
  },
];

/** Read-only view of posted CustConfirmJour and CustConfirmTrans snapshots. */
export function CustConfirmJournal(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  const { t } = useAppTranslation();
  const { salesOrderId } = useParams<{ salesOrderId: string }>();
  const navigate = useNavigate();
  const [tab, setTab] = useState<'overview' | 'lines'>('overview');
  const [selectedId, setSelectedId] = useState<string>();
  const [selectedLineId, setSelectedLineId] = useState<string>();
  const ordersQuery = useQuery({
    queryKey: salesOrderQueryKeys.list(company),
    queryFn: ({ signal }) => salesOrderListApi.list(signal),
  });
  const journalsQuery = useQuery({
    queryKey: salesOrderQueryKeys.confirmations(company, salesOrderId),
    queryFn: ({ signal }) => salesOrderConfirmationApi.list(salesOrderId!, signal),
    enabled: Boolean(salesOrderId),
  });
  const order = ordersQuery.data?.find((row) => row.id === salesOrderId);
  const journals = useMemo(() => journalsQuery.data ?? [], [journalsQuery.data]);
  const activeId = journals.some((row) => row.id === selectedId) ? selectedId : journals[0]?.id;
  const detailQuery = useQuery({
    queryKey: salesOrderQueryKeys.confirmation(company, salesOrderId, activeId),
    queryFn: ({ signal }) => salesOrderConfirmationApi.get(salesOrderId!, activeId!, signal),
    enabled: Boolean(salesOrderId && activeId && tab === 'lines'),
  });
  const rows = useMemo<GridRow[]>(
    () =>
      tab === 'overview'
        ? journals.map((journal) => ({ id: journal.id, salesOrderId: journal.salesId, journal }))
        : (detailQuery.data?.lines ?? []).map((line) => ({
            id: line.id,
            salesOrderId: order?.salesId ?? '',
            line,
          })),
    [tab, journals, detailQuery.data?.lines, order?.salesId]
  );
  if (!salesOrderId) return <ErrorState message={t('custConfirmJournal.errors.orderMissing')} />;
  if (ordersQuery.isLoading || journalsQuery.isLoading) return <LoadingState />;
  if (ordersQuery.error || journalsQuery.error)
    return (
      <ErrorState
        message={
          (ordersQuery.error || journalsQuery.error)?.message ??
          t('custConfirmJournal.errors.loadFailed')
        }
      />
    );
  if (!order) return <ErrorState message={t('custConfirmJournal.errors.orderNotFound')} />;
  const context = t('custConfirmJournal.context', {
    salesId: order.salesId,
    customerName: order.customerName,
  });
  return (
    <SimpleListPage<GridRow>
      key={tab}
      title={t('custConfirmJournal.title')}
      recordTableName={tab === 'overview' ? 'CustConfirmJour' : 'CustConfirmTrans'}
      dataSource={{
        type: 'controlled',
        rows,
        loading: tab === 'lines' && detailQuery.isLoading,
        error: tab === 'lines' ? detailQuery.error?.message : undefined,
        refresh: () => {
          void journalsQuery.refetch();
          if (tab === 'lines') void detailQuery.refetch();
        },
      }}
      columns={tab === 'overview' ? overviewColumns : lineColumns}
      filterBar={
        <Box sx={{ mx: { xs: 1, sm: 2.5 } }}>
          <Tabs
            value={tab}
            onChange={(_, value: 'overview' | 'lines') => setTab(value)}
            aria-label={t('custConfirmJournal.views')}
          >
            <Tab value="overview" label={t('custConfirmJournal.tabs.overview')} />
            <Tab value="lines" label={t('custConfirmJournal.tabs.lines')} disabled={!activeId} />
          </Tabs>
        </Box>
      }
      enterpriseConfig={{
        readOnly: true,
        contextLabel: context,
        backCommand: {
          onClick: () => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrder(order.id)),
        },
        initialSelection: 'first',
      }}
      dataGridProps={{
        selectedIds:
          tab === 'overview'
            ? activeId
              ? [activeId]
              : []
            : selectedLineId
              ? [selectedLineId]
              : [],
        onSelectionChange: (ids) =>
          tab === 'overview'
            ? setSelectedId(String(ids.at(-1) ?? ''))
            : setSelectedLineId(String(ids.at(-1) ?? '')),
        onRowClick: (row) =>
          tab === 'overview' ? setSelectedId(row.id) : setSelectedLineId(row.id),
        onRowDoubleClick: (row) => {
          if (tab === 'overview') {
            setSelectedId(row.id);
            setTab('lines');
          }
        },
        storageKey:
          tab === 'overview'
            ? 'accounts-receivable.sales-order-confirmations.overview'
            : 'accounts-receivable.sales-order-confirmations.lines',
        hideFooter: true,
      }}
    />
  );
}
