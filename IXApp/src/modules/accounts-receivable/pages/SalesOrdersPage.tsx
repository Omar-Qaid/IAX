import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { StatusBadge } from '@shared/components/status/StatusBadge';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { MOCK_SALES_ORDERS, type SalesOrder } from '@mocks/data/salesOrders';

export function SalesOrdersPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const navigate = useNavigate();
  const columns = useMemo<ColumnDef<SalesOrder>[]>(() => [
    { field: 'salesOrderNumber', headerName: 'fields.salesOrderNumber', width: 145 },
    { field: 'customerAccount', headerName: 'fields.customerAccount', width: 150 },
    { field: 'customerName', headerName: 'fields.customerName', minWidth: 230, flex: 1 },
    { field: 'orderDate', headerName: 'fields.orderDate', width: 130, type: 'date' },
    { field: 'orderTotal', headerName: 'fields.orderTotal', width: 130, type: 'number', align: 'right' },
    { field: 'status', headerName: 'common.status', width: 115, renderCell: ({ row }) => <StatusBadge status={row.status} /> },
  ], []);

  const openOrder = (order: SalesOrder) => navigate(ROUTE_PATHS.ACCOUNTS_RECEIVABLE.salesOrder(order.id));
  return <SimpleListPage title={t('pages.salesOrders.title')} subtitle={t('pages.salesOrders.subtitle')} dataSource={{ type: 'static', rows: MOCK_SALES_ORDERS }} columns={columns} dataGridProps={{ storageKey: 'accounts-receivable.sales-orders', hideAddRowButton: true, onRowDoubleClick: openOrder }} />;
}
