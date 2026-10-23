import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { FastTabsDrawer } from '@patterns/drawer-fast-tabs';
import type { FastTabSection, FastTabValue } from '@patterns/dialog-fast-tabs/FastTabsDialog';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { releasedProductApi, type ConfigureReleasedProductInput, type ReleasedProductRecord } from '../api/releasedProductApi';
import { useCompanyStore } from '@core/company/useCompanyStore';

interface Props { open: boolean; onClose: () => void; onCreated: (product: ReleasedProductRecord) => void | Promise<void> }
const initialValues = (): Record<string, FastTabValue> => ({ productType: 'Item', productSubtype: 'Product', productNumber: '', name: '', nameAlias: '', searchName: '', itemNumber: '', itemNameAlias: '', inventoryUnit: '', purchaseUnit: '', salesUnit: '', purchaseToInventoryFactor: 1, salesToInventoryFactor: 1, itemGroupId: '', modelGroupId: '', storageDimensionGroupId: '', trackingDimensionGroupId: '', salesTaxItemGroupId: '', purchaseTaxItemGroupId: '', purchasePrice: 0, salesPrice: 0, inventoryPrice: 0, defaultOrderType: 1, purchaseSiteId: '', purchaseWarehouseId: '', purchaseMinimumQuantity: 0, purchaseMaximumQuantity: 0, purchaseMultipleQuantity: 1, salesSiteId: '', salesWarehouseId: '', salesMinimumQuantity: 0, salesMaximumQuantity: 0, salesMultipleQuantity: 1, inventorySiteId: '', inventoryWarehouseId: '', inventoryMinimumQuantity: 0, inventoryMaximumQuantity: 0, inventoryMultipleQuantity: 1 });
const setupFields = ['modelGroupId', 'itemGroupId', 'storageDimensionGroupId', 'trackingDimensionGroupId', 'inventoryUnit', 'purchaseUnit', 'salesUnit'] as const;

function buildConfiguration(v: Record<string, FastTabValue>): ConfigureReleasedProductInput {
  return { inventoryUnit: String(v.inventoryUnit), purchaseUnit: String(v.purchaseUnit), salesUnit: String(v.salesUnit), purchaseToInventoryFactor: Number(v.purchaseToInventoryFactor), salesToInventoryFactor: Number(v.salesToInventoryFactor), inventoryPrice: Number(v.inventoryPrice), purchasePrice: Number(v.purchasePrice), salesPrice: Number(v.salesPrice), priceUnit: 1, purchaseTaxItemGroupId: String(v.purchaseTaxItemGroupId), salesTaxItemGroupId: String(v.salesTaxItemGroupId), itemGroupId: String(v.itemGroupId), modelGroupId: String(v.modelGroupId), storageDimensionGroupId: String(v.storageDimensionGroupId), trackingDimensionGroupId: String(v.trackingDimensionGroupId), defaultOrderSettings: { defaultOrderType: Number(v.defaultOrderType), purchase: { siteId: String(v.purchaseSiteId), warehouseId: String(v.purchaseWarehouseId), standardQuantity: 1, minimumQuantity: Number(v.purchaseMinimumQuantity), maximumQuantity: Number(v.purchaseMaximumQuantity), multipleQuantity: Number(v.purchaseMultipleQuantity), leadTimeDays: 0, stopped: false }, sales: { siteId: String(v.salesSiteId), warehouseId: String(v.salesWarehouseId), standardQuantity: 1, minimumQuantity: Number(v.salesMinimumQuantity), maximumQuantity: Number(v.salesMaximumQuantity), multipleQuantity: Number(v.salesMultipleQuantity), leadTimeDays: 0, stopped: false }, inventory: { siteId: String(v.inventorySiteId), warehouseId: String(v.inventoryWarehouseId), standardQuantity: 1, minimumQuantity: Number(v.inventoryMinimumQuantity), maximumQuantity: Number(v.inventoryMaximumQuantity), multipleQuantity: Number(v.inventoryMultipleQuantity), leadTimeDays: 0, stopped: false } } };
}

export function ReleasedProductCreateDrawer({ open, onClose, onCreated }: Props): React.ReactElement {
  const currentCompany = useCompanyStore((state) => state.currentCompany);
  const { t } = useAppTranslation();
  const lookups = useQuery({ queryKey: ['inventory', 'released-product-lookups', currentCompany], queryFn: ({ signal }) => releasedProductApi.lookups(signal), enabled: open, staleTime: 300_000 });
  const options = lookups.data;
  const sections = useMemo<FastTabSection[]>(() => [
    { id: 'identity', title: t('releasedProduct.create.identification', 'IDENTIFICATION'), fields: [
      { name: 'productType', label: t('releasedProduct.fields.productType'), type: 'display' },
      { name: 'productSubtype', label: t('releasedProduct.fields.productSubtype'), type: 'select', disabled: true, options: [{ value: 'Product', label: 'Product' }] },
      { name: 'productNumber', label: t('product.fields.productNumber'), required: true }, { name: 'name', label: t('product.fields.productName'), required: true },
      { name: 'nameAlias', label: t('product.fields.arabicName') }, { name: 'searchName', label: t('product.fields.searchName') },
      { name: 'itemNumber', label: t('releasedProduct.fields.itemNumber'), required: true }, { name: 'itemNameAlias', label: t('releasedProduct.fields.companyArabicName', 'Company search name') },
    ] },
    { id: 'groups', title: t('releasedProduct.create.referenceGroups', 'REFERENCE GROUPS'), fields: [
      { name: 'modelGroupId', label: t('releasedProduct.fields.modelGroup', 'Item model group'), type: 'select', options: options?.modelGroups ?? [] },
      { name: 'itemGroupId', label: t('releasedProduct.fields.itemGroup', 'Item group'), type: 'select', options: options?.itemGroups ?? [] },
      { name: 'storageDimensionGroupId', label: t('releasedProduct.fields.storageGroup', 'Storage dimension group'), type: 'select', options: options?.storageDimensionGroups ?? [] },
      { name: 'trackingDimensionGroupId', label: t('releasedProduct.fields.trackingGroup', 'Tracking dimension group'), type: 'select', options: options?.trackingDimensionGroups ?? [] },
    ] },
    { id: 'units', title: t('releasedProduct.create.units', 'UNITS OF MEASURE'), fields: [
      { name: 'inventoryUnit', label: t('releasedProduct.fields.inventoryUnit', 'Inventory unit'), type: 'select', options: options?.units ?? [] },
      { name: 'purchaseUnit', label: t('releasedProduct.fields.purchaseUnit', 'Purchase unit'), type: 'select', options: options?.units ?? [] },
      { name: 'purchaseToInventoryFactor', label: t('releasedProduct.fields.purchaseConversionFactor', 'Purchase-to-inventory factor'), type: 'number' },
      { name: 'salesUnit', label: t('releasedProduct.fields.salesUnit', 'Sales unit'), type: 'select', options: options?.units ?? [] },
      { name: 'salesToInventoryFactor', label: t('releasedProduct.fields.salesConversionFactor', 'Sales-to-inventory factor'), type: 'number' },
    ] },
    { id: 'tax-price', title: t('releasedProduct.create.commercial', 'TAX AND PRICES'), fields: [
      { name: 'salesTaxItemGroupId', label: t('releasedProduct.fields.salesTaxItemGroup', 'Sales item tax group'), type: 'select', options: options?.taxItemGroups ?? [] },
      { name: 'purchaseTaxItemGroupId', label: t('releasedProduct.fields.purchaseTaxItemGroup', 'Purchase item tax group'), type: 'select', options: options?.taxItemGroups ?? [] },
      { name: 'purchasePrice', label: t('releasedProduct.fields.purchasePrice'), type: 'number' }, { name: 'salesPrice', label: t('releasedProduct.fields.salesPrice'), type: 'number' },
      { name: 'inventoryPrice', label: t('releasedProduct.fields.inventoryPrice', 'Inventory price'), type: 'number' },
    ] },
    { id: 'default-order-settings', title: t('releasedProduct.create.defaultOrderSettings', 'DEFAULT ORDER SETTINGS'), fields: [
      { name: 'defaultOrderType', label: t('releasedProduct.fields.defaultOrderType', 'Default order type'), type: 'select', disabled: true, options: [{ value: 1, label: t('releasedProduct.values.purchase', 'Purchase') }] },
      { name: 'purchaseSiteId', label: t('releasedProduct.fields.purchaseSite', 'Purchase site'), type: 'select', options: options?.sites ?? [] },
      { name: 'purchaseWarehouseId', label: t('releasedProduct.fields.purchaseWarehouse', 'Purchase warehouse'), type: 'select', options: options?.warehouses ?? [] },
      { name: 'purchaseMinimumQuantity', label: t('releasedProduct.fields.purchaseMinimumQuantity', 'Purchase minimum quantity'), type: 'number' },
      { name: 'purchaseMaximumQuantity', label: t('releasedProduct.fields.purchaseMaximumQuantity', 'Purchase maximum quantity'), type: 'number' },
      { name: 'purchaseMultipleQuantity', label: t('releasedProduct.fields.purchaseMultipleQuantity', 'Purchase multiple'), type: 'number' },
      { name: 'salesSiteId', label: t('releasedProduct.fields.salesSite', 'Sales site'), type: 'select', options: options?.sites ?? [] },
      { name: 'salesWarehouseId', label: t('releasedProduct.fields.salesWarehouse', 'Sales warehouse'), type: 'select', options: options?.warehouses ?? [] },
      { name: 'salesMinimumQuantity', label: t('releasedProduct.fields.salesMinimumQuantity', 'Sales minimum quantity'), type: 'number' },
      { name: 'salesMaximumQuantity', label: t('releasedProduct.fields.salesMaximumQuantity', 'Sales maximum quantity'), type: 'number' },
      { name: 'salesMultipleQuantity', label: t('releasedProduct.fields.salesMultipleQuantity', 'Sales multiple'), type: 'number' },
      { name: 'inventorySiteId', label: t('releasedProduct.fields.inventorySite', 'Inventory site'), type: 'select', options: options?.sites ?? [] },
      { name: 'inventoryWarehouseId', label: t('releasedProduct.fields.inventoryWarehouse', 'Inventory warehouse'), type: 'select', options: options?.warehouses ?? [] },
      { name: 'inventoryMinimumQuantity', label: t('releasedProduct.fields.inventoryMinimumQuantity', 'Inventory minimum quantity'), type: 'number' },
      { name: 'inventoryMaximumQuantity', label: t('releasedProduct.fields.inventoryMaximumQuantity', 'Inventory maximum quantity'), type: 'number' },
      { name: 'inventoryMultipleQuantity', label: t('releasedProduct.fields.inventoryMultipleQuantity', 'Inventory multiple'), type: 'number' },
    ] },
  ], [options, t]);
  const required = ['productNumber','name','itemNumber'];
  return <FastTabsDrawer open={open} resetKey={open ? 'released-open' : 'released-closed'} title={t('releasedProduct.createTitle', 'New released product')} viewLabel={t('common.standardView')} sections={sections} loading={lookups.isLoading} loadError={lookups.error instanceof Error ? lookups.error.message : null} initialValues={initialValues}
    validate={(values) => {
      const errors = required.reduce<Record<string,string>>((result, field) => { if (!String(values[field] ?? '').trim()) result[field] = t('validation.required', { field }); return result; }, {});
      for (const field of setupFields) if (!String(values[field] ?? '').trim()) errors[field] = t('validation.required', { field });
      if (String(values.purchaseUnit) !== String(values.inventoryUnit) && Number(values.purchaseToInventoryFactor) <= 0) errors.purchaseToInventoryFactor = t('releasedProduct.validation.conversionFactor', 'A positive conversion factor is required when units differ.');
      if (String(values.salesUnit) !== String(values.inventoryUnit) && Number(values.salesToInventoryFactor) <= 0) errors.salesToInventoryFactor = t('releasedProduct.validation.conversionFactor', 'A positive conversion factor is required when units differ.');
      for (const prefix of ['purchase', 'sales', 'inventory']) {
        const siteField = `${prefix}SiteId`; const warehouseField = `${prefix}WarehouseId`;
        if (Boolean(String(values[siteField] ?? '').trim()) !== Boolean(String(values[warehouseField] ?? '').trim())) {
          errors[siteField] = t('releasedProduct.validation.siteWarehousePair', 'Site and warehouse must be selected together.');
          errors[warehouseField] = t('releasedProduct.validation.siteWarehousePair', 'Site and warehouse must be selected together.');
        }
        if (!String(values[siteField] ?? '').trim()) errors[siteField] = t('validation.required', { field: siteField });
        if (!String(values[warehouseField] ?? '').trim()) errors[warehouseField] = t('validation.required', { field: warehouseField });
        const minimum = Number(values[`${prefix}MinimumQuantity`] ?? 0); const maximum = Number(values[`${prefix}MaximumQuantity`] ?? 0); const multiple = Number(values[`${prefix}MultipleQuantity`] ?? 0);
        if (minimum < 0) errors[`${prefix}MinimumQuantity`] = t('validation.nonNegative', 'Value cannot be negative.');
        if (maximum < 0 || (maximum > 0 && minimum > maximum)) errors[`${prefix}MaximumQuantity`] = t('releasedProduct.validation.maximumQuantity', 'Maximum quantity must be zero or greater than or equal to minimum quantity.');
        if (multiple < 0) errors[`${prefix}MultipleQuantity`] = t('validation.nonNegative', 'Value cannot be negative.');
      }
      return errors;
    }}
    onSubmit={async (v) => onCreated(await releasedProductApi.create({ productNumber: String(v.productNumber), name: String(v.name), nameAlias: String(v.nameAlias), searchName: String(v.searchName), itemNumber: String(v.itemNumber), itemNameAlias: String(v.itemNameAlias), productType: String(v.productType) === 'Service' ? 'Service' : 'Item', configuration: buildConfiguration(v) }))}
    saveLabel={t('actions.save')} cancelLabel={t('actions.cancel')} closeLabel={t('actions.close')} helpLabel={t('common.help')} onCancel={onClose} />;
}
