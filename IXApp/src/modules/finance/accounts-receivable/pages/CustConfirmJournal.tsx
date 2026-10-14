import React, { useMemo, useState } from 'react';
import { Box, Tab, Tabs } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { salesOrderListApi } from '../api/salesOrderListApi';
import { salesOrderConfirmationApi, type SalesOrderConfirmationLine, type SalesOrderConfirmationSummary } from '../api/salesOrderConfirmationApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';

type GridRow = { id: string; journal?: SalesOrderConfirmationSummary; line?: SalesOrderConfirmationLine };
const amount = (value: number | undefined) => (value ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const date = (value: string) => value ? new Date(value).toLocaleDateString() : '';
const overviewColumns: ColumnDef<GridRow>[] = [
  { field: 'salesId', headerName: 'Sales order', width: 180, valueGetter: ({ row }) => row.journal?.salesId },
  { field: 'confirmDate', headerName: 'Date', width: 140, valueGetter: ({ row }) => date(row.journal?.confirmDate ?? '') },
  { field: 'confirmId', headerName: 'Confirmation', width: 180, valueGetter: ({ row }) => row.journal?.confirmId },
  { field: 'currencyCode', headerName: 'Currency', width: 120, valueGetter: ({ row }) => row.journal?.currencyCode },
  { field: 'confirmAmount', headerName: 'Amount', width: 150, align: 'right', valueGetter: ({ row }) => row.journal?.confirmAmount, renderCell: ({ row }) => amount(row.journal?.confirmAmount) },
];
const lineColumns: ColumnDef<GridRow>[] = [
  { field: 'salesId', headerName: 'Sales order', width: 170, valueGetter: ({ row }) => row.line?.salesId },
  { field: 'itemId', headerName: 'Item', width: 150, valueGetter: ({ row }) => row.line?.itemId },
  { field: 'salesCategory', headerName: 'Sales category', width: 150, valueGetter: ({ row }) => row.line?.salesCategory || '' },
  { field: 'name', headerName: 'Description', minWidth: 220, flex: 1, valueGetter: ({ row }) => row.line?.name },
  { field: 'inventLocationId', headerName: 'Warehouse', width: 150, valueGetter: ({ row }) => row.line?.inventLocationId },
  { field: 'wmsLocationId', headerName: 'Location', width: 130, valueGetter: ({ row }) => row.line?.wmsLocationId },
  { field: 'pdsCwQty', headerName: 'CW quantity', width: 130, align: 'right', renderCell: ({ row }) => amount(row.line?.pdsCwQty) },
  { field: 'qty', headerName: 'Quantity', width: 120, align: 'right', renderCell: ({ row }) => amount(row.line?.qty) },
  { field: 'salesPrice', headerName: 'Unit price', width: 120, align: 'right', renderCell: ({ row }) => amount(row.line?.salesPrice) },
  { field: 'discPercent', headerName: 'Discount percent', width: 150, align: 'right', renderCell: ({ row }) => amount(row.line?.discPercent || row.line?.linePercent) },
  { field: 'lineDisc', headerName: 'Discount', width: 120, align: 'right', renderCell: ({ row }) => amount(row.line?.lineDisc) },
  { field: 'lineAmount', headerName: 'Amount', width: 130, align: 'right', renderCell: ({ row }) => amount(row.line?.lineAmount) },
];

/** Read-only view of posted CustConfirmJour and CustConfirmTrans snapshots. */
export function CustConfirmJournal(): React.ReactElement {
  const { salesOrderId } = useParams<{ salesOrderId: string }>();
  const navigate = useNavigate();
  const [tab, setTab] = useState<'overview' | 'lines'>('overview');
  const [selectedId, setSelectedId] = useState<string>();
  const [selectedLineId, setSelectedLineId] = useState<string>();
  const ordersQuery = useQuery({ queryKey: ['accounts-receivable', 'sales-orders'], queryFn: ({ signal }) => salesOrderListApi.list(signal) });
  const journalsQuery = useQuery({ queryKey: ['sales-order-confirmations', salesOrderId], queryFn: ({ signal }) => salesOrderConfirmationApi.list(salesOrderId!, signal), enabled: Boolean(salesOrderId) });
  const order = ordersQuery.data?.find((row) => row.id === salesOrderId);
  const journals = journalsQuery.data ?? [];
  const activeId = journals.some((row) => row.id === selectedId) ? selectedId : journals[0]?.id;
  const detailQuery = useQuery({ queryKey: ['sales-order-confirmation', salesOrderId, activeId], queryFn: ({ signal }) => salesOrderConfirmationApi.get(salesOrderId!, activeId!, signal), enabled: Boolean(salesOrderId && activeId && tab === 'lines') });
  const rows = useMemo<GridRow[]>(() => tab === 'overview' ? journals.map((journal) => ({ id: journal.id, journal })) : (detailQuery.data?.lines ?? []).map((line) => ({ id: line.id, line })), [tab, journals, detailQuery.data?.lines]);
  if (!salesOrderId) return <ErrorState message="Sales order is missing." />;
  if (ordersQuery.isLoading || journalsQuery.isLoading) return <LoadingState />;
  if (ordersQuery.error || journalsQuery.error) return <ErrorState message={(ordersQuery.error || journalsQuery.error)?.message ?? 'Unable to load confirmations.'} />;
  if (!order) return <ErrorState message="Sales order not found." />;
  const context = 'Sales order confirmations | ' + order.salesId + ' : ' + order.customerName;
  return <SimpleListPage<GridRow>
    key={tab}
    title="Sales order confirmations"
    recordTableName={tab === 'overview' ? 'CustConfirmJour' : 'CustConfirmTrans'}
    dataSource={{ type: 'controlled', rows, loading: tab === 'lines' && detailQuery.isLoading, error: tab === 'lines' ? detailQuery.error?.message : undefined, refresh: () => { void journalsQuery.refetch(); if (tab === 'lines') void detailQuery.refetch(); } }}
    columns={tab === 'overview' ? overviewColumns : lineColumns}
    filterBar={<Box sx={{ mx: { xs: 1, sm: 2.5 } }}><Tabs value={tab} onChange={(_, value: 'overview' | 'lines') => setTab(value)} aria-label="Confirmation views"><Tab value="overview" label="Overview" /><Tab value="lines" label="Lines" disabled={!activeId} /></Tabs></Box>}
    enterpriseConfig={{ readOnly: true, contextLabel: context, viewLabel: 'Standard view', filterLabel: 'Filter', informationLabel: 'Information', searchFields: (tab === 'overview' ? overviewColumns : lineColumns).map((column) => ({ field: String(column.field) as keyof GridRow & string, label: column.headerName })), crud: { editLabel: '', newLabel: '', deleteLabel: '' }, utilities: { personalizeLabel: 'Personalize', guideLabel: 'Guide', notificationsLabel: 'Notifications', refreshLabel: 'Refresh', openWindowLabel: 'Open window' }, backCommand: { label: 'Back', onClick: () => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrder(order.id)) }, initialSelection: 'first' }}
    dataGridProps={{ selectedIds: tab === 'overview' ? activeId ? [activeId] : [] : selectedLineId ? [selectedLineId] : [], onSelectionChange: (ids) => tab === 'overview' ? setSelectedId(String(ids.at(-1) ?? '')) : setSelectedLineId(String(ids.at(-1) ?? '')), onRowClick: (row) => tab === 'overview' ? setSelectedId(row.id) : setSelectedLineId(row.id), onRowDoubleClick: (row) => { if (tab === 'overview') { setSelectedId(row.id); setTab('lines'); } }, storageKey: tab === 'overview' ? 'cust-confirm-jour-overview' : 'cust-confirm-trans-lines', hideFooter: true }}
  />;
}
