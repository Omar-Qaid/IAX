import React, { useMemo } from 'react';
import { Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { LookupField } from '@shared/components/lookups/LookupField';
import type { LookupOption } from '@shared/components/lookups/types';
import { TreeControl } from '@shared/components/tree-control';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { inventLocationApi, type InventLocationRecord } from '../api/inventLocationApi';
import { INVENTORY_ROUTE_PATHS } from '../routes/inventoryRoutePaths';

type InventoryHierarchyNode =
  | { kind: 'site'; id: string; label: string; children: InventoryHierarchyNode[] }
  | { kind: 'warehouse'; id: string; label: string; children: [] };

const inventoryHierarchyNodes = (
  sites: readonly LookupOption[],
  warehouses: readonly (LookupOption & { siteId?: string })[]
): InventoryHierarchyNode[] =>
  sites.map((site) => ({
    kind: 'site',
    id: `site:${String(site.id)}`,
    label: `${site.code}, ${site.name}`,
    children: warehouses
      .filter((warehouse) => warehouse.siteId === String(site.id))
      .map((warehouse) => ({
        kind: 'warehouse' as const,
        id: `warehouse:${String(warehouse.id)}`,
        label: `${warehouse.code}, ${warehouse.name}`,
        children: [] as [],
      })),
  }));

const empty = (): InventLocationRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  inventLocationId: '',
  name: '',
  inventSiteId: '',
  inventLocationType: 0,
  inventLocationLevel: 0,
  inventLocationIdTransit: '',
  inventLocationIdQuarantine: '',
  inventLocationIdReqMain: '',
  itmInventLocationIdGit: '',
  itmInventLocationIdUnder: '',
  vendAccount: '',
  workflowApproval: false,
  manual: false,
  reqRefill: false,
  wmsLocationIdDefaultReceipt: '',
  wmsLocationIdDefaultIssue: '',
  defaultProductionInputLocation: '',
  defaultProductionFinishGoodsLocation: '',
  defaultKanbanFinishedGoodsLocation: '',
  defaultReturnCreditOnlyLocation: '',
  defaultStatusId: '',
  wmsRackFormat: '',
  wmsLevelFormat: '',
  wmsPositionFormat: '',
  whsEnabled: false,
  warehouseAutoReleaseReservation: false,
  autoUpdateShipment: false,
  reserveAtLoadPost: false,
  decrementLoadLine: false,
  printBolBeforeShipConfirm: false,
  cycleCountAllowPalletMove: false,
  allowLaborStandards: false,
  allowMarkingReservationRemoval: false,
  useWmsOrders: false,
  wmsAisleNameActive: false,
  wmsRackNameActive: false,
  wmsLevelNameActive: false,
  wmsPositionNameActive: false,
  uniqueCheckDigits: false,
  enableQualityManagement: false,
  removeInventBlockingOnStatusChange: false,
  prodReserveOnlyWhse: false,
  whsProdOrderBackflushMustUseReservedQty: false,
  fshStore: false,
  consolidateShipAtRtw: false,
  retailInventNegPhysical: false,
  retailInventNegFinancial: false,
  enableExternalWarehouse: false,
  maxPickingRouteTime: 0,
  pickingLineTime: 0,
});
export function InventLocationPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const lookups = useQuery({
    queryKey: ['inventory-location-lookups'],
    queryFn: ({ signal }) => inventLocationApi.lookups(signal),
    staleTime: 300000,
  });
  const lookupField = (
    name: string,
    label: string,
    options: 'sites' | 'warehouses',
    record?: InventLocationRecord
  ) => ({
    name,
    label,
    renderOwnLabel: true,
    render: ({
      value,
      editing,
      disabled,
      onChange,
    }: {
      value: string | number | boolean | undefined;
      editing: boolean;
      disabled: boolean;
      onChange: (value: string) => void;
    }) => {
      const lookupOptions =
        options === 'sites'
          ? (lookups.data?.sites ?? [])
          : (lookups.data?.warehouses ?? []).filter(
              (option) =>
                Boolean(record?.inventSiteId) &&
                option.siteId === record?.inventSiteId &&
                option.id !== record?.inventLocationId
            );
      return (
        <LookupField
          name={name}
          masterRoute={options === 'sites' ? INVENTORY_ROUTE_PATHS.SITES : INVENTORY_ROUTE_PATHS.WAREHOUSES}
          label={label}
          value={String(value ?? '')}
          options={lookupOptions}
          displayMode="select"
          searchable
          lazyLoading={false}
          readOnly={!editing}
          disabled={disabled || lookups.isLoading}
          onChange={(v) => onChange(String(v ?? ''))}
        />
      );
    },
  });
  const fields = [
    'inventSiteId',
    'inventLocationType',
    'inventLocationLevel',
    'inventLocationIdTransit',
    'inventLocationIdQuarantine',
    'inventLocationIdReqMain',
    'itmInventLocationIdGit',
    'itmInventLocationIdUnder',
    'vendAccount',
    'workflowApproval',
    'manual',
    'reqRefill',
    'wmsLocationIdDefaultReceipt',
    'wmsLocationIdDefaultIssue',
    'defaultProductionInputLocation',
    'defaultProductionFinishGoodsLocation',
    'defaultKanbanFinishedGoodsLocation',
    'defaultReturnCreditOnlyLocation',
    'defaultStatusId',
    'wmsRackFormat',
    'wmsLevelFormat',
    'wmsPositionFormat',
    'whsEnabled',
    'warehouseAutoReleaseReservation',
    'autoUpdateShipment',
    'reserveAtLoadPost',
    'decrementLoadLine',
    'printBolBeforeShipConfirm',
    'cycleCountAllowPalletMove',
    'allowLaborStandards',
    'allowMarkingReservationRemoval',
    'useWmsOrders',
    'wmsAisleNameActive',
    'wmsRackNameActive',
    'wmsLevelNameActive',
    'wmsPositionNameActive',
    'uniqueCheckDigits',
    'enableQualityManagement',
    'removeInventBlockingOnStatusChange',
    'prodReserveOnlyWhse',
    'whsProdOrderBackflushMustUseReservedQty',
    'fshStore',
    'consolidateShipAtRtw',
    'retailInventNegPhysical',
    'retailInventNegFinancial',
    'enableExternalWarehouse',
    'maxPickingRouteTime',
    'pickingLineTime',
  ] as const;
  const config = useMemo<EnterpriseListDetailsConfig<InventLocationRecord>>(
    () => ({
      recordTableName: 'InventLocation',
      dataSource: {
        type: 'remote',
        key: 'inventory-locations',
        load: inventLocationApi.list,
        create: inventLocationApi.create,
        update: inventLocationApi.update,
        delete: inventLocationApi.delete,
      },
      createRecord: empty,
      getPrimaryText: (x) => x.inventLocationId,
      getSecondaryText: (x) => x.name,
      matchesSearch: (x, q) =>
        `${x.inventLocationId} ${x.name}`.toLowerCase().includes(q.toLowerCase()),
      getValues: (x) => Object.fromEntries(fields.map((k) => [k, x[k]])) as DetailValues,
      setValues: (x, v) => {
        const next = {
          ...x,
          ...Object.fromEntries(
            fields.map((k) => [
              k,
              typeof x[k] === 'boolean'
                ? Boolean(v[k])
                : typeof x[k] === 'number'
                  ? Number(v[k]) || 0
                  : String(v[k] ?? ''),
            ])
          ),
        } as InventLocationRecord;
        if (next.inventSiteId !== x.inventSiteId) {
          next.inventLocationIdTransit = '';
          next.inventLocationIdQuarantine = '';
          next.inventLocationIdReqMain = '';
          next.itmInventLocationIdGit = '';
          next.itmInventLocationIdUnder = '';
        }
        return next;
      },
      headerFields: [
        {
          id: 'inventLocationId',
          label: t('inventLocation.fields.warehouse', 'Warehouse'),
          getValue: (x) => x.inventLocationId,
          setValue: (x, v) => ({ ...x, inventLocationId: String(v).toUpperCase() }),
        },
        {
          id: 'name',
          label: t('inventLocation.fields.name', 'Name'),
          getValue: (x) => x.name,
          setValue: (x, v) => ({ ...x, name: String(v) }),
        },
      ],
      sections: ({ record }) =>
        [
          {
            id: 'general',
            title: t('common.general', 'General'),
            defaultExpanded: true,
            columns: 4,
            groups: [
              {
                id: 'identity',
                fields: [
                  lookupField('inventSiteId', t('inventLocation.fields.site', 'Site'), 'sites'),
                  {
                    name: 'inventLocationType',
                    label: t('inventLocation.fields.type', 'Type'),
                    type: 'select',
                    options: [
                      { value: '0', label: t('common.default', 'Default') },
                      { value: '1', label: 'Quarantine' },
                      { value: '2', label: 'Transit' },
                      { value: '3', label: 'Vendor' },
                    ],
                  },
                ],
              },
              {
                id: 'related',
                fields: [
                  lookupField(
                    'inventLocationIdQuarantine',
                    'Quarantine warehouse',
                    'warehouses',
                    record
                  ),
                  lookupField('inventLocationIdTransit', 'Transit warehouse', 'warehouses', record),
                ],
              },
              {
                id: 'delivery',
                fields: [
                  lookupField(
                    'itmInventLocationIdGit',
                    'Goods in transit warehouse',
                    'warehouses',
                    record
                  ),
                  lookupField(
                    'itmInventLocationIdUnder',
                    'Under delivery warehouse',
                    'warehouses',
                    record
                  ),
                ],
              },
              {
                id: 'reference',
                title: 'Reference',
                fields: [
                  { name: 'vendAccount', label: 'Vendor account' },
                  { name: 'workflowApproval', label: 'Active approval workflow', type: 'boolean' },
                ],
              },
            ],
          },
          {
            id: 'planning',
            title: 'Master planning',
            defaultExpanded: true,
            columns: 3,
            groups: [
              {
                id: 'coverage',
                title: 'Item coverage',
                fields: [{ name: 'manual', label: 'Manual', type: 'boolean' }],
              },
              {
                id: 'main',
                title: 'Main warehouse',
                fields: [{ name: 'reqRefill', label: 'Refilling', type: 'boolean' }],
              },
              {
                id: 'level',
                fields: [
                  lookupField('inventLocationIdReqMain', 'Main warehouse', 'warehouses', record),
                  { name: 'inventLocationLevel', label: 'Warehouse level', type: 'number' },
                ],
              },
            ],
          },
          {
            id: 'management',
            title: 'Inventory and warehouse management',
            defaultExpanded: true,
            columns: 4,
            groups: [
              {
                id: 'receipt',
                fields: [
                  { name: 'wmsLocationIdDefaultReceipt', label: 'Default receipt location' },
                  { name: 'wmsLocationIdDefaultIssue', label: 'Default issue location' },
                  { name: 'defaultStatusId', label: 'Default inventory status' },
                ],
              },
              {
                id: 'production',
                fields: [
                  {
                    name: 'defaultProductionInputLocation',
                    label: 'Default production input location',
                  },
                  {
                    name: 'defaultProductionFinishGoodsLocation',
                    label: 'Default production finished goods location',
                  },
                  {
                    name: 'defaultKanbanFinishedGoodsLocation',
                    label: 'Default kanban finished goods location',
                  },
                ],
              },
              {
                id: 'returns',
                fields: [
                  {
                    name: 'defaultReturnCreditOnlyLocation',
                    label: 'Default location for credit only returns',
                  },
                ],
              },
            ],
          },
          {
            id: 'locationNames',
            title: 'Location names',
            groups: [
              {
                id: 'formats',
                fields: [
                  { name: 'wmsAisleNameActive', label: 'Include aisle', type: 'boolean' },
                  { name: 'wmsRackNameActive', label: 'Include rack', type: 'boolean' },
                  { name: 'wmsRackFormat', label: 'Rack format' },
                  { name: 'wmsLevelNameActive', label: 'Include level', type: 'boolean' },
                  { name: 'wmsLevelFormat', label: 'Level format' },
                  { name: 'wmsPositionNameActive', label: 'Include position', type: 'boolean' },
                  { name: 'wmsPositionFormat', label: 'Position format' },
                  { name: 'uniqueCheckDigits', label: 'Use unique check digits', type: 'boolean' },
                ],
              },
            ],
          },
          {
            id: 'addresses',
            title: 'Addresses',
            content: (
              <Typography color="text.secondary">
                No warehouse address association is configured.
              </Typography>
            ),
          },
          {
            id: 'hierarchy',
            title: 'Hierarchy',
            defaultExpanded: true,
            content: (
              <TreeControl<InventoryHierarchyNode>
                key={record.id}
                nodes={inventoryHierarchyNodes(
                  lookups.data?.sites ?? [],
                  lookups.data?.warehouses ?? []
                )}
                config={{
                  getId: (node) => node.id,
                  getLabel: (node) => node.label,
                  getChildren: (node) => node.children,
                  isBranch: (node) => node.kind === 'site',
                  selectedId: `warehouse:${record.inventLocationId}`,
                  initialExpandedIds: record.inventSiteId ? [`site:${record.inventSiteId}`] : [],
                  expandAllLabel: t('actions.expand', 'Expand'),
                  collapseAllLabel: t('actions.collapse', 'Collapse'),
                  emptyChildrenLabel: 'No warehouses belong to this site.',
                  maxHeight: 198,
                  ariaLabel: 'Warehouse hierarchy',
                }}
              />
            ),
          },
          {
            id: 'retail',
            title: 'Retail',
            groups: [
              {
                id: 'retailOptions',
                fields: [
                  { name: 'fshStore', label: 'Retail store', type: 'boolean' },
                  {
                    name: 'consolidateShipAtRtw',
                    label: 'Consolidate ship at warehouse',
                    type: 'boolean',
                  },
                  {
                    name: 'retailInventNegPhysical',
                    label: 'Physical negative inventory',
                    type: 'boolean',
                  },
                  {
                    name: 'retailInventNegFinancial',
                    label: 'Financial negative inventory',
                    type: 'boolean',
                  },
                ],
              },
            ],
          },
          {
            id: 'warehouse',
            title: 'Warehouse',
            defaultExpanded: true,
            columns: 5,
            groups: [
              {
                id: 'processes',
                fields: [
                  {
                    name: 'whsEnabled',
                    label: 'Use warehouse management processes',
                    type: 'boolean',
                  },
                  {
                    name: 'printBolBeforeShipConfirm',
                    label: 'Print BOL before confirming shipment',
                    type: 'boolean',
                  },
                  { name: 'autoUpdateShipment', label: 'Auto update shipment', type: 'boolean' },
                  {
                    name: 'prodReserveOnlyWhse',
                    label: 'Reserve only warehouse in production',
                    type: 'boolean',
                  },
                ],
              },
              {
                id: 'release',
                fields: [
                  {
                    name: 'reserveAtLoadPost',
                    label: 'Reserve inventory at load posting',
                    type: 'boolean',
                  },
                  {
                    name: 'warehouseAutoReleaseReservation',
                    label: 'Reserve when orders are released',
                    type: 'boolean',
                  },
                ],
              },
              {
                id: 'counting',
                title: 'Cycle counting',
                fields: [
                  {
                    name: 'cycleCountAllowPalletMove',
                    label: 'Allow license plate moves',
                    type: 'boolean',
                  },
                  { name: 'uniqueCheckDigits', label: 'Use unique check digits', type: 'boolean' },
                  { name: 'decrementLoadLine', label: 'Decrement load line', type: 'boolean' },
                  { name: 'defaultStatusId', label: 'Default inventory status ID' },
                ],
              },
              {
                id: 'quality',
                title: 'Inventory status change',
                fields: [
                  {
                    name: 'allowMarkingReservationRemoval',
                    label: 'Remove reservations and markings',
                    type: 'boolean',
                  },
                  {
                    name: 'removeInventBlockingOnStatusChange',
                    label: 'Remove inventory blocking',
                    type: 'boolean',
                  },
                  { name: 'allowLaborStandards', label: 'Allow labor standards', type: 'boolean' },
                  {
                    name: 'enableQualityManagement',
                    label: 'Enable quality management',
                    type: 'boolean',
                  },
                ],
              },
              {
                id: 'production',
                title: 'Production orders',
                fields: [
                  {
                    name: 'whsProdOrderBackflushMustUseReservedQty',
                    label: 'Backflush must use reserved quantities',
                    type: 'boolean',
                  },
                  {
                    name: 'enableExternalWarehouse',
                    label: 'Enable external warehouse',
                    type: 'boolean',
                  },
                ],
              },
            ],
          },
          {
            id: 'picking',
            title: 'Picking workbench',
            defaultExpanded: true,
            groups: [
              {
                id: 'limits',
                fields: [
                  {
                    name: 'maxPickingRouteTime',
                    label: 'Maximum lines per picking list',
                    type: 'number',
                  },
                  { name: 'pickingLineTime', label: 'Pick lists per batch', type: 'number' },
                ],
              },
            ],
          },
        ] as DetailSectionConfig[],
      permissions: {
        view: PERMISSIONS.INVENTORY_TRANSACTION_VIEW,
        create: PERMISSIONS.INVENTORY_TRANSACTION_CREATE,
        edit: PERMISSIONS.INVENTORY_TRANSACTION_EDIT,
        delete: PERMISSIONS.INVENTORY_TRANSACTION_DELETE,
      },
      validate: (x) => ({
        ...(!x.inventLocationId.trim() ? { inventLocationId: 'Warehouse is required.' } : {}),
        ...(!x.name.trim() ? { name: 'Name is required.' } : {}),
        ...(!x.inventSiteId.trim() ? { inventSiteId: 'Site is required.' } : {}),
      }),
      advancedFilter: {
        fieldLabel: 'Warehouse',
        getValue: (x) => x.inventLocationId,
        matches: (x, v) =>
          `${x.inventLocationId} ${x.name}`.toLowerCase().includes(v.toLowerCase()),
      },
      presentation: { mode: 'list', listWidth: 270, listResizable: true },
    }),
    [lookups.data, lookups.isLoading, t]
  );
  return (
    <ListDetailsPage
      variant="enterprise"
      title={t('inventLocation.title', 'Warehouses')}
      config={config}
    />
  );
}
