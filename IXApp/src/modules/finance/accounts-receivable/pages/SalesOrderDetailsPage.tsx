import React, { useRef, useState } from 'react';
import { Alert, Box, Stack, Tab, Tabs, TextField, Typography } from '@mui/material';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { useNavigate, useParams } from 'react-router-dom';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailFieldConfig, DetailSectionConfig } from '@patterns/list-details/types';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { salesOrderLinesApi, type SalesOrderLineRecord } from '../api/salesOrderLinesApi';
import { PERMISSIONS } from '@core/permissions/permissions';
import { usePermission } from '@core/permissions/usePermission';
import { useQuery } from '@tanstack/react-query';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { salesOrderListApi, type SalesOrderHeaderInput } from '../api/salesOrderListApi';
import { useAppTranslation } from '@core/localization/useAppTranslation';

import { SalesOrderLinesGrid } from './SalesOrderLinesGrid';
import { SalesOrderLinesProvider } from './SalesOrderLineState';

type DetailLine = SalesOrderLineRecord;

export function SalesOrderDetailsPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
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
  const lines = linesQuery.data ?? [];
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
        <SalesOrderLinesGrid
          key={order.id}
          order={order}
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
    </SalesOrderLinesProvider>
  );
}
