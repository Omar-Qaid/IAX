import React, { useState, useEffect } from 'react';
import { DocumentPage } from '@patterns/document/DocumentPage';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { FastTabs } from '@shared/components/fast-tabs/FastTabs';
import { FastTab } from '@shared/components/fast-tabs/FastTab';
import { FormRow, FormColumn } from '@shared/components/forms/FormRow';
import { AppDisplayField } from '@shared/components/fields/AppDisplayField';
import { AppDataGrid } from '@shared/components/data-grid/AppDataGrid';
import { DataGridColumnFactory } from '@shared/components/data-grid/DataGridColumnFactory';
import { salesOrderService } from '../services/salesOrderService';
import { useNotifications } from '@shared/hooks/useNotifications';
import { useParams, useNavigate } from 'react-router-dom';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong';
import CancelOutlinedIcon from '@mui/icons-material/CancelOutlined';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { Typography, Stack, Box, Divider } from '@mui/material';
import type { SalesOrder } from '@mocks/data/salesOrders';

export const SalesOrderPage: React.FC = () => {
  const { salesOrderId } = useParams<{ salesOrderId: string }>();
  const [order, setOrder] = useState<SalesOrder | null>(null);
  const [loading, setLoading] = useState(true);
  const { notifySuccess, notifyError } = useNotifications();
  const navigate = useNavigate();

  const loadOrder = async () => {
    try {
      setLoading(true);
      const targetId = salesOrderId || 'so-101';
      const data = await salesOrderService.getSalesOrder(targetId);
      setOrder(data);
    } catch {
      notifyError('Failed to load sales order document details');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadOrder();
  }, [salesOrderId]);

  const handleConfirm = async () => {
    if (!order) return;
    try {
      const updated = await salesOrderService.confirmSalesOrder(order.id);
      setOrder(updated);
      notifySuccess(`Sales order ${order.salesOrderNumber} has been confirmed`);
    } catch {
      notifyError('Failed to confirm order');
    }
  };

  const handlePostInvoice = async () => {
    if (!order) return;
    try {
      const updated = await salesOrderService.postInvoice(order.id);
      setOrder(updated);
      notifySuccess(`Invoice posted for sales order ${order.salesOrderNumber}`);
    } catch {
      notifyError('Failed to post invoice');
    }
  };

  const handleCancel = async () => {
    if (!order) return;
    try {
      const updated = await salesOrderService.cancelSalesOrder(order.id);
      setOrder(updated);
      notifySuccess(`Sales order ${order.salesOrderNumber} cancelled`);
    } catch {
      notifyError('Failed to cancel order');
    }
  };

  const lineColumns = [
    DataGridColumnFactory.createNumberColumn('lineNumber', '#', { width: 60 }),
    DataGridColumnFactory.createTextColumn('itemNumber', 'Item Number', { width: 130 }),
    DataGridColumnFactory.createTextColumn('description', 'Item Description', { flex: 1, minWidth: 200 }),
    DataGridColumnFactory.createNumberColumn('quantity', 'Qty', { width: 80 }),
    DataGridColumnFactory.createTextColumn('unit', 'Unit', { width: 80 }),
    DataGridColumnFactory.createCurrencyColumn('unitPrice', 'Unit Price', 'USD', { width: 120 }),
    DataGridColumnFactory.createCurrencyColumn('discount', 'Discount', 'USD', { width: 100 }),
    DataGridColumnFactory.createCurrencyColumn('netAmount', 'Net Amount', 'USD', { width: 120 }),
    DataGridColumnFactory.createCurrencyColumn('taxAmount', 'Tax', 'USD', { width: 100 }),
    DataGridColumnFactory.createCurrencyColumn('lineTotal', 'Line Total', 'USD', { width: 130 }),
  ];

  if (!order && !loading) {
    return <Typography color="error">Sales order not found</Typography>;
  }

  return (
    <DocumentPage
      title={`Sales Order ${order?.salesOrderNumber || ''}`}
      subtitle={`Customer: ${order?.customerName || ''}`}
      statusBadge={order?.status.toUpperCase() || 'OPEN'}
      actionPane={
        <>
          <ActionPaneGroup label="Navigation">
            <ActionPaneButton
              label="Back to List"
              icon={<ArrowBackIcon fontSize="small" />}
              onClick={() => navigate('/accounts-receivable/sales-orders')}
            />
          </ActionPaneGroup>
          <ActionPaneGroup label="Process">
            <ActionPaneButton
              label="Confirm Order"
              icon={<CheckCircleIcon fontSize="small" />}
              disabled={order?.status === 'confirmed' || order?.status === 'invoiced' || order?.status === 'cancelled'}
              onClick={handleConfirm}
            />
            <ActionPaneButton
              label="Post Invoice"
              icon={<ReceiptLongIcon fontSize="small" />}
              disabled={order?.status !== 'confirmed'}
              onClick={handlePostInvoice}
            />
            <ActionPaneButton
              label="Cancel Order"
              icon={<CancelOutlinedIcon fontSize="small" />}
              disabled={order?.status === 'cancelled' || order?.status === 'invoiced'}
              onClick={handleCancel}
            />
          </ActionPaneGroup>
        </>
      }
      headerContent={
        <FastTabs>
          <FastTab id="header-general" title="Order Header Details" summary={`${order?.customerName} (${order?.currency})`}>
            <FormRow>
              <FormColumn md={3}>
                <AppDisplayField label="Order Number" value={order?.salesOrderNumber} />
              </FormColumn>
              <FormColumn md={3}>
                <AppDisplayField label="Customer Account" value={order?.customerAccount} />
              </FormColumn>
              <FormColumn md={3}>
                <AppDisplayField label="Order Date" value={order?.orderDate} />
              </FormColumn>
              <FormColumn md={3}>
                <AppDisplayField label="Delivery Date" value={order?.requestedDeliveryDate} />
              </FormColumn>
              <FormColumn md={3}>
                <AppDisplayField label="Currency Code" value={order?.currency} />
              </FormColumn>
              <FormColumn md={3}>
                <AppDisplayField label="Payment Terms" value={order?.paymentTerms} />
              </FormColumn>
              <FormColumn md={3}>
                <AppDisplayField label="Delivery Mode" value={order?.deliveryMode} />
              </FormColumn>
              <FormColumn md={3}>
                <AppDisplayField label="Customer Reference" value={order?.customerReference} />
              </FormColumn>
            </FormRow>
          </FastTab>
        </FastTabs>
      }
      linesContent={
        <Box>
          <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1 }}>
            Sales Order Items ({order?.lines.length || 0} lines)
          </Typography>
          <AppDataGrid
            rows={order?.lines || []}
            columns={lineColumns}
            loading={loading}
            pageSize={10}
            height={250}
            checkboxSelection={false}
          />
        </Box>
      }
      totalsContent={
        <Stack spacing={1}>
          <Typography variant="subtitle2" color="primary.main" sx={{ fontWeight: 700 }}>
            Order Financial Summary
          </Typography>
          <Divider />
          <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
            <Typography variant="body2" color="text.secondary">Subtotal:</Typography>
            <Typography variant="body2" sx={{ fontWeight: 600 }}>${order?.subtotal.toLocaleString()}</Typography>
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
            <Typography variant="body2" color="text.secondary">Discount Total:</Typography>
            <Typography variant="body2" color="error.main" sx={{ fontWeight: 600 }}>-${order?.discountTotal.toLocaleString()}</Typography>
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
            <Typography variant="body2" color="text.secondary">Sales Tax Total:</Typography>
            <Typography variant="body2" sx={{ fontWeight: 600 }}>${order?.taxTotal.toLocaleString()}</Typography>
          </Box>
          <Divider />
          <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
            <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>Total Amount:</Typography>
            <Typography variant="subtitle1" color="primary.main" sx={{ fontWeight: 700 }}>
              ${order?.orderTotal.toLocaleString()} {order?.currency}
            </Typography>
          </Box>
        </Stack>
      }
    />
  );
};
