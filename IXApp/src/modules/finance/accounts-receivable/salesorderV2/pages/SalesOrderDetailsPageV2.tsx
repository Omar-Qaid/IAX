import { Alert } from '@mui/material';
import React from 'react';
import {
  MasterDetailPage,
  MasterDetailTabs,
  type MasterDetailCommand,
} from '@patterns/master-detail';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../../routes/accountsReceivableRoutePaths';
import { SalesOrderDetailsDialogs } from '../../sales-order/components/SalesOrderDetailsDialogs';
import {
  SalesOrderLinesProvider,
  SalesOrderDocumentActions,
} from '../../sales-order/lines/SalesOrderLineState';
import { useSalesOrderDocument } from '../../sales-order/details/useSalesOrderDocument';
import { buildSalesOrderTotals } from '../../sales-order/details/salesOrderTotals';
import { buildSalesOrderHeaderPanel } from '../../sales-order/details/SalesOrderHeaderPanel';
import { buildSalesOrderLineFields } from '../../sales-order/details/SalesOrderLineFields';
import { buildSalesOrderLineTabs } from '../../sales-order/details/SalesOrderLineTabs';
import { buildSalesOrderHeaderSections } from '../../sales-order/details/SalesOrderHeaderSections';
import { SALES_ORDER_V2_ROUTE_PATHS } from '../routes/salesOrderV2RoutePaths';

/** Sales-order workflows presented using the shared master-details pattern. */
export function SalesOrderDetailsPageV2(): React.ReactElement {
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
          if (!activeHeader && !savingHeader) navigate(SALES_ORDER_V2_ROUTE_PATHS.detail(id));
        }}
        title={`${order.salesId} : ${order.customerName}`}
        subtitle={t('salesOrder.details', 'Sales order details')}
        viewLabel={t('pages.customers.standardView')}
        status={t(`status.${order.salesStatus.toLowerCase()}`, order.salesStatus)}
        breadcrumb={{
          listPath: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.SALES_ORDERS,
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
            onClick={() => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.SALES_ORDERS)}
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
        ]
          .map<MasterDetailCommand>((label) => ({ id: label, label, disabled: true }))
          .concat([
            {
              id: 'search',
              label: t('actions.search', 'Search'),
              disabled: Boolean(activeHeader),
              onClick: () => {
                setTab('lines');
                setLineFilterVisible((visible) => !visible);
              },
            },
          ])}
        tabs={
          <MasterDetailTabs
            value={tab}
            onChange={(value) => setTab(value === 'header' ? 'header' : 'lines')}
            ariaLabel={t('salesOrder.details', 'Sales order details')}
            tabs={[
              { value: 'lines', label: t('common.lines', 'Lines') },
              { value: 'header', label: t('common.header', 'Header') },
            ]}
          />
        }
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
