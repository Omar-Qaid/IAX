import React, { useEffect, useRef, useState } from 'react';
import {
  Alert,
  Box,
  IconButton,
  Stack,
  Switch,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
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
import { LogisticsPostalAddressDrawer } from '@shared/components/logistics/LogisticsPostalAddressDrawer';
import type { LogisticsPostalAddress } from '@shared/types/logistics';
import { PERMISSIONS } from '@core/permissions/permissions';
import { usePermission } from '@core/permissions/usePermission';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';
import { FOUNDATION_ROUTE_PATHS } from '@modules/finance/foundation/routes/foundationRoutePaths';
import { INVENTORY_ROUTE_PATHS } from '@modules/finance/inventory/routes/inventoryRoutePaths';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { salesOrderListApi, type SalesOrderHeaderInput } from '../api/salesOrderListApi';
import { customerQuickCreateApi } from '../api/customerQuickCreateApi';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { LookupField } from '@shared/components/lookups/LookupField';
import { EditableViewField } from '@shared/components/fields/EditableViewField';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import { synchronizeSalesLineDiscount } from '../utils/salesLineDiscount';
import {
  DocumentTotalsDrawer,
  type DocumentTotalsSection,
} from '@patterns/document/DocumentTotalsDrawer';
import { DocumentCopyDrawer } from '@patterns/document/DocumentCopyDrawer';
import { salesOrderCopyApi, type SalesOrderCopyMode } from '../api/salesOrderCopyApi';
import { SalesOrderConfirmationDialog } from '../components/SalesOrderConfirmationDialog';

import { SalesOrderLinesGrid } from './SalesOrderLinesGrid';
import { SalesOrderLinesProvider } from './SalesOrderLineState';

type DetailLine = SalesOrderLineRecord;

const headerMasterRoutes: Partial<Record<keyof SalesOrderHeaderInput, string>> = {
  customerAccount: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMERS,
  invoiceAccount: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMERS,
  customerGroup: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_GROUPS,
  currencyCode: FOUNDATION_ROUTE_PATHS.CURRENCIES,
  inventSiteId: INVENTORY_ROUTE_PATHS.SITES,
  inventLocationId: INVENTORY_ROUTE_PATHS.WAREHOUSES,
  taxGroupId: FOUNDATION_ROUTE_PATHS.TAX_GROUPS,
  paymentTerms: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_TERMS,
  paymentMethod: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_METHODS,
  chargesGroup: FOUNDATION_ROUTE_PATHS.CHARGES_CODES,
};
const salesOrderRibbonPinnedStorageKey = 'sales-order.action-pane.ribbon-pinned';

export function SalesOrderDetailsPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const salesLineTypeOptions = [
    { value: 0, label: t('salesOrder.lineTypes.journal', 'Journal') },
    { value: 1, label: t('salesOrder.lineTypes.quotation', 'Quotation') },
    { value: 2, label: t('salesOrder.lineTypes.subscription', 'Subscription') },
    { value: 3, label: t('salesOrder.lineTypes.sales', 'Sales') },
    { value: 4, label: t('salesOrder.lineTypes.returnItem', 'Return item') },
    { value: 5, label: t('salesOrder.lineTypes.blanket', 'Blanket') },
    { value: 6, label: t('salesOrder.lineTypes.itemRequirement', 'Item requirement') },
    { value: 7, label: t('salesOrder.lineTypes.prepayment', 'Prepayment') },
  ];
  const salesDeliveryTypeOptions = [
    { value: 0, label: t('salesOrder.deliveryTypes.none', 'None') },
    { value: 1, label: t('salesOrder.deliveryTypes.pickup', 'Pickup') },
    { value: 2, label: t('salesOrder.deliveryTypes.direct', 'Direct delivery') },
  ];
  const sourcingOriginOptions = [
    { value: 0, label: t('salesOrder.sourcingOrigin.none', 'None') },
    { value: 1, label: t('salesOrder.sourcingOrigin.direct', 'Direct') },
    { value: 2, label: t('salesOrder.sourcingOrigin.indirect', 'Indirect') },
  ];
  const deliveryDateControlOptions = [
    { value: 0, label: t('salesOrder.deliveryDateControl.none', 'None') },
    { value: 1, label: t('salesOrder.deliveryDateControl.salesLeadTime', 'Sales lead time') },
    { value: 2, label: 'ATP' },
    { value: 3, label: 'CTP' },
  ];
  const ctpStatusOptions = [
    { value: 0, label: t('salesOrder.ctpStatus.none', 'None') },
    { value: 1, label: t('salesOrder.ctpStatus.complete', 'Complete') },
    { value: 2, label: t('salesOrder.ctpStatus.failed', 'Failed') },
  ];
  const carrierServiceOptions = [
    { value: 0, label: t('salesOrder.carrierService.none', 'None') },
    { value: 1, label: t('salesOrder.carrierService.ground', 'Ground') },
    { value: 2, label: t('salesOrder.carrierService.air', 'Air') },
    { value: 3, label: t('salesOrder.carrierService.ocean', 'Ocean') },
    { value: 4, label: t('salesOrder.carrierService.other', 'Other') },
  ];
  const reservationOptions = [
    { value: 0, label: t('salesOrder.reservationTypes.none', 'None') },
    { value: 1, label: t('salesOrder.reservationTypes.standard', 'Standard') },
    { value: 2, label: t('salesOrder.reservationTypes.sameLocation', 'Same location') },
    { value: 3, label: t('salesOrder.reservationTypes.sameWarehouse', 'Same warehouse') },
    { value: 4, label: t('salesOrder.reservationTypes.sameSite', 'Same site') },
  ];
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
  const [deliveryAddressDrawerOpen, setDeliveryAddressDrawerOpen] = useState(false);
  const [deliveryAddressDrawerTarget, setDeliveryAddressDrawerTarget] = useState<'header' | 'line'>('header');
  const [deliveryAddressInitialData, setDeliveryAddressInitialData] = useState<LogisticsPostalAddress | null>(null);
  const [savingLineDetail, setSavingLineDetail] = useState(false);
  const [totalsOpen, setTotalsOpen] = useState(false);
  const [confirmationsOpen, setConfirmationsOpen] = useState(false);
  const [copyMode, setCopyMode] = useState<SalesOrderCopyMode>();
  const [copySourceId, setCopySourceId] = useState<string>();
  const [copyBusy, setCopyBusy] = useState(false);
  const [copyError, setCopyError] = useState('');
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
  const recalculateSalesOrderDiscounts = async () => {
    if (!order || !canEditHeader) return;
    try {
      await salesOrderLinesApi.recalculateDiscounts(order.id);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['sales-order-lines', order.id] }),
        queryClient.invalidateQueries({ queryKey: ['sales-order-totals', order.id] }),
        orderQuery.refetch(),
      ]);
    } catch (error) {
      setHeaderError(error instanceof Error ? error.message : t('errors.generic'));
    }
  };

  const salesOrderRibbonGroups: ActionPaneRibbonGroup[] = [
    {
      id: 'new',
      label: 'New',
      actions: [
        { id: 'purchase-order', label: 'Purchase order' },
        { id: 'direct-delivery', label: 'Direct delivery' },
      ],
    },
    {
      id: 'maintain',
      label: 'Maintain',
      actions: [{
        id: 'cancel',
        label: 'Cancel',
        disabled: order?.salesStatus.toLowerCase() !== 'backorder',
        onClick: async () => {
          if (!order || !window.confirm(`Cancel the remaining quantity for ${order.salesId}?`)) return;
          try {
            await salesOrderListApi.cancel(order.id);
            await Promise.all([
              queryClient.invalidateQueries({ queryKey: ['accounts-receivable', 'sales-orders'] }),
              queryClient.invalidateQueries({ queryKey: ['sales-order-lines', order.id] }),
              queryClient.invalidateQueries({ queryKey: ['sales-order-totals', order.id] }),
            ]);
          } catch (error) {
            setHeaderError(error instanceof Error ? error.message : t('errors.generic'));
          }
        },
      }],
    },
    {
      id: 'payments',
      label: 'Payments',
      actions: [{ id: 'payments', label: 'Payments', disabled: true }],
    },
    {
      id: 'copy',
      label: 'Copy',
      actions: [
        { id: 'from-all', label: 'From all', disabled: !canEditHeader, onClick: () => { setCopyError(''); setCopySourceId(undefined); setCopyMode('fromAll'); } },
        { id: 'from-journal', label: 'From journal', disabled: !canEditHeader, onClick: () => { setCopyError(''); setCopySourceId(undefined); setCopyMode('fromJournal'); } },
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
        {
          id: 'total-discount',
          label: 'Recalculate discounts',
          disabled: !canEditHeader,
          onClick: () => void recalculateSalesOrderDiscounts(),
        },
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
        { id: 'confirmation', label: 'Confirmation', onClick: () => setConfirmationsOpen(true) },
        { id: 'pro-forma-confirmation', label: 'Pro forma confirmation' },
      ],
    },
    {
      id: 'actions',
      label: 'Actions',
      actions: [{ id: 'confirm-now', label: 'Confirm now', disabled: !canEditHeader || Boolean(activeHeader), onClick: () => setConfirmationsOpen(true) }],
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
        { id: 'sales-order-confirmations', label: 'Sales order confirmations', onClick: () => setConfirmationsOpen(true) },
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

  const copySourcesQuery = useQuery({
    queryKey: ['sales-order-copy-sources', order?.id, copyMode],
    queryFn: ({ signal }) => salesOrderCopyApi.documents(order!.id, copyMode!, signal),
    enabled: Boolean(order && copyMode),
  });
  const copyLinesQuery = useQuery({
    queryKey: ['sales-order-copy-lines', order?.id, copyMode, copySourceId],
    queryFn: ({ signal }) => salesOrderCopyApi.lines(order!.id, copyMode!, copySourceId!, signal),
    enabled: Boolean(order && copyMode && copySourceId !== undefined),
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
      shippingDateRequested: order.shippingDateRequested?.slice(0, 10) ?? '',
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
      deliveryName: order.deliveryName,
      deliveryPostalAddress: order.deliveryPostalAddress,
      shippingDateConfirmed: order.shippingDateConfirmed?.slice(0, 10) ?? '',
      receiptDateConfirmed: order.receiptDateConfirmed?.slice(0, 10) ?? '',
      deliveryDateControlType: order.deliveryDateControlType,
      mpsFullRunCtpStatus: order.mpsFullRunCtpStatus,
      blindShipment: order.blindShipment,
      residentialDestination: order.residentialDestination,
      excludeFromMasterPlanning: order.excludeFromMasterPlanning,
      deliveryReason: order.deliveryReason,
      exportReason: order.exportReason,
      shippingCarrier: order.shippingCarrier,
      carrierId: order.carrierId,
      carrierGroup: order.carrierGroup,
      brokerId: order.brokerId,
      transportMode: order.transportMode,
      carrierService: order.carrierService,
      paymentMethod: order.paymentMethod,
      paymentSchedule: order.paymentSchedule,
      paymentSpecification: order.paymentSpecification,
      fixedDueDate: order.fixedDueDate?.slice(0, 10) ?? '',
      paymentTermsBaseDate: order.paymentTermsBaseDate?.slice(0, 10) ?? '',
      cashDiscountCode: order.cashDiscountCode,
      discountPercent: order.discountPercent ?? 0,
      totalDiscountPercent: order.totalDiscountPercent ?? 0,
      fixedExchangeRate: order.fixedExchangeRate,
      priceGroup: order.priceGroup,
      lineDiscountGroup: order.lineDiscountGroup,
      multiLineDiscountGroup: order.multiLineDiscountGroup,
      totalDiscountGroup: order.totalDiscountGroup,
      chargesGroup: order.chargesGroup,
      customerRebateGroup: order.customerRebateGroup,
      customerTmaGroup: order.customerTmaGroup,
      rebateReference: order.rebateReference,
      salesPool: order.salesPool,
      carrierCustomerAccount: order.carrierCustomerAccount,
      freightZone: order.freightZone,
      notes: order.notes,
      intercompanyAutoCreateOrders: order.intercompanyAutoCreateOrders,
      intercompanyDirectDelivery: order.intercompanyDirectDelivery,
      intercompanyOrigin: order.intercompanyOrigin,
      intercompanyAllowIndirectCreation: order.intercompanyAllowIndirectCreation,
      reservation: order.reservation,
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
      await queryClient.invalidateQueries({ queryKey: ['sales-order-totals', id] });
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
      if (name === 'oneTimeCustomer' || name === 'pricesIncludeSalesTax' || name === 'blindShipment' || name === 'residentialDestination' || name === 'excludeFromMasterPlanning' || name === 'intercompanyAutoCreateOrders' || name === 'intercompanyDirectDelivery' || name === 'intercompanyAllowIndirectCreation')
        return String(value) === 'true';
      if (name === 'salesType' || name === 'deliveryDateControlType' || name === 'mpsFullRunCtpStatus' || name === 'carrierService' || name === 'reservation' || name === 'intercompanyOrigin' || name === 'discountPercent' || name === 'totalDiscountPercent' || name === 'fixedExchangeRate') return Number(value);
      return String(value ?? '');
    };
    if (options) {
      return (
        <Box sx={{ minWidth: 0 }}>
          <LookupField
            name={name}
            masterRoute={headerMasterRoutes[name]}
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
        variant="outlined"
        label={label}
        type={
          name === 'deliveryDate' || name === 'orderDate' || name === 'deadline' || name === 'shippingDateConfirmed' || name === 'receiptDateConfirmed' || name === 'fixedDueDate' || name === 'paymentTermsBaseDate'
            ? 'date'
            : name === 'discountPercent' || name === 'totalDiscountPercent' || name === 'fixedExchangeRate'
              ? 'number'
              : 'text'
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
  const deliveryAddressesQuery = useQuery({
    queryKey: ['sales-order-delivery-addresses', order?.id],
    queryFn: ({ signal }) => salesOrderLinesApi.deliveryAddresses(order!.id, signal),
    enabled: Boolean(order),
    staleTime: 5 * 60 * 1000,
  });
  const totalsQuery = useQuery({
    queryKey: ['sales-order-totals', order?.id],
    queryFn: ({ signal }) => salesOrderLinesApi.totals(order!.id, signal),
    enabled: Boolean(order),
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
          masterRoute={headerMasterRoutes[name]}
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
    const updateDraft = {
      ...draft,
      usePriceAgreement: false,
    };
    lineDetailSaveLock.current = true;
    setSavingLineDetail(true);
    try {
      const saved = await salesOrderLinesApi.update(order!.id, updateDraft);
      const savedLine = {
        ...saved,
        ledgerDimensionDisplay: draft.ledgerDimensionDisplay ?? saved.ledgerDimensionDisplay,
      };
      queryClient.setQueryData<DetailLine[]>(['sales-order-lines', order!.id], (current = []) =>
        current.map((line) => (line.id === savedLine.id ? savedLine : line))
      );
      setLineDetailDraft({ ...savedLine, deliveryDate: savedLine.deliveryDate?.slice(0, 10) });
      await queryClient.invalidateQueries({
        queryKey: ['sales-order-totals', order!.id],
      });
      await orderQuery.refetch();
    } catch (error) {
      setHeaderError(error instanceof Error ? error.message : t('errors.generic'));
    } finally {
      lineDetailSaveLock.current = false;
      setSavingLineDetail(false);
    }
  };
  const openDeliveryAddressDrawer = (target: 'header' | 'line') => {
    if (target === 'header' && !activeHeader) startHeaderEdit();
    if (target === 'line' && !lineDetailDraft && selectedLine)
      setLineDetailDraft({ ...selectedLine, deliveryDate: selectedLine.deliveryDate?.slice(0, 10) });
    const selectedId = target === 'header'
      ? activeHeader?.deliveryPostalAddress ?? order?.deliveryPostalAddress
      : lineDetailDraft?.deliveryPostalAddress ?? selectedLine?.deliveryPostalAddress;
    const selectedAddress = deliveryAddressesQuery.data?.find(
      (address) => address.value === String(selectedId ?? '')
    );
    setDeliveryAddressInitialData(selectedAddress?.postalAddress ?? null);
    setDeliveryAddressDrawerTarget(target);
    setDeliveryAddressDrawerOpen(true);
  };
  const saveNewDeliveryAddress = async (address: LogisticsPostalAddress) => {
    if (!order) return;
    try {
      const lineId = deliveryAddressDrawerTarget === 'line' ? selectedLine?.id : undefined;
      if (deliveryAddressDrawerTarget === 'line' && !lineId)
        throw new Error('Select a sales order line before adding a delivery address.');
      const created = await salesOrderLinesApi.createDeliveryAddress(order.id, lineId, address);
      await queryClient.invalidateQueries({ queryKey: ['sales-order-delivery-addresses', order.id] });
      await Promise.all([deliveryAddressesQuery.refetch(), linesQuery.refetch(), orderQuery.refetch()]);
      if (deliveryAddressDrawerTarget === 'header') {
        setHeaderDraft((draft) => draft ? { ...draft, deliveryPostalAddress: created.id } : draft);
      } else {
        setLineDetailDraft((draft) => draft
          ? { ...draft, deliveryPostalAddress: created.id }
          : selectedLine
            ? { ...selectedLine, deliveryDate: selectedLine.deliveryDate?.slice(0, 10), deliveryPostalAddress: created.id }
            : draft);
      }
    } catch (error) {
      setHeaderError(error instanceof Error ? error.message : t('errors.generic'));
    }
  };
  if (orderQuery.isPending) return <LoadingState />;
  if (orderQuery.isError)
    return (
      <ErrorState message={orderQuery.error.message} onRetry={() => void orderQuery.refetch()} />
    );
  if (!order) return <ErrorState message={t('messages.noSalesOrders')} />;
  const amount = (value: number | null | undefined) =>
    `${(typeof value === 'number' && Number.isFinite(value) ? value : 0).toLocaleString(currentLanguage.code)} ${order.currencyCode}`;
  const number = (value: number | null | undefined, digits = 2) =>
    (typeof value === 'number' && Number.isFinite(value) ? value : 0).toLocaleString(currentLanguage.code, {
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
  const orderDiscount = totalsQuery.data?.orderDiscount ?? 0;
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
            { id: 'order-discount', label: 'Order discount', value: number(orderDiscount) },
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
  const field = (label: string, value?: React.ReactNode, editable = false, lookup = false) => (
    <Box key={label}>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <EditableViewField
        label={label}
        value={value == null ? '' : String(value)}
        lookup={lookup}
        onEdit={editable && canEditHeader && !activeHeader ? startHeaderEdit : undefined}
      />
    </Box>
  );
  const headerPanelField = (
    name: keyof SalesOrderHeaderInput,
    label: string,
    displayValue: React.ReactNode,
    options?: { value: string; label: string }[]
  ) => activeHeader
    ? name === 'inventSiteId' || name === 'inventLocationId'
      ? headerDimensionInput(name, label)
      : name === 'notes'
      ? (
          <TextField
            fullWidth
            size="small"
            label={label}
            multiline
            minRows={3}
            value={activeHeader.notes}
            disabled={savingHeader}
            onChange={(event) => setHeaderDraft((draft) => draft ? { ...draft, notes: event.target.value } : draft)}
          />
        )
      : headerInput(name, label, options)
    : field(label, displayValue, true, Boolean(options) || name === 'inventSiteId' || name === 'inventLocationId');
  const selectedDeliveryAddress = deliveryAddressesQuery.data?.find(
    (address) => address.value === String(activeHeader?.deliveryPostalAddress ?? order.deliveryPostalAddress)
  );
  const deliveryAddressPanel = (
    <Box sx={{ minWidth: 0 }}>
      <Typography variant="caption" color="text.secondary">Delivery address</Typography>
      <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
        {activeHeader ? (
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <LookupField
              name="deliveryPostalAddress"
              label="Delivery address"
              value={activeHeader.deliveryPostalAddress}
              options={(deliveryAddressesQuery.data ?? []).map((address) => ({
                id: address.value,
                code: address.label,
                name: `${address.label}${address.isPrimary ? ' (Primary)' : ''}`,
              }))}
              displayMode="select"
              searchable
              lazyLoading={false}
              disabled={savingHeader || deliveryAddressesQuery.isLoading}
              onChange={(value) => setHeaderDraft((draft) => draft
                ? { ...draft, deliveryPostalAddress: String(value ?? '') }
                : draft)}
            />
          </Box>
        ) : (
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <EditableViewField
              label="Delivery address"
              value={selectedDeliveryAddress?.label ?? order.deliveryName ?? ''}
              lookup
              onEdit={canEditHeader ? startHeaderEdit : undefined}
            />
          </Box>
        )}
        <IconButton
          size="small"
          aria-label={selectedDeliveryAddress ? 'Add or update delivery address' : 'Add delivery address'}
          disabled={!canEditHeader || savingHeader}
          onClick={() => openDeliveryAddressDrawer('header')}
          sx={{ border: 1, borderColor: 'divider', borderRadius: 1, flex: '0 0 auto' }}
        >
          <AddIcon fontSize="small" />
        </IconButton>
      </Stack>
      <TextField
        fullWidth
        size="small"
        variant="outlined"
        label="Address"
        value={selectedDeliveryAddress?.address ?? order.deliveryAddress ?? ''}
        multiline
        minRows={3}
        slotProps={{ input: { readOnly: true }, inputLabel: { shrink: true } }}
      />
    </Box>
  );
  const headerGroup = (title: string, children: React.ReactNode) => (
    <Box sx={{ minWidth: 0, display: 'grid', alignContent: 'start', gap: 1.25 }}>
      <Typography sx={{ fontSize: d365.labelFontSize, fontWeight: 700, lineHeight: 1.5, textTransform: 'uppercase' }}>{title}</Typography>
      {children}
    </Box>
  );
  const header = (
    <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: 3 }}>
      {headerGroup('DELIVERY ADDRESS', <>
        {headerPanelField('deliveryName', 'Name', order.deliveryName)}
        {deliveryAddressPanel}
      </>)}
      {headerGroup('DELIVERY DATE', <>
        {headerPanelField('shippingDateRequested', 'Requested ship date', order.shippingDateRequested?.slice(0, 10) ?? '')}
        {headerPanelField('deliveryDate', 'Requested receipt date', order.deliveryDate?.slice(0, 10) ?? '')}
        {headerPanelField('shippingDateConfirmed', 'Confirmed ship date', order.shippingDateConfirmed?.slice(0, 10) ?? '')}
        {headerPanelField('receiptDateConfirmed', 'Confirmed receipt date', order.receiptDateConfirmed?.slice(0, 10) ?? '')}
      </>)}
      {headerGroup('REFERENCES', <>
        {headerPanelField('customerReference', 'Customer reference', order.customerReference)}
        {headerPanelField('customerRequisitionNumber', 'Customer requisition', order.customerRequisitionNumber)}
      </>)}
      {headerGroup('DISCOUNTS', <>
        {field('Total discount', amount(totalDiscount))}
        {headerPanelField('totalDiscountPercent', 'Total discount %', number(order.totalDiscountPercent), undefined)}
      </>)}
      {headerGroup('WAREHOUSE', <>
        {field('Release status', order.releaseStatus)}
        {headerPanelField('inventSiteId', 'Site', order.inventSiteId)}
        {headerPanelField('inventLocationId', 'Warehouse', order.inventLocationId)}
      </>)}
      {headerGroup('TRANSPORTATION', <>
        {headerPanelField('freightZone', 'Routes', order.freightZone)}
        {headerPanelField('carrierCustomerAccount', 'Carrier customer account number', order.carrierCustomerAccount)}
      </>)}
      {headerGroup('NOTES', headerPanelField('notes', 'Notes', order.notes))}
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
      return field(label, String(displayedLine[name] ?? ''), name !== 'itemNumber');
    return (
      <TextField
        key={name}
        fullWidth
        size="small"
        variant="outlined"
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
                name === 'priceUnit' ||
                name === 'lineDiscount' ||
                name === 'lineDiscountPercent')
            ) {
              const updated = synchronizeSalesLineDiscount(draft, name, value);
              return name === 'unitPrice' || name === 'priceUnit'
                ? { ...updated, usePriceAgreement: false }
                : updated;
            }
            return { ...draft, [name]: value };
          });
        }}
        onBlur={() => void saveLineDetail()}
      />
    );
  };
  const lineMultilineField = (name: 'description' | 'deliveryName', label: string) => {
    if (!displayedLine) return null;
    if (!activeHeader) return field(label, displayedLine[name], true);
    return (
      <TextField
        fullWidth
        size="small"
        variant="outlined"
        label={label}
        multiline
        minRows={4}
        value={lineDetailDraft?.[name] ?? ''}
        disabled={savingLineDetail}
        onChange={(event) => {
          const value = event.target.value;
          setLineDetailDraft((draft) => draft ? { ...draft, [name]: value } : draft);
        }}
        onBlur={() => void saveLineDetail()}
      />
    );
  };
  const dimensionField = (name: 'site' | 'warehouse', label: string) => {
    if (!displayedLine) return null;
    if (!activeHeader) return field(label, displayedLine[name], true, true);
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
  const deliveryAddressField = () => {
    if (!displayedLine) return null;
    const options = deliveryAddressesQuery.data ?? [];
    const selected = options.find((option) => option.value === String(displayedLine.deliveryPostalAddress ?? ''));
    if (!activeHeader)
      return <>{field('Delivery address', selected?.label ?? '', true, true)}{field('Address', selected?.address ?? '', true)}</>;
    return (
      <>
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <LookupField
              name="deliveryPostalAddress"
              label="Delivery address"
              value={lineDetailDraft?.deliveryPostalAddress ?? ''}
              options={options.map((option) => ({
                id: option.value,
                code: option.label,
                name: `${option.label}${option.isPrimary ? ' (Primary)' : ''}`,
              }))}
              displayMode="select"
              searchable
              lazyLoading={false}
              disabled={savingLineDetail || deliveryAddressesQuery.isLoading}
              onChange={(value) => {
                if (!lineDetailDraft) return;
                const next = { ...lineDetailDraft, deliveryPostalAddress: String(value ?? '') };
                setLineDetailDraft(next);
                void saveLineDetail(next);
              }}
            />
          </Box>
          <IconButton
            size="small"
            aria-label="Add delivery address"
            onClick={() => openDeliveryAddressDrawer('line')}
          >
            <AddIcon fontSize="small" />
          </IconButton>
        </Stack>
        {field('Address', selected?.address ?? '')}
      </>
    );
  };
  const unitField = () => {
    const label = t('fields.unit');
    if (!displayedLine) return null;
    if (!activeHeader) return field(label, displayedLine.unit, true, true);
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
  const packingUnitField = () => {
    const label = t('salesOrder.packingUnit', 'وحدة التعبئة');
    if (!displayedLine) return null;
    if (!activeHeader) return field(label, displayedLine.packingUnit, true, true);
    return lineLookupShell(
      <LookupField
        name="packingUnit"
        label={label}
        value={lineDetailDraft?.packingUnit ?? ''}
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
          const next = { ...lineDetailDraft, packingUnit: String(value ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const lineValue = (name: keyof DetailLine, label: string, date = false, editable = false, lookup = false) => {
    const value = displayedLine?.[name];
    const text = value == null || value === '' ? undefined : String(value);
    return field(label, date ? text?.slice(0, 10) : text, editable, lookup);
  };
  const lineLookupField = (
    name: 'deliveryMode' | 'deliveryTerms',
    label: string,
    options: { value: string; label: string }[] = []
  ) => {
    if (!displayedLine) return null;
    if (!activeHeader) return lineValue(name, label, false, true, true);
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
  const lineEnumField = (
    name: 'lineType' | 'deliveryType' | 'lineDeliveryType' | 'sourcingOrigin' | 'deliveryDateControlType' | 'mpsFullRunCtpStatus' | 'shipCarrierDlvType' | 'reservation' | 'intercompanyOrigin' | 'itemReferenceType',
    label: string,
    options: { value: number; label: string }[]
  ) => {
    if (!displayedLine) return null;
    const value = Number(displayedLine[name] ?? 0);
    if (!activeHeader)
      return field(label, options.find((option) => option.value === value)?.label ?? String(value), true, true);
    return lineLookupShell(
      <LookupField
        name={name}
        label={label}
        value={value}
        options={options.map((option) => ({
          id: option.value,
          code: String(option.value),
          name: option.label,
        }))}
        displayMode="select"
        searchable={false}
        lazyLoading={false}
        disabled={savingLineDetail}
        onChange={(nextValue) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, [name]: Number(nextValue) };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const taxGroupField = (name: 'taxGroup' | 'taxItemGroup', label: string) => {
    if (!displayedLine) return null;
    if (!activeHeader) return lineValue(name, label, false, true, true);
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
  const lineBooleanField = (
    name: 'autoBatchReservation' | 'sameBatchSelection' | 'scrap' | 'stopped' | 'preventPartialDelivery' | 'directDelivery' | 'excludeFromMasterPlanning' | 'excludeFromRebate' | 'excludeFromRebateManagement',
    label: string
  ) => {
    if (!displayedLine) return null;
    const checked = name === 'directDelivery'
      ? Number(displayedLine.lineDeliveryType ?? 0) === 2
      : name === 'excludeFromMasterPlanning'
        ? Boolean(displayedLine.excludeFromMasterPlanning)
        : name === 'excludeFromRebate' || name === 'excludeFromRebateManagement'
          ? Boolean(displayedLine[name])
        : Boolean(displayedLine[name]);
    if (!activeHeader)
      return field(label, checked ? t('common.yes', 'Yes') : t('common.no', 'No'), true);
    return (
      <Box sx={{ minWidth: 0, minHeight: 48 }}>
        <Typography sx={{ mb: '6px', fontFamily: d365.fontFamily, fontSize: d365.labelFontSize, lineHeight: 1.2 }}>
          {label}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', height: d365.controlHeight }}>
          <Switch
              size="small"
              checked={checked}
              disabled={savingLineDetail}
              onChange={(_, nextValue) => {
                if (!lineDetailDraft) return;
                const next = name === 'directDelivery'
                  ? { ...lineDetailDraft, directDelivery: nextValue, lineDeliveryType: nextValue ? 2 : 0 }
                  : name === 'excludeFromMasterPlanning' || name === 'excludeFromRebate' || name === 'excludeFromRebateManagement'
                    ? { ...lineDetailDraft, [name]: nextValue }
                    : { ...lineDetailDraft, [name]: nextValue };
                setLineDetailDraft(next);
                void saveLineDetail(next);
              }}
          />
          <Typography sx={{ fontFamily: d365.fontFamily, fontSize: d365.fontSize }}>
            {checked ? t('common.yes', 'Yes') : t('common.no', 'No')}
          </Typography>
        </Box>
      </Box>
    );
  };
  const returnLotField = () => {
    if (!displayedLine) return null;
    const label = t('salesOrder.returnLotId', 'Return lot ID');
    if (!activeHeader) return field(label, displayedLine.returnLotId, true, true);
    return lineLookupShell(
      <LookupField
        name="returnLotId"
        label={label}
        value={lineDetailDraft?.returnLotId ?? ''}
        options={
          lineDetailDraft?.returnLotId
            ? [{
                id: lineDetailDraft.returnLotId,
                code: lineDetailDraft.returnLotId,
                name: lineDetailDraft.returnLotId,
              }]
            : []
        }
        queryKey={['sales-order-return-lots', displayedLine.itemNumber]}
        fetchPage={({ pageNumber, pageSize, search, signal }) =>
          salesOrderLinesApi.returnLots({
            itemNumber: displayedLine.itemNumber,
            pageNumber,
            pageSize,
            search,
            signal,
          })
        }
        displayMode="select"
        searchable
        lazyLoading
        sideMode="server"
        disabled={savingLineDetail}
        onChange={(value) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, returnLotId: String(value ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const inventoryTrackingField = (name: 'batchNumber' | 'serialNumber', label: string) => {
    if (!displayedLine) return null;
    const fetchPage = name === 'batchNumber'
      ? ({ pageNumber, pageSize, search, signal }: { pageNumber: number; pageSize: number; search: string; signal?: AbortSignal }) =>
          salesOrderLinesApi.batchNumbers({ itemNumber: displayedLine.itemNumber, pageNumber, pageSize, search, signal })
      : ({ pageNumber, pageSize, search, signal }: { pageNumber: number; pageSize: number; search: string; signal?: AbortSignal }) =>
          salesOrderLinesApi.serialNumbers({ itemNumber: displayedLine.itemNumber, pageNumber, pageSize, search, signal });
    const value = displayedLine[name] ?? '';
    if (!activeHeader) return field(label, value, true, true);
    return lineLookupShell(
      <LookupField
        name={name}
        label={label}
        value={value}
        options={value ? [{ id: value, code: value, name: value }] : []}
        queryKey={['sales-order-inventory-tracking', name, displayedLine.itemNumber]}
        fetchPage={fetchPage}
        displayMode="select"
        searchable
        lazyLoading
        sideMode="server"
        disabled={savingLineDetail}
        onChange={(nextValue) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, [name]: String(nextValue ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const mainAccountField = () => {
    if (!displayedLine) return null;
    const label = t('salesOrder.mainAccount', 'Main account');
    if (!activeHeader) return field(label, displayedLine.ledgerDimensionDisplay ?? '', true, true);
    return lineLookupShell(
      <LookupField
        name="ledgerDimension"
        label={label}
        value={displayedLine.ledgerDimension ?? 0}
        options={
          displayedLine.ledgerDimension
            ? [{
                id: displayedLine.ledgerDimension,
                code: displayedLine.ledgerDimensionDisplay ?? String(displayedLine.ledgerDimension),
                name: displayedLine.ledgerDimensionDisplay ?? String(displayedLine.ledgerDimension),
              }]
            : []
        }
        queryKey={['sales-order-ledger-dimensions']}
        fetchPage={salesOrderLinesApi.ledgerDimensions}
        displayMode="select"
        searchable
        lazyLoading
        sideMode="server"
        disabled={savingLineDetail}
        onChange={(value, option) => {
          if (!lineDetailDraft) return;
          const next = {
            ...lineDetailDraft,
            ledgerDimension: Number(value ?? 0),
            ledgerDimensionDisplay: option?.name ?? '',
          };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const lineTabContent = (): React.ReactNode => {
    switch (lineTab) {
      case 'General':
        return (
          <Box
            sx={{
              gridColumn: '1 / -1',
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
              gap: 3,
            }}
          >
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.generalGroups.orderLine', 'Order line')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineValue('salesCategory', t('salesOrder.salesCategory', 'Sales category'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.generalGroups.productName', 'Product name')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineValue('productName', t('salesOrder.productName', 'Product name'))}
                {lineMultilineField('description', t('salesOrder.text', 'النص'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.generalGroups.externalReferences', 'External references')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineDetailField('customerReference', t('salesOrder.external', 'External'))}
                <Box sx={{ maxWidth: 72 }}>
                  {lineDetailField('customerLineNumber', t('salesOrder.customerLineNumber', 'Line number'), 'number')}
                </Box>
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.generalGroups.intercompany', 'Intercompany')}
              </Typography>
              {lineEnumField('intercompanyOrigin', t('salesOrder.intercompanyOrigin', 'Origin (intercompany orders)'), [
                { value: 0, label: t('common.none', 'None') },
                { value: 1, label: t('salesOrder.intercompanyOutbound', 'Outbound') },
                { value: 2, label: t('salesOrder.intercompanyInbound', 'Inbound') },
              ])}
              <Typography variant="overline" fontWeight={700} sx={{ mt: 1.5 }}>
                {t('salesOrder.generalGroups.status', 'Status')}
              </Typography>
              {lineValue('salesStatus', t('salesOrder.lineStatus', 'Line status'))}
            </Box>
            <Box>
              <Box sx={{ display: 'grid', gap: 1.5, pt: 2.5 }}>
                {lineBooleanField('stopped', t('salesOrder.stopped', 'Stopped'))}
                {lineBooleanField(
                  'preventPartialDelivery',
                  t('salesOrder.preventPartialDelivery', 'Prevent partial delivery')
                )}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.generalGroups.fulfillment', 'Fulfillment')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {field(t('salesOrder.fulfillmentStatus', 'حالة التنفيذ'), t('salesOrder.unknown', 'غير معروف'))}
              </Box>
            </Box>
          </Box>
        );
      case 'Setup':
        return (
          <Box
            sx={{
              gridColumn: '1 / -1',
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
              gap: 3,
            }}
          >
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.setupGroups.inventory', 'Inventory')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineValue('inventTransId', t('salesOrder.lotId', 'Lot ID'))}
                {lineEnumField(
                  'reservation',
                  t('salesOrder.reservation', 'Reservation'),
                  reservationOptions
                )}
              </Box>
            </Box>
            <Box>
              <Box sx={{ display: 'grid', gap: 1.5, pt: 2.5 }}>
                {lineBooleanField(
                  'autoBatchReservation',
                  t('salesOrder.autoBatchReservation', 'Auto batch reservation')
                )}
                {lineBooleanField(
                  'sameBatchSelection',
                  t('salesOrder.sameBatchSelection', 'Same batch selection')
                )}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.setupGroups.returnedOrder', 'Returned order')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {returnLotField()}
                {lineValue('costPrice', t('salesOrder.returnCostPrice', 'تكلفة الإرجاع'))}
                {lineBooleanField('scrap', t('salesOrder.scrap', 'Scrap'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.setupGroups.posting', 'Posting')}
              </Typography>
              {mainAccountField()}
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.setupGroups.salesTax', 'Sales tax')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {taxGroupField('taxItemGroup', t('salesOrder.itemSalesTaxGroup', 'Item sales tax group'))}
                {taxGroupField('taxGroup', t('salesOrder.salesTaxGroup', 'Sales tax group'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.setupGroups.commission', 'Commission')}
              </Typography>
              {lineDetailField('salesGroup', t('salesOrder.salesGroup', 'Sales group'))}
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.setupGroups.dateTime', 'Date and time')}
              </Typography>
              {field(
                t('salesOrder.createdDateTime', 'Created date and time'),
                displayedLine.createdAt
                  ? new Date(displayedLine.createdAt).toLocaleString(currentLanguage.code)
                  : undefined
              )}
            </Box>
          </Box>
        );
      case 'Address':
        return (
          <Box sx={{ gridColumn: '1 / -1', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 220px))', gap: 3 }}>
            <Box>
              <Typography variant="overline" fontWeight={700}>DELIVERY ADDRESS</Typography>
              <Box sx={{ display: 'grid', gap: 1.5, mt: 1 }}>
                {lineMultilineField('deliveryName', 'Name')}
                {deliveryAddressField()}
              </Box>
            </Box>
          </Box>
        );
      case 'Product':
        return (
          <Box
            sx={{
              gridColumn: '1 / -1',
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
              gap: 3,
            }}
          >
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.sourcingGroups.tracking', 'أبعاد التتبع')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineDetailField('configId', 'Configuration')}
                {lineDetailField('inventSizeId', 'Size')}
                {lineDetailField('inventColorId', 'Color')}
                {lineDetailField('inventStyleId', 'Style')}
                {lineDetailField('inventVersionId', 'Version')}
                {inventoryTrackingField('batchNumber', t('salesOrder.batchNumber', 'رقم الدفعة'))}
                {inventoryTrackingField('serialNumber', t('salesOrder.serialNumber', 'الرقم التسلسلي'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.sourcingGroups.storage', 'أبعاد التخزين')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {dimensionField('site', t('salesOrderQuickCreate.site', 'Site'))}
                {dimensionField('warehouse', t('salesOrderQuickCreate.warehouse', 'Warehouse'))}
              </Box>
            </Box>
            <Box>
              <Box sx={{ display: 'grid', gap: 1.5, pt: 2.5 }}>
                {lineValue('location', t('salesOrder.location', 'الموقع'))}
                {lineValue('inventoryStatus', t('salesOrder.inventoryStatus', 'حالة المخزون'))}
                {lineValue('licensePlate', t('salesOrder.licensePlate', 'لوحة الترخيص'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.sourcingGroups.itemReference', 'مرجع الصنف')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineEnumField('itemReferenceType', t('salesOrder.referenceType', 'Reference type'), [
                  { value: 0, label: t('common.none', 'None') },
                  { value: 1, label: 'Purchase order' },
                  { value: 2, label: t('common.salesOrder', 'Sales order') },
                  { value: 3, label: 'Inventory journal' },
                  { value: 4, label: 'Transfer order' },
                  { value: 5, label: 'Production order' },
                  { value: 6, label: 'Inventory adjustment' },
                  { value: 7, label: 'Physical inventory count' },
                  { value: 8, label: 'Return order' },
                ])}
                {lineValue('itemReferenceNumber', t('salesOrder.referenceNumber', 'رقم المرجع'))}
                {lineValue('itemReferenceLot', t('salesOrder.referenceLot', 'تشغيلة المرجع'))}
              </Box>
            </Box>
          </Box>
        );
      case 'Packing':
        return (
          <Box
            sx={{
              gridColumn: '1 / -1',
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
              gap: 3,
            }}
          >
            <Box>
              <Typography variant="overline" fontWeight={700}>
                {t('salesOrder.packingGroups.material', 'مواد التعبئة')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {packingUnitField()}
                {lineDetailField(
                  'packingUnitQuantity',
                  t('salesOrder.packingUnitQuantity', 'كمية وحدة التعبئة'),
                  'number'
                )}
              </Box>
            </Box>
          </Box>
        );
      case 'Delivery':
        return (
          <>
            <Box sx={{ gridColumn: '1 / -1', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: 3 }}>
              <Box>
                <Typography variant="overline" fontWeight={700}>DELIVERY DATE</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineDetailField('shippingDateRequested', 'Requested ship date', 'date')}
                  {lineDetailField('deliveryDate', 'Requested receipt date', 'date')}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" fontWeight={700}>CONFIRMED DATES</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineDetailField('shippingDateConfirmed', 'Confirmed ship date', 'date')}
                  {lineDetailField('receiptDateConfirmed', 'Confirmed receipt date', 'date')}
                  {lineEnumField('deliveryDateControlType', 'Delivery date control', deliveryDateControlOptions)}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" fontWeight={700}>PLANNING</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineEnumField('mpsFullRunCtpStatus', 'Batch CTP status', ctpStatusOptions)}
                  {lineDetailField('planningPriority', 'Planning priority', 'number')}
                  {lineLookupField('deliveryTerms', t('customerQuickCreate.fields.deliveryTerms'), lookupsQuery.data?.deliveryTerms)}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" fontWeight={700}>DELIVERY</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineDetailField('overDeliveryPercent', 'Overdelivery', 'number')}
                  {lineDetailField('underDeliveryPercent', 'Underdelivery', 'number')}
                  {lineLookupField('deliveryMode', t('fields.deliveryMode'), lookupsQuery.data?.deliveryModes)}
                  {lineEnumField('deliveryType', 'Delivery type', salesDeliveryTypeOptions)}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" fontWeight={700}>DIRECT DELIVERY</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineBooleanField('directDelivery', 'Direct delivery')}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" fontWeight={700}>CARRIER INFORMATION</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineEnumField('shipCarrierDlvType', 'Carrier service', carrierServiceOptions)}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" fontWeight={700}>STORAGE LOCATION</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {dimensionField('site', t('salesOrderQuickCreate.site', 'Site'))}
                  {dimensionField('warehouse', t('salesOrderQuickCreate.warehouse', 'Warehouse'))}
                </Box>
              </Box>
            </Box>
          </>
        );
      case 'Sourcing':
        return (
          <Box sx={{ gridColumn: '1 / -1', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: 3 }}>
            <Box sx={{ display: 'grid', gap: 1.5 }}>
              {lineEnumField('deliveryType', 'Delivery type', [
                { value: 0, label: 'Stock' },
                { value: 1, label: 'Pickup' },
                { value: 2, label: 'Direct delivery' },
              ])}
            </Box>
            <Box sx={{ display: 'grid', gap: 1.5 }}>
              {lineEnumField('sourcingOrigin', 'Sourcing origin', sourcingOriginOptions)}
            </Box>
            <Box sx={{ display: 'grid', gap: 1.5 }}>{field('Sourcing vendor')}</Box>
            <Box sx={{ display: 'grid', gap: 1.5 }}>{field('Sourcing company')}</Box>
            <Box sx={{ display: 'grid', gap: 1.5 }}>{dimensionField('site', 'Sourcing site')}</Box>
            <Box sx={{ display: 'grid', gap: 1.5 }}>
              {dimensionField('warehouse', 'Sourcing warehouse')}
              <Typography variant="overline" fontWeight={700}>MASTER PLANNING</Typography>
              {lineBooleanField('excludeFromMasterPlanning', 'Exclude from master planning')}
            </Box>
          </Box>
        );      case 'Price and discount':
        return (
          <Box sx={{ gridColumn: '1 / -1', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: 3 }}>
            <Box>
              <Typography variant="overline" fontWeight={700}>DISCOUNT</Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineDetailField('lineDiscount', 'Discount', 'number')}
                {lineDetailField('lineDiscountPercent', 'Discount percent', 'number')}
              </Box>
            </Box>
            <Box>
              <Box sx={{ display: 'grid', gap: 1.5, pt: 2.5 }}>
                {lineDetailField('multiLineDiscount', 'Multiline discount', 'number')}
                {lineDetailField('multiLineDiscountPercent', 'Multiline discount percentage', 'number')}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>PRICES</Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineDetailField('priceUnit', 'Price unit', 'number')}
                {lineDetailField('salesMarkup', 'Sales charges', 'number')}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" fontWeight={700}>REBATES</Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineBooleanField('excludeFromRebate', 'Exclude from rebate')}
                {lineBooleanField('excludeFromRebateManagement', 'Exclude from rebate management')}
              </Box>
            </Box>
          </Box>
        );      case 'Foreign trade':
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
  const headerLookupFields = new Set([
    'customerAccount', 'invoiceAccount', 'customerGroup', 'currencyCode',
    'inventSiteId', 'inventLocationId', 'taxGroupId', 'salesGroup',
    'deliveryPostalAddress', 'deliveryMode', 'deliveryTerms', 'salesPool',
    'paymentTerms', 'paymentMethod', 'paymentSchedule', 'paymentSpecification',
    'cashDiscountCode', 'priceGroup', 'lineDiscountGroup',
    'multiLineDiscountGroup', 'totalDiscountGroup', 'chargesGroup',
    'customerRebateGroup', 'customerTmaGroup', 'reservation',
  ]);
  const readOnlyHeaderFields = new Set([
    'salesId', 'customerName', 'customerAccount', 'customerGroup',
    'salesStatus', 'documentStatus', 'deliveryAddress',
    'reportingCurrencyFixedExchangeRate', 'releaseStatus',
  ]);
  const headerField = (name: string, label: string, sectionTitle?: string): DetailFieldConfig => ({
    name,
    masterRoute: headerMasterRoutes[name as keyof SalesOrderHeaderInput],
    label: t(`salesOrder.headerFields.${name}`, label),
    type: readOnlyHeaderFields.has(name) ? 'display' : headerLookupFields.has(name) ? 'select' : 'text',
    sectionTitle,
    ...((activeHeader && (name in activeHeader || name === 'deliveryAddress') && name !== 'id')
      || name === 'deliveryPostalAddress'
      || name === 'deliveryAddress'
      ? {
          renderOwnLabel: true,
          render: () => {
            if (name === 'inventSiteId' || name === 'inventLocationId')
              return headerDimensionInput(name, t(`salesOrder.headerFields.${name}`, label));
            if (name === 'deliveryAddress') {
              const selectedAddress = deliveryAddressesQuery.data?.find(
                (address) => address.value === String(activeHeader?.deliveryPostalAddress ?? order.deliveryPostalAddress)
              );
              return (
                <TextField
                  fullWidth
                  size="small"
                  variant="outlined"
                  label={t(`salesOrder.headerFields.${name}`, label)}
                  value={selectedAddress?.address ?? order.deliveryAddress ?? ''}
                  multiline
                  minRows={3}
                  slotProps={{ input: { readOnly: true }, inputLabel: { shrink: true } }}
                />
              );
            }
            if (name === 'deliveryPostalAddress') {
              const addressOptions = deliveryAddressesQuery.data ?? [];
              const selected = addressOptions.find((address) =>
                address.value === String(activeHeader?.deliveryPostalAddress ?? order.deliveryPostalAddress)
              );
              return (
                <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                  {activeHeader ? (
                    <Box sx={{ flex: 1, minWidth: 0 }}>
                      <LookupField
                        name="deliveryPostalAddress"
                        label={t(`salesOrder.headerFields.${name}`, label)}
                        value={activeHeader.deliveryPostalAddress}
                        options={addressOptions.map((address) => ({
                          id: address.value,
                          code: address.label,
                          name: `${address.label}${address.isPrimary ? ' (Primary)' : ''}`,
                        }))}
                        displayMode="select"
                        searchable
                        lazyLoading={false}
                        disabled={savingHeader || deliveryAddressesQuery.isLoading}
                        onChange={(value) => setHeaderDraft((draft) => draft ? { ...draft, deliveryPostalAddress: String(value ?? '') } : draft)}
                      />
                    </Box>
                  ) : (
                    <Box sx={{ flex: 1, minWidth: 0 }}>
                      <EditableViewField label="Delivery address" value={selected?.label ?? order.deliveryName ?? ''} lookup onEdit={canEditHeader ? startHeaderEdit : undefined} />
                    </Box>
                  )}
                  <IconButton
                    size="small"
                    aria-label={selected ? 'Add or update delivery address' : 'Add delivery address'}
                    disabled={!canEditHeader || savingHeader}
                    onClick={() => openDeliveryAddressDrawer('header')}
                    sx={{ border: 1, borderColor: 'divider', borderRadius: 1, flex: '0 0 auto' }}
                  >
                    <AddIcon fontSize="small" />
                  </IconButton>
                </Stack>
              );
            }
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
            } else if (name === 'paymentMethod') {
              options = lookupsQuery.data?.paymentMethods;
            } else if (name === 'reservation') {
              options = reservationOptions.map((option) => ({ value: String(option.value), label: option.label }));
            } else if (name === 'intercompanyOrigin') {
              options = [
                { value: '0', label: 'None' },
                { value: '1', label: 'Outbound' },
                { value: '2', label: 'Inbound' },
              ];
            } else if (name === 'deliveryMode') {
              options = lookupsQuery.data?.deliveryModes;
            } else if (name === 'deliveryTerms') {
              options = lookupsQuery.data?.deliveryTerms;
            } else if (name === 'salesPool') {
              options = lookupsQuery.data?.salesPools;
            } else if (name === 'paymentSchedule') {
              options = lookupsQuery.data?.paymentSchedules;
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
            } else if (name === 'blindShipment' || name === 'residentialDestination' || name === 'excludeFromMasterPlanning' || name === 'intercompanyAutoCreateOrders' || name === 'intercompanyDirectDelivery' || name === 'intercompanyAllowIndirectCreation') {
              options = [
                { value: 'false', label: t('common.no', 'No') },
                { value: 'true', label: t('common.yes', 'Yes') },
              ];
            } else if (name === 'deliveryDateControlType') {
              options = [
                { value: '0', label: 'None' },
                { value: '1', label: 'Sales lead time' },
                { value: '2', label: 'ATP' },
                { value: '3', label: 'CTP' },
              ];
            } else if (name === 'mpsFullRunCtpStatus') {
              options = [
                { value: '0', label: 'None' },
                { value: '1', label: 'Complete' },
                { value: '2', label: 'Failed' },
              ];
            } else if (name === 'carrierService') {
              options = [
                { value: '0', label: 'None' },
                { value: '1', label: 'Ground' },
                { value: '2', label: 'Air' },
                { value: '3', label: 'Ocean' },
                { value: '4', label: 'Other' },
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
            headerField('salesPool', 'Pool'),
            headerField('reservation', 'Reservation'),
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
    {
      id: 'header-address',
      title: 'Address',
      visualVariant: 'legalEntity',
      columns: 5,
      groups: [
        {
          id: 'delivery-address',
          title: 'Delivery address',
          fields: [
            headerField('deliveryName', 'Name'),
            headerField('deliveryPostalAddress', 'Delivery address'),
            headerField('deliveryAddress', 'Address'),
          ],
        },
      ],
    },
    {
      id: 'header-delivery',
      title: 'Delivery',
      visualVariant: 'legalEntity',
      columns: 6,
      columnGap: 40,
      groups: [
        { id: 'delivery-dates', title: 'DELIVERY DATE', fields: [headerField('shippingDateRequested', 'Requested ship date'), headerField('deliveryDate', 'Requested receipt date')] },
        { id: 'confirmed-dates', title: 'CONFIRMED DATES', fields: [headerField('shippingDateConfirmed', 'Confirmed ship date'), headerField('receiptDateConfirmed', 'Confirmed receipt date'), headerField('deliveryDateControlType', 'Delivery date control')] },
        { id: 'planning', title: 'PLANNING', fields: [headerField('mpsFullRunCtpStatus', 'Batch CTP status')] },
        { id: 'delivery-details', title: 'DETAILS', fields: [headerField('blindShipment', 'Blind shipment'), headerField('deliveryReason', 'Delivery reason'), headerField('exportReason', 'Reason for export')] },
        { id: 'delivery-mode', title: 'MISC. DELIVERY INFO', fields: [headerField('deliveryMode', 'Mode of delivery'), headerField('deliveryTerms', 'Delivery terms')] },
        { id: 'carrier', title: 'CARRIER INFORMATION', fields: [headerField('residentialDestination', 'Residential destination'), headerField('shippingCarrier', 'Shipping carrier'), headerField('carrierId', 'Carrier ID'), headerField('carrierService', 'Carrier service'), headerField('carrierGroup', 'Carrier group'), headerField('brokerId', 'Broker ID'), headerField('transportMode', 'Mode')] },
      ],
    },
    {
      id: 'header-price-discount',
      title: 'Price and discount',
      visualVariant: 'legalEntity',
      columns: 6,
      columnGap: 40,
      groups: [
        { id: 'header-currency', title: 'CURRENCY', fields: [headerField('currencyCode', 'Currency'), headerField('fixedExchangeRate', 'Fixed exchange rate'), headerField('reportingCurrencyFixedExchangeRate', 'Reporting currency fixed exchange rate')] },
        { id: 'header-payment', title: 'PAYMENT', fields: [headerField('paymentTerms', 'Payment'), headerField('fixedDueDate', 'Due date'), headerField('paymentMethod', 'Method of payment'), headerField('paymentSpecification', 'Payment specification')] },
        { id: 'header-payment-details', fields: [headerField('paymentSchedule', 'Payment schedule'), headerField('paymentTermsBaseDate', 'Payment terms base date'), headerField('cashDiscountCode', 'Cash discount'), headerField('discountPercent', 'Discount percentage')] },
        { id: 'header-discount-charges', title: 'DISCOUNT OR CHARGES', fields: [headerField('priceGroup', 'Price group'), headerField('lineDiscountGroup', 'Line discount group')] },
        { id: 'header-discount-groups', fields: [headerField('multiLineDiscountGroup', 'Multiline discount group'), headerField('totalDiscountGroup', 'Total discount group'), headerField('chargesGroup', 'Charges group'), headerField('customerRebateGroup', 'Customer rebate group')] },
        { id: 'header-rebates', title: 'REBATES', fields: [headerField('customerTmaGroup', 'Customer TMA group'), headerField('rebateReference', 'Rebate reference'), headerField('totalDiscountPercent', 'Total discount %')] },
      ],
    },
    {
      id: 'header-intercompany',
      title: 'Intercompany settings',
      visualVariant: 'legalEntity',
      columns: 4,
      groups: [{ id: 'intercompany', fields: [headerField('intercompanyAutoCreateOrders', 'Autocreate intercompany orders'), headerField('intercompanyDirectDelivery', 'Direct delivery'), headerField('intercompanyOrigin', 'Origin (intercompany orders)'), headerField('intercompanyAllowIndirectCreation', 'Allow indirect creation')] }],
    },
    {
      id: 'header-warehouse',
      title: 'Warehouse',
      visualVariant: 'legalEntity',
      columns: 4,
      groups: [{ id: 'warehouse-status', title: 'STATUS', fields: [headerField('releaseStatus', 'Release status')] }],
    },
    {
      id: 'header-transportation',
      title: 'Transportation',
      visualVariant: 'legalEntity',
      columns: 4,
      groups: [{ id: 'transport-routes', title: 'ROUTES', fields: [headerField('carrierCustomerAccount', 'Carrier customer account number'), headerField('freightZone', 'Freight zone')] }],
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
        {field(t('fields.subtotal'), amount(subtotal))}
        {field(t('fields.discount'), amount(totalDiscount))}
        {field(t('fields.tax'), amount(salesTax))}
        {field(t('fields.total'), amount(invoiceAmount))}
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
          onFieldEdit: canEditHeader && !savingHeader ? () => startHeaderEdit() : undefined,
          canEditField: (name) => !readOnlyHeaderFields.has(name),
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
          presentation: { mode: 'list', listWidth: 280, listResizable: true, listInitiallyVisible: false, listVisibilityStorageKey: 'ixapp.sales-order.list-visible', detailEndPadding: 8 },
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
      <SalesOrderConfirmationDialog
        open={confirmationsOpen}
        orderId={order.id}
        salesId={order.salesId}
        canPost={Boolean(canEditHeader) && !activeHeader}
        onClose={() => setConfirmationsOpen(false)}
        onPosted={async () => {
          await Promise.all([
            queryClient.invalidateQueries({ queryKey: ['accounts-receivable', 'sales-orders'] }),
            queryClient.invalidateQueries({ queryKey: ['sales-order-totals', order.id] }),
          ]);
        }}
      />
      <DocumentCopyDrawer
        open={Boolean(copyMode)}
        title={copyMode === 'fromJournal' ? 'Copy from journal' : 'Copy from all'}
        sourceSectionLabel={copyMode === 'fromJournal' ? 'Confirmation journals' : 'Sales orders'}
        additionalSections={copyMode === 'fromJournal'
          ? ['Quotations', 'Packing slips', 'Invoices', 'Project invoices']
          : ['Quotations', 'Confirmation']}
        sources={copySourcesQuery.data ?? []}
        lines={copyLinesQuery.data ?? []}
        loadingSources={copySourcesQuery.isLoading}
        loadingLines={copyLinesQuery.isLoading}
        busy={copyBusy}
        error={copyError
          || (copySourcesQuery.error instanceof Error ? copySourcesQuery.error.message : '')
          || (copyLinesQuery.error instanceof Error ? copyLinesQuery.error.message : '')}
        onSourceChange={(source) => setCopySourceId(source?.id)}
        onClose={() => {
          if (copyBusy) return;
          setCopyMode(undefined);
          setCopySourceId(undefined);
          setCopyError('');
        }}
        onCopy={async (lineIds, options) => {
          if (!order || !copyMode || copyBusy) return;
          setCopyBusy(true);
          setCopyError('');
          try {
            await salesOrderCopyApi.copy(order.id, copyMode, lineIds, options);
            await Promise.all([
              queryClient.invalidateQueries({ queryKey: ['sales-order-lines', order.id] }),
              queryClient.invalidateQueries({ queryKey: ['sales-order-totals', order.id] }),
              queryClient.invalidateQueries({ queryKey: ['accounts-receivable', 'sales-orders'] }),
            ]);
            setCopyMode(undefined);
            setCopySourceId(undefined);
          } catch (error) {
            setCopyError(error instanceof Error ? error.message : t('errors.generic'));
          } finally {
            setCopyBusy(false);
          }
        }}
      />
      <LogisticsPostalAddressDrawer
        open={deliveryAddressDrawerOpen}
        onClose={() => setDeliveryAddressDrawerOpen(false)}
        onSave={(address) => { void saveNewDeliveryAddress(address); }}
        initialData={deliveryAddressInitialData}
      />
    </SalesOrderLinesProvider>
  );
}
