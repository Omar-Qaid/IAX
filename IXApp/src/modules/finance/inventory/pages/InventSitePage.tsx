import React, { useMemo } from 'react';
import { Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { TreeControl, type TreeControlConfig } from '@shared/components/tree-control';
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

type SiteHierarchyNode =
  | { kind: 'site'; id: string; label: string; children: SiteHierarchyNode[] }
  | { kind: 'warehouse'; id: string; label: string; children: [] };

export function InventSitePage(): React.ReactElement {
  const { t } = useAppTranslation();
  const hierarchySites = useQuery({
    queryKey: ['inventory-sites'],
    queryFn: ({ signal }) => inventSiteApi.list(signal),
    staleTime: 300000,
  });
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
        const hierarchyNodes: SiteHierarchyNode[] = (hierarchySites.data ?? [record]).map(
          (site) => ({
            kind: 'site',
            id: `site:${site.id}`,
            label: `${site.siteId}, ${site.name}`,
            children: site.warehouses.map((warehouse) => ({
              kind: 'warehouse' as const,
              id: `warehouse:${warehouse.id}`,
              label: `${warehouse.inventLocationId}, ${warehouse.name}`,
              children: [] as [],
            })),
          })
        );
        const hierarchyConfig: TreeControlConfig<SiteHierarchyNode> = {
          getId: (node) => node.id,
          getLabel: (node) => node.label,
          getChildren: (node) => node.children,
          isBranch: (node) => node.kind === 'site',
          selectedId: `site:${record.id}`,
          expandAllLabel: t('actions.expand', 'Expand'),
          collapseAllLabel: t('actions.collapse', 'Collapse'),
          emptyChildrenLabel: t('inventSite.noWarehouses', 'No warehouses belong to this site.'),
          maxHeight: 198,
          ariaLabel: t('inventSite.hierarchy', 'Hierarchy'),
        };
        const hierarchy = (
          <TreeControl<SiteHierarchyNode> nodes={hierarchyNodes} config={hierarchyConfig} />
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
    [hierarchySites.data, t]
  );
  return (
    <ListDetailsPage variant="enterprise" title={t('inventSite.title', 'Sites')} config={config} />
  );
}
