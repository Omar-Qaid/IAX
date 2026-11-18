import React from 'react';
import { Box, Typography } from '@mui/material';
import type { SalesOrderDocumentView } from './useSalesOrderDocument';
import type { buildSalesOrderTotals } from './salesOrderTotals';
import type { buildSalesOrderHeaderPanel } from './SalesOrderHeaderPanel';
import type { buildSalesOrderLineFields } from './SalesOrderLineFields';

type Scope = SalesOrderDocumentView & ReturnType<typeof buildSalesOrderTotals> & ReturnType<typeof buildSalesOrderHeaderPanel> & ReturnType<typeof buildSalesOrderLineFields>;

export function buildSalesOrderLineTabs(scope: Scope) {
  const {
    lineTab,
    t,
    currentLanguage,
    salesDeliveryTypeOptions,
    sourcingOriginOptions,
    deliveryDateControlOptions,
    ctpStatusOptions,
    carrierServiceOptions,
    reservationOptions,
    lookupsQuery,
    field,
    displayedLine,
    lineDetailField,
    lineMultilineField,
    dimensionField,
    deliveryAddressField,
    unitField,
    packingUnitField,
    lineValue,
    lineLookupField,
    lineEnumField,
    taxGroupField,
    lineBooleanField,
    returnLotField,
    inventoryTrackingField,
    mainAccountField,
  } = scope;
  const lineTabContent = (): React.ReactNode => {
    switch (lineTab) {
      case 'General':
        return (
          <Box
            sx={{
              gridColumn: '1 / -1',
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
              gap: 3,
            }}
          >
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.generalGroups.orderLine', 'Order line')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineValue('salesCategory', t('salesOrder.salesCategory', 'Sales category'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.generalGroups.productName', 'Product name')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineValue('productName', t('salesOrder.productName', 'Product name'))}
                {lineMultilineField('description', t('salesOrder.text', 'النص'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.generalGroups.externalReferences', 'External references')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineDetailField('customerReference', t('salesOrder.external', 'External'))}
                <Box sx={{ maxWidth: 72 }}>
                  {lineDetailField('customerLineNumber', t('salesOrder.customerLineNumber', 'Line number'), 'number')}
                </Box>
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.generalGroups.intercompany', 'Intercompany')}
              </Typography>
              {lineEnumField('intercompanyOrigin', t('salesOrder.intercompanyOrigin', 'Origin (intercompany orders)'), [
                { value: 0, label: t('common.none', 'None') },
                { value: 1, label: t('salesOrder.intercompanyOutbound', 'Outbound') },
                { value: 2, label: t('salesOrder.intercompanyInbound', 'Inbound') },
              ])}
              <Typography variant="overline" sx={{ fontWeight: 700, mt: 1.5 }}>
                {t('salesOrder.generalGroups.status', 'Status')}
              </Typography>
              {lineValue('salesStatus', t('salesOrder.lineStatus', 'Line status'))}
            </Box>
            <Box>
              <Box sx={{ display: 'grid', gap: 1.5, pt: 2.5 }}>
                {lineBooleanField('stopped', t('salesOrder.stopped', 'Stopped'))}
                {lineBooleanField(
                  'preventPartialDelivery',
                  t('salesOrder.preventPartialDelivery', 'Prevent partial delivery')
                )}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.generalGroups.fulfillment', 'Fulfillment')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {field(t('salesOrder.fulfillmentStatus', 'حالة التنفيذ'), t('salesOrder.unknown', 'غير معروف'))}
              </Box>
            </Box>
          </Box>
        );
      case 'Setup':
        return (
          <Box
            sx={{
              gridColumn: '1 / -1',
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
              gap: 3,
            }}
          >
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.setupGroups.inventory', 'Inventory')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineValue('inventTransId', t('salesOrder.lotId', 'Lot ID'))}
                {lineEnumField(
                  'reservation',
                  t('salesOrder.reservation', 'Reservation'),
                  reservationOptions
                )}
              </Box>
            </Box>
            <Box>
              <Box sx={{ display: 'grid', gap: 1.5, pt: 2.5 }}>
                {lineBooleanField(
                  'autoBatchReservation',
                  t('salesOrder.autoBatchReservation', 'Auto batch reservation')
                )}
                {lineBooleanField(
                  'sameBatchSelection',
                  t('salesOrder.sameBatchSelection', 'Same batch selection')
                )}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.setupGroups.returnedOrder', 'Returned order')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {returnLotField()}
                {lineValue('costPrice', t('salesOrder.returnCostPrice', 'تكلفة الإرجاع'))}
                {lineBooleanField('scrap', t('salesOrder.scrap', 'Scrap'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.setupGroups.posting', 'Posting')}
              </Typography>
              {mainAccountField()}
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.setupGroups.salesTax', 'Sales tax')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {taxGroupField('taxItemGroup', t('salesOrder.itemSalesTaxGroup', 'Item sales tax group'))}
                {taxGroupField('taxGroup', t('salesOrder.salesTaxGroup', 'Sales tax group'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.setupGroups.commission', 'Commission')}
              </Typography>
              {lineDetailField('salesGroup', t('salesOrder.salesGroup', 'Sales group'))}
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.setupGroups.dateTime', 'Date and time')}
              </Typography>
              {field(
                t('salesOrder.createdDateTime', 'Created date and time'),
                displayedLine.createdAt
                  ? new Date(displayedLine.createdAt).toLocaleString(currentLanguage.code)
                  : undefined
              )}
            </Box>
          </Box>
        );
      case 'Address':
        return (
          <Box sx={{ gridColumn: '1 / -1', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 220px))', gap: 3 }}>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>DELIVERY ADDRESS</Typography>
              <Box sx={{ display: 'grid', gap: 1.5, mt: 1 }}>
                {lineMultilineField('deliveryName', 'Name')}
                {deliveryAddressField()}
              </Box>
            </Box>
          </Box>
        );
      case 'Product':
        return (
          <Box
            sx={{
              gridColumn: '1 / -1',
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
              gap: 3,
            }}
          >
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.sourcingGroups.tracking', 'أبعاد التتبع')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineDetailField('configId', 'Configuration')}
                {lineDetailField('inventSizeId', 'Size')}
                {lineDetailField('inventColorId', 'Color')}
                {lineDetailField('inventStyleId', 'Style')}
                {lineDetailField('inventVersionId', 'Version')}
                {inventoryTrackingField('batchNumber', t('salesOrder.batchNumber', 'رقم الدفعة'))}
                {inventoryTrackingField('serialNumber', t('salesOrder.serialNumber', 'الرقم التسلسلي'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.sourcingGroups.storage', 'أبعاد التخزين')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {dimensionField('site', t('salesOrderQuickCreate.site', 'Site'))}
                {dimensionField('warehouse', t('salesOrderQuickCreate.warehouse', 'Warehouse'))}
              </Box>
            </Box>
            <Box>
              <Box sx={{ display: 'grid', gap: 1.5, pt: 2.5 }}>
                {lineValue('location', t('salesOrder.location', 'الموقع'))}
                {lineValue('inventoryStatus', t('salesOrder.inventoryStatus', 'حالة المخزون'))}
                {lineValue('licensePlate', t('salesOrder.licensePlate', 'لوحة الترخيص'))}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.sourcingGroups.itemReference', 'مرجع الصنف')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineEnumField('itemReferenceType', t('salesOrder.referenceType', 'Reference type'), [
                  { value: 0, label: t('common.none', 'None') },
                  { value: 1, label: t('salesOrder.ui.purchaseOrder', 'Purchase order') },
                  { value: 2, label: t('common.salesOrder', 'Sales order') },
                  { value: 3, label: t('salesOrder.ui.inventoryJournal', 'Inventory journal') },
                  { value: 4, label: t('salesOrder.ui.transferOrder', 'Transfer order') },
                  { value: 5, label: t('salesOrder.ui.productionOrder', 'Production order') },
                  { value: 6, label: t('salesOrder.ui.inventoryAdjustment', 'Inventory adjustment') },
                  { value: 7, label: t('salesOrder.ui.physicalInventoryCount', 'Physical inventory count') },
                  { value: 8, label: t('salesOrder.ui.returnOrder', 'Return order') },
                ])}
                {lineValue('itemReferenceNumber', t('salesOrder.referenceNumber', 'رقم المرجع'))}
                {lineValue('itemReferenceLot', t('salesOrder.referenceLot', 'تشغيلة المرجع'))}
              </Box>
            </Box>
          </Box>
        );
      case 'Packing':
        return (
          <Box
            sx={{
              gridColumn: '1 / -1',
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
              gap: 3,
            }}
          >
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>
                {t('salesOrder.packingGroups.material', 'مواد التعبئة')}
              </Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {packingUnitField()}
                {lineDetailField(
                  'packingUnitQuantity',
                  t('salesOrder.packingUnitQuantity', 'كمية وحدة التعبئة'),
                  'number'
                )}
              </Box>
            </Box>
          </Box>
        );
      case 'Delivery':
        return (
          <>
            <Box sx={{ gridColumn: '1 / -1', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: 3 }}>
              <Box>
                <Typography variant="overline" sx={{ fontWeight: 700 }}>DELIVERY DATE</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineDetailField('shippingDateRequested', 'Requested ship date', 'date')}
                  {lineDetailField('deliveryDate', 'Requested receipt date', 'date')}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" sx={{ fontWeight: 700 }}>CONFIRMED DATES</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineDetailField('shippingDateConfirmed', 'Confirmed ship date', 'date')}
                  {lineDetailField('receiptDateConfirmed', 'Confirmed receipt date', 'date')}
                  {lineEnumField('deliveryDateControlType', 'Delivery date control', deliveryDateControlOptions)}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" sx={{ fontWeight: 700 }}>PLANNING</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineEnumField('mpsFullRunCtpStatus', 'Batch CTP status', ctpStatusOptions)}
                  {lineDetailField('planningPriority', 'Planning priority', 'number')}
                  {lineLookupField('deliveryTerms', t('customerQuickCreate.fields.deliveryTerms'), lookupsQuery.data?.deliveryTerms)}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" sx={{ fontWeight: 700 }}>DELIVERY</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineDetailField('overDeliveryPercent', 'Overdelivery', 'number')}
                  {lineDetailField('underDeliveryPercent', 'Underdelivery', 'number')}
                  {lineLookupField('deliveryMode', t('fields.deliveryMode'), lookupsQuery.data?.deliveryModes)}
                  {lineEnumField('deliveryType', 'Delivery type', salesDeliveryTypeOptions)}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" sx={{ fontWeight: 700 }}>DIRECT DELIVERY</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineBooleanField('directDelivery', 'Direct delivery')}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" sx={{ fontWeight: 700 }}>CARRIER INFORMATION</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {lineEnumField('shipCarrierDlvType', 'Carrier service', carrierServiceOptions)}
                </Box>
              </Box>
              <Box>
                <Typography variant="overline" sx={{ fontWeight: 700 }}>STORAGE LOCATION</Typography>
                <Box sx={{ display: 'grid', gap: 1.5 }}>
                  {dimensionField('site', t('salesOrderQuickCreate.site', 'Site'))}
                  {dimensionField('warehouse', t('salesOrderQuickCreate.warehouse', 'Warehouse'))}
                </Box>
              </Box>
            </Box>
          </>
        );
      case 'Sourcing':
        return (
          <Box sx={{ gridColumn: '1 / -1', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: 3 }}>
            <Box sx={{ display: 'grid', gap: 1.5 }}>
              {lineEnumField('deliveryType', 'Delivery type', [
                { value: 0, label: t('salesOrder.ui.stock', 'Stock') },
                { value: 1, label: t('salesOrder.ui.pickup', 'Pickup') },
                { value: 2, label: t('salesOrder.ui.directDelivery', 'Direct delivery') },
              ])}
            </Box>
            <Box sx={{ display: 'grid', gap: 1.5 }}>
              {lineEnumField('sourcingOrigin', 'Sourcing origin', sourcingOriginOptions)}
            </Box>
            <Box sx={{ display: 'grid', gap: 1.5 }}>{field('Sourcing vendor')}</Box>
            <Box sx={{ display: 'grid', gap: 1.5 }}>{field('Sourcing company')}</Box>
            <Box sx={{ display: 'grid', gap: 1.5 }}>{dimensionField('site', 'Sourcing site')}</Box>
            <Box sx={{ display: 'grid', gap: 1.5 }}>
              {dimensionField('warehouse', 'Sourcing warehouse')}
              <Typography variant="overline" sx={{ fontWeight: 700 }}>MASTER PLANNING</Typography>
              {lineBooleanField('excludeFromMasterPlanning', 'Exclude from master planning')}
            </Box>
          </Box>
        );      case 'Price and discount':
        return (
          <Box sx={{ gridColumn: '1 / -1', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: 3 }}>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>DISCOUNT</Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineDetailField('lineDiscount', 'Discount', 'number')}
                {lineDetailField('lineDiscountPercent', 'Discount percent', 'number')}
              </Box>
            </Box>
            <Box>
              <Box sx={{ display: 'grid', gap: 1.5, pt: 2.5 }}>
                {lineDetailField('multiLineDiscount', 'Multiline discount', 'number')}
                {lineDetailField('multiLineDiscountPercent', 'Multiline discount percentage', 'number')}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>PRICES</Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineDetailField('priceUnit', 'Price unit', 'number')}
                {lineDetailField('salesMarkup', 'Sales charges', 'number')}
              </Box>
            </Box>
            <Box>
              <Typography variant="overline" sx={{ fontWeight: 700 }}>REBATES</Typography>
              <Box sx={{ display: 'grid', gap: 1.5 }}>
                {lineBooleanField('excludeFromRebate', 'Exclude from rebate')}
                {lineBooleanField('excludeFromRebateManagement', 'Exclude from rebate management')}
              </Box>
            </Box>
          </Box>
        );      case 'Foreign trade':
        return <>{lineValue('intrastatCommodity', 'Intrastat commodity')}</>;
      case 'Financial dimensions':
        return (
          <>
            {lineValue('ledgerDimension', 'Ledger dimension')}
            {lineValue('defaultDimension', 'Default dimension')}
          </>
        );
      case 'Loads':
        return (
          <>
            {lineValue('quantity', t('fields.quantity'))}
            {lineValue('salesDeliverNow', 'Deliver now')}
            {lineValue('inventDeliverNow', 'Inventory deliver now')}
            {lineValue('remainSalesPhysical', 'Remaining physical quantity')}
            {lineValue('remainSalesFinancial', 'Remaining financial quantity')}
          </>
        );
      case 'Financial tags':
        return <>{lineValue('financialTag', 'Financial tag')}</>;
      default:
        return (
          <>
            {lineDetailField('itemNumber', t('fields.item'))}
            {lineDetailField('description', t('salesOrder.productName', 'Product name'))}
            {lineDetailField('quantity', t('fields.quantity'), 'number')}
            {unitField()}
            {dimensionField('site', t('salesOrderQuickCreate.site', 'Site'))}
            {dimensionField('warehouse', t('salesOrderQuickCreate.warehouse', 'Warehouse'))}
            {lineDetailField('unitPrice', t('fields.unitPrice'), 'number')}
            {lineDetailField(
              'salesCategory',
              t('salesOrder.salesCategory', 'Sales category'),
              'number'
            )}
            {lineDetailField('deliveryDate', t('fields.requestedDelivery'), 'date')}
          </>
        );
    }
  };
  return {
    lineTabContent,
  };
}
