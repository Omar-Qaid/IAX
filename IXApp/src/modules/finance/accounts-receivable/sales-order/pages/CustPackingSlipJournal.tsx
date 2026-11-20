import React, { useMemo, useState } from 'react';
import { Box, Tab, Tabs } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { salesOrderListApi } from '../api/salesOrderListApi';
import {
  salesOrderPackingSlipApi,
  type PackingSlipJournalLine,
  type PackingSlipJournalSummary,
} from '../api/salesOrderPackingSlipApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../../routes/accountsReceivableRoutePaths';
import { salesOrderQueryKeys } from '../../queries/accountsReceivableQueryKeys';

type JournalTab = 'overview' | 'lines' | 'orderTracking';
type GridRow = {
  id: string;
  journal?: PackingSlipJournalSummary;
  line?: PackingSlipJournalLine;
};

const formatDate = (value?: string) => (value ? new Date(value).toLocaleDateString() : '');
type Translate = (key: string) => string;

const yesNo = (t: Translate, value: boolean) => (value ? t('common.yes') : t('common.no'));

const buildOverviewColumns = (t: Translate): ColumnDef<GridRow>[] => [
  {
    field: 'salesId',
    headerName: 'custPackingSlipJournal.columns.salesId',
    width: 160,
    valueGetter: ({ row }) => row.journal?.salesId,
  },
  {
    field: 'packingSlipId',
    headerName: 'custPackingSlipJournal.columns.packingSlipId',
    width: 175,
    valueGetter: ({ row }) => row.journal?.packingSlipId,
  },
  {
    field: 'version',
    headerName: 'custPackingSlipJournal.columns.version',
    width: 190,
    valueGetter: ({ row }) => row.journal?.version,
  },
  {
    field: 'deliveryDate',
    headerName: 'custPackingSlipJournal.columns.deliveryDate',
    width: 130,
    valueGetter: ({ row }) => formatDate(row.journal?.deliveryDate),
  },
  {
    field: 'invoiceDueDate',
    headerName: 'custPackingSlipJournal.columns.invoiceDueDate',
    width: 170,
    valueGetter: ({ row }) => formatDate(row.journal?.invoiceDueDate),
  },
  {
    field: 'terms',
    headerName: 'custPackingSlipJournal.columns.terms',
    width: 120,
    valueGetter: ({ row }) => row.journal?.terms,
  },
  {
    field: 'modeOfDelivery',
    headerName: 'custPackingSlipJournal.columns.modeOfDelivery',
    width: 150,
    valueGetter: ({ row }) => row.journal?.modeOfDelivery,
  },
  {
    field: 'blindShipment',
    headerName: 'custPackingSlipJournal.columns.blindShipment',
    width: 130,
    valueGetter: ({ row }) => yesNo(t, Boolean(row.journal?.blindShipment)),
  },
  {
    field: 'intercompanyCompanyId',
    headerName: 'custPackingSlipJournal.columns.intercompanyCompanyId',
    width: 160,
    valueGetter: ({ row }) => row.journal?.intercompanyCompanyId,
  },
  {
    field: 'intercompanyPosted',
    headerName: 'custPackingSlipJournal.columns.intercompanyPosted',
    width: 150,
    valueGetter: ({ row }) => yesNo(t, Boolean(row.journal?.intercompanyPosted)),
  },
  {
    field: 'printed',
    headerName: 'custPackingSlipJournal.columns.printed',
    width: 100,
    valueGetter: ({ row }) => yesNo(t, Boolean(row.journal?.printed)),
  },
  {
    field: 'ownerId',
    headerName: 'custPackingSlipJournal.columns.ownerId',
    width: 130,
    valueGetter: ({ row }) => row.journal?.ownerId,
  },
  {
    field: 'contractorId',
    headerName: 'custPackingSlipJournal.columns.contractorId',
    width: 145,
    valueGetter: ({ row }) => row.journal?.contractorId,
  },
];

const buildLineColumns = (): ColumnDef<GridRow>[] => [
  {
    field: 'salesId',
    headerName: 'custPackingSlipJournal.columns.salesId',
    width: 150,
    valueGetter: ({ row }) => row.line?.salesId,
  },
  {
    field: 'originalSalesId',
    headerName: 'custPackingSlipJournal.columns.originalSalesId',
    width: 170,
    valueGetter: ({ row }) => row.line?.originalSalesId,
  },
  {
    field: 'itemId',
    headerName: 'custPackingSlipJournal.columns.itemId',
    width: 150,
    valueGetter: ({ row }) => row.line?.itemId,
  },
  {
    field: 'description',
    headerName: 'custPackingSlipJournal.columns.description',
    minWidth: 230,
    flex: 1,
    valueGetter: ({ row }) => row.line?.description,
  },
  {
    field: 'siteId',
    headerName: 'custPackingSlipJournal.columns.siteId',
    width: 115,
    valueGetter: ({ row }) => row.line?.siteId,
  },
  {
    field: 'warehouseId',
    headerName: 'custPackingSlipJournal.columns.warehouseId',
    width: 140,
    valueGetter: ({ row }) => row.line?.warehouseId,
  },
  {
    field: 'batchNumber',
    headerName: 'custPackingSlipJournal.columns.batchNumber',
    width: 145,
    valueGetter: ({ row }) => row.line?.batchNumber,
  },
  {
    field: 'serialNumber',
    headerName: 'custPackingSlipJournal.columns.serialNumber',
    width: 145,
    valueGetter: ({ row }) => row.line?.serialNumber,
  },
  {
    field: 'quantity',
    headerName: 'custPackingSlipJournal.columns.quantity',
    width: 120,
    type: 'number',
    align: 'right',
    valueGetter: ({ row }) => row.line?.quantity,
  },
  {
    field: 'inventQuantity',
    headerName: 'custPackingSlipJournal.columns.inventQuantity',
    width: 145,
    type: 'number',
    align: 'right',
    valueGetter: ({ row }) => row.line?.inventQuantity,
  },
  {
    field: 'salesUnit',
    headerName: 'custPackingSlipJournal.columns.salesUnit',
    width: 90,
    valueGetter: ({ row }) => row.line?.salesUnit,
  },
  {
    field: 'amount',
    headerName: 'custPackingSlipJournal.columns.amount',
    width: 130,
    type: 'number',
    align: 'right',
    valueGetter: ({ row }) => row.line?.amount,
  },
];

const buildOrderTrackingColumns = (t: Translate): ColumnDef<GridRow>[] => [
  {
    field: 'salesId',
    headerName: 'custPackingSlipJournal.columns.salesId',
    width: 160,
    valueGetter: ({ row }) => row.line?.salesId,
  },
  {
    field: 'itemId',
    headerName: 'custPackingSlipJournal.columns.itemId',
    width: 150,
    valueGetter: ({ row }) => row.line?.itemId,
  },
  {
    field: 'inventoryTransactionId',
    headerName: 'custPackingSlipJournal.columns.inventoryTransactionId',
    width: 190,
    valueGetter: ({ row }) => row.line?.inventoryTransactionId,
  },
  {
    field: 'siteId',
    headerName: 'custPackingSlipJournal.columns.siteId',
    width: 120,
    valueGetter: ({ row }) => row.line?.siteId,
  },
  {
    field: 'warehouseId',
    headerName: 'custPackingSlipJournal.columns.warehouseId',
    width: 145,
    valueGetter: ({ row }) => row.line?.warehouseId,
  },
  {
    field: 'batchNumber',
    headerName: 'custPackingSlipJournal.columns.batchNumber',
    width: 150,
    valueGetter: ({ row }) => row.line?.batchNumber,
  },
  {
    field: 'serialNumber',
    headerName: 'custPackingSlipJournal.columns.serialNumber',
    width: 150,
    valueGetter: ({ row }) => row.line?.serialNumber,
  },
  {
    field: 'requestedShipDate',
    headerName: 'custPackingSlipJournal.columns.requestedShipDate',
    width: 170,
    valueGetter: ({ row }) => formatDate(row.line?.requestedShipDate),
  },
  {
    field: 'confirmedShipDate',
    headerName: 'custPackingSlipJournal.columns.confirmedShipDate',
    width: 170,
    valueGetter: ({ row }) => formatDate(row.line?.confirmedShipDate),
  },
  {
    field: 'fullyMatched',
    headerName: 'custPackingSlipJournal.columns.fullyMatched',
    width: 125,
    valueGetter: ({ row }) => yesNo(t, Boolean(row.line?.fullyMatched)),
  },
  {
    field: 'scrap',
    headerName: 'custPackingSlipJournal.columns.scrap',
    width: 95,
    valueGetter: ({ row }) => yesNo(t, Boolean(row.line?.scrap)),
  },
];

/** Read-only journal view of posted CustPackingSlipJour and CustPackingSlipTrans records. */
export function CustPackingSlipJournal(): React.ReactElement {
  const { salesOrderId, accountNum } = useParams<{ salesOrderId?: string; accountNum?: string }>();
  const navigate = useNavigate();
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const [tab, setTab] = useState<JournalTab>('overview');
  const [selectedId, setSelectedId] = useState<string>();
  const [selectedLineId, setSelectedLineId] = useState<string>();

  const ordersQuery = useQuery({
    queryKey: salesOrderQueryKeys.list(company),
    queryFn: ({ signal }) => salesOrderListApi.list(signal),
  });
  const selectedOrders = (ordersQuery.data ?? []).filter((row) =>
    salesOrderId ? row.id === salesOrderId : row.customerAccount === accountNum
  );
  const journalsQuery = useQuery({
    queryKey: [
      'sales-order-packing-slips',
      company,
      salesOrderId ?? accountNum,
      selectedOrders.map((row) => row.id),
    ],
    queryFn: async ({ signal }) => {
      const result: (PackingSlipJournalSummary & { orderRecId: string })[] = [];
      // Bound concurrent calls while reusing the existing order-scoped journal endpoint.
      for (let start = 0; start < selectedOrders.length; start += 5) {
        const batch = await Promise.all(
          selectedOrders.slice(start, start + 5).map(async (order) =>
            (await salesOrderPackingSlipApi.list(order.id, signal)).map((journal) => ({
              ...journal,
              orderRecId: order.id,
            }))
          )
        );
        result.push(...batch.flat());
      }
      return result;
    },
    enabled: Boolean(salesOrderId || accountNum) && ordersQuery.isSuccess,
  });
  const journals = useMemo(() => journalsQuery.data ?? [], [journalsQuery.data]);
  const activeId = journals.some((row) => row.id === selectedId) ? selectedId : journals[0]?.id;
  const activeOrderId = journals.find((row) => row.id === activeId)?.orderRecId ?? salesOrderId;
  const order = ordersQuery.data?.find((row) => row.id === activeOrderId);
  const detailQuery = useQuery({
    queryKey: salesOrderQueryKeys.packingSlip(company, activeOrderId, activeId),
    queryFn: ({ signal }) => salesOrderPackingSlipApi.get(activeOrderId!, activeId!, signal),
    enabled: Boolean(activeOrderId && activeId && tab !== 'overview'),
  });
  const rows = useMemo<GridRow[]>(() => {
    if (tab === 'overview') return journals.map((journal) => ({ id: journal.id, journal }));
    const lines = detailQuery.data?.lines ?? [];
    return lines.map((line) => ({ id: line.id, line }));
  }, [detailQuery.data?.lines, journals, tab]);
  const columns = useMemo(
    () =>
      tab === 'overview'
        ? buildOverviewColumns(t)
        : tab === 'lines'
          ? buildLineColumns()
          : buildOrderTrackingColumns(t),
    [tab, t]
  );

  if (!salesOrderId && !accountNum)
    return <ErrorState message={t('custPackingSlipJournal.errors.orderMissing')} />;
  if (ordersQuery.isLoading || journalsQuery.isLoading) return <LoadingState />;
  if (ordersQuery.error || journalsQuery.error)
    return (
      <ErrorState
        message={
          (ordersQuery.error || journalsQuery.error)?.message ??
          t('custPackingSlipJournal.errors.loadFailed')
        }
      />
    );
  if (!accountNum && !order)
    return <ErrorState message={t('custPackingSlipJournal.errors.orderNotFound')} />;

  const context = accountNum
    ? t('custPackingSlipJournal.contextAccount', { accountNum })
    : t('custPackingSlipJournal.context', {
        salesId: order!.salesId,
        customerName: order!.customerName,
      });

  return (
    <SimpleListPage<GridRow>
      key={tab}
      title={t('custPackingSlipJournal.title')}
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
          <Tabs
            value={tab}
            onChange={(_, value: JournalTab) => setTab(value)}
            aria-label={t('custPackingSlipJournal.views')}
          >
            <Tab value="overview" label={t('custPackingSlipJournal.tabs.overview')} />
            <Tab
              value="lines"
              label={t('custPackingSlipJournal.tabs.lines')}
              disabled={!activeId}
            />
            <Tab
              value="orderTracking"
              label={t('custPackingSlipJournal.tabs.orderTracking')}
              disabled={!activeId}
            />
          </Tabs>
        </Box>
      }
      enterpriseConfig={{
        readOnly: true,
        contextLabel: context,
        backCommand: {
          onClick: () =>
            accountNum
              ? navigate(-1)
              : navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrder(order!.id)),
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
            ? 'accounts-receivable.packing-slip-journal.overview'
            : tab === 'lines'
              ? 'accounts-receivable.packing-slip-journal.lines'
              : 'accounts-receivable.packing-slip-journal.order-tracking',
        hideFooter: true,
      }}
    />
  );
}
