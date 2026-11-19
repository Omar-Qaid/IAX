import React, { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import { ListInfoValue } from '@patterns/simple-list/ListInfoValue';
import { StatusBadge } from '@shared/components/status/StatusBadge';
import { FOUNDATION_RECORD_PREVIEWS } from '../../../foundation/previews/foundationRecordPreviews';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { salesOrderListApi, type SalesOrderListRecord } from '../api/salesOrderListApi';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { SalesOrderQuickCreate } from '../components/SalesOrderQuickCreate';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../../routes/accountsReceivableRoutePaths';
import { ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS } from '../../previews/accountsReceivableRecordPreviews';
import { salesOrderQueryKeys } from '../../queries/accountsReceivableQueryKeys';

export function SalesOrderListPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const navigate = useNavigate();
  const company = useCompanyStore((state) => state.currentCompany);
  const [quickCreateOpen, setQuickCreateOpen] = useState(false);
  const orderQuery = useQuery({
    queryKey: salesOrderQueryKeys.list(company),
    queryFn: ({ signal }) => salesOrderListApi.list(signal),
  });
  const [searchParams] = useSearchParams();
  const customerAccount = searchParams.get('customerAccount');
  const orders = (orderQuery.data ?? []).filter(
    (order) => !customerAccount || order.customerAccount === customerAccount
  );
  const columns = useMemo<ColumnDef<SalesOrderListRecord>[]>(
    () => [
      {
        field: 'salesId',
        headerName: 'fields.salesOrderNumber',
        width: 145,
        pinned: 'left',
        getDetailHref: (row) => ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrder(row.id),
      },
      {
        field: 'customerAccount',
        headerName: 'fields.customerAccount',
        width: 150,
        preview: { entity: ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.CUSTOMER },
      },
      { field: 'customerName', headerName: 'fields.customerName', minWidth: 220, flex: 1 },
      {
        field: 'invoiceAccount',
        headerName: 'fields.invoiceAccount',
        width: 150,
        preview: { entity: ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.CUSTOMER },
      },
      {
        field: 'customerGroup',
        headerName: 'fields.customerGroup',
        width: 145,
        preview: { entity: ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.CUSTOMER_GROUP },
      },
      {
        field: 'currencyCode',
        headerName: 'fields.currency',
        width: 95,
        preview: { entity: FOUNDATION_RECORD_PREVIEWS.CURRENCY },
      },
      { field: 'deliveryDate', headerName: 'fields.requestedDelivery', width: 135, type: 'date' },
      {
        field: 'orderTotal',
        headerName: 'fields.orderTotal',
        width: 125,
        type: 'number',
        align: 'right',
      },
      {
        field: 'salesStatus',
        headerName: 'common.status',
        width: 115,
        renderCell: ({ row }) => <StatusBadge status={row.salesStatus} />,
      },
    ],
    []
  );

  const commandIds = ['sell', 'invoice', 'collect', 'general', 'options'] as const;
  // Only what is specific to sales orders; labels, utilities, Back, filter texts
  // and locale are the SimpleList defaults.
  const config: EnterpriseListConfig<SalesOrderListRecord> = {
    showSearchCommand: true,
    recordTableName: 'SalesTable',
    getAuditRecordId: (order) => order.recId,
    contextLabel: customerAccount
      ? `${t('pages.salesOrders.title')} | ${customerAccount}`
      : undefined,
    searchFields: [
      { field: 'salesId', label: t('fields.salesOrderNumber') },
      { field: 'customerAccount', label: t('fields.customerAccount') },
      { field: 'customerName', label: t('fields.customerName') },
    ],
    crud: {
      onEdit: (order) => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrder(order.id)),
      onNew: () => setQuickCreateOpen(true),
    },
    commands: commandIds.map((id) => ({
      id,
      label: t(`customerCommands.${id}`),
      disabled: true,
    })),
    advancedFilter: {
      fieldLabel: t('fields.salesOrderNumber'),
      operatorLabel: t('filters.contains'),
      fields: [
        { field: 'salesId', label: t('fields.salesOrderNumber') },
        { field: 'customerAccount', label: t('fields.customerAccount') },
        { field: 'customerName', label: t('fields.customerName') },
        { field: 'customerGroup', label: t('fields.customerGroup') },
        { field: 'currencyCode', label: t('fields.currency') },
        { field: 'salesStatus', label: t('common.status') },
      ],
      getValue: (order) => order.salesId,
    },
    relatedInformation: {
      sections: (order) => [
        {
          id: 'customer',
          label: t('fields.customer'),
          defaultExpanded: true,
          content: (
            <ListInfoValue
              value={order ? `${order.customerAccount}\n${order.customerName}` : ''}
              empty={t('relatedInformation.selectCustomer')}
            />
          ),
        },
        {
          id: 'delivery',
          label: t('fields.delivery', 'Delivery'),
          content: (
            <ListInfoValue
              value={order ? `${order.deliveryDate}\n${order.deliveryMode}` : ''}
              empty={t('relatedInformation.selectCustomer')}
            />
          ),
        },
        {
          id: 'payment',
          label: t('fields.payment', 'Payment'),
          content: (
            <ListInfoValue
              value={order ? `${order.paymentTerms}\n${order.currencyCode}` : ''}
              empty={t('relatedInformation.selectCustomer')}
            />
          ),
        },
        {
          id: 'status',
          label: t('common.status'),
          content: (
            <ListInfoValue
              value={order?.documentStatus}
              empty={t('relatedInformation.selectCustomer')}
            />
          ),
        },
      ],
    },
  };

  return (
    <SimpleListPage
      title={t('pages.salesOrders.title')}
      enterpriseConfig={config}
      dataSource={{ type: 'controlled', rows: orders }}
      columns={columns}
      loading={orderQuery.isLoading}
      error={orderQuery.error instanceof Error ? orderQuery.error.message : null}
      onRetry={() => orderQuery.refetch()}
      dataGridProps={{
        storageKey: 'accounts-receivable.sales-orders.reference-view',
        selectionMode: 'multiple',
      }}
      dialogs={
        <SalesOrderQuickCreate
          open={quickCreateOpen}
          onClose={() => setQuickCreateOpen(false)}
          onSave={async (_order) => {
            setQuickCreateOpen(false);
            await orderQuery.refetch();
          }}
        />
      }
    />
  );
}
