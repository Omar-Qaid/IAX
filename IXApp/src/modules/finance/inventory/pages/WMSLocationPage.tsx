import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { inventLocationApi } from '../api/inventLocationApi';
import { wmsLocationApi, type WMSLocationRecord } from '../api/wmsLocationApi';

const createLocation = (): WMSLocationRecord => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, inventLocationId: '', warehouseName: '',
  wmsLocationId: '', inputLocation: '', checkText: '', locationType: 0, sortCode: 0,
});

export function WMSLocationPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const warehouses = useQuery({ queryKey: ['inventory', 'warehouse-lookups'], queryFn: ({ signal }) => inventLocationApi.lookups(signal) });
  const config = useMemo<EnterpriseListDetailsConfig<WMSLocationRecord>>(() => ({
    recordTableName: 'WMSLocation',
    dataSource: { type: 'remote', key: 'inventory-locations', load: wmsLocationApi.list,
      create: wmsLocationApi.create, update: wmsLocationApi.update, delete: wmsLocationApi.delete },
    createRecord: createLocation,
    getPrimaryText: (x) => x.wmsLocationId,
    getSecondaryText: (x) => `${x.inventLocationId} · ${x.warehouseName}`,
    matchesSearch: (x, query) => `${x.wmsLocationId} ${x.inventLocationId} ${x.warehouseName} ${x.checkText}`.toLowerCase().includes(query.toLowerCase()),
    getValues: (x): DetailValues => ({ inventLocationId: x.inventLocationId, inputLocation: x.inputLocation,
      checkText: x.checkText, locationType: String(x.locationType), sortCode: String(x.sortCode) }),
    setValues: (x, values) => ({ ...x, inventLocationId: String(values.inventLocationId ?? ''),
      inputLocation: String(values.inputLocation ?? ''), checkText: String(values.checkText ?? ''),
      locationType: Number(values.locationType) || 0, sortCode: Number(values.sortCode) || 0 }),
    headerFields: [{ id: 'wmsLocationId', label: t('wmsLocation.fields.location', 'Location'),
      getValue: (x) => x.wmsLocationId, setValue: (x, value) => ({ ...x, wmsLocationId: String(value).trim().toUpperCase() }) }],
    sections: [{ id: 'general', title: t('common.general', 'General'), defaultExpanded: true, columns: 2, groups: [
      { id: 'location', fields: [
        { name: 'inventLocationId', label: t('wmsLocation.fields.warehouse', 'Warehouse'), type: 'select',
          options: (warehouses.data?.warehouses ?? []).map((x) => ({ value: x.code, label: `${x.code} - ${x.name}` })) },
        { name: 'inputLocation', label: t('wmsLocation.fields.inputLocation', 'Destination location') },
        { name: 'checkText', label: t('wmsLocation.fields.checkText', 'Check text') },
        { name: 'locationType', label: t('wmsLocation.fields.locationType', 'Location type'), type: 'number' },
        { name: 'sortCode', label: t('wmsLocation.fields.sortCode', 'Sort code'), type: 'number' },
      ] },
    ] }],
    permissions: { view: PERMISSIONS.INVENTORY_TRANSACTION_VIEW, create: PERMISSIONS.INVENTORY_TRANSACTION_CREATE,
      edit: PERMISSIONS.INVENTORY_TRANSACTION_EDIT, delete: PERMISSIONS.INVENTORY_TRANSACTION_DELETE },
    validate: (x) => ({ ...(!x.inventLocationId.trim() ? { inventLocationId: t('validation.required', { field: 'Warehouse' }) } : {}),
      ...(!x.wmsLocationId.trim() ? { wmsLocationId: t('validation.required', { field: 'Location' }) } : {}),
      ...(x.wmsLocationId.length > 10 ? { wmsLocationId: 'Location ID cannot exceed 10 characters.' } : {}),
      ...(x.locationType < 0 ? { locationType: 'Location type cannot be negative.' } : {}),
      ...(x.sortCode < 0 ? { sortCode: 'Sort code cannot be negative.' } : {}) }),
    presentation: { mode: 'list', listWidth: 275, listResizable: true },
  }), [t, warehouses.data]);
  return <ListDetailsPage variant="enterprise" title={t('wmsLocation.title', 'Inventory locations')} config={config} />;
}
