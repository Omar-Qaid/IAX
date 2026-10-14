import React, { useMemo, useState } from 'react';
import { Box, Tab, Tabs } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { salesOrderListApi } from '../api/salesOrderListApi';
import {
  salesOrderPackingSlipApi,
  type PackingSlipJournalLine,
  type PackingSlipJournalSummary,
} from '../api/salesOrderPackingSlipApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';

type JournalTab = 'overview' | 'lines' | 'orderTracking';
type GridRow = {
  id: string;
  journal?: PackingSlipJournalSummary;
  line?: PackingSlipJournalLine;
};

const formatDate = (value?: string) => value ? new Date(value).toLocaleDateString() : '';
const yesNo = (value: boolean) => value ? 'Yes' : 'No';

const overviewColumns: ColumnDef<GridRow>[] = [
  { field: 'salesId', headerName: 'Sales order', width: 160, valueGetter: ({ row }) => row.journal?.salesId },
  { field: 'packingSlipId', headerName: 'Packing slip', width: 175, valueGetter: ({ row }) => row.journal?.packingSlipId },
  { field: 'version', headerName: 'Version', width: 190, valueGetter: ({ row }) => row.journal?.version },
  { field: 'deliveryDate', headerName: 'Ship date', width: 130, valueGetter: ({ row }) => formatDate(row.journal?.deliveryDate) },
  { field: 'invoiceDueDate', headerName: 'Invoice issue due date', width: 170, valueGetter: ({ row }) => formatDate(row.journal?.invoiceDueDate) },
  { field: 'terms', headerName: 'Terms', width: 120, valueGetter: ({ row }) => row.journal?.terms },
  { field: 'modeOfDelivery', headerName: 'Mode of delivery', width: 150, valueGetter: ({ row }) => row.journal?.modeOfDelivery },
  { field: 'blindShipment', headerName: 'Blind shipment', width: 130, valueGetter: ({ row }) => yesNo(Boolean(row.journal?.blindShipment)) },
  { field: 'intercompanyCompanyId', headerName: 'Intercompany company', width: 160, valueGetter: ({ row }) => row.journal?.intercompanyCompanyId },
  { field: 'intercompanyPosted', headerName: 'Intercompany posted', width: 150, valueGetter: ({ row }) => yesNo(Boolean(row.journal?.intercompanyPosted)) },
  { field: 'printed', headerName: 'Printed', width: 100, valueGetter: ({ row }) => yesNo(Boolean(row.journal?.printed)) },
  { field: 'ownerId', headerName: 'Owner (RecId)', width: 130, valueGetter: ({ row }) => row.journal?.ownerId },
  { field: 'contractorId', headerName: 'Contractor (RecId)', width: 145, valueGetter: ({ row }) => row.journal?.contractorId },
];

const lineColumns: ColumnDef<GridRow>[] = [
  { field: 'salesId', headerName: 'Sales order', width: 150, valueGetter: ({ row }) => row.line?.salesId },
  { field: 'originalSalesId', headerName: 'Original sales order', width: 170, valueGetter: ({ row }) => row.line?.originalSalesId },
  { field: 'itemId', headerName: 'Item number', width: 150, valueGetter: ({ row }) => row.line?.itemId },
  { field: 'description', headerName: 'Description', minWidth: 230, flex: 1, valueGetter: ({ row }) => row.line?.description },
  { field: 'siteId', headerName: 'Site', width: 115, valueGetter: ({ row }) => row.line?.siteId },
  { field: 'warehouseId', headerName: 'Warehouse', width: 140, valueGetter: ({ row }) => row.line?.warehouseId },
  { field: 'batchNumber', headerName: 'Batch number', width: 145, valueGetter: ({ row }) => row.line?.batchNumber },
  { field: 'serialNumber', headerName: 'Serial number', width: 145, valueGetter: ({ row }) => row.line?.serialNumber },
  { field: 'quantity', headerName: 'Quantity', width: 120, type: 'number', align: 'right', valueGetter: ({ row }) => row.line?.quantity },
  { field: 'inventQuantity', headerName: 'Inventory quantity', width: 145, type: 'number', align: 'right', valueGetter: ({ row }) => row.line?.inventQuantity },
  { field: 'salesUnit', headerName: 'Unit', width: 90, valueGetter: ({ row }) => row.line?.salesUnit },
  { field: 'amount', headerName: 'Amount', width: 130, type: 'number', align: 'right', valueGetter: ({ row }) => row.line?.amount },
];

const orderTrackingColumns: ColumnDef<GridRow>[] = [
  { field: 'salesId', headerName: 'Sales order', width: 160, valueGetter: ({ row }) => row.line?.salesId },
  { field: 'itemId', headerName: 'Item number', width: 150, valueGetter: ({ row }) => row.line?.itemId },
  { field: 'inventoryTransactionId', headerName: 'Inventory transaction', width: 190, valueGetter: ({ row }) => row.line?.inventoryTransactionId },
  { field: 'siteId', headerName: 'Site', width: 120, valueGetter: ({ row }) => row.line?.siteId },
  { field: 'warehouseId', headerName: 'Warehouse', width: 145, valueGetter: ({ row }) => row.line?.warehouseId },
  { field: 'batchNumber', headerName: 'Batch number', width: 150, valueGetter: ({ row }) => row.line?.batchNumber },
  { field: 'serialNumber', headerName: 'Serial number', width: 150, valueGetter: ({ row }) => row.line?.serialNumber },
  { field: 'requestedShipDate', headerName: 'Requested ship date', width: 170, valueGetter: ({ row }) => formatDate(row.line?.requestedShipDate) },
  { field: 'confirmedShipDate', headerName: 'Confirmed ship date', width: 170, valueGetter: ({ row }) => formatDate(row.line?.confirmedShipDate) },
  { field: 'fullyMatched', headerName: 'Fully matched', width: 125, valueGetter: ({ row }) => yesNo(Boolean(row.line?.fullyMatched)) },
  { field: 'scrap', headerName: 'Scrap', width: 95, valueGetter: ({ row }) => yesNo(Boolean(row.line?.scrap)) },
];

/** Read-only journal view of posted CustPackingSlipJour and CustPackingSlipTrans records. */
export function CustPackingSlipJournal(): React.ReactElement {
  const { salesOrderId } = useParams<{ salesOrderId: string }>();
  const navigate = useNavigate();
  const [tab, setTab] = useState<JournalTab>('overview');
  const [selectedId, setSelectedId] = useState<string>();
  const [selectedLineId, setSelectedLineId] = useState<string>();

  const ordersQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-orders'],
    queryFn: ({ signal }) => salesOrderListApi.list(signal),
  });
  const journalsQuery = useQuery({
    queryKey: ['sales-order-packing-slips', salesOrderId],
    queryFn: ({ signal }) => salesOrderPackingSlipApi.list(salesOrderId!, signal),
    enabled: Boolean(salesOrderId),
  });
  const order = ordersQuery.data?.find((row) => row.id === salesOrderId);
  const journals = journalsQuery.data ?? [];
  const activeId = journals.some((row) => row.id === selectedId) ? selectedId : journals[0]?.id;
  const detailQuery = useQuery({
    queryKey: ['sales-order-packing-slip', salesOrderId, activeId],
    queryFn: ({ signal }) => salesOrderPackingSlipApi.get(salesOrderId!, activeId!, signal),
    enabled: Boolean(salesOrderId && activeId && tab !== 'overview'),
  });
  const rows = useMemo<GridRow[]>(() => {
    if (tab === 'overview') return journals.map((journal) => ({ id: journal.id, journal }));
    const lines = detailQuery.data?.lines ?? [];
    return lines.map((line) => ({ id: line.id, line }));
  }, [detailQuery.data?.lines, journals, tab]);

  if (!salesOrderId) return <ErrorState message="Sales order is missing." />;
  if (ordersQuery.isLoading || journalsQuery.isLoading) return <LoadingState />;
  if (ordersQuery.error || journalsQuery.error)
    return <ErrorState message={(ordersQuery.error || journalsQuery.error)?.message ?? 'Unable to load packing slips.'} />;
  if (!order) return <ErrorState message="Sales order not found." />;

  const context = `Packing slip journal | ${order.salesId} : ${order.customerName}`;
  const columns = tab === 'overview'
    ? overviewColumns
    : tab === 'lines' ? lineColumns : orderTrackingColumns;
  const tabValue = tab === 'overview' ? 'Overview' : tab === 'lines' ? 'Lines' : 'Order tracking';

  return (
    <SimpleListPage<GridRow>
      key={tab}
      title="Packing slip journal"
      recordTableName={tab === 'overview' ? 'CustPackingSlipJour' : 'CustPackingSlipTrans'}
      dataSource={{
        type: 'controlled',
        rows,
        loading: tab !== 'overview' && detailQuery.isLoading,
        error: tab !== 'overview' ? detailQuery.error?.message : undefined,
        refresh: () => {
          void journalsQuery.refetch();
          if (tab !== 'overview') void detailQuery.refetch();
        },
      }}
      columns={columns}
      filterBar={
        <Box sx={{ mx: { xs: 1, sm: 2.5 } }}>
          <Tabs value={tab} onChange={(_, value: JournalTab) => setTab(value)} aria-label="Packing slip journal views">
            <Tab value="overview" label="Overview" />
            <Tab value="lines" label="Lines" disabled={!activeId} />
            <Tab value="orderTracking" label="Order tracking" disabled={!activeId} />
          </Tabs>
        </Box>
      }
      enterpriseConfig={{
        readOnly: true,
        contextLabel: context,
        viewLabel: 'Standard view',
        filterLabel: 'Filter',
        informationLabel: 'Information',
        searchFields: columns.map((column) => ({
          field: String(column.field) as keyof GridRow & string,
          label: column.headerName,
        })),
        crud: { editLabel: '', newLabel: '', deleteLabel: '' },
        utilities: {
          personalizeLabel: 'Personalize',
          guideLabel: 'Guide',
          notificationsLabel: 'Notifications',
          refreshLabel: 'Refresh',
          openWindowLabel: 'Open window',
        },
        backCommand: {
          label: 'Back',
          onClick: () => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrder(order.id)),
        },
        initialSelection: 'first',
      }}
      dataGridProps={{
        selectedIds: tab === 'overview'
          ? activeId ? [activeId] : []
          : selectedLineId ? [selectedLineId] : [],
        onSelectionChange: (ids) => tab === 'overview'
          ? setSelectedId(String(ids.at(-1) ?? ''))
          : setSelectedLineId(String(ids.at(-1) ?? '')),
        onRowClick: (row) => tab === 'overview'
          ? setSelectedId(row.id)
          : setSelectedLineId(row.id),
        onRowDoubleClick: (row) => {
          if (tab === 'overview') {
            setSelectedId(row.id);
            setTab('lines');
          }
        },
        storageKey: tab === 'overview'
          ? 'cust-packing-slip-journal-overview'
          : tab === 'lines' ? 'cust-packing-slip-journal-lines' : 'cust-packing-slip-journal-tracking',
        hideFooter: true,
      }}
    />
  );
}
