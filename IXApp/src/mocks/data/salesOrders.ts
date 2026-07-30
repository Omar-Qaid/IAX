export interface SalesOrderLine {
  id: string;
  lineNumber: number;
  itemNumber: string;
  description: string;
  quantity: number;
  unit: string;
  unitPrice: number;
  discount: number;
  netAmount: number;
  taxAmount: number;
  lineTotal: number;
}

export interface SalesOrder {
  id: string;
  salesOrderNumber: string;
  customerAccount: string;
  customerName: string;
  orderDate: string;
  requestedDeliveryDate: string;
  currency: string;
  paymentTerms: string;
  deliveryMode: string;
  status: 'open' | 'confirmed' | 'invoiced' | 'cancelled';
  customerReference?: string;
  lines: SalesOrderLine[];
  subtotal: number;
  discountTotal: number;
  taxTotal: number;
  orderTotal: number;
}

export const MOCK_SALES_ORDERS: SalesOrder[] = [
  {
    id: 'so-101',
    salesOrderNumber: 'SO-00101',
    customerAccount: 'US-001',
    customerName: 'Contoso Retail Americas',
    orderDate: '2025-07-01',
    requestedDeliveryDate: '2025-07-15',
    currency: 'USD',
    paymentTerms: 'Net 60',
    deliveryMode: 'Air Freight',
    status: 'open',
    customerReference: 'PO-CONT-9921',
    subtotal: 12000,
    discountTotal: 500,
    taxTotal: 1150,
    orderTotal: 12650,
    lines: [
      {
        id: 'sol-1',
        lineNumber: 1,
        itemNumber: 'ITEM-A10',
        description: 'Enterprise Server Rack Cabinet 42U',
        quantity: 2,
        unit: 'Pcs',
        unitPrice: 3500,
        discount: 200,
        netAmount: 6800,
        taxAmount: 680,
        lineTotal: 7480,
      },
      {
        id: 'sol-2',
        lineNumber: 2,
        itemNumber: 'ITEM-B20',
        description: 'High Performance Switch 48-Port',
        quantity: 4,
        unit: 'Pcs',
        unitPrice: 1300,
        discount: 300,
        netAmount: 4900,
        taxAmount: 490,
        lineTotal: 5390,
      },
    ],
  },
  {
    id: 'so-102',
    salesOrderNumber: 'SO-00102',
    customerAccount: 'US-002',
    customerName: 'Fabrikam Supplies Ltd.',
    orderDate: '2025-07-05',
    requestedDeliveryDate: '2025-07-20',
    currency: 'USD',
    paymentTerms: 'Net 30',
    deliveryMode: 'Standard Ground',
    status: 'confirmed',
    customerReference: 'PO-FAB-4410',
    subtotal: 4500,
    discountTotal: 0,
    taxTotal: 450,
    orderTotal: 4950,
    lines: [
      {
        id: 'sol-3',
        lineNumber: 1,
        itemNumber: 'ITEM-C30',
        description: 'Wireless Access Point Enterprise',
        quantity: 15,
        unit: 'Pcs',
        unitPrice: 300,
        discount: 0,
        netAmount: 4500,
        taxAmount: 450,
        lineTotal: 4950,
      },
    ],
  },
];
