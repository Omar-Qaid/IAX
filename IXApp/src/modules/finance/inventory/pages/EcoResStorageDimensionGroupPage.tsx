import React, { useMemo } from 'react';
import { Switch } from '@mui/material';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailValue, DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { storageDimensionGroupApi, type StorageDimensionGroupRecord } from '../api/storageDimensionGroupApi';

interface DimensionRow { id: 'site' | 'warehouse' | 'location'; name: string; active: boolean }
const emptyGroup = (): StorageDimensionGroupRecord => ({ id: `new-${crypto.randomUUID()}`, groupId: '', name: '', useSite: true, useWarehouse: true, useLocation: true });
const text = (value: DetailValue): string => String(value ?? '');

export function EcoResStorageDimensionGroupPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const config = useMemo<EnterpriseListDetailsConfig<StorageDimensionGroupRecord>>(() => ({
    recordTableName: 'EcoResStorageDimensionGroup', dataSource: { type: 'remote', key: 'inventory-storage-dimension-groups', load: (signal) => storageDimensionGroupApi.list(signal), create: (record) => storageDimensionGroupApi.create(record), update: (record) => storageDimensionGroupApi.update(record.id, record), delete: (record) => storageDimensionGroupApi.delete(record.id) },
    createRecord: emptyGroup, getPrimaryText: (record) => record.groupId, getSecondaryText: (record) => record.name,
    matchesSearch: (record, query) => `${record.groupId} ${record.name}`.toLocaleLowerCase().includes(query.trim().toLocaleLowerCase()),
    getValues: (): DetailValues => ({}), setValues: (record) => record,
    headerFields: [
      { id: 'groupId', label: t('storageDimensionGroup.fields.name', 'Name'), width: 180, getValue: (record) => record.groupId, setValue: (record, value) => ({ ...record, groupId: text(value) }) },
      { id: 'name', label: t('storageDimensionGroup.fields.description', 'Description'), width: 'minmax(320px, 520px)', getValue: (record) => record.name, setValue: (record, value) => ({ ...record, name: text(value) }) },
    ],
    sections: ({ record, editing, onRecordChange }) => {
      const rows: DimensionRow[] = [{ id: 'site', name: t('storageDimensionGroup.dimensions.site', 'Site'), active: record.useSite }, { id: 'warehouse', name: t('storageDimensionGroup.dimensions.warehouse', 'Warehouse'), active: record.useWarehouse }, { id: 'location', name: t('storageDimensionGroup.dimensions.location', 'Location'), active: record.useLocation }];
      const columns: ColumnDef<DimensionRow>[] = [{ field: 'name', headerName: t('storageDimensionGroup.fields.dimension', 'Storage dimension'), minWidth: 240, flex: 1 }, { field: 'active', headerName: t('storageDimensionGroup.fields.active', 'Active'), width: 130, align: 'center', headerAlign: 'center', renderCell: ({ row }) => <Switch size="small" checked={row.active} disabled={!editing || row.id === 'site' && (record.useWarehouse || record.useLocation) || row.id === 'warehouse' && record.useLocation} onChange={(_, checked) => { if (row.id === 'site') onRecordChange({ ...record, useSite: checked, useWarehouse: checked ? record.useWarehouse : false, useLocation: checked ? record.useLocation : false }); else if (row.id === 'warehouse') onRecordChange({ ...record, useWarehouse: checked, useSite: checked ? true : record.useSite, useLocation: checked ? record.useLocation : false }); else onRecordChange({ ...record, useLocation: checked, useWarehouse: checked ? true : record.useWarehouse, useSite: checked ? true : record.useSite }); }} /> }];
      return [{ id: 'storage-dimensions', title: t('storageDimensionGroup.sections.dimensions', 'Storage dimensions'), defaultExpanded: true, content: <DataGrid rows={rows} columns={columns} height={210} selectionMode="single" hideToolbar hideFooter hideSidebar hideFilterRow storageKey="inventory.storage-dimension-groups.dimensions" /> }];
    },
    permissions: { view: PERMISSIONS.INVENTORY_TRANSACTION_VIEW, create: PERMISSIONS.INVENTORY_TRANSACTION_CREATE, edit: PERMISSIONS.INVENTORY_TRANSACTION_EDIT, delete: PERMISSIONS.INVENTORY_TRANSACTION_DELETE },
    validate: (record) => ({ ...(!record.groupId.trim() ? { groupId: t('validation.required', { field: t('storageDimensionGroup.fields.name') }) } : {}), ...(!record.name.trim() ? { name: t('validation.required', { field: t('storageDimensionGroup.fields.description') }) } : {}) }),
    presentation: { mode: 'list', listWidth: 315, listResizable: true },
  }), [t]);
  return <ListDetailsPage variant="enterprise" title={t('storageDimensionGroup.title', 'Storage dimension groups')} config={config} />;
}
