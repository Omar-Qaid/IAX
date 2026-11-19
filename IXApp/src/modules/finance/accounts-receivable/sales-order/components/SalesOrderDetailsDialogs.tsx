import React from 'react';
import { Alert, Snackbar } from '@mui/material';
import { LogisticsPostalAddressDrawer } from '@shared/components/logistics/LogisticsPostalAddressDrawer';
import { DocumentTotalsDrawer } from '@patterns/document/DocumentTotalsDrawer';
import { DocumentCopyDrawer } from '@patterns/document/DocumentCopyDrawer';
import { salesOrderCopyApi } from '../api/salesOrderCopyApi';
import { SalesOrderConfirmationDialog } from './SalesOrderConfirmationDialog';
import { SalesOrderPackingSlipPostingDialog } from './SalesOrderPackingSlipPostingDialog';
import type { SalesOrderDocumentView } from '../details/useSalesOrderDocument';
import type { buildSalesOrderTotals } from '../details/salesOrderTotals';

type DialogScope = SalesOrderDocumentView & ReturnType<typeof buildSalesOrderTotals>;

/** Dialog workflows shared by the original and master-details sales-order pages. */
export function SalesOrderDetailsDialogs({ scope }: { scope: DialogScope }): React.ReactElement {
  const {
    order,
    t,
    invalidateOrder,
    canEditHeader,
    activeHeader,
    saveNewDeliveryAddress,
    deliveryAddressDrawerOpen,
    setDeliveryAddressDrawerOpen,
    deliveryAddressInitialData,
    totalsOpen,
    setTotalsOpen,
    totalsSections,
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
    copySourcesQuery,
    copyLinesQuery,
  } = scope;
  return (
    <>
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
    </>
  );
}
