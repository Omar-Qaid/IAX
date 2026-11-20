import React, { useEffect, useMemo, useState } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  Button,
  FormControlLabel,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import {
  salesOrderConfirmationApi,
  type SalesOrderConfirmationLine,
} from '../api/salesOrderConfirmationApi';
import { salesOrderQueryKeys } from '../../queries/accountsReceivableQueryKeys';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';

interface Props {
  open: boolean;
  mode: 'confirmation' | 'proForma';
  orderId: string;
  salesId: string;
  customerName: string;
  canPost: boolean;
  onClose: () => void;
  onPosted: () => Promise<void>;
}

interface OverviewRow {
  id: string;
  update: string;
  salesId: string;
  customerName: string;
  currencyCode: string;
  quantity: number;
  netAmount: number;
  taxAmount: number;
  totalAmount: number;
}

const money = (value: number, currency: string) =>
  `${value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ${currency}`;

const overviewColumns: ColumnDef<OverviewRow>[] = [
  { field: 'update', headerName: 'salesOrder.ui.update', width: 110 },
  { field: 'salesId', headerName: 'salesOrder.ui.salesOrder', width: 150 },
  { field: 'customerName', headerName: 'salesOrder.ui.name', minWidth: 220, flex: 1 },
  { field: 'currencyCode', headerName: 'salesOrder.ui.currency', width: 100 },
  {
    field: 'quantity',
    headerName: 'salesOrder.ui.quantity',
    width: 110,
    type: 'number',
    align: 'right',
  },
  {
    field: 'netAmount',
    headerName: 'salesOrder.ui.netAmount',
    width: 130,
    type: 'number',
    align: 'right',
    renderCell: ({ row }) => money(row.netAmount, row.currencyCode),
  },
  {
    field: 'taxAmount',
    headerName: 'salesOrder.ui.tax',
    width: 120,
    type: 'number',
    align: 'right',
    renderCell: ({ row }) => money(row.taxAmount, row.currencyCode),
  },
  {
    field: 'totalAmount',
    headerName: 'salesOrder.ui.total',
    width: 130,
    type: 'number',
    align: 'right',
    renderCell: ({ row }) => money(row.totalAmount, row.currencyCode),
  },
];

export function SalesOrderConfirmationDialog({
  open,
  mode,
  orderId,
  salesId,
  customerName,
  canPost,
  onClose,
  onPosted,
}: Props): React.ReactElement {
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const queryClient = useQueryClient();
  const [confirmationDate, setConfirmationDate] = useState(() =>
    new Date().toISOString().slice(0, 10)
  );
  const [posting, setPosting] = useState(false);
  const [pendingRequestId, setPendingRequestId] = useState<string>();
  const [error, setError] = useState('');
  const [postingEnabled, setPostingEnabled] = useState(true);
  const previewQuery = useQuery({
    queryKey: salesOrderQueryKeys.confirmationPreview(company, orderId),
    queryFn: ({ signal }) => salesOrderConfirmationApi.preview(orderId, signal),
    enabled: open && Boolean(orderId),
    staleTime: 0,
  });

  useEffect(() => {
    if (!open) return;
    setError('');
    setPendingRequestId(undefined);
    setPostingEnabled(mode === 'confirmation');
  }, [open, mode, orderId]);

  const overviewRows = useMemo<OverviewRow[]>(() => {
    const header = previewQuery.data?.header;
    if (!header) return [];
    return [
      {
        id: header.id,
        update: t('salesOrder.ui.confirmation'),
        salesId,
        customerName,
        currencyCode: header.currencyCode,
        quantity: header.qty,
        netAmount: header.salesBalance,
        taxAmount: header.sumTax,
        totalAmount: header.confirmAmount,
      },
    ];
  }, [customerName, previewQuery.data?.header, salesId, t]);

  const lineColumns = useMemo<ColumnDef<SalesOrderConfirmationLine>[]>(
    () => [
      {
        field: 'salesId',
        headerName: 'salesOrder.ui.salesOrder',
        width: 150,
        valueGetter: () => salesId,
      },
      { field: 'itemId', headerName: 'salesOrder.ui.itemNumber', width: 140 },
      { field: 'name', headerName: 'salesOrder.ui.description', minWidth: 220, flex: 1 },
      { field: 'inventLocationId', headerName: 'salesOrder.ui.warehouse', width: 140 },
      { field: 'wmsLocationId', headerName: 'salesOrder.ui.location', width: 130 },
      {
        field: 'qty',
        headerName: 'salesOrder.ui.update',
        width: 110,
        type: 'number',
        align: 'right',
      },
      {
        field: 'salesPrice',
        headerName: 'salesOrder.ui.unitPrice',
        width: 120,
        type: 'number',
        align: 'right',
        renderCell: ({ row }) =>
          money(row.salesPrice, previewQuery.data?.header.currencyCode ?? ''),
      },
      {
        field: 'lineAmount',
        headerName: 'salesOrder.ui.netAmount',
        width: 130,
        type: 'number',
        align: 'right',
        renderCell: ({ row }) =>
          money(row.lineAmount, previewQuery.data?.header.currencyCode ?? ''),
      },
    ],
    [previewQuery.data?.header.currencyCode, salesId]
  );

  const post = async () => {
    if (mode === 'proForma' || !postingEnabled) {
      onClose();
      return;
    }
    if (!canPost || posting || !confirmationDate) return;
    setPosting(true);
    setError('');
    try {
      const requestId = pendingRequestId ?? crypto.randomUUID();
      setPendingRequestId(requestId);
      await salesOrderConfirmationApi.post(orderId, confirmationDate, requestId);
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: salesOrderQueryKeys.confirmations(company, orderId),
        }),
        onPosted(),
      ]);
      setPendingRequestId(undefined);
      onClose();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'The sales order could not be confirmed.');
    } finally {
      setPosting(false);
    }
  };

  return (
    <AppActionDrawer
      open={open}
      onClose={onClose}
      title={`${mode === 'proForma' ? t('salesOrder.ui.proFormaConfirmation') : t('salesOrder.ui.confirmSalesOrder')} · ${salesId}`}
      width={1500}
      busy={posting}
      actions={
        <>
          <Button onClick={onClose} disabled={posting}>
            {t('actions.cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={() => void post()}
            disabled={
              posting ||
              previewQuery.isLoading ||
              !previewQuery.data ||
              (mode === 'confirmation' && postingEnabled && (!canPost || !confirmationDate))
            }
          >
            {posting
              ? t('salesOrder.ui.postingInProgress')
              : mode === 'proForma' || !postingEnabled
                ? t('salesOrder.ui.closePreview')
                : t('salesOrder.ui.ok')}
          </Button>
        </>
      }
    >
      <Stack spacing={1} sx={{ minHeight: '100%' }}>
        <Typography variant="body2" color="text.secondary">
          Standard view
        </Typography>
        {error && <Alert severity="error">{error}</Alert>}
        {previewQuery.error instanceof Error && (
          <Alert severity="error">{previewQuery.error.message}</Alert>
        )}
        {mode === 'proForma' && (
          <Alert severity="info">
            Preview only. No confirmation journal or order status change will be posted.
          </Alert>
        )}

        <Accordion disableGutters>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography sx={{ fontWeight: 600 }}>{t('salesOrder.ui.parameters')}</Typography>
          </AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, alignItems: 'center' }}>
              <TextField
                label={t('salesOrder.ui.confirmationDate')}
                type="date"
                size="small"
                value={confirmationDate}
                disabled={mode === 'proForma' || posting}
                onChange={(event) => setConfirmationDate(event.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
              {mode === 'confirmation' && (
                <FormControlLabel
                  label={t('salesOrder.ui.posting')}
                  control={
                    <Switch
                      checked={postingEnabled}
                      disabled={posting}
                      onChange={(event) => setPostingEnabled(event.target.checked)}
                    />
                  }
                />
              )}
              <Typography variant="body2">{`${t('salesOrder.ui.quantity')}: ${t('common.all')}`}</Typography>
            </Box>
          </AccordionDetails>
        </Accordion>

        <Accordion disableGutters defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography sx={{ fontWeight: 600 }}>{t('salesOrder.ui.overview')}</Typography>
          </AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <DataGrid
              rows={overviewRows}
              columns={overviewColumns}
              getRowId={(row) => row.id}
              loading={previewQuery.isLoading}
              selectionMode="single"
              checkboxSelection
              selectedIds={overviewRows.length ? [overviewRows[0].id] : []}
              height={138}
              rowHeight={28}
              headerHeight={30}
              showCellBorders
              hideToolbar
              hideFooter
              hideColumnMenu
              hideSidebar
            />
          </AccordionDetails>
        </Accordion>

        <Accordion disableGutters>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography sx={{ fontWeight: 600 }}>{t('salesOrder.ui.setup')}</Typography>
          </AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <Typography variant="body2">{`${t('salesOrder.ui.quantity')}: ${t('common.all')}`}</Typography>
          </AccordionDetails>
        </Accordion>

        <Accordion disableGutters defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography sx={{ fontWeight: 600 }}>{t('salesOrder.ui.lines')}</Typography>
          </AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <DataGrid
              rows={previewQuery.data?.lines ?? []}
              columns={lineColumns}
              getRowId={(line) => line.id}
              loading={previewQuery.isLoading}
              selectionMode="single"
              checkboxSelection
              selectedIds={previewQuery.data?.lines[0] ? [previewQuery.data.lines[0].id] : []}
              height={260}
              rowHeight={28}
              headerHeight={30}
              showCellBorders
              hideToolbar
              hideFooter
              hideColumnMenu
              hideSidebar
            />
          </AccordionDetails>
        </Accordion>
      </Stack>
    </AppActionDrawer>
  );
}
