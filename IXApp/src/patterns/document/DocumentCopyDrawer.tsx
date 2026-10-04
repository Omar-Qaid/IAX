import React, { useEffect, useMemo, useState } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  Button,
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';

export interface DocumentCopySource {
  id: number;
  documentNumber: string;
  account: string;
  accountName: string;
  createdAt: string;
  currencyCode: string;
  sourceType: string;
}

export interface DocumentCopyLine {
  id: number;
  itemId: string;
  description: string;
  quantity: number;
  unit: string;
  unitPrice: number;
  netAmount: number;
  discount: number;
  discountPercent: number;
  site: string;
  warehouse: string;
}

export interface DocumentCopyOptions {
  quantityFactor: number;
  invertSign: boolean;
  recalculatePrice: boolean;
  copyPrecisely: boolean;
}

export interface DocumentCopyDrawerProps {
  open: boolean;
  title: string;
  sourceSectionLabel: string;
  additionalSections?: string[];
  sources: DocumentCopySource[];
  lines: DocumentCopyLine[];
  loadingSources?: boolean;
  loadingLines?: boolean;
  busy?: boolean;
  error?: string;
  onClose: () => void;
  onSourceChange: (source?: DocumentCopySource) => void;
  onCopy: (lineIds: number[], options: DocumentCopyOptions) => Promise<void> | void;
}

const sourceColumns: ColumnDef<DocumentCopySource>[] = [
  { field: 'documentNumber', headerName: 'Document', width: 145 },
  { field: 'account', headerName: 'Customer account', width: 145 },
  { field: 'accountName', headerName: 'Customer name', minWidth: 220, flex: 1 },
  { field: 'createdAt', headerName: 'Created date and time', width: 190, type: 'date' },
  { field: 'currencyCode', headerName: 'Currency', width: 100 },
];

const lineColumns: ColumnDef<DocumentCopyLine>[] = [
  { field: 'itemId', headerName: 'Item number', width: 135 },
  { field: 'description', headerName: 'Text', minWidth: 190, flex: 1 },
  { field: 'site', headerName: 'Site', width: 100 },
  { field: 'warehouse', headerName: 'Warehouse', width: 120 },
  { field: 'quantity', headerName: 'Quantity', width: 100, type: 'number', align: 'right' },
  { field: 'unit', headerName: 'Unit', width: 80 },
  { field: 'unitPrice', headerName: 'Unit price', width: 110, type: 'number', align: 'right' },
  { field: 'netAmount', headerName: 'Net amount', width: 110, type: 'number', align: 'right' },
  { field: 'discount', headerName: 'Discount', width: 100, type: 'number', align: 'right' },
  { field: 'discountPercent', headerName: 'Discount percent', width: 125, type: 'number', align: 'right' },
];

export function DocumentCopyDrawer({
  open,
  title,
  sourceSectionLabel,
  additionalSections = [],
  sources,
  lines,
  loadingSources,
  loadingLines,
  busy,
  error,
  onClose,
  onSourceChange,
  onCopy,
}: DocumentCopyDrawerProps): React.ReactElement {
  const [selectedSource, setSelectedSource] = useState<number>();
  const [selectedLines, setSelectedLines] = useState<(string | number)[]>([]);
  const options: DocumentCopyOptions = {
    quantityFactor: 1,
    invertSign: false,
    recalculatePrice: false,
    copyPrecisely: true,
  };

  useEffect(() => {
    if (!open) {
      setSelectedSource(undefined);
      setSelectedLines([]);
    }
  }, [open]);
  useEffect(() => setSelectedLines([]), [selectedSource]);

  const selectedSourceIds = useMemo(
    () => (selectedSource === undefined ? [] : [selectedSource]),
    [selectedSource]
  );
  const selectSource = (source?: DocumentCopySource) => {
    setSelectedSource(source?.id);
    onSourceChange(source);
  };

  return (
    <AppActionDrawer
      open={open}
      onClose={onClose}
      title={title}
      width={1500}
      busy={busy}
      actions={
        <>
          <Button onClick={onClose} disabled={busy}>Cancel</Button>
          <Button
            variant="contained"
            disabled={busy || selectedLines.length === 0}
            onClick={() => void onCopy(selectedLines.map(Number), options)}
          >
            OK
          </Button>
        </>
      }
    >
      <Typography variant="body2" color="text.secondary">Standard view</Typography>
      {error && <Alert severity="error" sx={{ mt: 1 }}>{error}</Alert>}
      <Accordion defaultExpanded disableGutters sx={{ mt: 1 }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}><Typography sx={{ fontWeight: 600 }}>{sourceSectionLabel}</Typography></AccordionSummary>
        <AccordionDetails sx={{ pt: 0 }}>
          <Typography variant="overline" sx={{ fontWeight: 700 }}>Headers</Typography>
          <DataGrid
            rows={sources}
            columns={sourceColumns}
            getRowId={(row) => row.id}
            loading={loadingSources}
            selectionMode="single"
            checkboxSelection
            selectedIds={selectedSourceIds}
            onRowClick={selectSource}
            onSelectionChange={(ids) => {
              const id = ids.length ? Number(ids[ids.length - 1]) : undefined;
              selectSource(sources.find((source) => source.id === id));
            }}
            height={225}
            rowHeight={24}
            headerHeight={30}
            showCellBorders
            hideToolbar
            hideFooter
            hideColumnMenu
            hideSidebar
          />
          <Typography variant="overline" sx={{ display: 'block', mt: 1, fontWeight: 700 }}>Lines</Typography>
          <DataGrid
            rows={lines}
            columns={lineColumns}
            getRowId={(row) => row.id}
            loading={loadingLines}
            selectionMode="multiple"
            checkboxSelection
            selectedIds={selectedLines}
            onSelectionChange={setSelectedLines}
            height={260}
            rowHeight={24}
            headerHeight={30}
            showCellBorders
            hideToolbar
            hideFooter
            hideColumnMenu
            hideSidebar
          />
        </AccordionDetails>
      </Accordion>
      {additionalSections.map((label) => (
        <Accordion key={label} disableGutters>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}><Typography sx={{ fontWeight: 600 }}>{label}</Typography></AccordionSummary>
          <AccordionDetails><Box color="text.secondary">This source is not configured for the current document type.</Box></AccordionDetails>
        </Accordion>
      ))}
    </AppActionDrawer>
  );
}
