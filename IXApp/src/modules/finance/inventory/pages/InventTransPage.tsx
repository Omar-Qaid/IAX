import React, { useEffect, useMemo, useState } from 'react';
import {
  Box,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Typography,
} from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { inventTransApi, type InventTransRecord } from '../api/inventTransApi';

type TransactionFilter = 'all' | 'receipts' | 'issues' | 'open';
type DialogMode = 'details' | 'dimensions' | 'summary' | null;

const hasReceipt = (row: InventTransRecord): boolean => Boolean(row.receiptStatus);
const hasIssue = (row: InventTransRecord): boolean => Boolean(row.issueStatus);

export function InventTransPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const itemIdFilter = searchParams.get('itemId')?.trim() ?? '';
  const salesIdFilter = searchParams.get('salesId')?.trim() ?? '';
  const [transactionFilter, setTransactionFilter] = useState<TransactionFilter>('all');
  const [selected, setSelected] = useState<InventTransRecord | null>(null);
  const [dialogMode, setDialogMode] = useState<DialogMode>(null);
  const query = useQuery({
    queryKey: ['inventory', 'transactions'],
    queryFn: ({ signal }) => inventTransApi.list(signal),
  });

  const rows = useMemo(() => {
    const transactions = (query.data ?? []).filter(
      (row) =>
        (!itemIdFilter || row.itemNumber === itemIdFilter) &&
        (!salesIdFilter || row.referenceNumber === salesIdFilter)
    );
    if (transactionFilter === 'receipts') return transactions.filter(hasReceipt);
    if (transactionFilter === 'issues') return transactions.filter(hasIssue);
    if (transactionFilter === 'open') {
      return transactions.filter(
        (row) => row.issueStatus === 'On order' || row.receiptStatus === 'Ordered'
      );
    }
    return transactions;
  }, [itemIdFilter, query.data, salesIdFilter, transactionFilter]);

  useEffect(() => {
    setSelected((current) =>
      current && rows.some((row) => row.id === current.id) ? current : (rows[0] ?? null)
    );
  }, [rows]);

  const columns = useMemo<ColumnDef<InventTransRecord>[]>(
    () => [
      {
        field: 'itemNumber',
        headerName: t('inventTrans.fields.itemNumber'),
        width: 145,
        pinned: 'left',
      },
      {
        field: 'physicalDate',
        headerName: t('inventTrans.fields.physicalDate'),
        width: 130,
        type: 'date',
      },
      {
        field: 'financialDate',
        headerName: t('inventTrans.fields.financialDate'),
        width: 130,
        type: 'date',
      },
      { field: 'reference', headerName: t('inventTrans.fields.reference'), width: 175 },
      { field: 'referenceNumber', headerName: t('inventTrans.fields.number'), width: 160 },
      { field: 'receiptStatus', headerName: t('inventTrans.fields.receipt'), width: 120 },
      { field: 'issueStatus', headerName: t('inventTrans.fields.issue'), width: 120 },
      {
        field: 'quantity',
        headerName: t('inventTrans.fields.quantity'),
        width: 110,
        type: 'number',
        align: 'right',
      },
      {
        field: 'unitPrice',
        headerName: t('inventTrans.fields.unitPrice'),
        width: 110,
        type: 'number',
        align: 'right',
      },
      {
        field: 'unitCost',
        headerName: t('inventTrans.fields.unitCost'),
        width: 110,
        type: 'number',
        align: 'right',
      },
      {
        field: 'costAmount',
        headerName: t('inventTrans.fields.costAmount'),
        width: 125,
        type: 'number',
        align: 'right',
      },
      { field: 'site', headerName: t('inventTrans.fields.site'), width: 90 },
      { field: 'warehouse', headerName: t('inventTrans.fields.warehouse'), width: 115 },
      { field: 'currencyCode', headerName: t('fields.currency'), width: 95 },
    ],
    [t]
  );

  const config: EnterpriseListConfig<InventTransRecord> = {
    readOnly: true,
    showFilterOnLoad: true,
    backCommand: { label: t('actions.back'), onClick: () => navigate(-1) },
    showSearchCommand: true,
    recordTableName: 'InventTrans',
    getAuditRecordId: (row) => row.recId,
    contextLabel: t('pages.inventTrans.title'),
    viewLabel: t('common.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchFields: [
      { field: 'itemNumber', label: t('inventTrans.fields.itemNumber') },
      { field: 'referenceNumber', label: t('inventTrans.fields.number') },
      { field: 'inventTransId', label: t('inventTrans.fields.transactionId') },
    ],
    locale: currentLanguage.code,
    crud: { editLabel: '', newLabel: '', deleteLabel: '' },
    commands: [
      {
        id: 'details',
        label: t('inventTrans.actions.details'),
        disabled: !selected,
        onClick: () => setDialogMode('details'),
      },
      {
        id: 'dimensions',
        label: t('inventTrans.actions.dimensions'),
        disabled: !selected,
        onClick: () => setDialogMode('dimensions'),
      },
      {
        id: 'summation',
        label: t('inventTrans.actions.summation'),
        disabled: rows.length === 0,
        onClick: () => setDialogMode('summary'),
      },
      { id: 'split', label: t('inventTrans.actions.split'), disabled: true },
      { id: 'archived', label: t('inventTrans.actions.archived'), disabled: true },
      { id: 'inventory', label: t('inventTrans.actions.inventory'), disabled: true },
      { id: 'ledger', label: t('inventTrans.actions.ledger'), disabled: true },
    ],
    utilities: {
      personalizeLabel: t('utilities.personalize'),
      guideLabel: t('utilities.guide'),
      notificationsLabel: t('common.notifications'),
      refreshLabel: t('actions.refresh'),
      openWindowLabel: t('utilities.openWindow'),
      notificationCount: 0,
    },
    advancedFilter: {
      title: t('filters.title'),
      addLabel: t('actions.add'),
      fieldLabel: t('filters.field'),
      operatorLabel: t('filters.operator'),
      applyLabel: t('actions.apply'),
      resetLabel: t('actions.reset'),
      fields: columns.map((column) => ({
        field: column.field as keyof InventTransRecord & string,
        label: column.headerName,
      })),
      matches: () => true,
    },
  };

  return (
    <SimpleListPage
      title={t('pages.inventTrans.title')}
      enterpriseConfig={config}
      dataSource={{ type: 'controlled', rows }}
      columns={columns}
      loading={query.isLoading}
      error={query.error instanceof Error ? query.error.message : null}
      onRetry={() => query.refetch()}
      filterBar={
        <Box sx={{ mx: { xs: 1, sm: 2.5 }, mb: 1, width: 180 }}>
          <FormControl fullWidth size="small">
            <InputLabel>{t('inventTrans.transactionFilter')}</InputLabel>
            <Select
              value={transactionFilter}
              label={t('inventTrans.transactionFilter')}
              onChange={(event) => setTransactionFilter(event.target.value as TransactionFilter)}
            >
              <MenuItem value="all">{t('inventTrans.filters.all')}</MenuItem>
              <MenuItem value="receipts">{t('inventTrans.filters.receipts')}</MenuItem>
              <MenuItem value="issues">{t('inventTrans.filters.issues')}</MenuItem>
              <MenuItem value="open">{t('inventTrans.filters.open')}</MenuItem>
            </Select>
          </FormControl>
        </Box>
      }
      dataGridProps={{
        storageKey: 'inventory.transactions.standard-view',
        onRowClick: setSelected,
        onSelectionChange: (ids) => setSelected(rows.find((row) => ids.includes(row.id)) ?? null),
      }}
      dialogs={
        <InventTransDialog
          mode={dialogMode}
          selected={selected}
          rows={rows}
          onClose={() => setDialogMode(null)}
        />
      }
    />
  );
}

function InventTransDialog({
  mode,
  selected,
  rows,
  onClose,
}: {
  mode: DialogMode;
  selected: InventTransRecord | null;
  rows: InventTransRecord[];
  onClose: () => void;
}): React.ReactElement {
  const { t } = useAppTranslation();
  const summary = useMemo(
    () => ({
      quantity: rows.reduce((total, row) => total + row.quantity, 0),
      costAmount: rows.reduce((total, row) => total + row.costAmount, 0),
      receipts: rows.filter(hasReceipt).length,
      issues: rows.filter(hasIssue).length,
    }),
    [rows]
  );
  const values =
    mode === 'dimensions' && selected
      ? [
          [t('inventTrans.fields.site'), selected.site],
          [t('inventTrans.fields.warehouse'), selected.warehouse],
          [t('inventTrans.fields.batchNumber'), selected.batchNumber],
          [t('inventTrans.fields.serialNumber'), selected.serialNumber],
        ]
      : mode === 'details' && selected
        ? [
            [t('inventTrans.fields.transactionId'), selected.inventTransId],
            [t('inventTrans.fields.reference'), selected.reference],
            [t('inventTrans.fields.number'), selected.referenceNumber],
            [t('inventTrans.fields.expectedDate'), selected.expectedDate ?? ''],
            [t('inventTrans.fields.voucher'), selected.voucher],
            [t('fields.currency'), selected.currencyCode],
          ]
        : [
            [t('inventTrans.summary.transactions'), String(rows.length)],
            [t('inventTrans.summary.receipts'), String(summary.receipts)],
            [t('inventTrans.summary.issues'), String(summary.issues)],
            [t('inventTrans.fields.quantity'), summary.quantity.toLocaleString()],
            [t('inventTrans.fields.costAmount'), summary.costAmount.toLocaleString()],
          ];
  const title =
    mode === 'dimensions'
      ? t('inventTrans.actions.dimensions')
      : mode === 'summary'
        ? t('inventTrans.actions.summation')
        : t('inventTrans.actions.details');

  return (
    <Dialog open={mode !== null} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>{title}</DialogTitle>
      <DialogContent dividers>
        <Stack spacing={1.25}>
          {values.map(([label, value]) => (
            <Box
              key={label}
              sx={{ display: 'grid', gridTemplateColumns: 'minmax(150px, 40%) 1fr', gap: 2 }}
            >
              <Typography color="text.secondary" variant="body2">
                {label}
              </Typography>
              <Typography variant="body2">{value || '—'}</Typography>
            </Box>
          ))}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>{t('actions.close', 'Close')}</Button>
      </DialogActions>
    </Dialog>
  );
}
