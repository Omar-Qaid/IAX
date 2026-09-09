import React, { useRef, useState } from 'react';
import {
  Alert,
  Autocomplete,
  Box,
  Button,
  MenuItem,
  Stack,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/Delete';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { useNavigate, useParams } from 'react-router-dom';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailFieldConfig, DetailSectionConfig } from '@patterns/list-details/types';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { SalesLineGridSurface } from './SalesLineGridSurface';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import { LookupGridField } from '@shared/components/lookups/LookupGridField';
import {
  salesOrderLinesApi,
  type SalesItem,
  type SalesOrderLineRecord,
} from '../api/salesOrderLinesApi';
import { PERMISSIONS } from '@core/permissions/permissions';
import { usePermission } from '@core/permissions/usePermission';
import { useQuery } from '@tanstack/react-query';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { salesOrderListApi, type SalesOrderHeaderInput } from '../api/salesOrderListApi';
import { useAppTranslation } from '@core/localization/useAppTranslation';

type DetailLine = SalesOrderLineRecord;

export function SalesOrderDetailsPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const navigate = useNavigate();
  const { salesOrderId } = useParams<{ salesOrderId: string }>();
  const [draftLine, setDraftLine] = useState<(DetailLine & { orderId: string }) | null>(null);
  const [headerDraft, setHeaderDraft] = useState<(SalesOrderHeaderInput & { id: string }) | null>(
    null
  );
  const [savingHeader, setSavingHeader] = useState(false);
  const headerSaveLock = useRef(false);
  const [headerError, setHeaderError] = useState('');
  useUnsavedChanges(Boolean(headerDraft));
  const lineGridRef = useRef<DataGridHandle>(null);
  const addLineButtonRef = useRef<HTMLButtonElement>(null);
  const commitCell = async () => {
    navigatingCellRef.current = true;
    try {
      return await saveLine();
    } finally {
      // The destination editor mounts after the saved row has rendered.
      requestAnimationFrame(() => {
        navigatingCellRef.current = false;
      });
    }
  };
  const removeLineButtonRef = useRef<HTMLButtonElement>(null);
  const navigatingCellRef = useRef(false);
  const [lineBaseline, setLineBaseline] = useState<DetailLine | null>(null);
  const [activeField, setActiveField] = useState('itemNumber');
  const [savingLine, setSavingLine] = useState(false);
  const savingLineRef = useRef(false);
  const [lineError, setLineError] = useState('');
  const [lineFilterVisible, setLineFilterVisible] = useState(false);
  const { hasPermission: canEditLines } = usePermission(PERMISSIONS.SALES_ORDER_UPDATE);
  const [lineTab, setLineTab] = useState('General');
  const [tab, setTab] = useState('lines');
  const [selectedLineId, setSelectedLineId] = useState<string>();
  const orderQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-orders'],
    queryFn: ({ signal }) => salesOrderListApi.list(signal),
  });
  const orders = orderQuery.data ?? [];
  const order = salesOrderId
    ? orders.find((candidate) => candidate.id === salesOrderId)
    : orders[0];
  const activeHeader = headerDraft?.id === order?.id ? headerDraft : null;
  const canEditHeader = canEditLines && order?.salesStatus.toLowerCase() === 'backorder';
  const startHeaderEdit = () => {
    if (!order || !canEditHeader) return;
    setHeaderError('');
    setHeaderDraft({
      id: order.id,
      invoiceAccount: order.invoiceAccount,
      currencyCode: order.currencyCode,
      customerReference: order.customerReference,
      paymentTerms: order.paymentTerms,
      deliveryMode: order.deliveryMode,
      deliveryDate: order.deliveryDate?.slice(0, 10),
    });
  };
  const saveHeader = async () => {
    if (!activeHeader || !canEditHeader || headerSaveLock.current) return;
    if (
      !activeHeader.invoiceAccount.trim() ||
      !activeHeader.currencyCode.trim() ||
      !activeHeader.deliveryDate
    ) {
      setHeaderError(t('validation.required', 'This field is required.'));
      return;
    }
    headerSaveLock.current = true;
    setSavingHeader(true);
    setHeaderError('');
    try {
      const { id, ...input } = activeHeader;
      await salesOrderListApi.updateHeader(id, input);
      await orderQuery.refetch({ throwOnError: true });
      setHeaderDraft(null);
    } catch (error) {
      setHeaderError(error instanceof Error ? error.message : t('errors.generic'));
    } finally {
      headerSaveLock.current = false;
      setSavingHeader(false);
    }
  };
  const headerInput = (name: keyof SalesOrderHeaderInput, label: string) => (
    <TextField
      fullWidth
      size="small"
      variant="standard"
      label={label}
      type={name === 'deliveryDate' ? 'date' : 'text'}
      value={activeHeader?.[name] ?? ''}
      disabled={savingHeader}
      slotProps={{ inputLabel: { shrink: true } }}
      onChange={(event) =>
        setHeaderDraft((draft) => (draft ? { ...draft, [name]: event.target.value } : draft))
      }
    />
  );
  const linesQuery = useQuery({
    queryKey: ['sales-order-lines', order?.id],
    queryFn: ({ signal }) => salesOrderLinesApi.list(order!.id, signal),
    enabled: Boolean(order),
  });
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
  const lines = linesQuery.data ?? [];
  const columns: ColumnDef<DetailLine>[] = [
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
    { field: 'deliveryDate', headerName: t('fields.requestedDelivery'), width: 150, type: 'date' },
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
  ];
  const activeDraft = draftLine?.orderId === order?.id ? draftLine : null;
  const editableFields = new Set([
    'lineType',
    'salesCategory',
    'deliveryType',
    'quantity',
    'unit',
    'unitPrice',
    'deliveryDate',
  ]);
  const gridColumns = columns.map((column): ColumnDef<DetailLine> => ({
    ...column,
    minWidth: column.minWidth ?? 85,
    renderCell: ({ row, value }) => {
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
          <Box
            data-grid-cell-focus
            title={String(value ?? '')}
            aria-label={editable ? t(column.headerName) : undefined}
            tabIndex={
              row.id === (selectedLineId ?? lines[0]?.id) && column.field === 'itemNumber' ? 0 : -1
            }
            sx={{
              width: '100%',
              height: '100%',
              display: 'flex',
              alignItems: 'center',
              outlineOffset: -2,
              '&:focus-visible': { outline: '2px solid', outlineColor: 'primary.main' },
            }}
            onFocus={() => {
              setSelectedLineId(row.id);
              if (
                !editable ||
                !canEditLines ||
                (savingLine && row.id !== activeDraft?.id) ||
                order?.salesStatus.toLowerCase() !== 'backorder'
              )
                return;
              if (activeDraft?.id === 'new-sales-line' && row.id !== activeDraft.id) return;
              setActiveField(String(column.field));
              setSelectedLineId(row.id);
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
          </Box>
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
            onChange={(_, item) => {
              if (item && activeDraft) {
                const next = {
                  ...activeDraft,
                  itemNumber: item.itemNumber,
                  description: item.name,
                  itemType: item.itemType,
                  unit: item.unit ?? '',
                  unitPrice: item.unitPrice ?? 0,
                };
                setDraftLine(next);
                setActiveField('quantity');
              }
            }}
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
              value={row.unit || undefined}
              disabled={savingLine || unitsQuery.isLoading}
              onChange={(_, unit) => {
                const next = { ...activeDraft, unit: unit ?? '' };
                setDraftLine(next);
                if (next.itemNumber) void saveLine(next);
              }}
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
            slotProps={{ htmlInput: { 'aria-label': t(column.headerName) } }}
            onChange={(event) =>
              setDraftLine((draft) =>
                draft ? { ...draft, [column.field]: Number(event.target.value) } : draft
              )
            }
            onBlur={() => {
              if (activeDraft?.itemNumber && !navigatingCellRef.current) void saveLine();
            }}
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
        <TextField
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
          onKeyDown={(event) => {
            if (event.key === 'Enter') {
              event.preventDefault();
              event.stopPropagation();
              void saveLine();
            }
            if (event.key === 'Escape') {
              event.preventDefault();
              event.stopPropagation();
              const original = lines.find((line) => line.id === row.id);
              if (original)
                setDraftLine({
                  ...original,
                  deliveryDate: original.deliveryDate?.slice(0, 10),
                  orderId: activeDraft.orderId,
                });
              else setDraftLine(null);
            }
          }}
          onBlur={() => {
            if (activeDraft?.itemNumber && !navigatingCellRef.current) void saveLine();
          }}
          onChange={(event) => {
            const next = column.type === 'number' ? Number(event.target.value) : event.target.value;
            setDraftLine((draft) => (draft ? { ...draft, [column.field]: next } : draft));
          }}
        />
      );
    },
  }));
  const saveLine = async (line = activeDraft) => {
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
      setLineBaseline({ ...line, ...saved });
      setDraftLine({
        ...line,
        ...saved,
        deliveryDate: (saved.deliveryDate ?? line.deliveryDate)?.slice(0, 10),
        orderId: order.id,
      });
      setSelectedLineId(saved.id);
      await Promise.all([linesQuery.refetch(), orderQuery.refetch()]);
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
      await Promise.all([linesQuery.refetch(), orderQuery.refetch()]);
    } catch (error) {
      setLineError(error instanceof Error ? error.message : t('errors.generic'));
    } finally {
      savingLineRef.current = false;
      setSavingLine(false);
    }
  };
  if (orderQuery.isPending) return <LoadingState />;
  if (orderQuery.isError)
    return (
      <ErrorState message={orderQuery.error.message} onRetry={() => void orderQuery.refetch()} />
    );
  if (!order) return <ErrorState message={t('messages.noSalesOrders')} />;
  const amount = (value: number) =>
    `${value.toLocaleString(currentLanguage.code)} ${order.currencyCode}`;
  const field = (label: string, value?: string) => (
    <Box key={label}>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Typography
        variant="body2"
        sx={{ borderBottom: 1, borderColor: 'divider', py: 0.5, minHeight: 30 }}
      >
        {value || '-'}
      </Typography>
    </Box>
  );
  const header = (
    <Box
      sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: 2 }}
    >
      {field(t('fields.customerAccount'), order.customerAccount)}
      {field(t('fields.customerName'), order.customerName)}
      {activeHeader
        ? headerInput('invoiceAccount', t('fields.invoiceAccount'))
        : field(t('fields.invoiceAccount'), order.invoiceAccount)}
      {activeHeader
        ? headerInput('customerReference', t('fields.customerReference', 'Customer reference'))
        : field(t('fields.customerReference', 'Customer reference'), order.customerReference)}
      {activeHeader
        ? headerInput('currencyCode', t('fields.currency'))
        : field(t('fields.currency'), order.currencyCode)}
      {activeHeader
        ? headerInput('paymentTerms', t('fields.paymentTerms'))
        : field(t('fields.paymentTerms'), order.paymentTerms)}
      {activeHeader
        ? headerInput('deliveryMode', t('fields.deliveryMode'))
        : field(t('fields.deliveryMode'), order.deliveryMode)}
      {field(t('customerQuickCreate.fields.deliveryTerms'))}
      {field(t('fields.orderDate'))}
      {activeHeader
        ? headerInput('deliveryDate', t('fields.requestedDelivery'))
        : field(t('fields.requestedDelivery'), order.deliveryDate)}
    </Box>
  );
  const selectedLine: DetailLine | undefined =
    lines.find((line) => line.id === selectedLineId) ?? lines[0];
  const section = (
    title: string,
    content: React.ReactNode,
    expanded = true
  ): DetailSectionConfig => ({
    id: `${tab}-${title}`,
    title,
    content,
    defaultExpanded: expanded,
    visualVariant: 'legalEntity',
  });
  const headerField = (name: string, label: string, sectionTitle?: string): DetailFieldConfig => ({
    name,
    label: t(`salesOrder.headerFields.${name}`, label),
    type: 'display',
    sectionTitle,
    ...(activeHeader && name in activeHeader && name !== 'id'
      ? {
          renderOwnLabel: true,
          render: () =>
            headerInput(
              name as keyof SalesOrderHeaderInput,
              t(`salesOrder.headerFields.${name}`, label)
            ),
        }
      : {}),
  });
  const headerSections: DetailSectionConfig[] = [
    {
      summaryItems: [
        { id: 'salesId', label: t('fields.salesOrderNumber'), value: order.salesId, accent: true },
        { id: 'customerName', label: t('fields.customerName'), value: order.customerName },
        {
          id: 'customerAccount',
          label: t('fields.customerAccount'),
          value: order.customerAccount,
          accent: true,
        },
        {
          id: 'invoiceAccount',
          label: t('fields.invoiceAccount'),
          value: order.invoiceAccount,
          accent: true,
        },
        { id: 'site', label: t('salesOrderQuickCreate.site', 'Site'), accent: true },
        { id: 'warehouse', label: t('salesOrderQuickCreate.warehouse', 'Warehouse'), accent: true },
      ],
      id: 'header-general',
      title: t('salesOrder.general', 'General'),
      visualVariant: 'legalEntity',
      columns: 5,
      columnGap: 40,
      minHeight: 480,
      groups: [
        {
          id: 'order',
          title: t('salesOrder.salesOrder', 'Sales order'),
          fields: [
            headerField('salesId', 'Sales order'),
            headerField('source', 'Source'),
            headerField('retailSale', 'Retail sale'),
            headerField('customerName', 'Customer name'),
            headerField('arabicName', 'Arabic name'),
          ],
        },
        {
          id: 'customer',
          fields: [
            headerField('orderType', 'Order type'),
            headerField('continuityOrder', 'Continuity order'),
            headerField('customerAccount', 'Customer account', t('fields.customer')),
            headerField('oneTimeCustomer', 'One-time customer'),
            headerField('invoiceAccount', 'Invoice account'),
            headerField('contact', 'Contact'),
          ],
        },
        {
          id: 'contact',
          title: t('salesOrder.contactInformation', 'Contact information'),
          fields: [
            headerField('internetAddress', 'Internet address'),
            headerField('email', 'Email'),
            headerField('telephone', 'Telephone'),
            headerField('salesStatus', 'Status', t('common.status')),
            headerField('deadline', 'Deadline'),
          ],
        },
        {
          id: 'storage',
          fields: [
            headerField('documentStatus', 'Document status'),
            headerField('doNotProcess', 'Do not process'),
            headerField('site', 'Site', t('salesOrder.storageDimensions', 'Storage dimensions')),
            headerField('warehouse', 'Warehouse'),
            headerField('campaignId', 'Campaign ID'),
          ],
        },
        {
          id: 'references',
          title: t('salesOrder.references', 'References'),
          fields: [
            headerField('customerRequisition', 'Customer requisition'),
            headerField('customerReference', 'Customer reference'),
            headerField('rmaNumber', 'RMA number'),
            headerField('reasonCode', 'Reason code'),
            headerField('reasonComment', 'Reason comment'),
          ],
        },
      ],
    },
    {
      summaryItems: [
        {
          id: 'customerGroup',
          label: t('fields.customerGroup'),
          value: order.customerGroup,
          accent: true,
        },
        { id: 'currency', label: t('fields.currency'), value: order.currencyCode, accent: true },
        {
          id: 'paymentTerms',
          label: t('fields.paymentTerms'),
          value: order.paymentTerms,
          accent: true,
        },
      ],
      id: 'header-setup',
      title: t('salesOrder.setup', 'Setup'),
      visualVariant: 'legalEntity',
      columns: 5,
      columnGap: 40,
      groups: [
        {
          id: 'tax',
          title: t('salesOrder.salesTax', 'Sales tax'),
          fields: [
            headerField('salesTaxGroup', 'Sales tax group'),
            headerField('pricesIncludeSalesTax', 'Prices include sales tax'),
          ],
        },
        {
          id: 'posting',
          title: t('salesOrder.posting', 'Posting'),
          fields: [
            headerField('customerGroup', 'Customer group'),
            headerField('currencyCode', 'Currency'),
          ],
        },
        {
          id: 'commission',
          title: t('salesOrder.commission', 'Commission'),
          fields: [headerField('salesGroup', 'Sales group')],
        },
        {
          id: 'reservation',
          fields: [
            headerField('autoBatchReservation', 'Auto batch reservation'),
            headerField('deliveryMode', 'Delivery mode'),
            headerField('deliveryDate', 'Requested delivery'),
          ],
        },
        {
          id: 'language',
          fields: [
            headerField('language', 'Language'),
            headerField('paymentTerms', 'Payment terms'),
          ],
        },
      ],
    },
  ];
  const sections = [
    ...(tab === 'header'
      ? headerSections
      : [section(t('salesOrder.header', 'Sales order header'), header, true)]),
    tab === 'lines' &&
      section(
        t('salesOrder.lines', 'Sales order lines'),
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
              onClick={() => {
                setLineBaseline(null);
                setActiveField('itemNumber');
                setSelectedLineId('new-sales-line');
                setLineError('');
                setLineFilterVisible(false);
                setDraftLine({
                  id: 'new-sales-line',
                  orderId: order.id,
                  lineNumber: 0,
                  itemNumber: '',
                  description: '',
                  quantity: 1,
                  unit: '',
                  unitPrice: 0,
                  lineTotal: 0,
                  deliveryDate: order.deliveryDate?.slice(0, 10),
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
              ? t('common.saving', 'Saving?')
              : t(
                  'salesOrder.gridHelp',
                  'Tab / Enter: next cell ? Shift: previous ? ? ?: next row ? F2: edit ? Insert: add ? Alt+Delete: remove ? Ctrl+F: filter'
                )}
          </Typography>
          {linesQuery.isError && (
            <ErrorState
              message={linesQuery.error.message}
              onRetry={() => void linesQuery.refetch()}
            />
          )}
          <SalesLineGridSurface
            gridRef={lineGridRef}
            commit={commitCell}
            onAdd={() => addLineButtonRef.current?.click()}
            onRemove={() => removeLineButtonRef.current?.click()}
            onFilter={() => setLineFilterVisible((visible) => !visible)}
          >
            <DataGrid
              ref={lineGridRef}
              key={order.id}
              rows={
                activeDraft
                  ? lines.some((line) => line.id === activeDraft.id)
                    ? lines.map((line) => (line.id === activeDraft.id ? activeDraft : line))
                    : [...lines, activeDraft]
                  : lines
              }
              selectionMode="single"
              onSelectionChange={(ids) => {
                if (ids[0] != null) setSelectedLineId(String(ids[0]));
              }}
              selectedIds={selectedLineId ? [selectedLineId] : []}
              onRowClick={(line) => {
                if (line.id === activeDraft?.id) return;
                if (
                  savingLine ||
                  (activeDraft?.id === 'new-sales-line' && line.id !== activeDraft.id)
                )
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
              }}
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
              hideSidebar
              storageKey="accounts-receivable.sales-order-lines.compact"
            />
          </SalesLineGridSurface>
        </>
      ),
    tab === 'lines' &&
      section(
        t('salesOrder.lineDetails', 'Line details'),
        <>
          <Tabs
            value={lineTab}
            onChange={(_, value: string) => setLineTab(value)}
            variant="scrollable"
            scrollButtons="auto"
            aria-label={t('salesOrder.lineDetails', 'Line details')}
            sx={{
              mb: 2,
              minHeight: 34,
              '& .MuiTab-root': { fontSize: 12, minHeight: 34, minWidth: 0, px: 1.25 },
            }}
          >
            {[
              'General',
              'Setup',
              'Address',
              'Product',
              'Packing',
              'Delivery',
              'Sourcing',
              'Price and discount',
              'Foreign trade',
              'Financial dimensions',
              'Loads',
              'Financial tags',
            ].map((label) => (
              <Tab
                key={label}
                value={label}
                label={label}
                id={`line-tab-${label.replaceAll(' ', '-')}`}
                aria-controls="line-details-panel"
              />
            ))}
          </Tabs>
          <Box
            role="tabpanel"
            id="line-details-panel"
            aria-labelledby={`line-tab-${lineTab.replaceAll(' ', '-')}`}
          >
            {!selectedLine ? (
              <Typography variant="body2" color="text.secondary" sx={{ minHeight: 100 }}>
                {t('salesOrder.selectLine', 'Select a sales order line to view its details.')}
              </Typography>
            ) : (
              <Box
                sx={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))',
                  gap: 2,
                }}
              >
                {field(t('fields.item'), selectedLine.itemNumber)}
                {field(t('fields.quantity'), String(selectedLine.quantity))}
                {field(t('fields.unit'), selectedLine.unit)}
                {field(t('salesOrderQuickCreate.site', 'Site'), selectedLine.site)}
                {field(t('salesOrderQuickCreate.warehouse', 'Warehouse'), selectedLine.warehouse)}
                {field(t('fields.unitPrice'), amount(selectedLine.unitPrice))}
                {field(t('fields.requestedDelivery'), selectedLine.deliveryDate)}
              </Box>
            )}
          </Box>
        </>
      ),
    section(
      t('fields.totals', 'Totals'),
      <Stack direction="row" spacing={4} useFlexGap sx={{ flexWrap: 'wrap' }}>
        {field(t('fields.subtotal'))}
        {field(t('fields.discount'))}
        {field(t('fields.tax'))}
        {field(t('fields.total'), amount(order.orderTotal))}
      </Stack>
    ),
  ].filter((value): value is DetailSectionConfig => Boolean(value));
  return (
    <ListDetailsPage
      key={order.id}
      variant="enterprise"
      title={`${order.salesId} : ${order.customerName}`}
      config={{
        onSearch: () => {
          setTab('lines');
          setLineFilterVisible((visible) => !visible);
        },
        readOnly: true,
        initialSelectedId: order.id,
        dataSource: {
          type: 'controlled',
          records: orders,
          onRecordsChange: () => undefined,
          refresh: () => {
            void orderQuery.refetch();
          },
        },
        createRecord: () => order,
        getPrimaryText: (record) => record.salesId,
        getSecondaryText: (record) => `${record.customerAccount} - ${record.customerName}`,
        matchesSearch: (record, query) =>
          `${record.salesId} ${record.customerAccount} ${record.customerName}`
            .toLowerCase()
            .includes(query.toLowerCase()),
        getValues: (record) => ({ ...record }),
        setValues: (record) => record,
        headerFields: [],
        advancedFilter: {
          title: t('filters.title'),
          addLabel: t('actions.add'),
          fieldLabel: t('fields.salesOrderNumber'),
          operatorLabel: t('filters.contains'),
          applyLabel: t('actions.apply'),
          resetLabel: t('actions.reset'),
          fields: [
            {
              id: 'salesId',
              label: t('fields.salesOrderNumber'),
              getValue: (record) => record.salesId,
            },
            {
              id: 'customerAccount',
              label: t('fields.customerAccount'),
              getValue: (record) => record.customerAccount,
            },
            {
              id: 'customerName',
              label: t('fields.customerName'),
              getValue: (record) => record.customerName,
            },
            {
              id: 'customerGroup',
              label: t('fields.customerGroup'),
              getValue: (record) => record.customerGroup,
            },
            {
              id: 'currencyCode',
              label: t('fields.currency'),
              getValue: (record) => record.currencyCode,
            },
            {
              id: 'salesStatus',
              label: t('common.status'),
              getValue: (record) => record.salesStatus,
            },
          ],
          getValue: (record) => record.salesId,
          matches: (record, value) =>
            record.salesId
              .toLocaleLowerCase(currentLanguage.code)
              .includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
        },
        relatedInformation: {
          title: t('relatedInformation.title'),
          sections: (record) => [
            {
              id: 'customer',
              label: t('fields.customer'),
              defaultExpanded: true,
              content: (
                <Typography variant="body2">
                  {record ? `${record.customerAccount} - ${record.customerName}` : '-'}
                </Typography>
              ),
            },
            {
              id: 'delivery',
              label: t('fields.delivery', 'Delivery'),
              content: (
                <Typography variant="body2">
                  {record ? `${record.deliveryDate} - ${record.deliveryMode}` : '-'}
                </Typography>
              ),
            },
            {
              id: 'payment',
              label: t('fields.payment', 'Payment'),
              content: (
                <Typography variant="body2">
                  {record ? `${record.paymentTerms} - ${record.currencyCode}` : '-'}
                </Typography>
              ),
            },
            {
              id: 'status',
              label: t('common.status'),
              content: <Typography variant="body2">{record?.documentStatus || '-'}</Typography>,
            },
          ],
        },
        sections,
        recordTableName: 'SalesTable',
        getAuditRecordId: (record) => record.recId,
        onSelectionChange: (record) => {
          if (record && record.id !== order.id && !activeHeader)
            navigate(ROUTE_PATHS.ACCOUNTS_RECEIVABLE.salesOrder(record.id));
        },
        presentation: { mode: 'list', listWidth: 280, listResizable: true, detailEndPadding: 8 },
        actionPaneAfterListContent: (
          <EnterpriseCrudActions
            editLabel={t('actions.edit')}
            newLabel={t('actions.new')}
            deleteLabel={t('actions.delete')}
            saveLabel={t('actions.save')}
            cancelLabel={t('actions.cancel')}
            canEdit={Boolean(canEditHeader) && !savingHeader}
            canNew={false}
            canDelete={false}
            editing={Boolean(activeHeader)}
            saving={savingHeader}
            onEdit={startHeaderEdit}
            onSave={() => void saveHeader()}
            onCancel={() => {
              if (savingHeader) return;
              setHeaderDraft(null);
              setHeaderError('');
            }}
          />
        ),
        commands: [
          'Sales order',
          'Sell',
          'Manage',
          'Pick and pack',
          'Invoice',
          'Commerce',
          'General',
          'Warehouse',
          'Transportation',
          'Credit management',
        ].map((label) => ({ id: label, label, disabled: true })),
        detailHeader: (
          <>
            {headerError && <Alert severity="error">{headerError}</Alert>}
            <Typography color="primary" variant="body2" sx={{ mb: 1 }}>
              {t('salesOrder.details', 'Sales order details')} | {t('pages.customers.standardView')}
            </Typography>
            <Stack
              direction="row"
              sx={{ mb: 1, justifyContent: 'space-between', alignItems: 'center' }}
            >
              <Typography variant="h5">{`${order.salesId} : ${order.customerName}`}</Typography>
              <Typography variant="body2" sx={{ marginInlineEnd: 3, flexShrink: 0 }}>
                {t(`status.${order.salesStatus.toLowerCase()}`, order.salesStatus)}
              </Typography>
            </Stack>

            <Tabs
              value={tab}
              onChange={(_, value: string) => setTab(value)}
              sx={{
                mb: 3,
                minHeight: 36,
                '& .MuiTab-root': { minHeight: 36, minWidth: 48, px: 1, fontSize: 13 },
              }}
              aria-label={t('pages.salesOrders.title')}
            >
              <Tab
                value="lines"
                label={t('fields.lines', 'Lines')}
                id="sales-lines-tab"
                aria-controls="sales-detail-panel"
              />
              <Tab
                value="header"
                label={t('fields.header', 'Header')}
                id="sales-header-tab"
                aria-controls="sales-detail-panel"
              />
            </Tabs>
          </>
        ),
      }}
    />
  );
}
