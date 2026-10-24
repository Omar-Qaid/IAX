import React, { useMemo } from 'react';
import { Switch } from '@mui/material';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import {
  trackingDimensionGroupApi,
  type TrackingDimensionGroupRecord,
} from '../api/trackingDimensionGroupApi';

interface DimensionRow {
  id: 'batch-number' | 'serial-number';
  name: string;
  active: boolean;
}

const emptyGroup = (): TrackingDimensionGroupRecord => ({
  id: `new-${crypto.randomUUID()}`,
  groupId: '',
  name: '',
  useBatchNumber: false,
  useSerialNumber: false,
});

const text = (value: DetailValue): string => String(value ?? '');

export function EcoResTrackingDimensionGroupPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const config = useMemo<EnterpriseListDetailsConfig<TrackingDimensionGroupRecord>>(
    () => ({
      recordTableName: 'EcoResTrackingDimensionGroup',
      dataSource: {
        type: 'remote',
        key: 'inventory-tracking-dimension-groups',
        load: (signal) => trackingDimensionGroupApi.list(signal),
        create: (record) => trackingDimensionGroupApi.create(record),
        update: (record) => trackingDimensionGroupApi.update(record.id, record),
        delete: (record) => trackingDimensionGroupApi.delete(record.id),
      },
      createRecord: emptyGroup,
      getPrimaryText: (record) => record.groupId,
      getSecondaryText: (record) => record.name,
      matchesSearch: (record, query) =>
        `${record.groupId} ${record.name}`
          .toLocaleLowerCase()
          .includes(query.trim().toLocaleLowerCase()),
      getValues: (): DetailValues => ({}),
      setValues: (record) => record,
      headerFields: [
        {
          id: 'groupId',
          label: t('trackingDimensionGroup.fields.name', 'Name'),
          width: 180,
          getValue: (record) => record.groupId,
          setValue: (record, value) => ({ ...record, groupId: text(value) }),
        },
        {
          id: 'name',
          label: t('trackingDimensionGroup.fields.description', 'Description'),
          width: 'minmax(320px, 520px)',
          getValue: (record) => record.name,
          setValue: (record, value) => ({ ...record, name: text(value) }),
        },
      ],
      sections: ({ record }) => {
        const rows: DimensionRow[] = [
          {
            id: 'batch-number',
            name: t('trackingDimensionGroup.dimensions.batchNumber', 'Batch number'),
            active: record.useBatchNumber,
          },
          {
            id: 'serial-number',
            name: t('trackingDimensionGroup.dimensions.serialNumber', 'Serial number'),
            active: record.useSerialNumber,
          },
        ];
        const columns: ColumnDef<DimensionRow>[] = [
          {
            field: 'name',
            headerName: t('trackingDimensionGroup.fields.dimension', 'Tracking dimension'),
            minWidth: 240,
            flex: 1,
          },
          {
            field: 'active',
            headerName: t('trackingDimensionGroup.fields.active', 'Active'),
            width: 130,
            align: 'center',
            headerAlign: 'center',
            renderCell: ({ row }) => (
              <Switch
                size="small"
                checked={row.active}
                disabled
              />
            ),
          },
        ];
        return [
          {
            id: 'tracking-dimensions',
            title: t('trackingDimensionGroup.sections.dimensions', 'Tracking dimensions'),
            defaultExpanded: true,
            content: (
              <DataGrid
                rows={rows}
                columns={columns}
                height={210}
                selectionMode="single"
                hideToolbar
                hideFooter
                hideSidebar
                hideFilterRow
                storageKey="inventory.tracking-dimension-groups.dimensions"
              />
            ),
          },
        ];
      },
      permissions: {
        view: PERMISSIONS.INVENTORY_TRANSACTION_VIEW,
        create: PERMISSIONS.INVENTORY_TRANSACTION_CREATE,
        edit: PERMISSIONS.INVENTORY_TRANSACTION_EDIT,
        delete: PERMISSIONS.INVENTORY_TRANSACTION_DELETE,
      },
      validate: (record) => ({
        ...(!record.groupId.trim()
          ? {
              groupId: t('validation.required', { field: t('trackingDimensionGroup.fields.name') }),
            }
          : {}),
        ...(!record.name.trim()
          ? {
              name: t('validation.required', {
                field: t('trackingDimensionGroup.fields.description'),
              }),
            }
          : {}),
        ...(record.useBatchNumber || record.useSerialNumber
          ? { useBatchNumber: 'Batch and serial tracking are outside basic inventory.' }
          : {}),
      }),
      presentation: { mode: 'list', listWidth: 315, listResizable: true },
    }),
    [t]
  );

  return (
    <ListDetailsPage
      variant="enterprise"
      title={t('trackingDimensionGroup.title', 'Tracking dimension groups')}
      config={config}
    />
  );
}
