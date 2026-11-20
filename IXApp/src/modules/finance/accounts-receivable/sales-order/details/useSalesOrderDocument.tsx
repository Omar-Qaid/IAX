import { useGridFilterRow } from '@shared/components/data-grid/hooks/useGridFilterRow';
import { useEffect, useRef, useState } from 'react';
import { Box, TextField } from '@mui/material';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import type { ActionPaneRibbonGroup } from '@shared/components/action-pane/ActionPaneRibbon';
import { useNavigate, useParams } from 'react-router-dom';
import { salesOrderLinesApi } from '../api/salesOrderLinesApi';
import type { LogisticsPostalAddress } from '@shared/types/logistics';
import { PERMISSIONS } from '@core/permissions/permissions';
import { usePermission } from '@core/permissions/usePermission';
import { useQueryClient } from '@tanstack/react-query';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../../routes/accountsReceivableRoutePaths';
import {
  salesOrderListApi,
  type SalesOrderHeaderInput,
  type SalesOrderListRecord,
} from '../api/salesOrderListApi';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { LookupField } from '@shared/components/lookups/LookupField';
import { type SalesOrderCopyMode } from '../api/salesOrderCopyApi';
import { salesOrderConfirmationApi } from '../../sales-order-confirm/api/salesOrderConfirmationApi';
import { salesOrderQueryKeys } from '../../queries/accountsReceivableQueryKeys';
import { useCompanyStore } from '@core/company/useCompanyStore';
import {
  headerMasterRoutes,
  headerPreviewEntities,
  type DetailLine,
} from './salesOrderDetailsConfig';
import { useSalesOrderOptions } from './useSalesOrderOptions';
import { useSalesOrderDocumentData } from './useSalesOrderDocumentData';

/** Header fields sent as booleans / numbers (everything else is text). */
const BOOLEAN_HEADER_FIELDS = new Set<string>([
  'oneTimeCustomer',
  'pricesIncludeSalesTax',
  'blindShipment',
  'residentialDestination',
  'excludeFromMasterPlanning',
  'intercompanyAutoCreateOrders',
  'intercompanyDirectDelivery',
  'intercompanyAllowIndirectCreation',
]);
const NUMERIC_HEADER_FIELDS = new Set<string>([
  'salesType',
  'deliveryDateControlType',
  'mpsFullRunCtpStatus',
  'carrierService',
  'reservation',
  'intercompanyOrigin',
  'discountPercent',
  'totalDiscountPercent',
  'fixedExchangeRate',
]);

/**
 * State, server data and actions of the sales order document page: header and
 * line editing, address creation, confirmation/copy dialogs and the action pane.
 * The view builders in this folder render from what it returns.
 */
export function useSalesOrderDocument() {
  const company = useCompanyStore((state) => state.currentCompany);
  const { t, currentLanguage } = useAppTranslation();
  const {
    salesDeliveryTypeOptions,
    sourcingOriginOptions,
    deliveryDateControlOptions,
    ctpStatusOptions,
    carrierServiceOptions,
    reservationOptions,
  } = useSalesOrderOptions();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const { salesOrderId } = useParams<{ salesOrderId: string }>();
  const [headerDraft, setHeaderDraft] = useState<(SalesOrderHeaderInput & { id: string }) | null>(
    null
  );
  const [savingHeader, setSavingHeader] = useState(false);
  const headerSaveLock = useRef(false);
  const cancelOrderLock = useRef(false);
  const [cancelingOrder, setCancelingOrder] = useState(false);
  const [headerError, setHeaderError] = useState('');
  useUnsavedChanges(Boolean(headerDraft));
  const [lineFilterVisible, setLineFilterVisible] = useGridFilterRow(
    'accounts-receivable.sales-order-lines'
  );
  const { hasPermission: canEditLines } = usePermission(PERMISSIONS.SALES_ORDER_UPDATE);
  const [lineTab, setLineTab] = useState('General');
  const [tab, setTab] = useState('lines');
  const [selectedLineId, setSelectedLineId] = useState<string>();
  const [lineDetailDraft, setLineDetailDraft] = useState<DetailLine | null>(null);
  const [deliveryAddressDrawerOpen, setDeliveryAddressDrawerOpen] = useState(false);
  const [deliveryAddressDrawerTarget, setDeliveryAddressDrawerTarget] = useState<'header' | 'line'>(
    'header'
  );
  const [deliveryAddressInitialData, setDeliveryAddressInitialData] =
    useState<LogisticsPostalAddress | null>(null);
  const [savingLineDetail, setSavingLineDetail] = useState(false);
  const [totalsOpen, setTotalsOpen] = useState(false);
  const [confirmationsOpen, setConfirmationsOpen] = useState(false);
  const [packingSlipPostingOpen, setPackingSlipPostingOpen] = useState(false);
  const [invoicePostingOpen, setInvoicePostingOpen] = useState(false);
  const [confirmationMode, setConfirmationMode] = useState<'confirmation' | 'proForma'>(
    'confirmation'
  );
  const [confirmNowBusy, setConfirmNowBusy] = useState(false);
  const confirmNowLock = useRef(false);
  const recalculateLock = useRef(false);
  const [recalculating, setRecalculating] = useState(false);
  const [confirmNowError, setConfirmNowError] = useState('');
  const [copyMode, setCopyMode] = useState<SalesOrderCopyMode>();
  const [copySourceId, setCopySourceId] = useState<string>();
  const [copyBusy, setCopyBusy] = useState(false);
  const [copyError, setCopyError] = useState('');
  const lineDetailSaveLock = useRef(false);
  const {
    orders,
    orderQuery,
    orderLoading,
    order,
    refetchOrder,
    customersQuery,
    lookupsQuery,
    copySourcesQuery,
    copyLinesQuery,
    linesQuery,
    lines,
    deliveryAddressesQuery,
    totalsQuery,
    dimensionsQuery,
    taxGroupsQuery,
    invalidateOrder,
  } = useSalesOrderDocumentData({
    salesOrderId,
    editingOrderId: headerDraft?.id,
    copyMode,
    copySourceId,
  });
  const activeHeader = headerDraft?.id === order?.id ? headerDraft : null;
  const canEditHeader = canEditLines && order?.salesStatus.toLowerCase() === 'backorder';
  const recalculateSalesOrderDiscounts = async () => {
    if (!order || !canEditHeader || activeHeader || recalculateLock.current) return;
    recalculateLock.current = true;
    setRecalculating(true);
    setHeaderError('');
    try {
      await salesOrderLinesApi.recalculateDiscounts(order.id);
      await Promise.all([invalidateOrder(order.id), refetchOrder()]);
    } catch (error) {
      setHeaderError(error instanceof Error ? error.message : t('errors.generic'));
    } finally {
      recalculateLock.current = false;
      setRecalculating(false);
    }
  };

  const salesOrderRibbonGroups: ActionPaneRibbonGroup[] = [
    {
      id: 'new',
      label: t('salesOrder.ui.new', 'New'),
      actions: [
        {
          id: 'purchase-order',
          label: t('salesOrder.ui.purchaseOrder', 'Purchase order'),
          disabled: true,
        },
        {
          id: 'direct-delivery',
          label: t('salesOrder.ui.directDelivery', 'Direct delivery'),
          disabled: true,
        },
      ],
    },
    {
      id: 'maintain',
      label: t('salesOrder.ui.maintain', 'Maintain'),
      actions: [
        {
          id: 'cancel',
          label: t('salesOrder.ui.cancel', 'Cancel'),
          disabled: !canEditHeader || Boolean(activeHeader) || cancelingOrder,
          onClick: async () => {
            if (!order || !canEditHeader || activeHeader || cancelOrderLock.current) return;
            if (!window.confirm(t('salesOrder.confirmCancelRemaining', { salesId: order.salesId })))
              return;
            cancelOrderLock.current = true;
            setCancelingOrder(true);
            setHeaderError('');
            try {
              await salesOrderListApi.cancel(order.id);
              await Promise.all([invalidateOrder(order.id)]);
            } catch (error) {
              setHeaderError(error instanceof Error ? error.message : t('errors.generic'));
            } finally {
              cancelOrderLock.current = false;
              setCancelingOrder(false);
            }
          },
        },
      ],
    },
    {
      id: 'payments',
      label: t('salesOrder.ui.payments', 'Payments'),
      actions: [{ id: 'payments', label: t('salesOrder.ui.payments', 'Payments'), disabled: true }],
    },
    {
      id: 'copy',
      label: t('salesOrder.ui.copy', 'Copy'),
      actions: [
        {
          id: 'from-all',
          label: t('salesOrder.ui.fromAll', 'From all'),
          disabled: !canEditHeader || Boolean(activeHeader) || copyBusy,
          onClick: () => {
            setCopyError('');
            setCopySourceId(undefined);
            setCopyMode('fromAll');
          },
        },
        {
          id: 'from-journal',
          label: t('salesOrder.ui.fromJournal', 'From journal'),
          disabled: !canEditHeader || Boolean(activeHeader) || copyBusy,
          onClick: () => {
            setCopyError('');
            setCopySourceId(undefined);
            setCopyMode('fromJournal');
          },
        },
      ],
    },
    {
      id: 'view',
      label: t('salesOrder.ui.view', 'View'),
      actions: [
        {
          id: 'totals',
          label: t('salesOrder.ui.totals', 'Totals'),
          onClick: () => setTotalsOpen(true),
        },
        {
          id: 'order-events',
          label: t('salesOrder.ui.orderEvents', 'Order events'),
          disabled: true,
        },
      ],
    },
    {
      id: 'functions',
      label: t('salesOrder.ui.functions', 'Functions'),
      actions: [
        { id: 'sales-order-recap', label: t('salesOrder.ui.recap', 'Recap'), disabled: true },
        { id: 'order-holds', label: t('salesOrder.ui.orderHolds', 'Order holds'), disabled: true },
      ],
    },
    {
      id: 'attachments',
      label: t('salesOrder.ui.attachments', 'Attachments'),
      actions: [{ id: 'notes', label: t('salesOrder.ui.notes', 'Notes'), disabled: true }],
    },
  ];

  const sellRibbonGroups: ActionPaneRibbonGroup[] = [
    {
      id: 'credit-note',
      label: t('salesOrder.ui.creditNote', 'Credit note'),
      actions: [
        { id: 'credit-note', label: t('salesOrder.ui.creditNote', 'Credit note'), disabled: true },
      ],
    },
    {
      id: 'charges',
      label: t('salesOrder.ui.charges', 'Charges'),
      actions: [
        {
          id: 'maintain-charges',
          label: t('salesOrder.ui.maintainCharges', 'Maintain charges'),
          disabled: !order || Boolean(activeHeader),
          onClick: () =>
            order && navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrderCharges(order.id)),
        },
        {
          id: 'allocate-charges',
          label: t('salesOrder.ui.allocateCharges', 'Allocate charges'),
          disabled: true,
        },
      ],
    },
    {
      id: 'tax',
      label: t('salesOrder.ui.tax', 'Tax'),
      actions: [
        { id: 'sales-tax', label: t('salesOrder.ui.salesTax', 'Sales tax'), disabled: true },
      ],
    },
    {
      id: 'calculate-delivery',
      label: t('salesOrder.ui.calculate', 'Calculate'),
      actions: [
        {
          id: 'confirmed-delivery-dates',
          label: t('salesOrder.ui.confirmedDeliveryDates', 'Confirmed delivery dates'),
          disabled: true,
        },
        {
          id: 'total-discount',
          label: t('salesOrder.ui.recalculateDiscounts', 'Recalculate discounts'),
          disabled: !canEditHeader || Boolean(activeHeader) || recalculating,
          onClick: () => void recalculateSalesOrderDiscounts(),
        },
      ],
    },
    {
      id: 'calculate-price',
      label: '',
      actions: [
        {
          id: 'supplementary-items',
          label: t('salesOrder.ui.supplementaryItems', 'Supplementary items'),
          disabled: true,
        },
        {
          id: 'tiered-charges',
          label: t('salesOrder.ui.tieredCharges', 'Tiered charges'),
          disabled: true,
        },
        {
          id: 'push-price-and-totals',
          label: t('salesOrder.ui.pushPriceAndTotals', 'Push price and totals'),
          disabled: true,
        },
      ],
    },
    {
      id: 'generate',
      label: t('salesOrder.ui.generate', 'Generate'),
      actions: [
        {
          id: 'confirmation',
          label: t('salesOrder.ui.confirmation', 'Confirmation'),
          disabled: !canEditHeader || Boolean(activeHeader) || confirmNowBusy,
          onClick: () => {
            setConfirmationMode('confirmation');
            setConfirmationsOpen(true);
          },
        },
        {
          id: 'pro-forma-confirmation',
          label: t('salesOrder.ui.proFormaConfirmation', 'Pro forma confirmation'),
          disabled: !order || Boolean(activeHeader),
          onClick: () => {
            setConfirmationMode('proForma');
            setConfirmationsOpen(true);
          },
        },
      ],
    },
    {
      id: 'pick-and-pack',
      label: t('salesOrder.ui.pickAndPack', 'Pick and pack'),
      actions: [
        {
          id: 'post-packing-slip',
          label: t('salesOrder.ui.packingSlip', 'Packing slip'),
          disabled: !canEditHeader || Boolean(activeHeader),
          onClick: () => setPackingSlipPostingOpen(true),
        },
      ],
    },
    {
      id: 'invoice',
      label: t('salesOrder.ui.invoice'),
      actions: [
        {
          id: 'post-invoice',
          label: t('salesOrder.ui.invoice'),
          disabled: !canEditHeader || Boolean(activeHeader),
          onClick: () => setInvoicePostingOpen(true),
        },
        {
          id: 'cust-invoice-journal',
          label: t('salesOrder.ui.invoiceJournal'),
          onClick: () => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUST_INVOICE_JOURNAL),
        },
      ],
    },
    {
      id: 'actions',
      label: t('salesOrder.ui.actions', 'Actions'),
      actions: [
        {
          id: 'confirm-now',
          label: t('salesOrder.ui.confirmNow', 'Confirm now'),
          disabled: !canEditHeader || Boolean(activeHeader) || confirmNowBusy,
          onClick: async () => {
            if (!order || !canEditHeader || activeHeader || confirmNowLock.current) return;
            confirmNowLock.current = true;
            setConfirmNowBusy(true);
            setConfirmNowError('');
            try {
              await salesOrderConfirmationApi.post(
                order.id,
                new Date().toLocaleDateString('en-CA'),
                crypto.randomUUID()
              );
              await Promise.all([invalidateOrder(order.id)]);
            } catch (cause) {
              setConfirmNowError(
                cause instanceof Error ? cause.message : 'The sales order could not be confirmed.'
              );
            } finally {
              confirmNowLock.current = false;
              setConfirmNowBusy(false);
            }
          },
        },
      ],
    },
    {
      id: 'apply',
      label: t('salesOrder.ui.apply', 'Apply'),
      actions: [
        {
          id: 'service-agreement',
          label: t('salesOrder.ui.serviceAgreement', 'Service agreement'),
          disabled: true,
        },
      ],
    },
    {
      id: 'journals',
      label: t('salesOrder.ui.journals', 'Journals'),
      actions: [
        {
          id: 'sales-order-confirmations',
          label: t('salesOrder.ui.salesOrderConfirmations', 'Sales order confirmations'),
          onClick: () =>
            order && navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.custConfirmJournal(order.id)),
        },
        {
          id: 'sales-order-packing-slips',
          label: t('salesOrder.ui.packingSlipJournal', 'Packing slip journal'),
          onClick: () =>
            order && navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.custPackingSlipJournal(order.id)),
        },
        {
          id: 'quotation-confirmation-journal',
          label: t('salesOrder.ui.quotationConfirmationJournal', 'Quotation confirmation journal'),
          disabled: true,
        },
      ],
    },
    {
      id: 'prepayment',
      label: t('salesOrder.ui.prepayment', 'Prepayment'),
      actions: [
        { id: 'prepayment', label: t('salesOrder.ui.prepayment', 'Prepayment'), disabled: true },
      ],
    },
  ];

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
      await queryClient.invalidateQueries({ queryKey: salesOrderQueryKeys.totals(company, id) });
      await refetchOrder({ throwOnError: true });
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
      if (BOOLEAN_HEADER_FIELDS.has(name)) return String(value) === 'true';
      if (NUMERIC_HEADER_FIELDS.has(name)) return Number(value);
      return String(value ?? '');
    };
    if (options) {
      return (
        <Box sx={{ minWidth: 0 }}>
          <LookupField
            name={name}
            masterRoute={headerMasterRoutes[name]}
            previewEntity={headerPreviewEntities[name as keyof SalesOrderListRecord]}
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
          name === 'deliveryDate' ||
          name === 'orderDate' ||
          name === 'deadline' ||
          name === 'shippingDateConfirmed' ||
          name === 'receiptDateConfirmed' ||
          name === 'fixedDueDate' ||
          name === 'paymentTermsBaseDate'
            ? 'date'
            : name === 'discountPercent' ||
                name === 'totalDiscountPercent' ||
                name === 'fixedExchangeRate'
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
  const selectedLine: DetailLine | undefined =
    lines.find((line) => line.id === selectedLineId) ?? lines[0];
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
      queryClient.setQueryData<DetailLine[]>(
        salesOrderQueryKeys.lines(company, order!.id),
        (current = []) => current.map((line) => (line.id === savedLine.id ? savedLine : line))
      );
      setLineDetailDraft({ ...savedLine, deliveryDate: savedLine.deliveryDate?.slice(0, 10) });
      await queryClient.invalidateQueries({
        queryKey: salesOrderQueryKeys.totals(company, order!.id),
      });
      await refetchOrder();
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
      setLineDetailDraft({
        ...selectedLine,
        deliveryDate: selectedLine.deliveryDate?.slice(0, 10),
      });
    const selectedId =
      target === 'header'
        ? (activeHeader?.deliveryPostalAddress ?? order?.deliveryPostalAddress)
        : (lineDetailDraft?.deliveryPostalAddress ?? selectedLine?.deliveryPostalAddress);
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
      await queryClient.invalidateQueries({
        queryKey: salesOrderQueryKeys.deliveryAddresses(company, order.id),
      });
      await Promise.all([deliveryAddressesQuery.refetch(), linesQuery.refetch(), refetchOrder()]);
      if (deliveryAddressDrawerTarget === 'header') {
        setHeaderDraft((draft) =>
          draft ? { ...draft, deliveryPostalAddress: created.id } : draft
        );
      } else {
        setLineDetailDraft((draft) =>
          draft
            ? { ...draft, deliveryPostalAddress: created.id }
            : selectedLine
              ? {
                  ...selectedLine,
                  deliveryDate: selectedLine.deliveryDate?.slice(0, 10),
                  deliveryPostalAddress: created.id,
                }
              : draft
        );
      }
    } catch (error) {
      setHeaderError(error instanceof Error ? error.message : t('errors.generic'));
    }
  };
  return {
    invalidateOrder,
    company,
    queryClient,
    navigate,
    headerSaveLock,
    cancelOrderLock,
    lineDetailSaveLock,
    activeHeader,
    canEditHeader,
    recalculateSalesOrderDiscounts,
    salesOrderRibbonGroups,
    sellRibbonGroups,
    startHeaderEdit,
    saveHeader,
    headerInput,
    selectedLine,
    headerDimensionInput,
    saveLineDetail,
    openDeliveryAddressDrawer,
    saveNewDeliveryAddress,
    headerDraft,
    setHeaderDraft,
    savingHeader,
    setSavingHeader,
    cancelingOrder,
    setCancelingOrder,
    headerError,
    setHeaderError,
    lineFilterVisible,
    setLineFilterVisible,
    lineTab,
    setLineTab,
    tab,
    setTab,
    selectedLineId,
    setSelectedLineId,
    lineDetailDraft,
    setLineDetailDraft,
    deliveryAddressDrawerOpen,
    setDeliveryAddressDrawerOpen,
    deliveryAddressDrawerTarget,
    setDeliveryAddressDrawerTarget,
    deliveryAddressInitialData,
    setDeliveryAddressInitialData,
    savingLineDetail,
    setSavingLineDetail,
    totalsOpen,
    setTotalsOpen,
    confirmationsOpen,
    setConfirmationsOpen,
    packingSlipPostingOpen,
    setPackingSlipPostingOpen,
    invoicePostingOpen,
    setInvoicePostingOpen,
    confirmationMode,
    setConfirmationMode,
    confirmNowBusy,
    setConfirmNowBusy,
    confirmNowError,
    setConfirmNowError,
    copyMode,
    setCopyMode,
    copySourceId,
    setCopySourceId,
    copyBusy,
    setCopyBusy,
    copyError,
    setCopyError,
    t,
    currentLanguage,
    salesDeliveryTypeOptions,
    sourcingOriginOptions,
    deliveryDateControlOptions,
    ctpStatusOptions,
    carrierServiceOptions,
    reservationOptions,
    salesOrderId,
    canEditLines,
    orders,
    orderQuery,
    orderLoading,
    order,
    refetchOrder,
    customersQuery,
    lookupsQuery,
    copySourcesQuery,
    copyLinesQuery,
    linesQuery,
    lines,
    deliveryAddressesQuery,
    totalsQuery,
    dimensionsQuery,
    taxGroupsQuery,
  };
}

export type SalesOrderDocument = ReturnType<typeof useSalesOrderDocument>;
/** The document once its order has loaded (`order` is defined). */
export type SalesOrderDocumentView = Omit<SalesOrderDocument, 'order'> & {
  order: NonNullable<SalesOrderDocument['order']>;
};
