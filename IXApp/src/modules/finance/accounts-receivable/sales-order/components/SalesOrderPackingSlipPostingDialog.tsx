import React, { useEffect, useMemo, useState } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  Button,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { UnavailableCommandButton } from '@shared/components/action-pane/UnavailableCommandButton';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import {
  salesOrderPackingSlipApi,
  type PackingSlipPostingLine,
  type PackingSlipQuantityMode,
} from '../api/salesOrderPackingSlipApi';
import { salesOrderQueryKeys } from '../../queries/accountsReceivableQueryKeys';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';

interface Props {
  open: boolean;
  orderId: string;
  salesId: string;
  canPost: boolean;
  onClose: () => void;
  onPosted: () => Promise<void>;
}

interface OverviewRow {
  id: string;
  update: string;
  customerPackingSlip: string;
  shipmentNumber: string;
  salesId: string;
  siteId: string;
  customerName: string;
  currencyCode: string;
}

const dateToday = () => {
  const date = new Date();
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
};

const overviewColumns: ColumnDef<OverviewRow>[] = [
  { field: 'update', headerName: 'salesOrder.ui.update', width: 134 },
  { field: 'customerPackingSlip', headerName: 'salesOrder.ui.customerPackingSlip', width: 84 },
  { field: 'shipmentNumber', headerName: 'salesOrder.ui.shipmentNumber', width: 150, hidden: true },
  { field: 'salesId', headerName: 'salesOrder.ui.salesOrder', width: 134 },
  { field: 'siteId', headerName: 'salesOrder.ui.site', width: 134, preview: { entity: 'inventSite' } },
  { field: 'customerName', headerName: 'salesOrder.ui.name', width: 185 },
  { field: 'currencyCode', headerName: 'salesOrder.ui.currency', minWidth: 110, flex: 1, preview: { entity: 'currency' } },
];

export function SalesOrderPackingSlipPostingDialog({
  open,
  orderId,
  salesId,
  canPost,
  onClose,
  onPosted,
}: Props): React.ReactElement {
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const queryClient = useQueryClient();
  const [deliveryDate, setDeliveryDate] = useState(dateToday);
  const [quantityMode, setQuantityMode] = useState<PackingSlipQuantityMode>('all');
  const [postingEnabled, setPostingEnabled] = useState(true);
  const [quantities, setQuantities] = useState<Record<string, string>>({});
  const [posting, setPosting] = useState(false);
  const [requestId, setRequestId] = useState<string>();
  const [error, setError] = useState('');

  const previewQuery = useQuery({
    queryKey: salesOrderQueryKeys.packingSlipPreview(company, orderId, quantityMode),
    queryFn: ({ signal }) => salesOrderPackingSlipApi.postingPreview(orderId, quantityMode, signal),
    enabled: open && Boolean(orderId),
    staleTime: 0,
  });

  useEffect(() => {
    if (!open) return;
    setDeliveryDate(dateToday());
    setPostingEnabled(true);
    setError('');
    setRequestId(undefined);
    setQuantityMode('all');
    setQuantities({});
  }, [open, orderId]);

  const preview = previewQuery.data;
  const overviewRows = useMemo<OverviewRow[]>(
    () =>
      preview
        ? [
            {
              id: preview.header.id,
              update: t('salesOrder.ui.packingSlip'),
              customerPackingSlip: '',
              shipmentNumber: '',
              salesId,
              siteId: preview.header.siteId,
              customerName: preview.header.customerName,
              currencyCode: preview.header.currencyCode,
            },
          ]
        : [],
    [preview, salesId, t]
  );

  const lineColumns = useMemo<ColumnDef<PackingSlipPostingLine>[]>(
    () => [
      { field: 'salesCategory', headerName: 'salesOrder.ui.salesCategory', width: 185 },
      { field: 'name', headerName: 'salesOrder.ui.text', width: 185 },
      {
        field: 'salesId',
        headerName: 'salesOrder.ui.salesOrder',
        width: 134,
        valueGetter: () => salesId,
      },
      { field: 'inventSiteId', headerName: 'salesOrder.ui.site', width: 134, preview: { entity: 'inventSite' } },
      { field: 'inventLocationId', headerName: 'salesOrder.ui.warehouse', width: 134, preview: { entity: 'inventLocation' } },
      { field: 'batchNumber', headerName: 'salesOrder.ui.batchNumber', width: 134, renderCell: () => '' },
      { field: 'wmsLocationId', headerName: 'salesOrder.ui.location', width: 145, hidden: true },
      {
        field: 'catchWeightQuantity',
        headerName: 'salesOrder.ui.cwUpdate',
        width: 120,
        type: 'number',
        hidden: true,
        align: 'right',
      },
      { field: 'productNumber', headerName: 'salesOrder.ui.productNumber', width: 190, hidden: true },
      { field: 'itemId', headerName: 'salesOrder.ui.itemNumber', width: 178 },
      {
        field: 'quantity',
        headerName: 'salesOrder.ui.update',
        width: 65,
        align: 'right',
        renderCell: ({ row }) => (
          <TextField
            size="small"
            type="number"
            value={quantities[row.id] ?? String(row.quantity)}
            disabled={!postingEnabled || posting}
            onChange={(event) =>
              setQuantities((current) => ({ ...current, [row.id]: event.target.value }))
            }
            slotProps={{
              htmlInput: {
                min: 0,
                max: row.quantity,
                step: 'any',
                'aria-label': t('salesOrder.ui.updateQuantityFor', { itemId: row.itemId }),
              },
            }}
            sx={{
              width: '100%',
              '& .MuiInputBase-input': { py: 0.25, px: 0.5, textAlign: 'right' },
            }}
          />
        ),
      },
      {
        field: 'netAmount',
        headerName: 'salesOrder.ui.netAmount',
        width: 85,
        type: 'number',
        align: 'right',
        renderCell: ({ row }) => {
          const entered = Number(quantities[row.id] ?? row.quantity);
          const amount = row.quantity > 0 ? (row.netAmount * entered) / row.quantity : 0;
          return amount.toLocaleString(undefined, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          });
        },
      },
      { field: 'close', headerName: t('salesOrder.ui.close', 'Close'), width: 84, renderCell: () => '' },
      { field: 'scrap', headerName: t('salesOrder.ui.scrap', 'Scrap'), width: 84, renderCell: () => '' },
      { field: 'modeOfDelivery', headerName: t('salesOrder.ui.modeOfDelivery', 'Mode of delivery'), width: 134, renderCell: () => '' },
      { field: 'deliveryName', headerName: t('salesOrder.ui.deliveryName', 'Delivery name'), width: 185, renderCell: () => '' },
    ],
    [posting, postingEnabled, quantities, salesId, t]
  );

  const selectedLines = (preview?.lines ?? [])
    .map((line) => ({
      salesLineId: line.id,
      quantity: Number(quantities[line.id] ?? line.quantity),
    }))
    .filter((line) => Number.isFinite(line.quantity) && line.quantity > 0);
  const hasInvalidQuantity = (preview?.lines ?? []).some((line) => {
    const value = Number(quantities[line.id] ?? line.quantity);
    return !Number.isFinite(value) || value < 0 || value > line.quantity;
  });

  const submit = async () => {
    if (!postingEnabled) {
      onClose();
      return;
    }
    if (
      !canPost ||
      posting ||
      !preview ||
      !deliveryDate ||
      selectedLines.length === 0 ||
      hasInvalidQuantity
    )
      return;
    setPosting(true);
    setError('');
    try {
      const id = requestId ?? crypto.randomUUID();
      setRequestId(id);
      await salesOrderPackingSlipApi.post(orderId, {
        deliveryDate,
        quantityMode,
        posting: true,
        requestId: id,
        lines: selectedLines,
      });
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: salesOrderQueryKeys.list(company) }),
        queryClient.invalidateQueries({ queryKey: salesOrderQueryKeys.lines(company, orderId) }),
        queryClient.invalidateQueries({
          queryKey: salesOrderQueryKeys.packingSlips(company, orderId),
        }),
        queryClient.invalidateQueries({
          queryKey: salesOrderQueryKeys.packingSlipPreview(company, orderId),
        }),
        onPosted(),
      ]);
      setRequestId(undefined);
      onClose();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'The packing slip could not be posted.');
    } finally {
      setPosting(false);
    }
  };

  return (
    <AppActionDrawer
      open={open}
      onClose={onClose}
      title={t('salesOrder.ui.packingSlipPosting')}
      width={1793}
      appearance="posting"
      viewLabel={t('common.standardView')}
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
              previewQuery.isLoading ||
              !preview ||
              (postingEnabled &&
                (!canPost || !deliveryDate || selectedLines.length === 0 || hasInvalidQuantity))
            }
          >
            {posting
              ? t('salesOrder.ui.postingInProgress')
              : postingEnabled
                ? t('salesOrder.ui.ok')
                : t('salesOrder.ui.closePreview')}
          </Button>
          <Button variant="outlined" color="inherit" onClick={onClose} disabled={posting}>
            {t('actions.cancel')}
          </Button>
          <UnavailableCommandButton label={t('salesOrder.ui.batch')} />
        </>
      }
    >
      <Stack spacing={0} sx={{ minHeight: '100%',
        '& .MuiAccordion-root': { bgcolor: 'background.paper', boxShadow: 'none', border: 0, borderRadius: '0 !important', m: '0 !important', '&:before': { display: 'none' } },
        '& .MuiAccordionSummary-root': { minHeight: '42px !important', px: 1, borderBottom: '1px solid', borderColor: 'divider' },
        '& .MuiAccordionSummary-content': { my: '0 !important' },
        '& .MuiAccordionSummary-content .MuiTypography-root': { fontSize: 13, fontWeight: 500 },
        '& .MuiAccordionSummary-expandIconWrapper': { border: '1px solid', borderColor: 'divider', borderRadius: '3px', width: 23, height: 25, alignItems: 'center', justifyContent: 'center' },
        '& .MuiAccordionSummary-expandIconWrapper svg': { fontSize: 16 },
        '& .MuiAccordionDetails-root': { p: '0 8px 20px' },
        '& .MuiButton-root': { fontSize: 11, minWidth: 0, px: .5, py: .25, textTransform: 'none' },
        '& [role="columnheader"]': { fontSize: 10 },
        '& [role="gridcell"]': { fontSize: 11 },
      }}>
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center', minHeight: 46, pb: 1 }}>
          <UnavailableCommandButton label={t('salesOrder.ui.arrange')} />
          <UnavailableCommandButton label={t('salesOrder.ui.select')} />
          <UnavailableCommandButton label={t('salesOrder.ui.printerSetup')} />
        </Stack>
        {previewQuery.error instanceof Error && (
          <Alert severity="error">{previewQuery.error.message}</Alert>
        )}

        <Accordion disableGutters>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography sx={{ flex: 1 }}>{t('salesOrder.ui.parameters')}</Typography>
            <Typography sx={{ mr: 2, fontSize: '10px !important', color: 'primary.main' }}>{quantityMode === 'picked' ? t('salesOrder.ui.picked') : quantityMode === 'deliverNow' ? t('salesOrder.ui.deliverNow') : t('common.all')}</Typography>
          </AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, alignItems: 'center' }}>
              <FormControl size="small" sx={{ minWidth: 180 }}>
                <InputLabel id="packing-slip-quantity-mode-label">
                  {t('salesOrder.ui.quantity')}
                </InputLabel>
                <Select
                  labelId="packing-slip-quantity-mode-label"
                  label={t('salesOrder.ui.quantity')}
                  value={quantityMode}
                  disabled={posting}
                  onChange={(event) => {
                    setQuantityMode(event.target.value as PackingSlipQuantityMode);
                    setQuantities({});
                  }}
                >
                  <MenuItem value="all">{t('common.all')}</MenuItem>
                  <MenuItem value="deliverNow">{t('salesOrder.ui.deliverNow')}</MenuItem>
                  <MenuItem value="picked">{t('salesOrder.ui.picked')}</MenuItem>
                </Select>
              </FormControl>
              <TextField
                label={t('salesOrder.ui.date')}
                type="date"
                size="small"
                value={deliveryDate}
                disabled={posting}
                onChange={(event) => setDeliveryDate(event.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
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
            </Box>
          </AccordionDetails>
        </Accordion>

        <Accordion disableGutters defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography sx={{ fontWeight: 600 }}>{t('salesOrder.ui.overview')}</Typography>
          </AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <Stack direction="row" spacing={1} sx={{ mb: 0.5 }}>
              <UnavailableCommandButton label={t('salesOrder.ui.add')} startIcon={<AddIcon />} />
              <UnavailableCommandButton label={t('salesOrder.ui.remove')} startIcon={<DeleteOutlineIcon />} />
              <UnavailableCommandButton label={t('salesOrder.ui.trackingInformation')} />
              <UnavailableCommandButton label={t('salesOrder.ui.salesTax')} />
            </Stack>
            <DataGrid
              rows={overviewRows}
              columns={overviewColumns}
              getRowId={(row) => row.id}
              loading={previewQuery.isLoading}
              selectionMode="single"
              checkboxSelection={false}
              selectedIds={overviewRows.length ? [overviewRows[0].id] : []}
              height={139}
              rowHeight={28}
              headerHeight={27}
              showCellBorders
              hideToolbar
              hideFilterRow
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
            <Typography variant="body2">{t('salesOrder.ui.packingSlipSetupHint')}</Typography>
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
              <UnavailableCommandButton label={t('salesOrder.ui.resetBatchDispositionCode')} />
              <UnavailableCommandButton label={t('salesOrder.ui.updateLine')} />
            </Stack>
            <DataGrid
              rows={preview?.lines ?? []}
              columns={lineColumns}
              getRowId={(line) => line.id}
              loading={previewQuery.isLoading}
              selectionMode="single"
              checkboxSelection={false}
              selectedIds={preview?.lines[0] ? [preview.lines[0].id] : []}
              height={206}
              rowHeight={28}
              headerHeight={27}
              showCellBorders
              hideToolbar
              hideFilterRow
              hideFooter
              hideColumnMenu
              hideSidebar
            />
          </AccordionDetails>
        </Accordion>
        <Accordion disableGutters>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}><Typography>{t('salesOrder.ui.lineDetails', 'Line details')}</Typography></AccordionSummary>
          <AccordionDetails><Typography sx={{ fontSize: 12 }}>{preview?.lines[0]?.name ?? t('common.noData', 'No data')}</Typography></AccordionDetails>
        </Accordion>
        <Accordion disableGutters>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}><Typography>{t('salesOrder.ui.salesOrders', 'Sales orders')}</Typography></AccordionSummary>
          <AccordionDetails><Typography sx={{ fontSize: 12 }}>{salesId} · {preview?.header.customerName}</Typography></AccordionDetails>
        </Accordion>
      </Stack>
    </AppActionDrawer>
  );
}
