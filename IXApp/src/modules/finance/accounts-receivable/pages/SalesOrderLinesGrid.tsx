import React, { useRef, useMemo, useEffect, useCallback } from 'react';
import {
  Alert,
  Autocomplete,
  Box,
  Button,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/Delete';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { usePermission } from '@core/permissions/usePermission';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LookupGridField } from '@shared/components/lookups/LookupGridField';
import { SalesLineValueField } from './SalesLineValueField';
import { SalesLineGridSurface } from './SalesLineGridSurface';
import { SalesLineCell, SalesLineCellProvider } from './SalesLineCell';
import { useSalesOrderLineState } from './SalesOrderLineState';
import {
  salesOrderLinesApi,
  type SalesItem,
  type SalesOrderLineRecord,
} from '../api/salesOrderLinesApi';
import type { SalesOrderListRecord } from '../api/salesOrderListApi';

type DetailLine = SalesOrderLineRecord;
const EMPTY_LINES: DetailLine[] = [];
// Most visible cells are plain text. Avoid running MUI/Emotion style processing
// for each one whenever the active editor changes.
const displayCellStyle: React.CSSProperties = {
  width: '100%',
  height: '100%',
  display: 'flex',
  alignItems: 'center',
};
interface Props {
  order: SalesOrderListRecord;
  selectedLineId?: string;
  setSelectedLineId: (id: string | undefined) => void;
  lineFilterVisible: boolean;
  setLineFilterVisible: React.Dispatch<React.SetStateAction<boolean>>;
  refreshOrder: () => Promise<unknown>;
}
export function SalesOrderLinesGrid({
  order,
  selectedLineId,
  setSelectedLineId,
  lineFilterVisible,
  setLineFilterVisible,
  refreshOrder,
}: Props) {
  const { t, currentLanguage } = useAppTranslation();
  const queryClient = useQueryClient();
  const selectedLineIds = useMemo(() => (selectedLineId ? [selectedLineId] : []), [selectedLineId]);
  const handleSelectionChange = useCallback(
    (ids: (string | number)[]) => {
      if (ids[0] != null) setSelectedLineId(String(ids[0]));
    },
    [setSelectedLineId]
  );
  const {
    draftLine,
    setDraftLine,
    lineBaseline,
    setLineBaseline,
    activeField,
    setActiveField,
    savingLine,
    setSavingLine,
    lineError,
    setLineError,
    savedNewRowIdRef,
    pendingSaveRef,
    savingLineRef,
    navigatingCellRef,
    totalsRefreshTimerRef,
  } = useSalesOrderLineState();
  const newRowFocusFrame = useRef<number | null>(null);
  useEffect(
    () => () => {
      if (newRowFocusFrame.current != null) cancelAnimationFrame(newRowFocusFrame.current);
    },
    []
  );
  const scheduleTotalsRefresh = () => {
    void queryClient.invalidateQueries({
      queryKey: ['accounts-receivable', 'sales-orders'],
      refetchType: 'none',
    });
    if (totalsRefreshTimerRef.current) clearTimeout(totalsRefreshTimerRef.current);
    totalsRefreshTimerRef.current = setTimeout(() => {
      totalsRefreshTimerRef.current = null;
      void refreshOrder();
    }, 400);
  };
  const { hasPermission: canEditLines } = usePermission(PERMISSIONS.SALES_ORDER_UPDATE);
  const lineGridRef = useRef<DataGridHandle>(null);
  const addLineButtonRef = useRef<HTMLButtonElement>(null);
  const commitCell = async () => {
    const focusedControl = document.activeElement as HTMLElement | null;
    navigatingCellRef.current = true;
    try {
      const saved = await saveLine();
      if (!saved && document.activeElement === document.body && focusedControl?.isConnected) {
        // Disabling an input during a request moves browser focus to the body.
        // Return to the unsaved value once its editor is enabled for a retry.
        for (let frame = 0; frame < 12; frame++) {
          if (document.activeElement !== document.body || !focusedControl.isConnected) break;
          if (!focusedControl.matches(':disabled, [aria-disabled="true"]')) {
            focusedControl.focus({ preventScroll: true });
            break;
          }
          await new Promise<void>((resolve) => requestAnimationFrame(() => resolve()));
        }
      }
      return saved;
    } finally {
      // The destination editor mounts after the saved row has rendered.
      requestAnimationFrame(() => {
        navigatingCellRef.current = false;
      });
    }
  };
  const removeLineButtonRef = useRef<HTMLButtonElement>(null);
  const linesQuery = useQuery({
    queryKey: ['sales-order-lines', order?.id],
    queryFn: ({ signal }) => salesOrderLinesApi.list(order!.id, signal),
    enabled: Boolean(order),
  });
  const { refetch: refetchLines } = linesQuery;
  const handleRefreshLines = useCallback(() => {
    void refetchLines();
  }, [refetchLines]);
  const unitsQuery = useQuery({
    queryKey: ['sales-order-unit-options'],
    queryFn: async ({ signal }) => {
      const first = await salesOrderLinesApi.units({
        pageNumber: 1,
        pageSize: 100,
        search: '',
        signal,
      });
      const units = [...first.data];
      for (let pageNumber = 2; pageNumber <= first.totalPages; pageNumber++) {
        const page = await salesOrderLinesApi.units({
          pageNumber,
          pageSize: 100,
          search: '',
          signal,
        });
        units.push(...page.data);
      }
      return units;
    },
    enabled: Boolean(draftLine),
  });
  const lines = linesQuery.data ?? EMPTY_LINES;
  const columns = useMemo<ColumnDef<DetailLine>[]>(
    () => [
      {
        field: 'lineType',
        headerName: t('salesOrder.lineType', 'Type'),
        width: 110,
        minWidth: 85,
        pinned: 'left',
        type: 'number',
      },
      {
        field: 'itemNumber',
        headerName: t('salesOrder.itemNumber', 'Item number'),
        width: 160,
        minWidth: 120,
        pinned: 'left',
      },
      { field: 'description', headerName: t('salesOrder.productName', 'Product name'), width: 220 },
      {
        field: 'quantity',
        headerName: 'fields.quantity',
        width: 85,
        type: 'number',
        align: 'right',
      },
      { field: 'unit', headerName: t('salesOrder.unit', 'Unit'), width: 100 },
      { field: 'unitPrice', headerName: t('fields.unitPrice'), width: 110, type: 'number' },
      {
        field: 'deliveryDate',
        headerName: t('fields.requestedDelivery'),
        width: 150,
        type: 'date',
      },
      {
        field: 'taxAmount',
        headerName: t('fields.tax', 'Tax'),
        width: 110,
        type: 'number',
        align: 'right',
      },
      {
        field: 'netAmount',
        headerName: t('salesOrder.netAmount', 'Net Amount'),
        width: 130,
        type: 'number',
        align: 'right',
        valueGetter: ({ row }) => row.lineTotal,
      },
      {
        field: 'deliveryType',
        headerName: t('salesOrder.deliveryType', 'Delivery type'),
        width: 180,
      },
    ],
    [t]
  );
  const activeDraft = draftLine?.orderId === order?.id ? draftLine : null;
  const activeDraftId = activeDraft?.id;
  const handleRowClick = useCallback(
    (line: DetailLine) => {
      if (line.id === activeDraftId) return;
      if (savingLine || (activeDraftId === 'new-sales-line' && line.id !== activeDraftId))
        return;
      setSelectedLineId(line.id);
      if (canEditLines && order.salesStatus.toLowerCase() === 'backorder') {
        setLineBaseline(line);
        setDraftLine({
          ...line,
          deliveryDate: line.deliveryDate?.slice(0, 10),
          orderId: order.id,
        });
      }
    },
    [
      activeDraftId,
      savingLine,
      setSelectedLineId,
      canEditLines,
      order.salesStatus,
      order.id,
      setLineBaseline,
      setDraftLine,
    ]
  );
  const newRow = useMemo<DetailLine>(
    () => ({
      id: 'new-sales-line',
      lineNumber: 0,
      itemNumber: '',
      description: '',
      quantity: 1,
      unit: '',
      unitPrice: 0,
      lineTotal: 0,
      deliveryDate: order.deliveryDate?.slice(0, 10),
    }),
    [order.deliveryDate]
  );
  const gridRows = useMemo(
    () => (activeDraft?.id === 'new-sales-line' ? [...lines, newRow] : lines),
    [lines, newRow, activeDraft?.id]
  );
  const editableFields = new Set([
    'lineType',
    'salesCategory',
    'deliveryType',
    'quantity',
    'unit',
    'unitPrice',
    'deliveryDate',
  ]);
  const handleCellBlur = () => {
    if (activeDraft?.itemNumber && !navigatingCellRef.current) void saveLine();
  };
  const cancelCellEdit = (row: DetailLine) => {
    navigatingCellRef.current = true;
    setLineError('');
    const original =
      lineBaseline?.id === row.id ? lineBaseline : lines.find((line) => line.id === row.id);
    if (original)
      setDraftLine({
        ...original,
        deliveryDate: original.deliveryDate?.slice(0, 10),
        orderId: order.id,
      });
    else {
      setDraftLine(null);
      setSelectedLineId(undefined);
    }
    requestAnimationFrame(() => {
      navigatingCellRef.current = false;
    });
  };
  const handleValueKeyDown: React.KeyboardEventHandler<HTMLDivElement> = (event) => {
    if (event.key === 'Enter') {
      event.preventDefault();
      event.stopPropagation();
      void saveLine();
    }
    if (event.key === 'Escape' && activeDraft) {
      event.preventDefault();
      event.stopPropagation();
      cancelCellEdit(activeDraft);
    }
  };
  const handleItemChange = (_: unknown, item?: SalesItem | null) => {
    if (!item || !activeDraft) return;
    lineGridRef.current?.cancelPendingFocus();
    if (newRowFocusFrame.current != null) {
      cancelAnimationFrame(newRowFocusFrame.current);
      newRowFocusFrame.current = null;
    }
    setDraftLine({
      ...activeDraft,
      itemNumber: item.itemNumber,
      description: item.name,
      itemType: item.itemType,
      unit: item.unit ?? '',
      unitPrice: item.unitPrice ?? 0,
    });
    setActiveField('quantity');
  };
  const handleUnitChange = (_: unknown, unit: string | null) => {
    if (!activeDraft) return;
    const next = { ...activeDraft, unit: unit ?? '' };
    setDraftLine(next);
    if (next.itemNumber) void saveLine(next);
  };
  const gridColumns = useMemo(
    () =>
      columns.map((column): ColumnDef<DetailLine> => ({
        ...column,
        minWidth: column.minWidth ?? 85,
        renderCell: (params) => <SalesLineCell column={column} params={params} />,
      })),
    [columns]
  );
  const renderLineCell = (
    column: ColumnDef<DetailLine>,
    params: Parameters<NonNullable<ColumnDef<DetailLine>['renderCell']>>[0]
  ) => {
    const row = params.row.id === activeDraft?.id ? activeDraft : params.row;
    const value = column.valueGetter
      ? column.valueGetter({ row })
      : row[column.field as keyof DetailLine];
    if (column.field === 'netAmount')
      return (
        <>
          {(row.id === activeDraft?.id
            ? row.quantity * row.unitPrice
            : row.lineTotal
          ).toLocaleString(currentLanguage.code)}
        </>
      );
    if (column.field === 'taxAmount')
      return (
        <>{row.taxAmount == null ? '-' : row.taxAmount.toLocaleString(currentLanguage.code)}</>
      );
    const editable =
      editableFields.has(String(column.field)) ||
      (column.field === 'itemNumber' && !row.itemNumber);
    if (row.id !== activeDraft?.id || activeField !== column.field)
      return (
        <div
          data-grid-cell-focus
          title={String(value ?? '')}
          aria-label={editable ? t(column.headerName) : undefined}
          tabIndex={
            row.id === (selectedLineId ?? lines[0]?.id) && column.field === 'itemNumber' ? 0 : -1
          }
          style={displayCellStyle}
          onFocus={() => {
            if (activeDraft?.id === 'new-sales-line' && row.id !== activeDraft.id) return;
            if (selectedLineId !== row.id) setSelectedLineId(row.id);
            if (
              !editable ||
              !canEditLines ||
              (savingLine && row.id !== activeDraft?.id) ||
              order?.salesStatus.toLowerCase() !== 'backorder'
            )
              return;
            setActiveField(String(column.field));
            if (row.id !== activeDraft?.id && order) {
              setLineBaseline(row);
              setDraftLine({
                ...row,
                deliveryDate: row.deliveryDate?.slice(0, 10),
                orderId: order.id,
              });
            }
          }}
          onClick={(event) => {
            event.stopPropagation();
            event.currentTarget.focus();
          }}
        >
          {String(value ?? '')}
        </div>
      );
    if (column.field === 'itemNumber' && !row.itemNumber)
      return (
        <LookupGridField<SalesItem>
          name="itemNumber"
          label={t('salesOrder.itemNumber', 'Item number')}
          value={null}
          valueField="itemNumber"
          labelField="itemNumber"
          queryKey={['sales-order-items']}
          fetchPage={salesOrderLinesApi.items}
          columns={[
            { field: 'itemNumber', header: t('salesOrder.itemNumber', 'Item number') },
            { field: 'name', header: t('fields.name', 'Name') },
            { field: 'unit', header: t('salesOrder.unit', 'Unit') },
          ]}
          onChange={handleItemChange}
        />
      );
    if (column.field === 'unit')
      return (
        <Box sx={{ width: '100%' }}>
          <Autocomplete
            autoHighlight
            disableClearable
            size="small"
            options={Array.from(
              new Set(
                [row.unit, ...(unitsQuery.data ?? []).map((unit) => unit.symbol)].filter(Boolean)
              )
            )}
            value={row.unit}
            disabled={savingLine}
            loading={unitsQuery.isLoading}
            onChange={handleUnitChange}
            renderInput={(params) => (
              <TextField
                {...params}
                autoFocus
                variant="standard"
                error={unitsQuery.isError}
                helperText={unitsQuery.isError ? unitsQuery.error.message : undefined}
                slotProps={{
                  ...params.slotProps,
                  htmlInput: {
                    ...params.slotProps.htmlInput,
                    'aria-label': t('salesOrder.unit', 'Unit'),
                  },
                }}
              />
            )}
          />
        </Box>
      );
    if (column.field === 'lineType' || column.field === 'deliveryType') {
      const options =
        column.field === 'lineType'
          ? [
              'Journal',
              'Quotation',
              'Subscription',
              'Sales',
              'Return item',
              'Blanket',
              'Item requirement',
              'Prepayment',
            ]
          : ['None', 'Pickup', 'Direct delivery'];
      return (
        <TextField
          select
          autoFocus
          variant="standard"
          size="small"
          fullWidth
          disabled={savingLine}
          value={value ?? (column.field === 'lineType' ? 3 : 0)}
          slotProps={{ select: { 'aria-label': t(column.headerName) } }}
          onChange={(event) =>
            setDraftLine((draft) =>
              draft ? { ...draft, [column.field]: Number(event.target.value) } : draft
            )
          }
          onBlur={handleCellBlur}
        >
          {options.map((label, index) => (
            <MenuItem key={index} value={index}>
              {label}
            </MenuItem>
          ))}
        </TextField>
      );
    }
    if (!editableFields.has(String(column.field))) return <>{String(value ?? '')}</>;
    const invalidCell =
      Boolean(lineError) &&
      ((column.field === 'quantity' && (!Number.isFinite(row.quantity) || row.quantity <= 0)) ||
        (column.field === 'unitPrice' && (!Number.isFinite(row.unitPrice) || row.unitPrice < 0)));
    return (
      <SalesLineValueField
        autoFocus
        error={invalidCell}
        variant="standard"
        size="small"
        fullWidth
        disabled={savingLine}
        sx={{
          '& input[type=number]': { MozAppearance: 'textfield' },
          '& input::-webkit-inner-spin-button, & input::-webkit-outer-spin-button': {
            WebkitAppearance: 'none',
            margin: 0,
          },
        }}
        type={column.type === 'number' ? 'number' : column.type === 'date' ? 'date' : 'text'}
        value={value ?? ''}
        slotProps={{
          htmlInput: {
            'aria-label': t(column.headerName),
            'aria-describedby': lineError ? 'sales-line-error' : undefined,
          },
        }}
        onKeyDown={handleValueKeyDown}
        onBlur={handleCellBlur}
        onChange={(event) => {
          const next = column.type === 'number' ? Number(event.target.value) : event.target.value;
          setDraftLine((draft) => (draft ? { ...draft, [column.field]: next } : draft));
        }}
      />
    );
  };
  const saveLine = (line = activeDraft): Promise<boolean> => {
    if (pendingSaveRef.current) return pendingSaveRef.current;
    const work = persistLine(line);
    pendingSaveRef.current = work;
    void work.finally(() => {
      if (pendingSaveRef.current === work) pendingSaveRef.current = null;
    });
    return work;
  };
  const persistLine = async (line = activeDraft) => {
    if (!line || !order) return true;
    if (savingLineRef.current) return false;
    if (
      !line.itemNumber ||
      !Number.isFinite(line.quantity) ||
      line.quantity <= 0 ||
      !Number.isFinite(line.unitPrice) ||
      line.unitPrice < 0
    ) {
      setLineError(
        t(
          'salesOrder.invalidLine',
          'Select an item and enter a positive quantity and a nonnegative unit price.'
        )
      );
      return false;
    }
    const baseline = lineBaseline;
    const fields = [
      'itemNumber',
      'quantity',
      'unit',
      'unitPrice',
      'lineType',
      'deliveryType',
      'salesCategory',
      'deliveryDate',
    ] as const;
    if (
      line.id !== 'new-sales-line' &&
      baseline?.id === line.id &&
      fields.every((field) =>
        field === 'deliveryDate'
          ? baseline[field]?.slice(0, 10) === line[field]?.slice(0, 10)
          : baseline[field] === line[field]
      )
    )
      return true;
    savingLineRef.current = true;
    setSavingLine(true);
    setLineError('');
    try {
      const saved =
        line.id !== 'new-sales-line'
          ? await salesOrderLinesApi.update(order.id, line)
          : await salesOrderLinesApi.add(order.id, {
              itemNumber: line.itemNumber,
              salesCategory: line.salesCategory ?? 0,
              lineType: line.lineType ?? 3,
              deliveryType: line.deliveryType ?? 0,
              quantity: line.quantity,
              unit: line.unit,
              unitPrice: line.unitPrice,
              deliveryDate: line.deliveryDate || undefined,
            });
      if (line.id === 'new-sales-line') savedNewRowIdRef.current = saved.id;
      setLineBaseline({ ...line, ...saved });
      setDraftLine({
        ...line,
        ...saved,
        deliveryDate: (saved.deliveryDate ?? line.deliveryDate)?.slice(0, 10),
        orderId: order.id,
      });
      setSelectedLineId(saved.id);
      await queryClient.cancelQueries({ queryKey: ['sales-order-lines', order.id] });
      const confirmed = { ...line, ...saved };
      queryClient.setQueryData<DetailLine[]>(['sales-order-lines', order.id], (current = []) =>
        current.some((row) => row.id === confirmed.id)
          ? current.map((row) => (row.id === confirmed.id ? confirmed : row))
          : [...current, confirmed]
      );
      scheduleTotalsRefresh();
      return true;
    } catch (error) {
      setLineError(error instanceof Error ? error.message : t('errors.generic'));
      return false;
    } finally {
      savingLineRef.current = false;
      setSavingLine(false);
    }
  };
  const removeLine = async () => {
    if (!order || savingLineRef.current) return;
    const lineId = selectedLineId ?? activeDraft?.id;
    if (!lineId) return;
    if (lineId === 'new-sales-line') {
      setDraftLine(null);
      setSelectedLineId(undefined);
      setLineError('');
      return;
    }
    savingLineRef.current = true;
    setSavingLine(true);
    setLineError('');
    try {
      await salesOrderLinesApi.remove(order.id, lineId);
      if (activeDraft?.id === lineId) setDraftLine(null);
      setSelectedLineId(undefined);
      await queryClient.cancelQueries({ queryKey: ['sales-order-lines', order.id] });
      queryClient.setQueryData<DetailLine[]>(['sales-order-lines', order.id], (current = []) =>
        current.filter((row) => row.id !== lineId)
      );
      scheduleTotalsRefresh();
    } catch (error) {
      setLineError(error instanceof Error ? error.message : t('errors.generic'));
    } finally {
      savingLineRef.current = false;
      setSavingLine(false);
    }
  };
  return (
    <>
      <Stack
        direction="row"
        spacing={0.5}
        sx={{
          mb: 0.5,
          minHeight: 28,
          overflowX: 'auto',
          whiteSpace: 'nowrap',
          '& .MuiButton-root': {
            flexShrink: 0,
            fontSize: 12,
            minWidth: 0,
            px: 0.75,
            py: 0.25,
            minHeight: 28,
          },
        }}
      >
        <Button
          disabled={
            savingLine ||
            activeDraft?.id === 'new-sales-line' ||
            !canEditLines ||
            order.salesStatus.toLowerCase() !== 'backorder'
          }
          ref={addLineButtonRef}
          onMouseDown={(event) => event.preventDefault()}
          onClick={async () => {
            if (activeDraft && !(await saveLine())) return;
            setLineBaseline(null);
            setActiveField('itemNumber');
            setSelectedLineId('new-sales-line');
            setLineError('');
            setLineFilterVisible(false);
            lineGridRef.current?.clearFilters();
            setDraftLine({ ...newRow, orderId: order.id });
            newRowFocusFrame.current = requestAnimationFrame(() => {
              newRowFocusFrame.current = null;
              void lineGridRef.current?.focusRecordCell({
                rowId: 'new-sales-line',
                field: 'itemNumber',
              });
            });
          }}
          size="small"
          startIcon={<AddIcon />}
        >
          {t('actions.addLine', 'Add line')}
        </Button>
        <Button disabled size="small" startIcon={<AddIcon />}>
          {t('salesOrder.addLines', 'Add lines')}
        </Button>
        <Button disabled size="small">
          {t('salesOrder.addProducts', 'Add products')}
        </Button>
        <Button
          disabled={
            savingLine ||
            !canEditLines ||
            order.salesStatus.toLowerCase() !== 'backorder' ||
            !(selectedLineId ?? activeDraft?.id)
          }
          ref={removeLineButtonRef}
          onMouseDown={(event) => event.preventDefault()}
          onClick={() => void removeLine()}
          size="small"
          startIcon={<DeleteOutlineIcon />}
        >
          {t('actions.remove', 'Remove')}
        </Button>
        {[
          'Sales order line',
          'Financials',
          'Inventory',
          'Product and supply',
          'Update line',
          'Warehouse',
          'Retail',
          'Engineering change',
        ].map((label) => (
          <Button key={label} disabled endIcon={<ExpandMoreIcon />}>
            {label}
          </Button>
        ))}
      </Stack>
      {lineError && (
        <Alert id="sales-line-error" severity="error">
          {lineError}
        </Alert>
      )}
      <Typography
        variant="caption"
        color="text.secondary"
        role="status"
        sx={{ display: 'block', minHeight: 22 }}
      >
        {savingLine
          ? t('common.saving', 'Saving...')
          : t(
              'salesOrder.gridHelp',
              'Tab / Enter: next cell | Shift: previous | Up / Down: next row | F2: edit | Insert: add | Alt+Delete: remove | Ctrl+F: filter'
            )}
      </Typography>
      {linesQuery.isError && (
        <ErrorState message={linesQuery.error.message} onRetry={() => void linesQuery.refetch()} />
      )}
      <SalesLineGridSurface
        gridRef={lineGridRef}
        resolveRowId={(id) => (id === 'new-sales-line' ? (savedNewRowIdRef.current ?? id) : id)}
        onPasteError={() =>
          setLineError(
            t(
              'salesOrder.pasteSingleCell',
              'Paste one cell at a time. Select a value in the lookup for item and unit fields.'
            )
          )
        }
        commit={commitCell}
        onAdd={() => addLineButtonRef.current?.click()}
        onRemove={() => removeLineButtonRef.current?.click()}
        onFilter={() => {
          setLineFilterVisible(true);
          requestAnimationFrame(() => lineGridRef.current?.focusFilter());
        }}
      >
        <SalesLineCellProvider renderCell={renderLineCell}>
          <DataGrid
            ref={lineGridRef}
            key={order.id}
            rows={gridRows}
            selectionMode="single"
            onSelectionChange={handleSelectionChange}
            selectedIds={selectedLineIds}
            onRowClick={handleRowClick}
            columns={gridColumns}
            height={232}
            rowHeight={33}
            headerHeight={32}
            hideFooter
            hideColumnMenu={false}
            showColumnBorders={false}
            showCellBorders
            hideAddRowButton
            hideToolbar
            loading={linesQuery.isLoading}
            hideFilterRow={!lineFilterVisible}
            hideSidebarTabs
            onRefresh={handleRefreshLines}
            storageKey="accounts-receivable.sales-order-lines.compact"
          />
        </SalesLineCellProvider>
      </SalesLineGridSurface>
    </>
  );
}
