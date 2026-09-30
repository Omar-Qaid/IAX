import React, { useMemo, useState } from 'react';
import { Box, Button, ButtonBase, Collapse, Typography } from '@mui/material';
import KeyboardArrowRightIcon from '@mui/icons-material/KeyboardArrowRight';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import { useQuery } from '@tanstack/react-query';
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

interface SiteHierarchyProps {
  sites: InventSiteRecord[];
  selectedSiteId: string;
  expandLabel: string;
  collapseLabel: string;
  noWarehousesLabel: string;
}

function SiteHierarchy({
  sites,
  selectedSiteId,
  expandLabel,
  collapseLabel,
  noWarehousesLabel,
}: SiteHierarchyProps): React.ReactElement {
  const [expandedSiteIds, setExpandedSiteIds] = useState<Set<string>>(() => new Set());

  const allExpanded = sites.length > 0 && sites.every((site) => expandedSiteIds.has(site.id));
  const toggleAll = () =>
    setExpandedSiteIds(allExpanded ? new Set() : new Set(sites.map((site) => site.id)));
  const toggleSite = (siteId: string) =>
    setExpandedSiteIds((current) => {
      const next = new Set(current);
      if (next.has(siteId)) next.delete(siteId);
      else next.add(siteId);
      return next;
    });

  return (
    <Box sx={{ display: 'grid', gap: 0.25 }}>
      <Box>
        <Button
          onClick={toggleAll}
          size="small"
          sx={{ minWidth: 0, px: 0.5, py: 0.25, textTransform: 'none', fontSize: 12 }}
        >
          {allExpanded ? collapseLabel : expandLabel}
        </Button>
      </Box>
      <Box sx={{ maxHeight: 198, overflowY: 'auto', pe: 0.5 }}>
        {sites.map((site) => {
          const expanded = expandedSiteIds.has(site.id);
          const selected = site.id === selectedSiteId;
          const contentId = `site-hierarchy-${site.id}`;
          return (
            <Box key={site.id}>
              <ButtonBase
                aria-controls={contentId}
                aria-expanded={expanded}
                onClick={() => toggleSite(site.id)}
                sx={{
                  display: 'flex',
                  justifyContent: 'flex-start',
                  width: '100%',
                  minHeight: 28,
                  gap: 0.75,
                  px: 0.5,
                  bgcolor: selected ? '#dbe7fb' : undefined,
                  textAlign: 'start',
                  '&:hover': { bgcolor: selected ? '#dbe7fb' : 'action.hover' },
                }}
              >
                {expanded ? (
                  <KeyboardArrowDownIcon sx={{ fontSize: 16 }} />
                ) : (
                  <KeyboardArrowRightIcon sx={{ fontSize: 16 }} />
                )}
                <Typography component="span" variant="body2" sx={{ fontSize: 13 }}>
                  {site.siteId}, {site.name}
                </Typography>
              </ButtonBase>
              <Collapse in={expanded} timeout="auto" unmountOnExit>
                <Box id={contentId}>
                  {site.warehouses.map((warehouse) => (
                    <Typography
                      key={warehouse.id}
                      variant="body2"
                      sx={{
                        minHeight: 28,
                        display: 'flex',
                        alignItems: 'center',
                        marginInlineStart: '10px',
                        paddingInlineStart: '34px',
                        fontSize: 13,
                        cursor: 'default',
                        '&:hover': { bgcolor: 'action.hover' },
                      }}
                    >
                      {warehouse.inventLocationId}, {warehouse.name}
                    </Typography>
                  ))}
                  {!site.warehouses.length && (
                    <Typography
                      sx={{
                        minHeight: 28,
                        display: 'flex',
                        alignItems: 'center',
                        marginInlineStart: '10px',
                        paddingInlineStart: '34px',
                        fontSize: 13,
                      }}
                      color="text.secondary"
                      variant="body2"
                    >
                      {noWarehousesLabel}
                    </Typography>
                  )}
                </Box>
              </Collapse>
            </Box>
          );
        })}
      </Box>
    </Box>
  );
}

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
        const hierarchy = (
          <SiteHierarchy
            sites={hierarchySites.data ?? [record]}
            selectedSiteId={record.id}
            expandLabel={t('actions.expand', 'Expand')}
            collapseLabel={t('actions.collapse', 'Collapse')}
            noWarehousesLabel={t('inventSite.noWarehouses', 'No warehouses belong to this site.')}
          />
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
