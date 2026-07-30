import { MOCK_SALES_ORDERS, type SalesOrder } from '@mocks/data/salesOrders';

let mockState = [...MOCK_SALES_ORDERS];

export const salesOrderService = {
  async getSalesOrders(): Promise<SalesOrder[]> {
    await new Promise((r) => setTimeout(r, 200));
    return [...mockState];
  },

  async getSalesOrder(id: string): Promise<SalesOrder | null> {
    await new Promise((r) => setTimeout(r, 150));
    return mockState.find((so) => so.id === id) || null;
  },

  async confirmSalesOrder(id: string): Promise<SalesOrder> {
    await new Promise((r) => setTimeout(r, 250));
    const order = mockState.find((so) => so.id === id);
    if (!order) throw new Error('Sales order not found');
    order.status = 'confirmed';
    return { ...order };
  },

  async postInvoice(id: string): Promise<SalesOrder> {
    await new Promise((r) => setTimeout(r, 300));
    const order = mockState.find((so) => so.id === id);
    if (!order) throw new Error('Sales order not found');
    order.status = 'invoiced';
    return { ...order };
  },

  async cancelSalesOrder(id: string): Promise<SalesOrder> {
    await new Promise((r) => setTimeout(r, 200));
    const order = mockState.find((so) => so.id === id);
    if (!order) throw new Error('Sales order not found');
    order.status = 'cancelled';
    return { ...order };
  },
};
