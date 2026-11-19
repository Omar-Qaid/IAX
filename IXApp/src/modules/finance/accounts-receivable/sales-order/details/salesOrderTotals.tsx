import React from 'react';
import { MasterDetailTotals } from '@patterns/master-detail';
import { type DocumentTotalsSection } from '@patterns/document/DocumentTotalsDrawer';
import type { SalesOrderDocumentView } from './useSalesOrderDocument';

type Scope = SalesOrderDocumentView;

export function buildSalesOrderTotals(scope: Scope) {
  const { t, currentLanguage, order, lines, totalsQuery } = scope;
  const amount = (value: number | null | undefined) =>
    `${(typeof value === 'number' && Number.isFinite(value) ? value : 0).toLocaleString(currentLanguage.code)} ${order.currencyCode}`;
  const number = (value: number | null | undefined, digits = 2) =>
    (typeof value === 'number' && Number.isFinite(value) ? value : 0).toLocaleString(
      currentLanguage.code,
      {
        minimumFractionDigits: digits,
        maximumFractionDigits: digits,
      }
    );
  const calculatedLineDiscount = lines.reduce((sum, line) => sum + (line.lineDiscount || 0), 0);
  const calculatedTotalDiscount = lines.reduce(
    (sum, line) => sum + (line.lineDiscount || 0) + (line.multiLineDiscount || 0),
    0
  );
  const calculatedSubtotal =
    lines.reduce((sum, line) => sum + (line.lineTotal || 0), 0) - calculatedTotalDiscount;
  const calculatedCostValue = lines.reduce(
    (sum, line) => sum + (line.costPrice || 0) * (line.quantity || 0),
    0
  );
  const subtotal = totalsQuery.data?.subtotal ?? calculatedSubtotal;
  const lineDiscount = totalsQuery.data?.lineDiscount ?? calculatedLineDiscount;
  const orderDiscount = totalsQuery.data?.orderDiscount ?? 0;
  const totalDiscount = totalsQuery.data?.totalDiscount ?? calculatedTotalDiscount;
  const salesTax = totalsQuery.data?.salesTax ?? 0;
  const totalCharges = totalsQuery.data?.totalCharges ?? 0;
  const costValue = totalsQuery.data?.costValue ?? calculatedCostValue;
  const invoiceAmount = totalsQuery.data?.invoiceAmount ?? subtotal + totalCharges + salesTax;
  const margin = subtotal - costValue;
  const quantity =
    totalsQuery.data?.quantity ?? lines.reduce((sum, line) => sum + (line.quantity || 0), 0);
  const totalsSections: DocumentTotalsSection[] = [
    {
      id: 'sales-order-totals',
      title: 'Sales order totals',
      columns: [
        {
          id: 'totals',
          title: 'Totals',
          fields: [
            {
              id: 'currency',
              label: t('salesOrder.ui.currency', 'Currency'),
              value: order.currencyCode,
            },
            {
              id: 'exchange-rate',
              label: t('salesOrder.ui.exchangeRate', 'Exchange rate'),
              value: number(1, 4),
            },
            {
              id: 'line-discount',
              label: t('salesOrder.ui.lineDiscount', 'Line discount'),
              value: number(lineDiscount),
            },
            {
              id: 'order-discount',
              label: t('salesOrder.ui.orderDiscount', 'Order discount'),
              value: number(orderDiscount),
            },
            {
              id: 'subtotal',
              label: t('salesOrder.ui.subtotalAmount', 'Subtotal amount'),
              value: number(subtotal),
              emphasized: true,
            },
            {
              id: 'total-discount',
              label: t('salesOrder.ui.totalDiscount', 'Total discount'),
              value: number(totalDiscount),
            },
            {
              id: 'cash-discount',
              label: t('salesOrder.ui.cashDiscount', 'Cash discount'),
              value: number(0),
            },
            {
              id: 'total-charges',
              label: t('salesOrder.ui.totalCharges', 'Total charges'),
              value: number(totalCharges),
            },
            {
              id: 'sales-tax',
              label: t('salesOrder.ui.salesTax', 'Sales tax'),
              value: number(salesTax),
            },
            { id: 'round-off', label: t('salesOrder.ui.roundOff', 'Round-off'), value: number(0) },
            {
              id: 'coupon',
              label: t('salesOrder.ui.totalCouponAmount', 'Total coupon amount'),
              value: number(0),
            },
            {
              id: 'invoice-amount',
              label: t('salesOrder.ui.invoiceAmount', 'Invoice amount'),
              value: number(invoiceAmount),
              emphasized: true,
            },
          ],
        },
        {
          id: 'analysis',
          fields: [
            {
              id: 'credit-limit',
              label: t('salesOrder.ui.creditLimit', 'Credit limit'),
              value: '-',
            },
            {
              id: 'credit-available',
              label: t('salesOrder.ui.creditAvailable', 'Credit available'),
              value: '-',
            },
            {
              id: 'cost-value',
              label: t(
                'salesOrder.ui.costValueInAccountingCurrency',
                'Cost value in accounting currency'
              ),
              value: number(costValue),
            },
            {
              id: 'margin',
              label: t('salesOrder.ui.marginInAccountingCurrency', 'Margin in accounting currency'),
              value: number(margin),
            },
            {
              id: 'contribution-ratio',
              label: t('salesOrder.ui.contributionRatio', 'Contribution ratio'),
              value: `${number(subtotal ? (margin / subtotal) * 100 : 0)}%`,
            },
            {
              id: 'quantity',
              label: t('salesOrder.ui.quantity', 'Quantity'),
              value: number(quantity),
            },
            { id: 'weight', label: t('salesOrder.ui.weight', 'Weight'), value: number(0) },
            { id: 'volume', label: t('salesOrder.ui.volume', 'Volume'), value: number(0) },
          ],
        },
      ],
    },
  ];
  const totalsContent = (
    <MasterDetailTotals
      items={[
        { id: 'subtotal', label: t('fields.subtotal'), value: amount(subtotal) },
        { id: 'discount', label: t('fields.discount'), value: amount(totalDiscount) },
        { id: 'tax', label: t('fields.tax'), value: amount(salesTax) },
        { id: 'total', label: t('fields.total'), value: amount(invoiceAmount), emphasis: true },
      ]}
    />
  );
  return {
    totalsContent,
    amount,
    number,
    calculatedLineDiscount,
    calculatedTotalDiscount,
    calculatedSubtotal,
    calculatedCostValue,
    subtotal,
    lineDiscount,
    orderDiscount,
    totalDiscount,
    salesTax,
    totalCharges,
    costValue,
    invoiceAmount,
    margin,
    quantity,
    totalsSections,
  };
}
