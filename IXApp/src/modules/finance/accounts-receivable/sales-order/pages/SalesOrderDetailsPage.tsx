import React from 'react';
import { Alert, Snackbar, Stack, Tab, Tabs, Typography } from '@mui/material';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneRibbonTrigger } from '@shared/components/action-pane/ActionPaneRibbonTrigger';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LogisticsPostalAddressDrawer } from '@shared/components/logistics/LogisticsPostalAddressDrawer';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../../routes/accountsReceivableRoutePaths';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { DocumentTotalsDrawer } from '@patterns/document/DocumentTotalsDrawer';
import { DocumentCopyDrawer } from '@patterns/document/DocumentCopyDrawer';
import { salesOrderCopyApi } from '../api/salesOrderCopyApi';
import { SalesOrderConfirmationDialog } from '../components/SalesOrderConfirmationDialog';
import { SalesOrderPackingSlipPostingDialog } from '../components/SalesOrderPackingSlipPostingDialog';
import { SalesOrderLinesProvider, SalesOrderDocumentActions } from '../lines/SalesOrderLineState';
import { salesOrderRibbonPinnedStorageKey } from '../details/salesOrderDetailsConfig';
import { useSalesOrderDocument } from '../details/useSalesOrderDocument';
import { buildSalesOrderTotals } from '../details/salesOrderTotals';
import { buildSalesOrderHeaderPanel } from '../details/SalesOrderHeaderPanel';
import { buildSalesOrderLineFields } from '../details/SalesOrderLineFields';
import { buildSalesOrderLineTabs } from '../details/SalesOrderLineTabs';
import { buildSalesOrderHeaderSections } from '../details/SalesOrderHeaderSections';

export function SalesOrderDetailsPage(): React.ReactElement {
  const doc = useSalesOrderDocument();
  const { orderQuery, orderLoading, refetchOrder, t, order } = doc;
  if (orderLoading) return <LoadingState />;
  if (orderQuery.isError && !order)
    return <ErrorState message={orderQuery.error.message} onRetry={() => void refetchOrder()} />;
  if (!order) return <ErrorState message={t('messages.noSalesOrders')} />;
  // View builders, in order: each one renders from the document plus earlier parts.
  const s0 = { ...doc, order };
  const s1 = { ...s0, ...buildSalesOrderTotals(s0) };
  const s2 = { ...s1, ...buildSalesOrderHeaderPanel(s1) };
  const s3 = { ...s2, ...buildSalesOrderLineFields(s2) };
  const s4 = { ...s3, ...buildSalesOrderLineTabs(s3) };
  const s5 = { ...s4, ...buildSalesOrderHeaderSections(s4) };
  const {
    invalidateOrder,
    navigate,
    activeHeader,
    canEditHeader,
    salesOrderRibbonGroups,
    sellRibbonGroups,
    startHeaderEdit,
    saveHeader,
    saveNewDeliveryAddress,
    setHeaderDraft,
    savingHeader,
    headerError,
    setHeaderError,
    setLineFilterVisible,
    tab,
    setTab,
    deliveryAddressDrawerOpen,
    setDeliveryAddressDrawerOpen,
    deliveryAddressInitialData,
    totalsOpen,
    setTotalsOpen,
    confirmationsOpen,
    setConfirmationsOpen,
    packingSlipPostingOpen,
    setPackingSlipPostingOpen,
    confirmationMode,
    confirmNowError,
    setConfirmNowError,
    copyMode,
    setCopyMode,
    setCopySourceId,
    copyBusy,
    setCopyBusy,
    copyError,
    setCopyError,
    currentLanguage,
    orders,
    copySourcesQuery,
    copyLinesQuery,
    totalsSections,
    readOnlyHeaderFields,
    sections,
  } = s5;
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
              void refetchOrder();
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
          presentation: {
            mode: 'list',
            listWidth: 280,
            listResizable: true,
            listInitiallyVisible: false,
            listVisibilityStorageKey: 'ixapp.sales-order.list-visible',
            detailEndPadding: 8,
          },
          actionPaneAfterListContent: (
            <>
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
                  if (savingHeader) return;
                  setHeaderDraft(null);
                  setHeaderError('');
                }}
              />
              <ActionPaneGroup>
                <ActionPaneRibbonTrigger
                  id="sales-order"
                  label={t('salesOrder.ui.salesOrder', 'Sales order')}
                  groups={salesOrderRibbonGroups}
                  disabled={Boolean(activeHeader)}
                  persistenceKey={salesOrderRibbonPinnedStorageKey}
                />
                <ActionPaneRibbonTrigger
                  id="sell"
                  label={t('salesOrder.ui.sell', 'Sell')}
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
      <Snackbar
        open={Boolean(confirmNowError)}
        onClose={() => setConfirmNowError('')}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="error" onClose={() => setConfirmNowError('')}>
          {confirmNowError}
        </Alert>
      </Snackbar>
      <SalesOrderConfirmationDialog
        open={confirmationsOpen}
        mode={confirmationMode}
        orderId={order.id}
        salesId={order.salesId}
        customerName={order.customerName}
        canPost={Boolean(canEditHeader) && !activeHeader}
        onClose={() => setConfirmationsOpen(false)}
        onPosted={async () => {
          await Promise.all([invalidateOrder(order.id)]);
        }}
      />
      <SalesOrderPackingSlipPostingDialog
        open={packingSlipPostingOpen}
        orderId={order.id}
        salesId={order.salesId}
        canPost={Boolean(canEditHeader) && !activeHeader}
        onClose={() => setPackingSlipPostingOpen(false)}
        onPosted={async () => {
          await Promise.all([invalidateOrder(order.id)]);
        }}
      />
      <DocumentCopyDrawer
        open={Boolean(copyMode)}
        title={copyMode === 'fromJournal' ? 'Copy from journal' : 'Copy from all'}
        sourceSectionLabel={
          copyMode === 'fromJournal'
            ? t('salesOrder.ui.confirmationJournals', 'Confirmation journals')
            : t('salesOrder.ui.salesOrders', 'Sales orders')
        }
        additionalSections={
          copyMode === 'fromJournal'
            ? ['Quotations', 'Packing slips', 'Invoices', 'Project invoices']
            : ['Quotations', 'Confirmation']
        }
        sources={copySourcesQuery.data ?? []}
        lines={copyLinesQuery.data ?? []}
        loadingSources={copySourcesQuery.isLoading}
        loadingLines={copyLinesQuery.isLoading}
        busy={copyBusy}
        error={
          copyError ||
          (copySourcesQuery.error instanceof Error ? copySourcesQuery.error.message : '') ||
          (copyLinesQuery.error instanceof Error ? copyLinesQuery.error.message : '')
        }
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
            await Promise.all([invalidateOrder(order.id)]);
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
        onSave={(address) => {
          void saveNewDeliveryAddress(address);
        }}
        initialData={deliveryAddressInitialData}
      />
    </SalesOrderLinesProvider>
  );
}
