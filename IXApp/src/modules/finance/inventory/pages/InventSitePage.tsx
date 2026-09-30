import React, { useMemo } from 'react';
import { Box, Typography } from '@mui/material';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import WarehouseOutlinedIcon from '@mui/icons-material/WarehouseOutlined';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { inventSiteApi, type InventSiteRecord } from '../api/inventSiteApi';

const emptySite = (): InventSiteRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  siteId: '',
  name: '',
  defaultInventStatusId: '',
  timeZone: 0,
  isReceivingWarehouseOverrideAllowed: false,
  defaultDimension: 0,
  warehouses: [],
});

export function InventSitePage(): React.ReactElement {
  const { t } = useAppTranslation();
  const config = useMemo<EnterpriseListDetailsConfig<InventSiteRecord>>(
    () => ({
      recordTableName: 'InventSite',
      dataSource: {
        type: 'remote',
        key: 'inventory-sites',
        load: inventSiteApi.list,
        create: inventSiteApi.create,
        update: inventSiteApi.update,
        delete: inventSiteApi.delete,
      },
      createRecord: emptySite,
      getPrimaryText: (site) => site.siteId,
      getSecondaryText: (site) => site.name,
      matchesSearch: (site, query) =>
        `${site.siteId} ${site.name}`.toLowerCase().includes(query.toLowerCase()),
      getValues: (site): DetailValues => ({
        timeZone: String(site.timeZone),
        isReceivingWarehouseOverrideAllowed: site.isReceivingWarehouseOverrideAllowed,
        defaultInventStatusId: site.defaultInventStatusId,
        defaultDimension: site.defaultDimension,
      }),
      setValues: (site, values) => ({
        ...site,
        timeZone: Number(values.timeZone) || 0,
        isReceivingWarehouseOverrideAllowed: Boolean(values.isReceivingWarehouseOverrideAllowed),
        defaultInventStatusId: String(values.defaultInventStatusId ?? ''),
        defaultDimension: Number(values.defaultDimension) || 0,
      }),
      headerFields: [
        {
          id: 'siteId',
          label: t('inventSite.fields.site', 'Site'),
          getValue: (x) => x.siteId,
          setValue: (x, value) => ({ ...x, siteId: String(value).toUpperCase() }),
          disabled: false,
        },
        {
          id: 'name',
          label: t('inventSite.fields.name', 'Name'),
          getValue: (x) => x.name,
          setValue: (x, value) => ({ ...x, name: String(value) }),
        },
      ],
      sections: ({ record }) => {
        const hierarchy = (
          <Box sx={{ display: 'grid', gap: 0.75 }}>
            <Typography color="primary" variant="body2">
              {t('actions.expand', 'Expand')}
            </Typography>
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1,
                bgcolor: 'action.selected',
                px: 1,
                py: 0.5,
              }}
            >
              <KeyboardArrowDownIcon fontSize="small" />
              <strong>{record.siteId}</strong>
              <span>{record.name}</span>
            </Box>
            {record.warehouses.map((warehouse) => (
              <Box
                key={warehouse.id}
                sx={{ display: 'flex', alignItems: 'center', gap: 1, ps: 5, py: 0.25 }}
              >
                <WarehouseOutlinedIcon fontSize="small" color="action" />
                <Typography variant="body2" color="primary">
                  {warehouse.inventLocationId}
                </Typography>
                <Typography variant="body2">{warehouse.name}</Typography>
              </Box>
            ))}
            {!record.warehouses.length && (
              <Typography sx={{ ps: 5 }} color="text.secondary" variant="body2">
                {t('inventSite.noWarehouses', 'No warehouses belong to this site.')}
              </Typography>
            )}
          </Box>
        );
        return [
          {
            id: 'general',
            title: t('common.general', 'General'),
            defaultExpanded: true,
            columns: 2,
            groups: [
              {
                id: 'orderEntry',
                title: t('inventSite.orderEntry', 'Order entry'),
                fields: [
                  {
                    name: 'timeZone',
                    label: t('inventSite.fields.timeZone', 'Time zone'),
                    type: 'number',
                  },
                ],
              },
              {
                id: 'planning',
                title: t('inventSite.masterPlanning', 'Master planning'),
                fields: [
                  {
                    name: 'isReceivingWarehouseOverrideAllowed',
                    label: t(
                      'inventSite.fields.receivingOverride',
                      'Allow receiving warehouse override'
                    ),
                    type: 'boolean',
                  },
                ],
              },
            ],
          },
          {
            id: 'addresses',
            title: t('inventSite.addresses', 'Addresses'),
            content: (
              <Typography color="text.secondary">
                {t('inventSite.addressesHelp', 'No site address is configured.')}
              </Typography>
            ),
          },
          {
            id: 'financialDimensions',
            title: t('inventSite.financialDimensions', 'Financial dimensions'),
            groups: [
              {
                id: 'dimension',
                fields: [
                  {
                    name: 'defaultDimension',
                    label: t('inventSite.fields.defaultDimension', 'Default dimension'),
                    type: 'number',
                  },
                ],
              },
            ],
          },
          {
            id: 'hierarchy',
            title: t('inventSite.hierarchy', 'Hierarchy'),
            defaultExpanded: true,
            content: hierarchy,
          },
          {
            id: 'warehouse',
            title: t('inventSite.warehouse', 'Warehouse'),
            defaultExpanded: true,
            groups: [
              {
                id: 'defaults',
                fields: [
                  {
                    name: 'defaultInventStatusId',
                    label: t('inventSite.fields.defaultStatus', 'Default inventory status ID'),
                  },
                ],
              },
            ],
          },
        ] satisfies DetailSectionConfig[];
      },
    permissions: {
      view: PERMISSIONS.INVENTORY_TRANSACTION_VIEW,
      create: PERMISSIONS.INVENTORY_TRANSACTION_CREATE,
      edit: PERMISSIONS.INVENTORY_TRANSACTION_EDIT,
      delete: PERMISSIONS.INVENTORY_TRANSACTION_DELETE,
    },
      validate: (site) => ({
        ...(!site.siteId.trim()
          ? { siteId: t('validation.required', { field: t('inventSite.fields.site', 'Site') }) }
          : {}),
        ...(!site.name.trim()
          ? { name: t('validation.required', { field: t('inventSite.fields.name', 'Name') }) }
          : {}),
      }),
      advancedFilter: {
        fieldLabel: t('inventSite.fields.site', 'Site'),
        getValue: (site) => site.siteId,
        matches: (site, value) =>
          `${site.siteId} ${site.name}`.toLowerCase().includes(value.trim().toLowerCase()),
      },
      presentation: { mode: 'list', listWidth: 270, listResizable: true },
    }),
    [t]
  );
  return (
    <ListDetailsPage variant="enterprise" title={t('inventSite.title', 'Sites')} config={config} />
  );
}
