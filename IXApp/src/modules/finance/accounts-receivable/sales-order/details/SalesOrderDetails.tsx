import { Alert } from '@mui/material';
import React from 'react';
import { MasterDetailPage } from '@patterns/master-detail';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../../routes/accountsReceivableRoutePaths';
import { SalesOrderDetailsDialogs } from '../components/SalesOrderDetailsDialogs';
import {
  SalesOrderLinesProvider,
  SalesOrderDocumentActions,
} from '../lines/SalesOrderLineState';
import { useSalesOrderDocument } from './useSalesOrderDocument';
import { buildSalesOrderTotals } from './salesOrderTotals';
import { buildSalesOrderHeaderPanel } from './SalesOrderHeaderPanel';
import { buildSalesOrderLineFields } from './SalesOrderLineFields';
import { buildSalesOrderLineTabs } from './SalesOrderLineTabs';
import { buildSalesOrderHeaderSections } from './SalesOrderHeaderSections';

/** Sales-order workflows presented using the shared master-details pattern. */
export function SalesOrderDetails({
  detailPath = ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrder,
  listPath = ACCOUNTS_RECEIVABLE_ROUTE_PATHS.SALES_ORDERS,
}: { detailPath?: (id: string) => string; listPath?: string } = {}): React.ReactElement {
  const doc = useSalesOrderDocument();
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
  const s2 = { ...s1, ...buildSalesOrderHeaderPanel(s1) };
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
        navigationWidthStorageKey="ixapp.sales-order-v2.navigation-width"
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
            onSave={() => void saveHeader()}
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
            persistenceKey: 'ixapp.sales-order-v2.ribbon-pinned',
          },
          { id: 'sell', label: t('salesOrder.ui.sell', 'Sell'), groups: sellRibbonGroups },
        ]}
        commands={[
          'Manage',
          'Pick and pack',
          'Invoice',
          'Commerce',
          'General',
          'Warehouse',
          'Transportation',
          'Credit management',
        ].map((label) => ({ id: label, label, disabled: true }))}
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
    </SalesOrderLinesProvider>
  );
}
