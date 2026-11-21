import type { ColumnDef } from '@shared/components/data-grid/types';
import type { SalesOrderLineRecord as DetailLine } from '../api/salesOrderLinesApi';
import { INVENTORY_RECORD_PREVIEWS } from '../../../inventory/previews/inventoryRecordPreviews';

/** `headerName` holds a translation key; the grid translates it where it renders. */
const SALES_ORDER_LINE_COLUMNS: ColumnDef<DetailLine>[] = [
  {
    field: 'lineType',
    headerName: 'salesOrder.lineType',
    width: 110,
    minWidth: 85,
    type: 'number',
  },
  {
    preview: { entity: 'releasedProduct' },
    field: 'itemNumber',
    headerName: 'salesOrder.itemNumber',
    width: 160,
    minWidth: 120,
  },
  { field: 'description', headerName: 'salesOrder.productName', width: 220 },
  {
    field: 'quantity',
    headerName: 'fields.quantity',
    width: 85,
    type: 'number',
    align: 'right',
    headerAlign: 'right',
  },
  {
    preview: { entity: 'unitOfMeasure' },
    field: 'unit',
    headerName: 'salesOrder.unit',
    width: 100,
  },
  {
    field: 'unitPrice',
    headerName: 'fields.unitPrice',
    width: 110,
    type: 'number',
    align: 'right',
    headerAlign: 'right',
  },
  {
    field: 'lineDiscount',
    headerName: 'salesOrder.lineDiscount',
    width: 110,
    type: 'number',
    align: 'right',
    headerAlign: 'right',
  },
  {
    field: 'lineDiscountPercent',
    headerName: 'salesOrder.lineDiscountPercent',
    width: 110,
    type: 'number',
    align: 'right',
    headerAlign: 'right',
  },
  {
    field: 'deliveryDate',
    headerName: 'fields.requestedDelivery',
    width: 150,
    type: 'date',
  },
  {
    field: 'netAmount',
    headerName: 'salesOrder.netAmount',
    width: 130,
    type: 'number',
    align: 'right',
    headerAlign: 'right',
    valueGetter: ({ row }) => row.lineTotal,
  },
  {
    field: 'deliveryType',
    headerName: 'salesOrder.deliveryType',
    width: 180,
  },
  {
    field: 'site',
    headerName: 'fields.site',
    width: 120,
    preview: { entity: INVENTORY_RECORD_PREVIEWS.SITE },
  },
  {
    field: 'warehouse',
    headerName: 'fields.warehouse',
    width: 140,
    preview: { entity: INVENTORY_RECORD_PREVIEWS.WAREHOUSE },
  },
];

export function useSalesOrderLineColumns(): ColumnDef<DetailLine>[] {
  return SALES_ORDER_LINE_COLUMNS;
}
