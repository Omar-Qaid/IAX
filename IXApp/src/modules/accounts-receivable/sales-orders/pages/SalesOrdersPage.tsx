import React, { useState, useEffect, useMemo } from 'react';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { DataGridColumnFactory } from '@shared/components/data-grid/DataGridColumnFactory';
import { salesOrderService } from '../services/salesOrderService';
import { useNotifications } from '@shared/hooks/useNotifications';
import { useNavigate } from 'react-router-dom';
import AddIcon from '@mui/icons-material/Add';
import RefreshIcon from '@mui/icons-material/Refresh';
import VisibilityIcon from '@mui/icons-material/Visibility';
import type { SalesOrder } from '@mocks/data/salesOrders';

export const SalesOrdersPage: React.FC = () => {
  const [orders, setOrders] = useState<SalesOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const { notifyError } = useNotifications();
  const navigate = useNavigate();

  const loadData = async () => {
    try {
      setLoading(true);
      const data = await salesOrderService.getSalesOrders();
      setOrders(data);
    } catch {
      notifyError('Failed to load sales orders');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const columns = useMemo(
    () => [
      DataGridColumnFactory.createTextColumn('salesOrderNumber', 'Sales Order', { width: 130 }),
      DataGridColumnFactory.createTextColumn('customerAccount', 'Customer Account', { width: 140 }),
      DataGridColumnFactory.createTextColumn('customerName', 'Customer Name', { flex: 1, minWidth: 200 }),
      DataGridColumnFactory.createDateColumn('orderDate', 'Order Date', { width: 120 }),
      DataGridColumnFactory.createTextColumn('currency', 'Currency', { width: 90 }),
      DataGridColumnFactory.createCurrencyColumn('orderTotal', 'Total Amount', 'USD', { width: 140 }),
      DataGridColumnFactory.createStatusColumn('status', 'Status', { width: 110 }),
    ],
    []
  );

  return (
    <SimpleListPage
      title="Sales Orders"
      subtitle="Accounts Receivable Transactional Orders List"
      loading={loading}
      actionPane={
        <>
          <ActionPaneGroup label="Maintain">
            <ActionPaneButton
              label="New Order"
              icon={<AddIcon fontSize="small" />}
              onClick={() => navigate('/accounts-receivable/sales-orders/so-101')}
            />
            <ActionPaneButton
              label="Open Document"
              icon={<VisibilityIcon fontSize="small" />}
              disabled={!selectedId}
              onClick={() => navigate(`/accounts-receivable/sales-orders/${selectedId}`)}
            />
          </ActionPaneGroup>
          <ActionPaneGroup label="Page">
            <ActionPaneButton label="Refresh" icon={<RefreshIcon fontSize="small" />} onClick={loadData} />
          </ActionPaneGroup>
        </>
      }
      dataGridProps={{
        rows: orders,
        columns,
        loading,
        pageSize: 15,
        onRowDoubleClick: (params) => navigate(`/accounts-receivable/sales-orders/${params.id}`),
        onSelectionChange: (ids) => {
          if (ids.length > 0) setSelectedId(ids[0]!);
        },
      }}
    />
  );
};
