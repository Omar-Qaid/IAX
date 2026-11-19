import { useMemo } from 'react';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { ColumnDef } from '@shared/components/data-grid/types';
import type { SalesOrderLineRecord as DetailLine } from '../api/salesOrderLinesApi';
import { INVENTORY_RECORD_PREVIEWS } from '../../../inventory/previews/inventoryRecordPreviews';

export function useSalesOrderLineColumns() {
  const { t } = useAppTranslation();
  return useMemo<ColumnDef<DetailLine>[]>(
    () => [
      {
        field: 'lineType',
        headerName: t('salesOrder.lineType', 'Type'),
        width: 110,
        minWidth: 85,
        type: 'number',
      },
      {
        field: 'itemNumber',
        headerName: t('salesOrder.itemNumber', 'Item number'),
        width: 160,
        minWidth: 120,
      },
      { field: 'description', headerName: t('salesOrder.productName', 'Product name'), width: 220 },
      {
        field: 'quantity',
        headerName: t('fields.quantity', 'Quantity'),
        width: 85,
        type: 'number',
        align: 'right',
        headerAlign: 'right',
      },
      { field: 'unit', headerName: t('salesOrder.unit', 'Unit'), width: 100 },
      {
        field: 'unitPrice',
        headerName: t('fields.unitPrice'),
        width: 110,
        type: 'number',
        align: 'right',
        headerAlign: 'right',
      },
      {
        field: 'lineDiscount',
        headerName: t('salesOrder.lineDiscount', 'Discount'),
        width: 110,
        type: 'number',
        align: 'right',
        headerAlign: 'right',
      },
      {
        field: 'lineDiscountPercent',
        headerName: t('salesOrder.lineDiscountPercent', 'Discount %'),
        width: 110,
        type: 'number',
        align: 'right',
        headerAlign: 'right',
      },
      {
        field: 'deliveryDate',
        headerName: t('fields.requestedDelivery'),
        width: 150,
        type: 'date',
      },
      {
        field: 'netAmount',
        headerName: t('salesOrder.netAmount', 'Net Amount'),
        width: 130,
        type: 'number',
        align: 'right',
        headerAlign: 'right',
        valueGetter: ({ row }) => row.lineTotal,
      },
      {
        field: 'deliveryType',
        headerName: t('salesOrder.deliveryType', 'Delivery type'),
        width: 180,
      },
      {
        field: 'site',
        headerName: t('salesOrderQuickCreate.site', 'Site'),
        width: 120,
        preview: { entity: INVENTORY_RECORD_PREVIEWS.SITE },
      },
      {
        field: 'warehouse',
        headerName: t('salesOrderQuickCreate.warehouse', 'Warehouse'),
        width: 140,
        preview: { entity: INVENTORY_RECORD_PREVIEWS.WAREHOUSE },
      },
    ],
    [t]
  );
}
