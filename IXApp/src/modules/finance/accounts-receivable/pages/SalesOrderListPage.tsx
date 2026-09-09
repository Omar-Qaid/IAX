import React, { useMemo, useState } from 'react';
import { Typography } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import { StatusBadge } from '@shared/components/status/StatusBadge';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { salesOrderListApi, type SalesOrderListRecord } from '../api/salesOrderListApi';
import { useNavigate } from 'react-router-dom';
import { SalesOrderQuickCreate } from '../components/SalesOrderQuickCreate';

export function SalesOrderListPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [quickCreateOpen, setQuickCreateOpen] = useState(false);
  const orderQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-orders'],
    queryFn: ({ signal }) => salesOrderListApi.list(signal),
  });
  const orders = orderQuery.data ?? [];
  const columns = useMemo<ColumnDef<SalesOrderListRecord>[]>(() => [
    { field: 'salesId', headerName: 'fields.salesOrderNumber', width: 145, pinned: 'left' },
    { field: 'customerAccount', headerName: 'fields.customerAccount', width: 150 },
    { field: 'customerName', headerName: 'fields.customerName', minWidth: 220, flex: 1 },
    { field: 'invoiceAccount', headerName: 'fields.invoiceAccount', width: 150 },
    { field: 'customerGroup', headerName: 'fields.customerGroup', width: 145 },
    { field: 'currencyCode', headerName: 'fields.currency', width: 95 },
    { field: 'deliveryDate', headerName: 'fields.requestedDelivery', width: 135, type: 'date' },
    { field: 'orderTotal', headerName: 'fields.orderTotal', width: 125, type: 'number', align: 'right' },
    { field: 'salesStatus', headerName: 'common.status', width: 115, renderCell: ({ row }) => <StatusBadge status={row.salesStatus.toLocaleLowerCase()} /> },
  ], []);

  const commandIds = ['sell', 'invoice', 'collect', 'general', 'options'] as const;
  const config: EnterpriseListConfig<SalesOrderListRecord> = {
    backCommand: { label: t('actions.back'), onClick: () => navigate(-1) },
    showSearchCommand: true,
    recordTableName: 'SalesTable',
    getAuditRecordId: (order) => order.recId,
    contextLabel: t('pages.salesOrders.title'),
    viewLabel: t('pages.customers.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchFields: [
      { field: 'salesId', label: t('fields.salesOrderNumber') },
      { field: 'customerAccount', label: t('fields.customerAccount') },
      { field: 'customerName', label: t('fields.customerName') },
    ],
    locale: currentLanguage.code,
    crud: {
      editLabel: t('actions.edit'),
      newLabel: t('actions.new'),
      deleteLabel: t('actions.delete'),
      onNew: () => setQuickCreateOpen(true),
    },
    commands: commandIds.map((id) => ({
      id,
      label: t(`customerCommands.${id}`),
      disabled: true,
    })),
    utilities: {
      personalizeLabel: t('utilities.personalize'), guideLabel: t('utilities.guide'), notificationsLabel: t('common.notifications'),
      refreshLabel: t('actions.refresh'), openWindowLabel: t('utilities.openWindow'), notificationCount: 0,
    },
    advancedFilter: {
      title: t('filters.title'), addLabel: t('actions.add'), fieldLabel: t('fields.salesOrderNumber'), operatorLabel: t('filters.contains'),
      applyLabel: t('actions.apply'), resetLabel: t('actions.reset'),
      fields: [
        { field: 'salesId', label: t('fields.salesOrderNumber') },
        { field: 'customerAccount', label: t('fields.customerAccount') },
        { field: 'customerName', label: t('fields.customerName') },
        { field: 'customerGroup', label: t('fields.customerGroup') },
        { field: 'currencyCode', label: t('fields.currency') },
        { field: 'salesStatus', label: t('common.status') },
      ],
      getValue: (order) => order.salesId,
      matches: (order, value) => order.salesId.toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
    relatedInformation: {
      title: t('relatedInformation.title'),
      sections: (order) => [
        { id: 'customer', label: t('fields.customer'), defaultExpanded: true, content: <InfoValue value={order ? `${order.customerAccount}\n${order.customerName}` : ''} empty={t('relatedInformation.selectCustomer')} /> },
        { id: 'delivery', label: t('fields.delivery', 'Delivery'), content: <InfoValue value={order ? `${order.deliveryDate}\n${order.deliveryMode}` : ''} empty={t('relatedInformation.selectCustomer')} /> },
        { id: 'payment', label: t('fields.payment', 'Payment'), content: <InfoValue value={order ? `${order.paymentTerms}\n${order.currencyCode}` : ''} empty={t('relatedInformation.selectCustomer')} /> },
        { id: 'status', label: t('common.status'), content: <InfoValue value={order?.documentStatus} empty={t('relatedInformation.selectCustomer')} /> },
      ],
    },
  };

  return <SimpleListPage
    variant="enterprise"
    title={t('pages.salesOrders.title')}
    enterpriseConfig={config}
    dataSource={{ type: 'controlled', rows: orders }}
    columns={columns}
    loading={orderQuery.isLoading}
    error={orderQuery.error instanceof Error ? orderQuery.error.message : null}
    onRetry={() => orderQuery.refetch()}
    dataGridProps={{ storageKey: 'accounts-receivable.sales-orders.reference-view' }}
    dialogs={
      <SalesOrderQuickCreate
        open={quickCreateOpen}
        onClose={() => setQuickCreateOpen(false)}
        onSave={(order) => {
          queryClient.setQueryData<SalesOrderListRecord[]>(
            ['accounts-receivable', 'sales-orders'],
            (current = []) => [order, ...current.filter((existing) => existing.recId !== order.recId)],
          );
          setQuickCreateOpen(false);
          void queryClient.invalidateQueries({ queryKey: ['accounts-receivable', 'sales-orders'] });
        }}
      />
    }
  />;
}

function InfoValue({ value, empty }: { value?: string | null; empty: string }): React.ReactElement {
  return <Typography sx={{ fontSize: '0.75rem', color: value ? 'text.primary' : 'text.secondary', whiteSpace: 'pre-line' }}>{value || empty}</Typography>;
}
