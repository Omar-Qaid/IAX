import React, { useMemo } from 'react';
import { useParams } from 'react-router-dom';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailValue, DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { localizedName } from '@shared/utilities/localizedName';
import { ecoResProductApi, type EcoResProductInput, type EcoResProductRecord } from '../api/ecoResProductApi';

const text = (value: DetailValue | undefined): string => String(value ?? '');
const emptyProduct = (): EcoResProductRecord => ({ id: `new-${crypto.randomUUID()}`, productNumber: '', name: '', nameAlias: '', description: '', searchName: '', productType: 'Item', productSubtype: 'Product' });
const payload = (record: EcoResProductRecord): EcoResProductInput => ({ productNumber: record.productNumber.trim(), name: record.name.trim(), nameAlias: record.nameAlias.trim(), description: record.description.trim(), searchName: record.searchName.trim() });

export function EcoResDistinctProductDetailsPage(): React.ReactElement {
  const { t, isRtl } = useAppTranslation(); const { productId } = useParams<{ productId: string }>();
  const config = useMemo<EnterpriseListDetailsConfig<EcoResProductRecord>>(() => ({
    initialSelectedId: productId, recordTableName: 'EcoResProduct', filterStorageKey: 'inventory.shared-products.details-filter',
    dataSource: { type: 'remote', key: 'inventory-shared-products-details', load: (signal) => ecoResProductApi.list(signal), create: (record) => ecoResProductApi.create(payload(record)), update: (record) => ecoResProductApi.update(record.id, payload(record)), delete: (record) => ecoResProductApi.delete(record.id) },
    createRecord: emptyProduct, getPrimaryText: (record) => localizedName(record, isRtl), getSecondaryText: (record) => record.productNumber,
    matchesSearch: (record, query) => `${record.productNumber} ${record.name} ${record.nameAlias} ${record.searchName}`.toLocaleLowerCase().includes(query.trim().toLocaleLowerCase()),
    getValues: (record): DetailValues => ({ nameAlias: record.nameAlias, searchName: record.searchName, description: record.description, productType: record.productType, productSubtype: record.productSubtype }),
    setValues: (record, values) => ({ ...record, nameAlias: text(values.nameAlias), searchName: text(values.searchName), description: text(values.description) }),
    headerFields: [
      { id: 'productNumber', label: t('product.fields.productNumber', 'Product number'), width: 200, getValue: (record) => record.productNumber, setValue: (record, value) => ({ ...record, productNumber: text(value) }) },
      { id: 'name', label: t('product.fields.productName', 'Product name'), width: 'minmax(300px, 520px)', getValue: (record) => record.name, getDisplayValue: (record) => localizedName(record, isRtl), setValue: (record, value) => ({ ...record, name: text(value) }) },
    ],
    sections: [{ id: 'general', title: t('product.sections.general', 'General'), defaultExpanded: true, columns: 4, groups: [
      { id: 'identification', title: t('product.groups.identification', 'IDENTIFICATION'), fields: [{ name: 'nameAlias', label: t('product.fields.arabicName', 'Arabic name') }, { name: 'searchName', label: t('product.fields.searchName', 'Search name') }, { name: 'description', label: t('product.fields.description', 'Description'), multiline: true, rows: 4 }] },
      { id: 'classification', title: t('product.groups.classification', 'CLASSIFICATION'), fields: [{ name: 'productType', label: t('product.fields.productType', 'Product type'), type: 'display' }, { name: 'productSubtype', label: t('product.fields.productSubtype', 'Product subtype'), type: 'display' }] },
    ] }],
    permissions: { view: PERMISSIONS.INVENTORY_TRANSACTION_VIEW, create: PERMISSIONS.INVENTORY_TRANSACTION_CREATE, edit: PERMISSIONS.INVENTORY_TRANSACTION_EDIT, delete: PERMISSIONS.INVENTORY_TRANSACTION_DELETE },
    validate: (record) => ({ ...(!record.productNumber.trim() ? { productNumber: t('validation.required', { field: t('product.fields.productNumber') }) } : {}), ...(!record.name.trim() ? { name: t('validation.required', { field: t('product.fields.productName') }) } : {}), ...(!record.nameAlias.trim() ? { nameAlias: t('validation.required', { field: t('product.fields.arabicName') }) } : {}) }),
    presentation: { mode: 'list', listWidth: 300, listResizable: true },
  }), [isRtl, productId, t]);
  return <ListDetailsPage variant="enterprise" title={t('product.detailsTitle', 'Product details')} config={config} />;
}
