import React, { useMemo } from 'react';
import { FastTabsDrawer } from '@patterns/drawer-fast-tabs';
import type { FastTabSection, FastTabValue } from '@patterns/dialog-fast-tabs/FastTabsDialog';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ecoResProductApi, type EcoResProductRecord } from '../api/ecoResProductApi';

interface Props { open: boolean; onClose: () => void; onCreated: (product: EcoResProductRecord) => void | Promise<void> }
const initialValues = (): Record<string, FastTabValue> => ({ productType: 'Item', productSubtype: 'Product', productNumber: '', name: '', nameAlias: '', searchName: '', description: '' });

export function EcoResProductCreateDrawer({ open, onClose, onCreated }: Props): React.ReactElement {
  const { t } = useAppTranslation();
  const sections = useMemo<FastTabSection[]>(() => [{ id: 'identification', title: t('product.groups.identification', 'IDENTIFICATION'), fields: [
    { name: 'productType', label: t('product.fields.productType', 'Product type'), type: 'select', disabled: true, options: [{ value: 'Item', label: t('product.options.item', 'Item') }] },
    { name: 'productSubtype', label: t('product.fields.productSubtype', 'Product subtype'), type: 'select', disabled: true, options: [{ value: 'Product', label: t('product.options.product', 'Product') }] },
    { name: 'productNumber', label: t('product.fields.productNumber', 'Product number'), required: true },
    { name: 'name', label: t('product.fields.productName', 'Product name'), required: true },
    { name: 'nameAlias', label: t('product.fields.arabicName', 'Arabic name'), required: true },
    { name: 'searchName', label: t('product.fields.searchName', 'Search name') },
    { name: 'description', label: t('product.fields.description', 'Description'), type: 'multiline', rows: 3 },
  ] }], [t]);
  return <FastTabsDrawer open={open} resetKey={open ? 'product-open' : 'product-closed'} title={t('product.createTitle', 'New product')} viewLabel={t('common.standardView')} sections={sections} initialValues={initialValues}
    validate={(values) => { const errors: Record<string, string> = {}; for (const field of ['productNumber', 'name', 'nameAlias']) if (!String(values[field] ?? '').trim()) errors[field] = t('validation.required', { field: t(`product.fields.${field}`) }); return errors; }}
    onSubmit={async (values) => onCreated(await ecoResProductApi.create({ productNumber: String(values.productNumber), name: String(values.name), nameAlias: String(values.nameAlias), searchName: String(values.searchName), description: String(values.description) }))}
    saveLabel={t('actions.save')} cancelLabel={t('actions.cancel')} closeLabel={t('actions.close')} helpLabel={t('common.help')} onCancel={onClose} />;
}
