import { Alert } from '@mui/material';
import { usePermission } from '@core/permissions/usePermission';
import { PERMISSIONS } from '@core/permissions/permissions';
import { CustOpenTrans } from '../../settlement/components/CustOpenTrans';
import type { ActionPaneRibbonGroup } from '@shared/components/action-pane/ActionPaneRibbon';
import React from 'react';
import { MasterDetailPage } from '@patterns/master-detail';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { recordTableId } from '@shared/components/documents/recordTableIds';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../../routes/accountsReceivableRoutePaths';
import { SalesOrderDetailsDialogs } from './SalesOrderDetailsDialogs';
import { SalesOrderLinesProvider, SalesOrderDocumentActions } from '../lines/SalesOrderLineState';
import { useSalesOrderDocument } from './useSalesOrderDocument';
import { buildSalesOrderTotals } from './salesOrderTotals';
import { buildSalesOrderHeaderFields } from './SalesOrderHeaderFields';
import { buildSalesOrderLineFields } from '../lines/SalesOrderLineFields';
import { buildSalesOrderLineTabs } from '../lines/SalesOrderLineTabs';
import { buildSalesOrderHeaderSections } from './SalesOrderHeaderSections';

/** Sales-order workflows presented using the shared master-details pattern. */
/** Reference command tabs of the D365 sales order; each one awaits its own page. */
const SALES_ORDER_COMMAND_IDS = [
  'manage',
  'commerce',
  'general',
  'warehouse',
  'transportation',
  'creditManagement',
] as const;

export function SalesOrderDetails({
  detailPath = ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrder,
  listPath = ACCOUNTS_RECEIVABLE_ROUTE_PATHS.SALES_ORDERS,
}: { detailPath?: (id: string) => string; listPath?: string } = {}): React.ReactElement {
  const doc = useSalesOrderDocument();
  const [settlementOpen, setSettlementOpen] = React.useState(false);
  const canViewSettlement = usePermission(PERMISSIONS.CUSTOMER_PAYMENT_VIEW).hasPermission;
  const { orderQuery, orderLoading, refetchOrder, t, order } = doc;
  if (orderLoading) return <LoadingState />;
  if (!order)
    return (
      <ErrorState
        message={orderQuery.isError ? orderQuery.error.message : t('messages.noSalesOrders')}
        onRetry={() => void refetchOrder()}
      />
    );
  const s0 = { ...doc, order };
  const s1 = { ...s0, ...buildSalesOrderTotals(s0) };
  const s2 = { ...s1, ...buildSalesOrderHeaderFields(s1) };
  const s3 = { ...s2, ...buildSalesOrderLineFields(s2) };
  const s4 = { ...s3, ...buildSalesOrderLineTabs(s3) };
  const scope = { ...s4, ...buildSalesOrderHeaderSections(s4) };
  const {
    navigate,
    orders,
    activeHeader,
    savingHeader,
    canEditHeader,
    startHeaderEdit,
    saveHeader,
    setHeaderDraft,
    headerError,
    setHeaderError,
    sections,
    readOnlyHeaderFields,
    salesOrderRibbonGroups,
    sellRibbonGroups,
    tab,
    setTab,
    setLineFilterVisible,
  } = scope;
  const command = (id: string) => sellRibbonGroups.flatMap((group) => group.actions).find((action) => action.id === id);
  const unavailable = (id: string, label: string) => ({ id, label: t(`salesOrder.ui.${id}`, label), disabled: true });
  const packing = command('post-packing-slip');
  const packingJournal = command('packing-slip-journal');
  const invoice = command('post-invoice');
  const invoiceJournal = command('cust-invoice-journal');
  const pickAndPackGroups: ActionPaneRibbonGroup[] = [
    { id: 'generate', label: t('salesOrder.ui.generate', 'Generate'), actions: [
      unavailable('generatePickingList', 'Generate picking list'), unavailable('pickingListRegistration', 'Picking list registration'), unavailable('intercompanyPickingList', 'Intercompany picking list'),
      ...(packing ? [{ ...packing, label: t('salesOrder.ui.postPackingSlip', 'Post packing slip') }] : []), unavailable('proFormaPackingSlip', 'Pro forma packing slip'),
    ] },
    { id: 'journals', label: t('salesOrder.ui.journals', 'Journals'), actions: [unavailable('pickingList', 'Picking list'), ...(packingJournal ? [{ ...packingJournal, label: t('salesOrder.ui.packingSlip', 'Packing slip') }] : [])] },
  ];
  const invoiceGroups: ActionPaneRibbonGroup[] = [
    { id: 'generate', label: t('salesOrder.ui.generate', 'Generate'), actions: [...(invoice ? [invoice] : []), unavailable('proFormaInvoice', 'Pro forma invoice')] },
    { id: 'bill', label: t('salesOrder.ui.bill', 'Bill'), actions: [unavailable('paymentSchedule', 'Payment schedule'), unavailable('cashFlowForecasts', 'Cash flow forecasts')] },
    { id: 'settle', label: t('salesOrder.ui.settle', 'Settle'), actions: [{ id: 'open-transactions', label: t('salesOrder.ui.openTransactions', 'Open transactions'), disabled: !canViewSettlement || Boolean(activeHeader) || savingHeader || !order.invoiceAccount, onClick: () => setSettlementOpen(true) }] },
    { id: 'correction', label: t('salesOrder.ui.correction', 'Correction'), actions: [unavailable('creditInvoicing', 'Credit invoicing')] },
    { id: 'journals', label: t('salesOrder.ui.journals', 'Journals'), actions: invoiceJournal ? [{ ...invoiceJournal, label: t('salesOrder.ui.invoice', 'Invoice') }] : [] },
    { id: 'prepayment', label: t('salesOrder.ui.prepayment', 'Prepayment'), actions: [unavailable('prepaymentProposal', 'Prepayment proposal'), unavailable('prepaymentInvoice', 'Prepayment invoice'), unavailable('applyPrepayment', 'Apply prepayment')] },
    { id: 'deductions', label: t('salesOrder.ui.deductions', 'Deductions'), actions: [unavailable('attachCreditToDeduction', 'Attach credit to deduction'), unavailable('detachCreditFromDeduction', 'Detach credit from deduction')] },
  ];
  const records = (
    orders.some((record) => record.id === order.id) ? orders : [order, ...orders]
  ).map((record) => ({
    ...record,
    title: record.salesId,
    subtitle: `${record.customerAccount} - ${record.customerName}`,
  }));
  return (
    <SalesOrderLinesProvider key={order.id}>
      <MasterDetailPage
        records={records}
        selectedId={order.id}
        onSelect={(id) => {
          if (!activeHeader && !savingHeader) navigate(detailPath(id));
        }}
        title={`${order.salesId} : ${order.customerName}`}
        subtitle={t('salesOrder.details', 'Sales order details')}
        viewLabel={t('pages.customers.standardView')}
        status={t(`status.${order.salesStatus.toLowerCase()}`, order.salesStatus)}
        breadcrumb={{
          listPath,
          labelKey: 'pages.salesOrder.breadcrumb',
        }}
        filterLabel={t('common.filter', 'Filter')}
        emptyLabel={t('messages.noSalesOrders')}
        editing={Boolean(activeHeader)}

        onRefresh={() => void refetchOrder()}
        navigationWidthStorageKey="ixapp.sales-order.navigation-width"
        backAction={
          <ActionPaneBackButton
            label={t('actions.back', 'Back')}
            disabled={Boolean(activeHeader) || savingHeader}
            onClick={() => navigate(listPath)}
          />
        }
        actionPane={
          <SalesOrderDocumentActions
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
            onSave={() => saveHeader()}
            onCancel={() => {
              if (!savingHeader) {
                setHeaderDraft(null);
                setHeaderError('');
              }
            }}
          />
        }
        ribbons={[
          {
            id: 'sales-order',
            label: t('salesOrder.ui.salesOrder', 'Sales order'),
            groups: salesOrderRibbonGroups,
            persistenceKey: 'ixapp.sales-order.ribbon-pinned',
          },
          {
            id: 'sell',
            label: t('salesOrder.ui.sell', 'Sell'),
            groups: sellRibbonGroups.filter((group) => group.id !== 'invoice'),
          },
          {
            id: 'pick-and-pack',
            label: t('salesOrder.ui.pickAndPack', 'Pick and pack'),
            groups: pickAndPackGroups,
          },
          {
            id: 'invoice',
            label: t('salesOrder.ui.invoice'),
            groups: invoiceGroups,
          },
        ]}
        commands={SALES_ORDER_COMMAND_IDS.map((id) => ({
          id,
          label: t(`salesOrder.commands.${id}`),
          disabled: true,
          tooltip: t('common.notAvailableYet'),
        }))}
        onSearch={() => {
          setTab('lines');
          setLineFilterVisible((visible) => !visible);
        }}
        viewTabs={{
          value: tab,
          onChange: (value) => setTab(value === 'header' ? 'header' : 'lines'),
        }}
        sections={
          headerError
            ? [
                {
                  id: 'save-error',
                  title: '',
                  hideHeader: true,
                  content: <Alert severity="error">{headerError}</Alert>,
                },
                ...sections,
              ]
            : sections
        }
        values={{ ...order, ...activeHeader }}
        onFieldEdit={canEditHeader && !savingHeader ? () => startHeaderEdit() : undefined}
        canEditField={(name) => !readOnlyHeaderFields.has(name)}
        recordTableName="SalesTable"
        getAuditRecordId={(record) => record.recId}
        attachments={
          activeHeader
            ? undefined
            : { refTableId: recordTableId('SalesTable'), refRecId: order.recId }
        }
        advancedFilter={{
          title: t('filters.title'),
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
        }}
        relatedInformation={{
          sections: [
            {
              id: 'customer',
              label: t('fields.customer', 'Customer'),
              defaultExpanded: true,
              content: `${order.customerAccount} - ${order.customerName}`,
            },
            {
              id: 'delivery',
              label: t('fields.delivery', 'Delivery'),
              content: `${order.deliveryDate} - ${order.deliveryMode}`,
            },
            {
              id: 'payment',
              label: t('fields.payment', 'Payment'),
              content: `${order.paymentTerms} - ${order.currencyCode}`,
            },
            { id: 'status', label: t('common.status'), content: order.documentStatus || '-' },
          ],
        }}
      />
      <SalesOrderDetailsDialogs scope={scope} />
      <CustOpenTrans open={settlementOpen} accountNum={order.invoiceAccount}
        customerName={order.invoiceAccount === order.customerAccount ? order.customerName : order.invoiceAccount}
        onClose={() => setSettlementOpen(false)} />
    </SalesOrderLinesProvider>
  );
}
