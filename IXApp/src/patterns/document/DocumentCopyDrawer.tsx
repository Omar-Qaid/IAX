import React, { useEffect, useMemo, useState } from 'react';
import { Alert, Box, Button, Typography } from '@mui/material';
import { FastTab } from '@shared/components/fast-tabs/FastTab';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';

export interface DocumentCopySource {
  id: string;
  documentNumber: string;
  account: string;
  accountName: string;
  createdAt: string;
  currencyCode: string;
  sourceType: string;
}

export interface DocumentCopyLine {
  id: string;
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
  onCopy: (lineIds: string[], options: DocumentCopyOptions) => Promise<void> | void;
}

const sourceColumns: ColumnDef<DocumentCopySource>[] = [
  { field: 'documentNumber', headerName: 'documentCopy.fields.document', width: 145 },
  { field: 'account', headerName: 'fields.customerAccount', width: 145 },
  { field: 'accountName', headerName: 'documentCopy.fields.customerName', minWidth: 220, flex: 1 },
  { field: 'createdAt', headerName: 'salesOrder.createdDateTime', width: 190, type: 'date' },
  { field: 'currencyCode', headerName: 'fields.currency', width: 100 },
];

const lineColumns: ColumnDef<DocumentCopyLine>[] = [
  { field: 'itemId', headerName: 'salesOrder.itemNumber', width: 135 },
  { field: 'description', headerName: 'salesOrder.text', minWidth: 190, flex: 1 },
  { field: 'site', headerName: 'fields.site', width: 100 },
  { field: 'warehouse', headerName: 'fields.warehouse', width: 120 },
  { field: 'quantity', headerName: 'fields.quantity', width: 100, type: 'number', align: 'right' },
  { field: 'unit', headerName: 'salesOrder.unit', width: 80 },
  {
    field: 'unitPrice',
    headerName: 'fields.unitPrice',
    width: 110,
    type: 'number',
    align: 'right',
  },
  {
    field: 'netAmount',
    headerName: 'salesOrder.netAmount',
    width: 110,
    type: 'number',
    align: 'right',
  },
  { field: 'discount', headerName: 'fields.discount', width: 100, type: 'number', align: 'right' },
  {
    field: 'discountPercent',
    headerName: 'documentCopy.fields.discountPercent',
    width: 125,
    type: 'number',
    align: 'right',
  },
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
  const [selectedSource, setSelectedSource] = useState<string>();
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
          <Button onClick={onClose} disabled={busy}>
            Cancel
          </Button>
          <Button
            variant="contained"
            disabled={busy || selectedLines.length === 0}
            onClick={() => void onCopy(selectedLines.map(String), options)}
          >
            OK
          </Button>
        </>
      }
    >
      <Typography variant="body2" color="text.secondary">
        Standard view
      </Typography>
      {error && (
        <Alert severity="error" sx={{ mt: 1 }}>
          {error}
        </Alert>
      )}
      <FastTab
        title={sourceSectionLabel}
        appearance="enterprise"
        sx={{ mt: 1 }}
        detailsSx={{ pt: 0 }}
      >
        <Typography variant="overline" sx={{ fontWeight: 700 }}>
          Headers
        </Typography>
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
            const id = ids.length ? String(ids[ids.length - 1]) : undefined;
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
        <Typography variant="overline" sx={{ display: 'block', mt: 1, fontWeight: 700 }}>
          Lines
        </Typography>
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
      </FastTab>
      {additionalSections.map((label) => (
        <FastTab key={label} title={label} appearance="enterprise" defaultExpanded={false}>
          <Box color="text.secondary">
            This source is not configured for the current document type.
          </Box>
        </FastTab>
      ))}
    </AppActionDrawer>
  );
}
