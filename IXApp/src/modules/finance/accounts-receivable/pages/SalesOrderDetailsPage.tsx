import React, { useEffect, useRef, useState } from 'react';
import { Alert, Box, Stack, Tab, Tabs, TextField, Typography } from '@mui/material';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import type { ActionPaneRibbonGroup } from '@shared/components/action-pane/ActionPaneRibbon';
import { ActionPaneRibbonTrigger } from '@shared/components/action-pane/ActionPaneRibbonTrigger';
import { useNavigate, useParams } from 'react-router-dom';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailFieldConfig, DetailSectionConfig } from '@patterns/list-details/types';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { salesOrderLinesApi, type SalesOrderLineRecord } from '../api/salesOrderLinesApi';
import { PERMISSIONS } from '@core/permissions/permissions';
import { usePermission } from '@core/permissions/usePermission';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { salesOrderListApi, type SalesOrderHeaderInput } from '../api/salesOrderListApi';
import { customerQuickCreateApi } from '../api/customerQuickCreateApi';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { LookupField } from '@shared/components/lookups/LookupField';
import { synchronizeSalesLineDiscount } from '../utils/salesLineDiscount';
import {
  DocumentTotalsDrawer,
  type DocumentTotalsSection,
} from '@patterns/document/DocumentTotalsDrawer';

import { SalesOrderLinesGrid } from './SalesOrderLinesGrid';
import { SalesOrderLinesProvider } from './SalesOrderLineState';

type DetailLine = SalesOrderLineRecord;
const salesOrderRibbonPinnedStorageKey = 'sales-order.action-pane.ribbon-pinned';

export function SalesOrderDetailsPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const { salesOrderId } = useParams<{ salesOrderId: string }>();
  const [headerDraft, setHeaderDraft] = useState<(SalesOrderHeaderInput & { id: string }) | null>(
    null
  );
  const [savingHeader, setSavingHeader] = useState(false);
  const headerSaveLock = useRef(false);
  const [headerError, setHeaderError] = useState('');
  useUnsavedChanges(Boolean(headerDraft));
  const [lineFilterVisible, setLineFilterVisible] = useState(false);
  const { hasPermission: canEditLines } = usePermission(PERMISSIONS.SALES_ORDER_UPDATE);
  const [lineTab, setLineTab] = useState('General');
  const [tab, setTab] = useState('lines');
  const [selectedLineId, setSelectedLineId] = useState<string>();
  const [lineDetailDraft, setLineDetailDraft] = useState<DetailLine | null>(null);
  const [savingLineDetail, setSavingLineDetail] = useState(false);
  const [totalsOpen, setTotalsOpen] = useState(false);
  const lineDetailSaveLock = useRef(false);
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

  const salesOrderRibbonGroups: ActionPaneRibbonGroup[] = [
    {
      id: 'new',
      label: 'New',
      actions: [
        { id: 'purchase-order', label: 'Purchase order' },
        { id: 'direct-delivery', label: 'Direct delivery' },
      ],
    },
    { id: 'maintain', label: 'Maintain', actions: [{ id: 'cancel', label: 'Cancel' }] },
    {
      id: 'payments',
      label: 'Payments',
      actions: [{ id: 'payments', label: 'Payments', disabled: true }],
    },
    {
      id: 'copy',
      label: 'Copy',
      actions: [
        { id: 'from-all', label: 'From all' },
        { id: 'from-journal', label: 'From journal' },
      ],
    },
    {
      id: 'view',
      label: 'View',
      actions: [
        { id: 'totals', label: 'Totals', onClick: () => setTotalsOpen(true) },
        { id: 'order-events', label: 'Order events' },
      ],
    },
    {
      id: 'functions',
      label: 'Functions',
      actions: [
        { id: 'sales-order-recap', label: 'Recap', disabled: true },
        { id: 'order-holds', label: 'Order holds' },
      ],
    },
    { id: 'attachments', label: 'Attachments', actions: [{ id: 'notes', label: 'Notes' }] },
  ];

  const sellRibbonGroups: ActionPaneRibbonGroup[] = [
    {
      id: 'credit-note',
      label: 'Credit note',
      actions: [{ id: 'credit-note', label: 'Credit note' }],
    },
    {
      id: 'charges',
      label: 'Charges',
      actions: [
        {
          id: 'maintain-charges',
          label: 'Maintain charges',
          onClick: () => order && navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrderCharges(order.id)),
        },
        { id: 'allocate-charges', label: 'Allocate charges' },
      ],
    },
    {
      id: 'tax',
      label: 'Tax',
      actions: [{ id: 'sales-tax', label: 'Sales tax' }],
    },
    {
      id: 'calculate-delivery',
      label: 'Calculate',
      actions: [
        { id: 'confirmed-delivery-dates', label: 'Confirmed delivery dates' },
        { id: 'multiline-discount', label: 'Multiline discount' },
        { id: 'total-discount', label: 'Total discount' },
      ],
    },
    {
      id: 'calculate-price',
      label: '',
      actions: [
        { id: 'supplementary-items', label: 'Supplementary items' },
        { id: 'tiered-charges', label: 'Tiered charges' },
        { id: 'push-price-and-totals', label: 'Push price and totals' },
      ],
    },
    {
      id: 'generate',
      label: 'Generate',
      actions: [
        { id: 'confirmation', label: 'Confirmation' },
        { id: 'pro-forma-confirmation', label: 'Pro forma confirmation' },
      ],
    },
    {
      id: 'actions',
      label: 'Actions',
      actions: [{ id: 'confirm-now', label: 'Confirm now' }],
    },
    {
      id: 'apply',
      label: 'Apply',
      actions: [{ id: 'service-agreement', label: 'Service agreement' }],
    },
    {
      id: 'journals',
      label: 'Journals',
      actions: [
        { id: 'sales-order-confirmations', label: 'Sales order confirmations', disabled: true },
        { id: 'quotation-confirmation-journal', label: 'Quotation confirmation journal', disabled: true },
      ],
    },
    {
      id: 'prepayment',
      label: 'Prepayment',
      actions: [{ id: 'prepayment', label: 'Prepayment', disabled: true }],
    },
  ];

  const customersQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-order-header-customers'],
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
    enabled: Boolean(activeHeader),
    staleTime: 5 * 60 * 1000,
  });

  const lookupsQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-order-header-lookups'],
    queryFn: ({ signal }) => customerQuickCreateApi.lookups(signal),
    enabled: Boolean(activeHeader),
    staleTime: 5 * 60 * 1000,
  });

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
      deliveryTerms: order.deliveryTerms,
      deliveryDate: order.deliveryDate?.slice(0, 10),
      orderDate: order.orderDate?.slice(0, 10),
      inventSiteId: order.inventSiteId,
      inventLocationId: order.inventLocationId,
      salesNameAlias: order.salesNameAlias,
      salesType: order.salesType,
      oneTimeCustomer: order.oneTimeCustomer,
      email: order.email,
      phone: order.phone,
      deadline: order.deadline?.slice(0, 10),
      customerRequisitionNumber: order.customerRequisitionNumber,
      campaignId: order.campaignId,
      taxGroupId: order.taxGroupId,
      pricesIncludeSalesTax: order.pricesIncludeSalesTax,
      salesGroup: order.salesGroup,
      languageId: order.languageId,
    });
  };
  const saveHeader = async () => {
    if (!activeHeader || !canEditHeader || headerSaveLock.current) return;
    if (
      !activeHeader.invoiceAccount.trim() ||
      !activeHeader.currencyCode.trim() ||
      !activeHeader.orderDate ||
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
  const headerInput = (
    name: keyof SalesOrderHeaderInput,
    label: string,
    options?: { value: string; label: string }[]
  ) => {
    const convertValue = (value: unknown) => {
      if (name === 'oneTimeCustomer' || name === 'pricesIncludeSalesTax')
        return String(value) === 'true';
      if (name === 'salesType') return Number(value);
      return String(value ?? '');
    };
    if (options) {
      return (
        <Box sx={{ minWidth: 0 }}>
          <LookupField
            name={name}
            label={label}
            value={String(activeHeader?.[name] ?? '')}
            disabled={savingHeader || !customersQuery.data || !lookupsQuery.data}
            options={options.map((opt) => ({ id: opt.value, code: opt.value, name: opt.label }))}
            displayMode="select"
            searchable
            lazyLoading={false}
            onChange={(value) =>
              setHeaderDraft((draft) => (draft ? { ...draft, [name]: convertValue(value) } : draft))
            }
          />
        </Box>
      );
    }
    return (
      <TextField
        fullWidth
        size="small"
        variant="standard"
        label={label}
        type={
          name === 'deliveryDate' || name === 'orderDate' || name === 'deadline' ? 'date' : 'text'
        }
        value={activeHeader?.[name] ?? ''}
        disabled={savingHeader}
        slotProps={{ inputLabel: { shrink: true } }}
        onChange={(event) =>
          setHeaderDraft((draft) =>
            draft ? { ...draft, [name]: convertValue(event.target.value) } : draft
          )
        }
      />
    );
  };
  const linesQuery = useQuery({
    queryKey: ['sales-order-lines', order?.id],
    queryFn: ({ signal }) => salesOrderLinesApi.list(order!.id, signal),
    enabled: Boolean(order),
  });
  const lines = linesQuery.data ?? [];
  const totalsQuery = useQuery({
    queryKey: ['sales-order-totals', order?.id],
    queryFn: ({ signal }) => salesOrderLinesApi.totals(order!.id, signal),
    enabled: Boolean(order && totalsOpen),
  });
  const selectedLine: DetailLine | undefined =
    lines.find((line) => line.id === selectedLineId) ?? lines[0];
  const dimensionsQuery = useQuery({
    queryKey: ['sales-order-inventory-dimensions'],
    queryFn: ({ signal }) => salesOrderLinesApi.inventoryDimensions(signal),
    enabled: Boolean(activeHeader),
    staleTime: 5 * 60 * 1000,
  });
  const headerDimensionInput = (name: 'inventSiteId' | 'inventLocationId', label: string) => {
    const options =
      name === 'inventSiteId'
        ? (dimensionsQuery.data?.sites ?? [])
        : (dimensionsQuery.data?.warehouses ?? []).filter(
            (warehouse) =>
              Boolean(activeHeader?.inventSiteId) && warehouse.siteId === activeHeader?.inventSiteId
          );
    return (
      <Box sx={{ minWidth: 0 }}>
        <LookupField
          name={name}
          label={label}
          value={activeHeader?.[name] ?? ''}
          options={options}
          displayMode="select"
          searchable
          lazyLoading={false}
          disabled={savingHeader || dimensionsQuery.isLoading}
          onChange={(value) =>
            setHeaderDraft((draft) =>
              draft
                ? {
                    ...draft,
                    [name]: String(value ?? ''),
                    ...(name === 'inventSiteId' ? { inventLocationId: '' } : {}),
                  }
                : draft
            )
          }
        />
      </Box>
    );
  };
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
    enabled: Boolean(activeHeader),
    staleTime: 5 * 60 * 1000,
  });
  const taxGroupsQuery = useQuery({
    queryKey: ['sales-order-tax-groups'],
    queryFn: ({ signal }) => salesOrderLinesApi.taxGroups(signal),
    enabled: Boolean(activeHeader),
    staleTime: 5 * 60 * 1000,
  });
  useEffect(() => {
    if (!activeHeader || !selectedLine) {
      setLineDetailDraft(null);
      return;
    }
    setLineDetailDraft({ ...selectedLine, deliveryDate: selectedLine.deliveryDate?.slice(0, 10) });
  }, [activeHeader, selectedLine]);
  const saveLineDetail = async (draft = lineDetailDraft) => {
    if (!draft || lineDetailSaveLock.current) return;
    lineDetailSaveLock.current = true;
    setSavingLineDetail(true);
    try {
      const saved = await salesOrderLinesApi.update(order!.id, draft);
      queryClient.setQueryData<DetailLine[]>(['sales-order-lines', order!.id], (current = []) =>
        current.map((line) => (line.id === saved.id ? saved : line))
      );
      setLineDetailDraft({ ...saved, deliveryDate: saved.deliveryDate?.slice(0, 10) });
      await orderQuery.refetch();
    } catch (error) {
      setHeaderError(error instanceof Error ? error.message : t('errors.generic'));
    } finally {
      lineDetailSaveLock.current = false;
      setSavingLineDetail(false);
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
  const number = (value: number, digits = 2) =>
    value.toLocaleString(currentLanguage.code, {
      minimumFractionDigits: digits,
      maximumFractionDigits: digits,
    });
  const calculatedLineDiscount = lines.reduce((sum, line) => sum + (line.lineDiscount || 0), 0);
  const calculatedTotalDiscount = lines.reduce(
    (sum, line) => sum + (line.lineDiscount || 0) + (line.multiLineDiscount || 0),
    0
  );
  const calculatedSubtotal =
    lines.reduce((sum, line) => sum + (line.lineTotal || 0), 0) - calculatedTotalDiscount;
  const calculatedCostValue = lines.reduce(
    (sum, line) => sum + (line.costPrice || 0) * (line.quantity || 0),
    0
  );
  const subtotal = totalsQuery.data?.subtotal ?? calculatedSubtotal;
  const lineDiscount = totalsQuery.data?.lineDiscount ?? calculatedLineDiscount;
  const totalDiscount = totalsQuery.data?.totalDiscount ?? calculatedTotalDiscount;
  const salesTax = totalsQuery.data?.salesTax ?? 0;
  const totalCharges = totalsQuery.data?.totalCharges ?? 0;
  const costValue = totalsQuery.data?.costValue ?? calculatedCostValue;
  const invoiceAmount = totalsQuery.data?.invoiceAmount ?? subtotal + totalCharges + salesTax;
  const margin = subtotal - costValue;
  const quantity = totalsQuery.data?.quantity ?? lines.reduce((sum, line) => sum + (line.quantity || 0), 0);
  const totalsSections: DocumentTotalsSection[] = [
    {
      id: 'sales-order-totals',
      title: 'Sales order totals',
      columns: [
        {
          id: 'totals',
          title: 'Totals',
          fields: [
            { id: 'currency', label: 'Currency', value: order.currencyCode },
            { id: 'exchange-rate', label: 'Exchange rate', value: number(1, 4) },
            { id: 'line-discount', label: 'Line discount', value: number(lineDiscount) },
            { id: 'subtotal', label: 'Subtotal amount', value: number(subtotal), emphasized: true },
            { id: 'total-discount', label: 'Total discount', value: number(totalDiscount) },
            { id: 'cash-discount', label: 'Cash discount', value: number(0) },
            { id: 'total-charges', label: 'Total charges', value: number(totalCharges) },
            { id: 'sales-tax', label: 'Sales tax', value: number(salesTax) },
            { id: 'round-off', label: 'Round-off', value: number(0) },
            { id: 'coupon', label: 'Total coupon amount', value: number(0) },
            {
              id: 'invoice-amount',
              label: 'Invoice amount',
              value: number(invoiceAmount),
              emphasized: true,
            },
          ],
        },
        {
          id: 'analysis',
          fields: [
            { id: 'credit-limit', label: 'Credit limit', value: '-' },
            { id: 'credit-available', label: 'Credit available', value: '-' },
            { id: 'cost-value', label: 'Cost value in accounting currency', value: number(costValue) },
            { id: 'margin', label: 'Margin in accounting currency', value: number(margin) },
            {
              id: 'contribution-ratio',
              label: 'Contribution ratio',
              value: `${number(subtotal ? (margin / subtotal) * 100 : 0)}%`,
            },
            { id: 'quantity', label: 'Quantity', value: number(quantity) },
            { id: 'weight', label: 'Weight', value: number(0) },
            { id: 'volume', label: 'Volume', value: number(0) },
          ],
        },
      ],
    },
  ];
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
        ? headerInput(
            'invoiceAccount',
            t('fields.invoiceAccount'),
            customersQuery.data?.map((c) => ({
              value: c.accountNumber,
              label: `${c.accountNumber} - ${c.name}`,
            }))
          )
        : field(t('fields.invoiceAccount'), order.invoiceAccount)}
      {activeHeader
        ? headerInput('customerReference', t('fields.customerReference', 'Customer reference'))
        : field(t('fields.customerReference', 'Customer reference'), order.customerReference)}
      {activeHeader
        ? headerInput('currencyCode', t('fields.currency'), lookupsQuery.data?.currencies)
        : field(t('fields.currency'), order.currencyCode)}
      {activeHeader
        ? headerInput('paymentTerms', t('fields.paymentTerms'), lookupsQuery.data?.paymentTerms)
        : field(t('fields.paymentTerms'), order.paymentTerms)}
      {activeHeader
        ? headerInput('deliveryMode', t('fields.deliveryMode'), lookupsQuery.data?.deliveryModes)
        : field(t('fields.deliveryMode'), order.deliveryMode)}
      {activeHeader
        ? headerInput(
            'deliveryTerms',
            t('customerQuickCreate.fields.deliveryTerms'),
            lookupsQuery.data?.deliveryTerms
          )
        : field(t('customerQuickCreate.fields.deliveryTerms'), order.deliveryTerms)}
      {activeHeader
        ? headerDimensionInput('inventSiteId', t('salesOrderQuickCreate.site', 'Site'))
        : field(t('salesOrderQuickCreate.site', 'Site'), order.inventSiteId)}
      {activeHeader
        ? headerDimensionInput(
            'inventLocationId',
            t('salesOrderQuickCreate.warehouse', 'Warehouse')
          )
        : field(t('salesOrderQuickCreate.warehouse', 'Warehouse'), order.inventLocationId)}
      {activeHeader
        ? headerInput('orderDate', t('fields.orderDate'))
        : field(t('fields.orderDate'), order.orderDate?.slice(0, 10))}
      {activeHeader
        ? headerInput('deliveryDate', t('fields.requestedDelivery'))
        : field(t('fields.requestedDelivery'), order.deliveryDate)}
    </Box>
  );
  const displayedLine = lineDetailDraft ?? selectedLine;
  const lineDetailField = (
    name: keyof DetailLine,
    label: string,
    type: 'text' | 'number' | 'date' = 'text'
  ) => {
    if (!displayedLine) return null;
    if (!activeHeader || name === 'itemNumber')
      return field(label, String(displayedLine[name] ?? ''));
    return (
      <TextField
        key={name}
        fullWidth
        size="small"
        variant="standard"
        label={label}
        type={type}
        value={lineDetailDraft?.[name] ?? ''}
        disabled={savingLineDetail}
        slotProps={{ inputLabel: { shrink: true } }}
        onChange={(event) => {
          const value = type === 'number' ? Number(event.target.value) : event.target.value;
          setLineDetailDraft((draft) => {
            if (!draft) return draft;
            if (
              typeof value === 'number' &&
              (name === 'quantity' ||
                name === 'unitPrice' ||
                name === 'lineDiscount' ||
                name === 'lineDiscountPercent')
            )
              return synchronizeSalesLineDiscount(draft, name, value);
            return { ...draft, [name]: value };
          });
        }}
        onBlur={() => void saveLineDetail()}
      />
    );
  };
  const dimensionField = (name: 'site' | 'warehouse', label: string) => {
    if (!displayedLine) return null;
    if (!activeHeader) return field(label, displayedLine[name]);
    const options =
      name === 'site'
        ? (dimensionsQuery.data?.sites ?? [])
        : (dimensionsQuery.data?.warehouses ?? []).filter(
            (warehouse) => !lineDetailDraft?.site || warehouse.siteId === lineDetailDraft.site
          );
    return lineLookupShell(
      <LookupField
        key={name}
        name={name}
        label={label}
        value={lineDetailDraft?.[name] ?? ''}
        options={options}
        displayMode="select"
        searchable
        lazyLoading={false}
        disabled={savingLineDetail || dimensionsQuery.isLoading}
        onChange={(value) => {
          if (!lineDetailDraft) return;
          const next = {
            ...lineDetailDraft,
            [name]: String(value ?? ''),
            ...(name === 'site' ? { warehouse: '' } : {}),
          };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const lineLookupShell = (content: React.ReactNode) => (
    <Box sx={{ minWidth: 0 }}>{content}</Box>
  );
  const unitField = () => {
    const label = t('fields.unit');
    if (!displayedLine) return null;
    if (!activeHeader) return field(label, displayedLine.unit);
    return lineLookupShell(
      <LookupField
        name="unit"
        label={label}
        value={lineDetailDraft?.unit ?? ''}
        options={(unitsQuery.data ?? []).map((unit) => ({
          id: unit.symbol,
          code: unit.symbol,
          name: unit.symbol,
        }))}
        displayMode="select"
        searchable
        lazyLoading={false}
        disabled={savingLineDetail || unitsQuery.isLoading}
        onChange={(value) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, unit: String(value ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const lineValue = (name: keyof DetailLine, label: string, date = false) => {
    const value = displayedLine?.[name];
    const text = value == null || value === '' ? undefined : String(value);
    return field(label, date ? text?.slice(0, 10) : text);
  };
  const lineLookupField = (
    name: 'deliveryMode' | 'deliveryTerms',
    label: string,
    options: { value: string; label: string }[] = []
  ) => {
    if (!displayedLine) return null;
    if (!activeHeader) return lineValue(name, label);
    return lineLookupShell(
      <LookupField
        name={name}
        label={label}
        value={lineDetailDraft?.[name] ?? ''}
        options={options.map((option) => ({
          id: option.value,
          code: option.value,
          name: option.label,
        }))}
        displayMode="select"
        searchable
        lazyLoading={false}
        disabled={savingLineDetail || lookupsQuery.isLoading}
        onChange={(value) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, [name]: String(value ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const taxGroupField = (name: 'taxGroup' | 'taxItemGroup', label: string) => {
    if (!displayedLine) return null;
    if (!activeHeader) return lineValue(name, label);
    const options =
      name === 'taxGroup'
        ? (taxGroupsQuery.data?.salesTaxGroups ?? [])
        : (taxGroupsQuery.data?.itemSalesTaxGroups ?? []);
    return lineLookupShell(
      <LookupField
        name={name}
        label={label}
        value={lineDetailDraft?.[name] ?? ''}
        options={options}
        displayMode="select"
        searchable
        lazyLoading={false}
        disabled={savingLineDetail || taxGroupsQuery.isLoading}
        onChange={(value) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, [name]: String(value ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const lineTabContent = (): React.ReactNode => {
    switch (lineTab) {
      case 'Setup':
        return (
          <>
            {lineDetailField('lineType', 'Line type', 'number')}
            {lineDetailField('deliveryType', 'Delivery type', 'number')}
            {lineDetailField(
              'salesCategory',
              t('salesOrder.salesCategory', 'Sales category'),
              'number'
            )}
            {lineValue('salesStatus', 'Sales status')}
            {lineValue('currencyCode', t('fields.currency'))}
          </>
        );
      case 'Address':
        return (
          <>
            {lineDetailField('deliveryName', 'Delivery name')}
            {lineDetailField('deliveryPostalAddress', 'Delivery postal address', 'number')}
            {lineDetailField(
              'customerReference',
              t('fields.customerReference', 'Customer reference')
            )}
          </>
        );
      case 'Product':
        return (
          <>
            {lineDetailField('itemNumber', t('fields.item'))}
            {lineDetailField('description', t('salesOrder.productName', 'Product name'))}
            {unitField()}
            {lineValue('inventDimId', 'Inventory dimension')}
            {taxGroupField('taxGroup', 'Sales tax group')}
            {taxGroupField('taxItemGroup', 'Item sales tax group')}
          </>
        );
      case 'Packing':
        return (
          <>
            {lineDetailField('packingUnit', 'Packing unit')}
            {lineDetailField('packingUnitQuantity', 'Packing unit quantity', 'number')}
          </>
        );
      case 'Delivery':
        return (
          <>
            {lineDetailField('deliveryDate', t('fields.requestedDelivery'), 'date')}
            {dimensionField('site', t('salesOrderQuickCreate.site', 'Site'))}
            {dimensionField('warehouse', t('salesOrderQuickCreate.warehouse', 'Warehouse'))}
            {lineLookupField(
              'deliveryMode',
              t('fields.deliveryMode'),
              lookupsQuery.data?.deliveryModes
            )}
            {lineLookupField(
              'deliveryTerms',
              t('customerQuickCreate.fields.deliveryTerms'),
              lookupsQuery.data?.deliveryTerms
            )}
            {lineDetailField('shippingDateRequested', 'Requested shipping date', 'date')}
            {lineDetailField('shippingDateConfirmed', 'Confirmed shipping date', 'date')}
            {lineDetailField('receiptDateConfirmed', 'Confirmed receipt date', 'date')}
            {lineDetailField('overDeliveryPercent', 'Overdelivery percentage', 'number')}
            {lineDetailField('underDeliveryPercent', 'Underdelivery percentage', 'number')}
          </>
        );
      case 'Sourcing':
        return (
          <>
            {lineValue('inventTransId', 'Inventory transaction')}
            {lineValue('inventDimId', 'Inventory dimension')}
            {lineValue('site', t('salesOrderQuickCreate.site', 'Site'))}
            {lineValue('warehouse', t('salesOrderQuickCreate.warehouse', 'Warehouse'))}
          </>
        );
      case 'Price and discount':
        return (
          <>
            {lineDetailField('unitPrice', t('fields.unitPrice'), 'number')}
            {lineDetailField('priceUnit', 'Price unit', 'number')}
            {lineValue('costPrice', 'Cost price')}
            {lineValue('lineTotal', 'Net amount')}
            {lineDetailField('lineDiscount', 'Line discount', 'number')}
            {lineDetailField('lineDiscountPercent', 'Line discount percentage', 'number')}
            {lineDetailField('multiLineDiscount', 'Multiline discount', 'number')}
            {lineDetailField('multiLineDiscountPercent', 'Multiline discount percentage', 'number')}
          </>
        );
      case 'Foreign trade':
        return <>{lineValue('intrastatCommodity', 'Intrastat commodity')}</>;
      case 'Financial dimensions':
        return (
          <>
            {lineValue('ledgerDimension', 'Ledger dimension')}
            {lineValue('defaultDimension', 'Default dimension')}
          </>
        );
      case 'Loads':
        return (
          <>
            {lineValue('quantity', t('fields.quantity'))}
            {lineValue('salesDeliverNow', 'Deliver now')}
            {lineValue('inventDeliverNow', 'Inventory deliver now')}
            {lineValue('remainSalesPhysical', 'Remaining physical quantity')}
            {lineValue('remainSalesFinancial', 'Remaining financial quantity')}
          </>
        );
      case 'Financial tags':
        return <>{lineValue('financialTag', 'Financial tag')}</>;
      default:
        return (
          <>
            {lineDetailField('itemNumber', t('fields.item'))}
            {lineDetailField('description', t('salesOrder.productName', 'Product name'))}
            {lineDetailField('quantity', t('fields.quantity'), 'number')}
            {unitField()}
            {dimensionField('site', t('salesOrderQuickCreate.site', 'Site'))}
            {dimensionField('warehouse', t('salesOrderQuickCreate.warehouse', 'Warehouse'))}
            {lineDetailField('unitPrice', t('fields.unitPrice'), 'number')}
            {lineDetailField(
              'salesCategory',
              t('salesOrder.salesCategory', 'Sales category'),
              'number'
            )}
            {lineDetailField('deliveryDate', t('fields.requestedDelivery'), 'date')}
          </>
        );
    }
  };
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
          render: () => {
            if (name === 'inventSiteId' || name === 'inventLocationId')
              return headerDimensionInput(name, t(`salesOrder.headerFields.${name}`, label));
            let options: { value: string; label: string }[] | undefined = undefined;
            if (name === 'invoiceAccount') {
              options = customersQuery.data?.map((c) => ({
                value: c.accountNumber,
                label: `${c.accountNumber} - ${c.name}`,
              }));
            } else if (name === 'currencyCode') {
              options = lookupsQuery.data?.currencies;
            } else if (name === 'paymentTerms') {
              options = lookupsQuery.data?.paymentTerms;
            } else if (name === 'deliveryMode') {
              options = lookupsQuery.data?.deliveryModes;
            } else if (name === 'taxGroupId') {
              options = (taxGroupsQuery.data?.salesTaxGroups ?? []).map((group) => ({
                value: group.code,
                label: `${group.code} - ${group.name}`,
              }));
            } else if (name === 'salesType') {
              options = [
                { value: '3', label: 'Sales order' },
                { value: '4', label: 'Returned order' },
              ];
            } else if (name === 'oneTimeCustomer' || name === 'pricesIncludeSalesTax') {
              options = [
                { value: 'false', label: t('common.no', 'No') },
                { value: 'true', label: t('common.yes', 'Yes') },
              ];
            }
            return headerInput(
              name as keyof SalesOrderHeaderInput,
              t(`salesOrder.headerFields.${name}`, label),
              options
            );
          },
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
        {
          id: 'site',
          label: t('salesOrderQuickCreate.site', 'Site'),
          value: order.inventSiteId,
          accent: true,
        },
        {
          id: 'warehouse',
          label: t('salesOrderQuickCreate.warehouse', 'Warehouse'),
          value: order.inventLocationId,
          accent: true,
        },
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
            headerField('customerName', 'Customer name'),
            headerField('salesNameAlias', 'Arabic name'),
          ],
        },
        {
          id: 'customer',
          fields: [
            headerField('salesType', 'Order type'),
            headerField('customerAccount', 'Customer account', t('fields.customer')),
            headerField('oneTimeCustomer', 'One-time customer'),
            headerField('invoiceAccount', 'Invoice account'),
          ],
        },
        {
          id: 'contact',
          title: t('salesOrder.contactInformation', 'Contact information'),
          fields: [
            headerField('email', 'Email'),
            headerField('phone', 'Telephone'),
            headerField('salesStatus', 'Status', t('common.status')),
            headerField('deadline', 'Deadline'),
          ],
        },
        {
          id: 'storage',
          fields: [
            headerField('documentStatus', 'Document status'),
            headerField(
              'inventSiteId',
              'Site',
              t('salesOrder.storageDimensions', 'Storage dimensions')
            ),
            headerField('inventLocationId', 'Warehouse'),
            headerField('campaignId', 'Campaign ID'),
          ],
        },
        {
          id: 'references',
          title: t('salesOrder.references', 'References'),
          fields: [
            headerField('customerRequisitionNumber', 'Customer requisition'),
            headerField('customerReference', 'Customer reference'),
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
            headerField('taxGroupId', 'Sales tax group'),
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
            headerField('deliveryMode', 'Delivery mode'),
            headerField('deliveryDate', 'Requested delivery'),
          ],
        },
        {
          id: 'language',
          fields: [
            headerField('languageId', 'Language'),
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
        <SalesOrderLinesGrid
          key={order.id}
          order={order}
          editing={Boolean(activeHeader)}
          selectedLineId={selectedLineId}
          setSelectedLineId={setSelectedLineId}
          lineFilterVisible={lineFilterVisible}
          setLineFilterVisible={setLineFilterVisible}
          refreshOrder={() => orderQuery.refetch()}
        />
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
            {!displayedLine ? (
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
                {lineTabContent()}
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
    <SalesOrderLinesProvider key={order.id}>
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
              navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrder(record.id));
          },
          presentation: { mode: 'list', listWidth: 280, listResizable: true, detailEndPadding: 8 },
          actionPaneAfterListContent: (
            <>
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
              <ActionPaneGroup>
                <ActionPaneRibbonTrigger
                  id="sales-order"
                  label="Sales order"
                  groups={salesOrderRibbonGroups}
                  disabled={Boolean(activeHeader)}
                  persistenceKey={salesOrderRibbonPinnedStorageKey}
                />
                <ActionPaneRibbonTrigger
                  id="sell"
                  label="Sell"
                  groups={sellRibbonGroups}
                />
              </ActionPaneGroup>
            </>
          ),
          commands: [
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
                {t('salesOrder.details', 'Sales order details')} |{' '}
                {t('pages.customers.standardView')}
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
      <DocumentTotalsDrawer
        open={totalsOpen}
        onClose={() => setTotalsOpen(false)}
        title="Totals"
        viewLabel={t('pages.customers.standardView')}
        sections={totalsSections}
        okLabel={t('common.ok', 'OK')}
      />
    </SalesOrderLinesProvider>
  );
}
