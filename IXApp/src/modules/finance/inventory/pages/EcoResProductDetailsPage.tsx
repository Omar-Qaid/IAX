import React, { useMemo, useState } from 'react';
import { Alert, Snackbar } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { localizedName } from '@shared/utilities/localizedName';
import { releasedProductApi, type ReleasedProductRecord, type ReleasedProductValidation } from '../api/releasedProductApi';
import { useCompanyStore } from '@core/company/useCompanyStore';

const createEmptyProduct = (): ReleasedProductRecord => ({
  id: 'empty',
  itemNumber: '',
  name: '',
  nameAliasLocalized: '',
  nameAlias: '',
  productType: '',
  productSubtype: '',
  salesPrice: 0,
  purchasePrice: 0,
  salesPriceQuantity: 1,
  purchasePriceQuantity: 1,
  salesTaxItemGroupId: '',
  purchaseTaxItemGroupId: '',
  costPrice: 0,
  itemGroupId: '',
  modelGroupId: '',
  storageDimensionGroupId: '',
  trackingDimensionGroupId: '',
  inventoryUnit: '',
  purchaseUnit: '',
  salesUnit: '',
  purchaseToInventoryFactor: 1,
  salesToInventoryFactor: 1,
  defaultOrderSettings: {
    defaultOrderType: 1,
    purchase: { siteId: '', warehouseId: '', standardQuantity: 0, minimumQuantity: 0, maximumQuantity: 0, multipleQuantity: 0, leadTimeDays: 0, stopped: false },
    sales: { siteId: '', warehouseId: '', standardQuantity: 0, minimumQuantity: 0, maximumQuantity: 0, multipleQuantity: 0, leadTimeDays: 0, stopped: false },
    inventory: { siteId: '', warehouseId: '', standardQuantity: 0, minimumQuantity: 0, maximumQuantity: 0, multipleQuantity: 0, leadTimeDays: 0, stopped: false },
  },
});

export function EcoResProductDetailsPage(): React.ReactElement {
  const { t, currentLanguage, isRtl } = useAppTranslation();
  const currentCompany = useCompanyStore((state) => state.currentCompany);
  const { productId } = useParams<{ productId: string }>();
  const [validation, setValidation] = useState<ReleasedProductValidation | null>(null);
  const [validationError, setValidationError] = useState<string | null>(null);
  const [validating, setValidating] = useState(false);
  const lookups = useQuery({
    queryKey: ['inventory', 'released-product-lookups', currentCompany],
    queryFn: ({ signal }) => releasedProductApi.lookups(signal),
    staleTime: 300_000,
  });

  const config = useMemo<EnterpriseListDetailsConfig<ReleasedProductRecord>>(
    () => ({
      recordTableName: 'InventTable',
      interactionLocked: validating,
      readOnly: false,
      dataSource: {
        type: 'remote',
        key: `inventory-released-products-${currentCompany}`,
        load: (signal) => releasedProductApi.list(signal),
        create: async () => { throw new Error(t('releasedProduct.createFromList', 'Create released products from the Released products list.')); },
        update: (record) => releasedProductApi.assignUnits(record.id, { inventoryUnit: record.inventoryUnit, purchaseUnit: record.purchaseUnit, salesUnit: record.salesUnit, purchaseToInventoryFactor: record.purchaseToInventoryFactor, salesToInventoryFactor: record.salesToInventoryFactor, groups: { itemGroupId: record.itemGroupId, modelGroupId: record.modelGroupId, storageDimensionGroupId: record.storageDimensionGroupId, trackingDimensionGroupId: record.trackingDimensionGroupId }, commercial: { purchasePrice: record.purchasePrice, purchasePriceQuantity: record.purchasePriceQuantity, purchaseTaxItemGroupId: record.purchaseTaxItemGroupId, salesPrice: record.salesPrice, salesPriceQuantity: record.salesPriceQuantity, salesTaxItemGroupId: record.salesTaxItemGroupId }, defaultOrderSettings: record.defaultOrderSettings }),
      },
      initialSelectedId: productId,
      createRecord: createEmptyProduct,
      getPrimaryText: (product) => product.itemNumber,
      getSecondaryText: (product) => localizedName({ name: product.name, nameAlias: product.nameAliasLocalized }, isRtl),
      matchesSearch: (product, query) =>
        `${product.itemNumber} ${product.name} ${product.nameAlias}`
          .toLowerCase()
          .includes(query.trim().toLowerCase()),
      getValues: (product): DetailValues => ({
        itemNumber: product.itemNumber,
        name: product.name,
        nameAliasLocalized: product.nameAliasLocalized,
        nameAlias: product.nameAlias,
        productType: product.productType,
        productSubtype: product.productSubtype,
        salesPrice: product.salesPrice,
        purchasePrice: product.purchasePrice,
        salesPriceQuantity: product.salesPriceQuantity,
        purchasePriceQuantity: product.purchasePriceQuantity,
        salesTaxItemGroupId: product.salesTaxItemGroupId,
        purchaseTaxItemGroupId: product.purchaseTaxItemGroupId,
        defaultOrderType: String(product.defaultOrderSettings.defaultOrderType),
        purchaseDefaultSiteId: product.defaultOrderSettings.purchase.siteId, purchaseDefaultWarehouseId: product.defaultOrderSettings.purchase.warehouseId,
        purchaseMinimumQuantity: product.defaultOrderSettings.purchase.minimumQuantity, purchaseMaximumQuantity: product.defaultOrderSettings.purchase.maximumQuantity, purchaseMultipleQuantity: product.defaultOrderSettings.purchase.multipleQuantity, purchaseStandardQuantity: product.defaultOrderSettings.purchase.standardQuantity, purchaseLeadTimeDays: product.defaultOrderSettings.purchase.leadTimeDays, purchaseStopped: product.defaultOrderSettings.purchase.stopped,
        salesDefaultSiteId: product.defaultOrderSettings.sales.siteId, salesDefaultWarehouseId: product.defaultOrderSettings.sales.warehouseId,
        salesMinimumQuantity: product.defaultOrderSettings.sales.minimumQuantity, salesMaximumQuantity: product.defaultOrderSettings.sales.maximumQuantity, salesMultipleQuantity: product.defaultOrderSettings.sales.multipleQuantity, salesStandardQuantity: product.defaultOrderSettings.sales.standardQuantity, salesLeadTimeDays: product.defaultOrderSettings.sales.leadTimeDays, salesStopped: product.defaultOrderSettings.sales.stopped,
        inventoryDefaultSiteId: product.defaultOrderSettings.inventory.siteId, inventoryDefaultWarehouseId: product.defaultOrderSettings.inventory.warehouseId,
        inventoryMinimumQuantity: product.defaultOrderSettings.inventory.minimumQuantity, inventoryMaximumQuantity: product.defaultOrderSettings.inventory.maximumQuantity, inventoryMultipleQuantity: product.defaultOrderSettings.inventory.multipleQuantity, inventoryStandardQuantity: product.defaultOrderSettings.inventory.standardQuantity, inventoryLeadTimeDays: product.defaultOrderSettings.inventory.leadTimeDays, inventoryStopped: product.defaultOrderSettings.inventory.stopped,
        costPrice: product.costPrice,
        itemGroupId: product.itemGroupId,
        modelGroupId: product.modelGroupId,
        storageDimensionGroupId: product.storageDimensionGroupId,
        trackingDimensionGroupId: product.trackingDimensionGroupId,
        inventoryUnit: product.inventoryUnit,
        purchaseUnit: product.purchaseUnit,
        salesUnit: product.salesUnit,
        purchaseToInventoryFactor: product.purchaseToInventoryFactor,
        salesToInventoryFactor: product.salesToInventoryFactor,
      }),
      setValues: (product, values) => ({ ...product,
        itemGroupId: String(values.itemGroupId ?? ''), modelGroupId: String(values.modelGroupId ?? ''),
        storageDimensionGroupId: String(values.storageDimensionGroupId ?? ''), trackingDimensionGroupId: String(values.trackingDimensionGroupId ?? ''),
        inventoryUnit: String(values.inventoryUnit ?? ''), purchaseUnit: String(values.purchaseUnit ?? ''), salesUnit: String(values.salesUnit ?? ''),
        purchaseToInventoryFactor: Number(values.purchaseToInventoryFactor ?? 0), salesToInventoryFactor: Number(values.salesToInventoryFactor ?? 0),
        purchasePrice: Number(values.purchasePrice ?? 0), salesPrice: Number(values.salesPrice ?? 0),
        purchasePriceQuantity: Number(values.purchasePriceQuantity ?? 1), salesPriceQuantity: Number(values.salesPriceQuantity ?? 1),
        purchaseTaxItemGroupId: String(values.purchaseTaxItemGroupId ?? ''), salesTaxItemGroupId: String(values.salesTaxItemGroupId ?? ''),
        defaultOrderSettings: {
          defaultOrderType: Number(values.defaultOrderType ?? 1),
          purchase: { siteId: String(values.purchaseDefaultSiteId ?? ''), warehouseId: String(values.purchaseDefaultWarehouseId ?? ''), minimumQuantity: Number(values.purchaseMinimumQuantity ?? 0), maximumQuantity: Number(values.purchaseMaximumQuantity ?? 0), multipleQuantity: Number(values.purchaseMultipleQuantity ?? 0), standardQuantity: Number(values.purchaseStandardQuantity ?? 0), leadTimeDays: Number(values.purchaseLeadTimeDays ?? 0), stopped: Boolean(values.purchaseStopped) },
          sales: { siteId: String(values.salesDefaultSiteId ?? ''), warehouseId: String(values.salesDefaultWarehouseId ?? ''), minimumQuantity: Number(values.salesMinimumQuantity ?? 0), maximumQuantity: Number(values.salesMaximumQuantity ?? 0), multipleQuantity: Number(values.salesMultipleQuantity ?? 0), standardQuantity: Number(values.salesStandardQuantity ?? 0), leadTimeDays: Number(values.salesLeadTimeDays ?? 0), stopped: Boolean(values.salesStopped) },
          inventory: { siteId: String(values.inventoryDefaultSiteId ?? ''), warehouseId: String(values.inventoryDefaultWarehouseId ?? ''), minimumQuantity: Number(values.inventoryMinimumQuantity ?? 0), maximumQuantity: Number(values.inventoryMaximumQuantity ?? 0), multipleQuantity: Number(values.inventoryMultipleQuantity ?? 0), standardQuantity: Number(values.inventoryStandardQuantity ?? 0), leadTimeDays: Number(values.inventoryLeadTimeDays ?? 0), stopped: Boolean(values.inventoryStopped) },
        },
      }),
      headerFields: [
        {
          id: 'itemNumber',
          label: t('releasedProduct.fields.itemNumber', 'Item number'),
          getValue: (product) => product.itemNumber,
          setValue: (product) => product,
        },
        {
          id: 'name',
          label: t('releasedProduct.fields.productName', 'Product name'),
          getValue: (product) => localizedName({ name: product.name, nameAlias: product.nameAliasLocalized }, isRtl),
          setValue: (product) => product,
        },
      ],
      sections: ({ record }) => {
        const formatPrice = (value: number) => value.toLocaleString(currentLanguage.code, {
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        });
        const general: DetailSectionConfig = {
          id: 'general',
          title: t('releasedProduct.sections.general', 'General'),
          defaultExpanded: true,
          columns: 5,
          groups: [
            {
              id: 'identification',
              title: t('releasedProduct.groups.identification', 'IDENTIFICATION'),
              fields: [
                { name: 'productType', label: t('releasedProduct.fields.productType', 'Product type'), type: 'display' },
                { name: 'productSubtype', label: t('releasedProduct.fields.productSubtype', 'Product subtype'), type: 'display' },
                { name: 'itemNumber', label: t('releasedProduct.fields.itemNumber', 'Item number'), type: 'display' },
              ],
            },
            {
              id: 'names',
              title: t('releasedProduct.groups.furtherIdentification', 'FURTHER IDENTIFICATION'),
              fields: [
                { name: 'name', label: t('releasedProduct.fields.productName', 'Product name'), type: 'display' },
                { name: 'nameAliasLocalized', label: t('releasedProduct.fields.arabicName', 'Arabic name'), type: 'display' },
              ],
            },
            {
              id: 'administration',
              title: t('releasedProduct.groups.administration', 'ADMINISTRATION'),
              fields: [{ name: 'modelGroupId', label: t('releasedProduct.fields.modelGroup', 'Item model group'), type: 'select', options: lookups.data?.modelGroups ?? [] }],
            },
          ],
        };
        const purchase: DetailSectionConfig = {
          id: 'purchase',
          title: t('releasedProduct.sections.purchase', 'Purchase'),
          defaultExpanded: true,
          columns: 5,
          groups: [
            { id: 'purchase-unit', title: t('releasedProduct.groups.units', 'UNITS'), fields: [{ name: 'purchaseUnit', label: t('releasedProduct.fields.purchaseUnit', 'Purchase unit'), type: 'select', options: lookups.data?.units ?? [] }, { name: 'purchaseToInventoryFactor', label: t('releasedProduct.fields.purchaseConversionFactor', 'Purchase-to-inventory factor'), type: 'number' }] },
            { id: 'purchase-price', title: t('releasedProduct.groups.prices', 'PRICES'), fields: [
              { name: 'purchasePrice', label: t('releasedProduct.fields.purchasePrice', 'Purchase price'), type: 'number' },
              { name: 'purchasePriceQuantity', label: t('releasedProduct.fields.priceQuantity', 'Price quantity'), type: 'number' },
              { name: 'purchaseTaxItemGroupId', label: t('releasedProduct.fields.purchaseTaxItemGroup', 'Purchase item sales tax group'), type: 'select', options: lookups.data?.taxItemGroups ?? [] },
            ] },
          ],
        };
        const sell: DetailSectionConfig = {
          id: 'sell',
          title: t('releasedProduct.sections.sell', 'Sell'),
          defaultExpanded: true,
          columns: 5,
          groups: [
            { id: 'sales-unit', title: t('releasedProduct.groups.units', 'UNITS'), fields: [{ name: 'salesUnit', label: t('releasedProduct.fields.salesUnit', 'Sales unit'), type: 'select', options: lookups.data?.units ?? [] }, { name: 'salesToInventoryFactor', label: t('releasedProduct.fields.salesConversionFactor', 'Sales-to-inventory factor'), type: 'number' }] },
            { id: 'sales-price', title: t('releasedProduct.groups.baseSalesPrice', 'BASE SALES PRICE'), fields: [
              { name: 'salesPrice', label: t('releasedProduct.fields.salesPrice', 'Sales price'), type: 'number' },
              { name: 'salesPriceQuantity', label: t('releasedProduct.fields.priceQuantity', 'Price quantity'), type: 'number' },
              { name: 'salesTaxItemGroupId', label: t('releasedProduct.fields.salesTaxItemGroup', 'Sales item sales tax group'), type: 'select', options: lookups.data?.taxItemGroups ?? [] },
            ] },
          ],
        };
        const inventory: DetailSectionConfig = {
          id: 'manage-inventory',
          title: t('releasedProduct.sections.manageInventory', 'Manage inventory'),
          defaultExpanded: true,
          columns: 5,
          groups: [
            { id: 'inventory-classification', title: t('releasedProduct.groups.inventory', 'INVENTORY'), fields: [{ name: 'productType', label: t('releasedProduct.fields.productType', 'Product type'), type: 'display' }, { name: 'productSubtype', label: t('releasedProduct.fields.productSubtype', 'Product subtype'), type: 'display' }, { name: 'inventoryUnit', label: t('releasedProduct.fields.inventoryUnit', 'Inventory unit'), type: 'select', options: lookups.data?.units ?? [] }] },
          ],
        };
        const costs: DetailSectionConfig = {
          id: 'manage-costs',
          title: t('releasedProduct.sections.manageCosts', 'Manage costs'),
          defaultExpanded: true,
          columns: 5,
          groups: [
            { id: 'cost-prices', title: t('releasedProduct.groups.prices', 'PRICES'), fields: [{ name: 'costPrice', label: t('releasedProduct.fields.costPrice', 'Cost price'), type: 'display', formatValue: (v) => formatPrice(Number(v ?? 0)) }] },
            { id: 'item-group', title: t('releasedProduct.groups.itemGroup', 'ITEM GROUP'), fields: [{ name: 'itemGroupId', label: t('releasedProduct.fields.itemGroup', 'Item group'), type: 'select', options: lookups.data?.itemGroups ?? [] }] },
            { id: 'dimension-groups', title: t('releasedProduct.groups.dimensionGroups', 'DIMENSION GROUPS'), fields: [
              { name: 'storageDimensionGroupId', label: t('releasedProduct.fields.storageGroup', 'Storage dimension group'), type: 'select', options: lookups.data?.storageDimensionGroups ?? [] },
              { name: 'trackingDimensionGroupId', label: t('releasedProduct.fields.trackingGroup', 'Tracking dimension group'), type: 'select', options: lookups.data?.trackingDimensionGroups ?? [] },
            ] },
          ],
        };
        const defaultOrders: DetailSectionConfig = {
          id: 'default-order-settings', title: t('releasedProduct.sections.defaultOrderSettings', 'Default order settings'), defaultExpanded: true, columns: 4,
          groups: [
            { id: 'supply-type', title: t('releasedProduct.groups.supplyType', 'SUPPLY TYPE'), fields: [{ name: 'defaultOrderType', label: t('releasedProduct.fields.defaultOrderType', 'Default order type'), type: 'select', options: [{ value: '1', label: t('releasedProduct.values.purchase', 'Purchase') }, { value: '2', label: t('releasedProduct.values.production', 'Production') }, { value: '3', label: t('releasedProduct.values.kanban', 'Kanban') }] }] },
            { id: 'purchase-defaults', title: t('releasedProduct.groups.purchaseDefaults', 'PURCHASE DEFAULTS'), fields: [
              { name: 'purchaseDefaultSiteId', label: t('releasedProduct.fields.defaultSite', 'Default site'), type: 'select', options: lookups.data?.sites ?? [] }, { name: 'purchaseDefaultWarehouseId', label: t('releasedProduct.fields.defaultWarehouse', 'Default warehouse'), type: 'select', options: lookups.data?.warehouses ?? [] },
              { name: 'purchaseMinimumQuantity', label: t('releasedProduct.fields.minimumQuantity', 'Minimum quantity'), type: 'number' }, { name: 'purchaseMaximumQuantity', label: t('releasedProduct.fields.maximumQuantity', 'Maximum quantity'), type: 'number' }, { name: 'purchaseMultipleQuantity', label: t('releasedProduct.fields.multipleQuantity', 'Multiple'), type: 'number' }, { name: 'purchaseStandardQuantity', label: t('releasedProduct.fields.standardQuantity', 'Standard quantity'), type: 'number' }, { name: 'purchaseLeadTimeDays', label: t('releasedProduct.fields.leadTime', 'Lead time (days)'), type: 'number' }, { name: 'purchaseStopped', label: t('releasedProduct.fields.stopped', 'Stopped'), type: 'boolean' },
            ] },
            { id: 'sales-defaults', title: t('releasedProduct.groups.salesDefaults', 'SALES DEFAULTS'), fields: [
              { name: 'salesDefaultSiteId', label: t('releasedProduct.fields.defaultSite', 'Default site'), type: 'select', options: lookups.data?.sites ?? [] }, { name: 'salesDefaultWarehouseId', label: t('releasedProduct.fields.defaultWarehouse', 'Default warehouse'), type: 'select', options: lookups.data?.warehouses ?? [] },
              { name: 'salesMinimumQuantity', label: t('releasedProduct.fields.minimumQuantity', 'Minimum quantity'), type: 'number' }, { name: 'salesMaximumQuantity', label: t('releasedProduct.fields.maximumQuantity', 'Maximum quantity'), type: 'number' }, { name: 'salesMultipleQuantity', label: t('releasedProduct.fields.multipleQuantity', 'Multiple'), type: 'number' }, { name: 'salesStandardQuantity', label: t('releasedProduct.fields.standardQuantity', 'Standard quantity'), type: 'number' }, { name: 'salesLeadTimeDays', label: t('releasedProduct.fields.leadTime', 'Lead time (days)'), type: 'number' }, { name: 'salesStopped', label: t('releasedProduct.fields.stopped', 'Stopped'), type: 'boolean' },
            ] },
            { id: 'inventory-defaults', title: t('releasedProduct.groups.inventoryDefaults', 'INVENTORY DEFAULTS'), fields: [
              { name: 'inventoryDefaultSiteId', label: t('releasedProduct.fields.defaultSite', 'Default site'), type: 'select', options: lookups.data?.sites ?? [] }, { name: 'inventoryDefaultWarehouseId', label: t('releasedProduct.fields.defaultWarehouse', 'Default warehouse'), type: 'select', options: lookups.data?.warehouses ?? [] },
              { name: 'inventoryMinimumQuantity', label: t('releasedProduct.fields.minimumQuantity', 'Minimum quantity'), type: 'number' }, { name: 'inventoryMaximumQuantity', label: t('releasedProduct.fields.maximumQuantity', 'Maximum quantity'), type: 'number' }, { name: 'inventoryMultipleQuantity', label: t('releasedProduct.fields.multipleQuantity', 'Multiple'), type: 'number' }, { name: 'inventoryStandardQuantity', label: t('releasedProduct.fields.standardQuantity', 'Standard quantity'), type: 'number' }, { name: 'inventoryLeadTimeDays', label: t('releasedProduct.fields.leadTime', 'Lead time (days)'), type: 'number' }, { name: 'inventoryStopped', label: t('releasedProduct.fields.stopped', 'Stopped'), type: 'boolean' },
            ] },
          ],
        };
        return [general, purchase, sell, inventory, costs, defaultOrders];
      },
      permissions: { view: PERMISSIONS.INVENTORY_TRANSACTION_VIEW, edit: PERMISSIONS.INVENTORY_TRANSACTION_EDIT },
      commands: (record) => [{
        id: 'validate',
        label: validating ? t('releasedProduct.actions.validating', 'Validating...') : t('releasedProduct.actions.validate', 'Validate'),
        requiresSelection: true,
        disabled: !record || validating,
        onClick: async (selected) => {
          if (!selected) return;
          setValidation(null);
          setValidationError(null);
          setValidating(true);
          try {
            setValidation(await releasedProductApi.validate(selected.id));
          } catch (error) {
            setValidationError(error instanceof Error ? error.message : t('releasedProduct.validation.failed', 'The released product could not be validated.'));
          } finally {
            setValidating(false);
          }
        },
      }],
      validate: (record) => ({
        ...(!record.modelGroupId ? { modelGroupId: t('validation.required', { field: t('releasedProduct.fields.modelGroup', 'Item model group') }) } : {}),
        ...(!record.itemGroupId ? { itemGroupId: t('validation.required', { field: t('releasedProduct.fields.itemGroup', 'Item group') }) } : {}),
        ...(!record.storageDimensionGroupId ? { storageDimensionGroupId: t('validation.required', { field: t('releasedProduct.fields.storageGroup', 'Storage dimension group') }) } : {}),
        ...(!record.trackingDimensionGroupId ? { trackingDimensionGroupId: t('validation.required', { field: t('releasedProduct.fields.trackingGroup', 'Tracking dimension group') }) } : {}),
        ...(!record.inventoryUnit ? { inventoryUnit: t('validation.required', { field: t('releasedProduct.fields.inventoryUnit', 'Inventory unit') }) } : {}),
        ...(!record.purchaseUnit ? { purchaseUnit: t('validation.required', { field: t('releasedProduct.fields.purchaseUnit', 'Purchase unit') }) } : {}),
        ...(!record.salesUnit ? { salesUnit: t('validation.required', { field: t('releasedProduct.fields.salesUnit', 'Sales unit') }) } : {}),
        ...(record.purchaseUnit && record.inventoryUnit && record.purchaseUnit !== record.inventoryUnit && record.purchaseToInventoryFactor <= 0 ? { purchaseToInventoryFactor: t('releasedProduct.validation.conversionFactor', 'A positive conversion factor is required when units differ.') } : {}),
        ...(record.salesUnit && record.inventoryUnit && record.salesUnit !== record.inventoryUnit && record.salesToInventoryFactor <= 0 ? { salesToInventoryFactor: t('releasedProduct.validation.conversionFactor', 'A positive conversion factor is required when units differ.') } : {}),
        ...(record.purchasePrice < 0 ? { purchasePrice: t('validation.nonNegative', 'Value cannot be negative.') } : {}),
        ...(record.salesPrice < 0 ? { salesPrice: t('validation.nonNegative', 'Value cannot be negative.') } : {}),
        ...(record.purchasePriceQuantity <= 0 ? { purchasePriceQuantity: t('releasedProduct.validation.priceQuantity', 'Price quantity must be greater than zero.') } : {}),
        ...(record.salesPriceQuantity <= 0 ? { salesPriceQuantity: t('releasedProduct.validation.priceQuantity', 'Price quantity must be greater than zero.') } : {}),
      }),
      presentation: { mode: 'list', listWidth: 270, listResizable: true },
      showAttachmentAction: false,
      showOptionsMenu: false,
    }),
    [currentCompany, currentLanguage.code, isRtl, lookups.data, productId, t, validating]
  );

  return (
    <>
      <ListDetailsPage
        variant="enterprise"
        title={t('releasedProduct.detailsTitle', 'Released product details')}
        config={config}
      />
      <Snackbar open={validation !== null || validationError !== null} autoHideDuration={validation?.isValid ? 6000 : null} onClose={() => { setValidation(null); setValidationError(null); }} anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}>
        <Alert severity={validationError || (validation && !validation.isValid) ? 'error' : validation?.warnings.length ? 'warning' : 'success'} onClose={() => { setValidation(null); setValidationError(null); }} sx={{ whiteSpace: 'pre-line', maxWidth: 720 }}>
          {validationError ?? (validation?.isValid
            ? validation.warnings.length
              ? `${t('releasedProduct.validation.readyWithWarnings', 'The released item setup is complete, with posting warnings:')}\n${validation.warnings.map((message) => `\u2022 ${message}`).join('\n')}`
              : t('releasedProduct.validation.ready', 'The released item is ready for purchase and sales order entry.')
            : validation?.errors.map((message) => `\u2022 ${message}`).join('\n'))}
        </Alert>
      </Snackbar>
    </>
  );
}
