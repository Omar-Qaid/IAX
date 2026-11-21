import React, { useEffect, useMemo, useState } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { UnavailableCommandButton } from '@shared/components/action-pane/UnavailableCommandButton';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { salesOrderInvoiceApi } from '../api/salesOrderInvoiceApi';
import { salesOrderLinesApi, type SalesOrderLineRecord } from '../api/salesOrderLinesApi';
import { salesOrderQueryKeys } from '../../queries/accountsReceivableQueryKeys';

interface Props {
  open: boolean;
  orderId: string;
  salesId: string;
  customerName?: string;
  currencyCode?: string;
  canPost: boolean;
  onClose: () => void;
  onPosted: () => Promise<void>;
}

/** What the invoice is drawn from, as the D365 posting parameters offer it. */
type InvoiceQuantityMode = 'packingSlip' | 'all';

interface OverviewRow {
  id: string;
  packingSlipId: string;
  update: string;
  customerPackingSlip: string;
  salesId: string;
  customerName: string;
  currencyCode: string;
}

/**
 * A line is invoiceable for its remaining financial quantity: what has been ordered
 * but not yet invoiced. The server derives the same figure, so a line it would skip
 * is not offered here.
 */
const invoiceableQuantity = (line: SalesOrderLineRecord) =>
  line.remainSalesFinancial ?? line.quantity;

/** Only what has been delivered can be invoiced from packing slips. */
const deliveredQuantity = (line: SalesOrderLineRecord) =>
  Math.max(invoiceableQuantity(line) - (line.remainSalesPhysical ?? 0), 0);

const dateToday = () => {
  const date = new Date();
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
};

export function SalesOrderInvoicePostingDialog({
  open,
  orderId,
  salesId,
  customerName = '',
  currencyCode = '',
  canPost,
  onClose,
  onPosted,
}: Props): React.ReactElement {
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const queryClient = useQueryClient();
  const [invoiceDate, setInvoiceDate] = useState(dateToday);
  const [quantityMode, setQuantityMode] = useState<InvoiceQuantityMode>('packingSlip');
  const [customerPackingSlip, setCustomerPackingSlip] = useState('');
  const [quantities, setQuantities] = useState<Record<string, string>>({});
  const [posting, setPosting] = useState(false);
  const [requestId, setRequestId] = useState<string>();
  const [error, setError] = useState('');

  const linesQuery = useQuery({
    queryKey: salesOrderQueryKeys.lines(company, orderId),
    queryFn: ({ signal }) => salesOrderLinesApi.list(orderId, signal),
    enabled: open && Boolean(orderId),
  });

  useEffect(() => {
    if (!open) return;
    setInvoiceDate(dateToday());
    setQuantityMode('packingSlip');
    setCustomerPackingSlip('');
    setQuantities({});
    setError('');
    setRequestId(undefined);
  }, [open, orderId]);

  const allLines = useMemo(() => linesQuery.data ?? [], [linesQuery.data]);

  /** Proposed quantity per line, which is what the parameter above selects. */
  const proposed = React.useCallback(
    (line: SalesOrderLineRecord) =>
      quantityMode === 'packingSlip' ? deliveredQuantity(line) : invoiceableQuantity(line),
    [quantityMode]
  );

  const invoiceableLines = useMemo(
    () => allLines.filter((line) => proposed(line) > 0),
    [allLines, proposed]
  );

  const overviewRows = useMemo<OverviewRow[]>(
    () => [
      {
        id: orderId,
        packingSlipId: '',
        update: t('salesOrder.ui.invoice'),
        customerPackingSlip,
        salesId,
        customerName,
        currencyCode,
      },
    ],
    [currencyCode, customerName, customerPackingSlip, orderId, salesId, t]
  );

  const overviewColumns = useMemo<ColumnDef<OverviewRow>[]>(
    () => [
      { field: 'packingSlipId', headerName: 'salesOrder.ui.packingSlip', width: 150 },
      { field: 'update', headerName: 'salesOrder.ui.update', width: 130 },
      {
        field: 'customerPackingSlip',
        headerName: 'salesOrder.ui.customerPackingSlip',
        width: 170,
        renderCell: () => (
          <TextField
            size="small"
            value={customerPackingSlip}
            disabled={posting}
            onChange={(event) => setCustomerPackingSlip(event.target.value)}
            slotProps={{ htmlInput: { 'aria-label': t('salesOrder.ui.customerPackingSlip') } }}
            sx={{ width: '100%', '& .MuiInputBase-input': { py: 0.25, px: 0.5 } }}
          />
        ),
      },
      { field: 'salesId', headerName: 'salesOrder.ui.salesOrder', width: 160 },
      { field: 'customerName', headerName: 'salesOrder.ui.name', minWidth: 220, flex: 1 },
      {
        preview: { entity: 'currency' },
        field: 'currencyCode',
        headerName: 'salesOrder.ui.currency',
        width: 110,
      },
    ],
    [customerPackingSlip, posting, t]
  );

  const lineColumns = useMemo<ColumnDef<SalesOrderLineRecord>[]>(
    () => [
      { field: 'inventTransId', headerName: 'salesOrder.ui.packingSlip', width: 150 },
      {
        field: 'shippingDateConfirmed',
        headerName: 'salesOrder.ui.shipDate',
        width: 120,
        valueGetter: ({ row }) => row.shippingDateConfirmed ?? row.deliveryDate ?? '',
      },
      { field: 'salesCategory', headerName: 'salesOrder.ui.salesCategory', width: 150 },
      { field: 'description', headerName: 'salesOrder.ui.text', minWidth: 200, flex: 1 },
      {
        field: 'salesId',
        headerName: 'salesOrder.ui.salesOrder',
        width: 150,
        valueGetter: () => salesId,
      },
      {
        preview: { entity: 'inventSite' },
        field: 'site',
        headerName: 'salesOrder.ui.site',
        width: 100,
      },
      {
        preview: { entity: 'inventLocation' },
        field: 'warehouse',
        headerName: 'salesOrder.ui.warehouse',
        width: 120,
      },
      { field: 'batchNumber', headerName: 'salesOrder.ui.batchNumber', width: 140 },
      {
        preview: { entity: 'releasedProduct' },
        field: 'itemNumber',
        headerName: 'salesOrder.ui.itemNumber',
        width: 160,
      },
      {
        field: 'quantity',
        headerName: 'salesOrder.ui.update',
        width: 120,
        align: 'right',
        renderCell: ({ row }) => (
          <TextField
            size="small"
            type="number"
            value={quantities[row.id] ?? String(proposed(row))}
            disabled={posting}
            onChange={(event) =>
              setQuantities((current) => ({ ...current, [row.id]: event.target.value }))
            }
            slotProps={{
              htmlInput: {
                min: 0,
                max: invoiceableQuantity(row),
                step: 'any',
                'aria-label': t('salesOrder.ui.updateQuantityFor', { itemId: row.itemNumber }),
              },
            }}
            sx={{
              width: '100%',
              '& .MuiInputBase-input': { py: 0.25, px: 0.5, textAlign: 'right' },
            }}
          />
        ),
      },
    ],
    [posting, proposed, quantities, salesId, t]
  );

  const selectedLines = invoiceableLines
    .map((line) => ({
      salesLineRecId: Number(line.id),
      qty: Number(quantities[line.id] ?? proposed(line)),
    }))
    .filter((line) => Number.isFinite(line.qty) && line.qty > 0);
  // A line cannot be invoiced beyond what is still uninvoiced on it.
  const hasInvalidQuantity = invoiceableLines.some((line) => {
    const value = Number(quantities[line.id] ?? proposed(line));
    return !Number.isFinite(value) || value < 0 || value > invoiceableQuantity(line);
  });

  const submit = async () => {
    if (!canPost || posting || !invoiceDate || selectedLines.length === 0 || hasInvalidQuantity)
      return;
    setPosting(true);
    setError('');
    try {
      // The server treats the request id as an idempotency key, so a retry after a
      // network failure returns the invoice already posted instead of posting a second one.
      const id = requestId ?? crypto.randomUUID();
      setRequestId(id);
      await salesOrderInvoiceApi.post(orderId, {
        invoiceDate,
        requestId: id,
        lines: selectedLines,
      });
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: salesOrderQueryKeys.list(company) }),
        queryClient.invalidateQueries({ queryKey: salesOrderQueryKeys.lines(company, orderId) }),
        queryClient.invalidateQueries({ queryKey: salesOrderQueryKeys.invoices(company, orderId) }),
        onPosted(),
      ]);
      setRequestId(undefined);
      onClose();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : t('salesOrder.ui.invoicePostFailed'));
    } finally {
      setPosting(false);
    }
  };

  return (
    <AppActionDrawer
      open={open}
      onClose={onClose}
      title={t('salesOrder.ui.postingInvoice')}
      width={1770}
      busy={posting}
      actions={
        <>
          {error && (
            <Alert severity="error" sx={{ flex: 1, minWidth: 0 }}>
              {error}
            </Alert>
          )}
          <Button
            variant="contained"
            onClick={() => void submit()}
            disabled={
              posting ||
              linesQuery.isLoading ||
              !canPost ||
              !invoiceDate ||
              selectedLines.length === 0 ||
              hasInvalidQuantity
            }
          >
            {posting ? t('salesOrder.ui.postingInProgress') : t('salesOrder.ui.ok')}
          </Button>
          <Button onClick={onClose} disabled={posting}>
            {t('actions.cancel')}
          </Button>
          <UnavailableCommandButton label={t('salesOrder.ui.batch')} />
        </>
      }
    >
      <Stack spacing={1} sx={{ minHeight: '100%' }}>
        <Typography variant="body2" color="text.secondary">
          {t('common.standardView')}
        </Typography>
        <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
          <UnavailableCommandButton label={t('salesOrder.ui.arrange')} />
          <UnavailableCommandButton label={t('salesOrder.ui.select')} />
          <UnavailableCommandButton label={t('salesOrder.ui.printerSetup')} />
        </Stack>
        {linesQuery.isError && <Alert severity="error">{t('salesOrder.ui.linesLoadFailed')}</Alert>}
        {!linesQuery.isLoading && invoiceableLines.length === 0 && (
          <Alert severity="info">{t('salesOrder.ui.nothingToInvoice')}</Alert>
        )}

        <Accordion disableGutters>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box sx={{ display: 'flex', width: '100%', alignItems: 'center', pr: 1, gap: 2 }}>
              <Typography sx={{ fontWeight: 600, flex: 1 }}>
                {t('salesOrder.ui.parameters')}
              </Typography>
              {/* D365 shows the selected quantity parameter on the collapsed summary. */}
              <Typography variant="body2" color="text.secondary">
                {quantityMode === 'packingSlip' ? t('salesOrder.ui.packingSlip') : t('common.all')}
              </Typography>
            </Box>
          </AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, alignItems: 'center' }}>
              <FormControl size="small" sx={{ minWidth: 180 }}>
                <InputLabel id="invoice-quantity-mode-label">
                  {t('salesOrder.ui.quantity')}
                </InputLabel>
                <Select
                  labelId="invoice-quantity-mode-label"
                  label={t('salesOrder.ui.quantity')}
                  value={quantityMode}
                  disabled={posting}
                  onChange={(event) => {
                    setQuantityMode(event.target.value as InvoiceQuantityMode);
                    setQuantities({});
                  }}
                >
                  <MenuItem value="packingSlip">{t('salesOrder.ui.packingSlip')}</MenuItem>
                  <MenuItem value="all">{t('common.all')}</MenuItem>
                </Select>
              </FormControl>
              <TextField
                label={t('salesOrder.ui.invoiceDate')}
                type="date"
                size="small"
                value={invoiceDate}
                disabled={posting}
                onChange={(event) => setInvoiceDate(event.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
            </Box>
          </AccordionDetails>
        </Accordion>

        <Accordion disableGutters defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography sx={{ fontWeight: 600 }}>{t('salesOrder.ui.overview')}</Typography>
          </AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <Stack direction="row" spacing={1} sx={{ mb: 0.5 }}>
              <UnavailableCommandButton label={t('salesOrder.ui.add')} />
              <UnavailableCommandButton label={t('salesOrder.ui.remove')} />
              <UnavailableCommandButton label={t('salesOrder.ui.trackingInformation')} />
              <UnavailableCommandButton label={t('salesOrder.ui.selectPackingSlip')} />
              <UnavailableCommandButton label={t('salesOrder.ui.totals')} />
              <UnavailableCommandButton label={t('salesOrder.ui.salesTax')} />
              <UnavailableCommandButton label={t('salesOrder.ui.paymentSchedule')} />
            </Stack>
            <DataGrid
              rows={overviewRows}
              columns={overviewColumns}
              getRowId={(row) => row.id}
              loading={linesQuery.isLoading}
              selectionMode="single"
              checkboxSelection
              selectedIds={[orderId]}
              height={164}
              rowHeight={32}
              headerHeight={32}
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
            <Typography variant="body2">{t('salesOrder.ui.invoiceSetupHint')}</Typography>
          </AccordionDetails>
        </Accordion>

        <Accordion disableGutters defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography sx={{ fontWeight: 600 }}>{t('salesOrder.ui.lines')}</Typography>
          </AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <Stack direction="row" spacing={1} sx={{ mb: 0.5 }}>
              <UnavailableCommandButton label={t('salesOrder.ui.delete')} />
              <UnavailableCommandButton label={t('salesOrder.ui.salesOrderLine')} />
              <UnavailableCommandButton label={t('salesOrder.ui.inventory')} />
              <UnavailableCommandButton label={t('salesOrder.ui.updateLine')} />
            </Stack>
            <DataGrid
              rows={invoiceableLines}
              columns={lineColumns}
              getRowId={(row) => row.id}
              loading={linesQuery.isLoading}
              selectionMode="single"
              checkboxSelection
              selectedIds={invoiceableLines[0] ? [invoiceableLines[0].id] : []}
              height={260}
              rowHeight={34}
              headerHeight={32}
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
