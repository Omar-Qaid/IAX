import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Alert, Box, MenuItem, TextField, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsTreePage } from '@patterns/list-details-tree';
import type { ListDetailsTreePageConfig } from '@patterns/list-details-tree';
import type { DetailValue, DetailValues } from '@patterns/list-details/types';
import { LookupField } from '@shared/components/lookups/LookupField';
import { localizedName } from '@shared/utilities/localizedName';
import {
  organizationStructureApi as api,
  type OrganizationHierarchy,
  type OrganizationHierarchyNode,
  type OrganizationUnit,
} from '../api/organizationStructureApi';

interface HierarchyNodeRecord {
  id: string;
  recordId: number;
  hierarchyId: number;
  organizationUnitId: number;
  parentNodeId: number | null;
  validFrom: string;
  validTo: string | null;
  code: string;
  name: string;
  nameAlias?: string | null;
}

const today = () => new Date().toISOString().slice(0, 10);
const numberValue = (value: DetailValue | undefined): number => Number(value) || 0;
const textValue = (value: DetailValue | undefined): string => String(value ?? '');

interface OrganizationHierarchyNodesPageProps {
  initialHierarchyId?: number;
  onHierarchyChange?: (id: number) => void;
  onExit?: () => void;
}

export function OrganizationHierarchyNodesPage({
  initialHierarchyId,
  onHierarchyChange,
  onExit,
}: OrganizationHierarchyNodesPageProps = {}): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  return (
    <OrganizationHierarchyNodesContent
      key={company}
      company={company}
      initialHierarchyId={initialHierarchyId}
      onHierarchyChange={onHierarchyChange}
      onExit={onExit}
    />
  );
}

function OrganizationHierarchyNodesContent({
  company,
  initialHierarchyId,
  onHierarchyChange,
  onExit,
}: {
  company: string;
  initialHierarchyId?: number;
  onHierarchyChange?: (id: number) => void;
  onExit?: () => void;
}): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const navigate = useNavigate();
  const { hierarchyId: routeHierarchyId } = useParams<{ hierarchyId: string }>();
  const asOf = today();
  const hierarchies = useQuery({
    queryKey: ['organization-structure', company, 'hierarchies'],
    queryFn: ({ signal }) => api.hierarchies(signal),
  });
  const units = useQuery({
    queryKey: ['organization-structure', company, 'units', asOf],
    queryFn: ({ signal }) => api.units(asOf, signal),
  });
  const requestedHierarchyId = initialHierarchyId ?? (Number(routeHierarchyId) || 0);
  const hierarchyId = hierarchies.data?.some((hierarchy) => hierarchy.id === requestedHierarchyId)
    ? requestedHierarchyId
    : (hierarchies.data?.[0]?.id ?? 0);

  useEffect(() => {
    if (!hierarchyId || hierarchyId === requestedHierarchyId) return;
    if (onHierarchyChange) onHierarchyChange(hierarchyId);
    else
      navigate(`/organization-administration/organization-hierarchies/${hierarchyId}/nodes`, {
        replace: true,
      });
  }, [hierarchyId, navigate, onHierarchyChange, requestedHierarchyId]);

  if (hierarchies.error || units.error) {
    return (
      <Alert severity="error">
        {(hierarchies.error ?? units.error)?.message ?? t('common.error')}
      </Alert>
    );
  }
  if (hierarchies.isLoading || units.isLoading) {
    return <Typography sx={{ p: 2 }}>{t('common.loading')}</Typography>;
  }
  if (!hierarchies.data?.length) {
    return <Typography sx={{ p: 2 }}>{t('common.noData')}</Typography>;
  }

  return (
    <HierarchyNodesTreePage
      key={hierarchyId}
      company={company}
      asOf={asOf}
      hierarchyId={hierarchyId}
      hierarchies={hierarchies.data}
      units={units.data ?? []}
      isRtl={isRtl}
      onHierarchyChange={(id) => {
        if (onHierarchyChange) onHierarchyChange(id);
        else navigate(`/organization-administration/organization-hierarchies/${id}/nodes`);
      }}
      onExit={onExit}
    />
  );
}

function HierarchyNodesTreePage({
  company,
  asOf,
  hierarchyId,
  hierarchies,
  units,
  isRtl,
  onHierarchyChange,
  onExit,
}: {
  company: string;
  asOf: string;
  hierarchyId: number;
  hierarchies: OrganizationHierarchy[];
  units: OrganizationUnit[];
  isRtl: boolean;
  onHierarchyChange: (id: number) => void;
  onExit?: () => void;
}): React.ReactElement {
  const { t } = useAppTranslation();
  const [nodes, setNodes] = useState<HierarchyNodeRecord[]>([]);
  const unitsById = useMemo(() => new Map(units.map((unit) => [unit.id, unit])), [units]);
  const toRecord = (node: OrganizationHierarchyNode): HierarchyNodeRecord => {
    const unit = unitsById.get(node.organizationUnitId);
    return {
      ...node,
      id: String(node.id),
      recordId: node.id,
      validFrom: node.validFrom.split('T')[0],
      validTo: node.validTo ? node.validTo.split('T')[0] : null,
      code: unit?.code ?? String(node.organizationUnitId),
      name: unit?.name ?? t('organizationStructure.unitReference', { id: node.organizationUnitId }),
      nameAlias: unit?.nameAlias,
    };
  };
  const emptyNode = (): HierarchyNodeRecord => ({
    id: `new-${crypto.randomUUID()}`,
    recordId: 0,
    hierarchyId,
    organizationUnitId: 0,
    parentNodeId: null,
    validFrom: asOf,
    validTo: null,
    code: '',
    name: '',
  });
  const config: ListDetailsTreePageConfig<HierarchyNodeRecord> = {
    recordTableName: 'OrganizationHierarchyNode',
    filterStorageKey: `organization-hierarchy-nodes-${hierarchyId}`,
    dataSource: {
      type: 'remote',
      key: `organization-hierarchy-nodes-${company}-${hierarchyId}-${asOf}`,
      load: async (signal) => {
        const loaded = (await api.nodes(hierarchyId, asOf, signal)).map(toRecord);
        setNodes(loaded);
        return loaded;
      },
      create: async (record) => {
        const recordId = await api.createNode({
          hierarchyId,
          organizationUnitId: record.organizationUnitId,
          parentNodeId: record.parentNodeId,
          validFrom: record.validFrom,
          validTo: record.validTo,
        });
        const created = { ...record, id: String(recordId), recordId };
        setNodes((current) => [created, ...current]);
        return created;
      },
      update: async (record) => {
        await api.updateNode(record.recordId, {
          organizationUnitId: record.organizationUnitId,
          parentNodeId: record.parentNodeId,
          validFrom: record.validFrom,
          validTo: record.validTo,
        });
        setNodes((current) =>
          current.map((node) => (node.recordId === record.recordId ? record : node))
        );
        return record;
      },
      delete: async (record) => {
        await api.closeNode(record.recordId, asOf);
        setNodes((current) => current.filter((node) => node.recordId !== record.recordId));
      },
    },
    createRecord: emptyNode,
    getPrimaryText: (record) => localizedName(record, isRtl),
    getSecondaryText: (record) => record.code,
    matchesSearch: (record, query) =>
      `${record.code} ${record.name} ${record.nameAlias ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      organizationUnitId: record.organizationUnitId || '',
      parentNodeId: record.parentNodeId ?? '',
      validFrom: record.validFrom,
      validTo: record.validTo ?? '',
    }),
    setValues: (record, values) => {
      const organizationUnitId = numberValue(values.organizationUnitId);
      const unit = unitsById.get(organizationUnitId);
      return {
        ...record,
        organizationUnitId,
        parentNodeId: values.parentNodeId === '' ? null : numberValue(values.parentNodeId),
        validFrom: textValue(values.validFrom),
        validTo: textValue(values.validTo) || null,
        code: unit?.code ?? '',
        name: unit?.name ?? '',
        nameAlias: unit?.nameAlias,
      };
    },
    headerFields: [
      {
        id: 'code',
        label: t('organizationStructure.code'),
        type: 'display',
        disabled: true,
        width: 180,
        getValue: (record) => record.code,
        setValue: (record) => record,
      },
      {
        id: 'name',
        label: t('organizationStructure.name'),
        type: 'display',
        disabled: true,
        width: 'minmax(320px, 520px)',
        getValue: (record) => record.name,
        getDisplayValue: (record) => localizedName(record, isRtl),
        setValue: (record) => record,
      },
    ],
    actionPaneEndContent: (
      <TextField
        select
        size="small"
        label={t('organizationStructure.hierarchy')}
        value={hierarchyId}
        onChange={(event) => onHierarchyChange(Number(event.target.value))}
        sx={{ minWidth: 220 }}
      >
        {hierarchies.map((hierarchy) => (
          <MenuItem key={hierarchy.id} value={hierarchy.id}>
            {hierarchy.code} — {localizedName(hierarchy, isRtl)}
          </MenuItem>
        ))}
      </TextField>
    ),
    sections: ({ record }) => [
      {
        id: 'node',
        title: t('organizationStructure.node'),
        groups: [
          {
            id: 'membership',
            title: t('organizationStructure.initialMembership'),
            fields: [
              {
                name: 'organizationUnitId',
                label: t('organizationStructure.unit'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <PagedLookup
                    name="organizationUnitId"
                    label={t('organizationStructure.unit')}
                    value={value}
                    disabled={disabled}
                    records={units.filter(
                      (unit) =>
                        unit.id === record.organizationUnitId ||
                        !nodes.some(
                          (node) =>
                            node.organizationUnitId === unit.id && node.recordId !== record.recordId
                        )
                    )}
                    getId={(unit) => unit.id}
                    getCode={(unit) => unit.code}
                    getName={(unit) => localizedName(unit, isRtl)}
                    queryKey={['hierarchy-node-unit', company, hierarchyId, record.id]}
                    onChange={onChange}
                  />
                ),
              },
              {
                name: 'parentNodeId',
                label: t('organizationStructure.parentNode'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => {
                  const descendants = descendantNodeIds(nodes, record.recordId);
                  return (
                    <PagedLookup
                      name="parentNodeId"
                      label={t('organizationStructure.parentNode')}
                      value={value}
                      disabled={disabled}
                      records={nodes.filter(
                        (node) =>
                          node.recordId !== record.recordId && !descendants.has(node.recordId)
                      )}
                      getId={(node) => node.recordId}
                      getCode={(node) => node.code}
                      getName={(node) => localizedName(node, isRtl)}
                      queryKey={['hierarchy-node-parent', company, hierarchyId, record.id]}
                      onChange={onChange}
                    />
                  );
                },
              },
              {
                name: 'validFrom',
                label: t('organizationStructure.validFrom'),
                type: 'date',
              },
              {
                name: 'validTo',
                label: t('organizationStructure.validTo'),
                type: 'date',
              },
            ],
          },
        ],
      },
    ],
    tree: {
      getParentId: (record) => (record.parentNodeId == null ? null : String(record.parentNodeId)),
      getLabel: (record) => localizedName(record, isRtl),
      getSecondaryText: (record) => record.code,
      compare: (left, right) =>
        localizedName(left, isRtl).localeCompare(localizedName(right, isRtl)),
      initiallyExpanded: 'all',
      ariaLabel: t('organizationStructure.nodes'),
    },
    presentation: {
      mode: 'list',
      listWidth: 340,
      listMinWidth: 250,
      listMaxWidth: 560,
      listWidthStorageKey: 'organization-hierarchy-nodes-tree',
    },
    commands: onExit
      ? [
          {
            id: 'hierarchies',
            label: t('organizationStructure.hierarchiesTitle'),
            onClick: onExit,
          },
        ]
      : undefined,
    permissions: {
      view: 'Organization.Structure.View',
      create: 'Organization.Structure.Create',
      edit: 'Organization.Structure.Edit',
      delete: 'Organization.Structure.Edit',
    },
    validate: (record) => ({
      ...(record.organizationUnitId <= 0
        ? { organizationUnitId: t('organizationStructure.unitRequired') }
        : {}),
      ...(!record.validFrom ? { validFrom: t('organizationStructure.validFromRequired') } : {}),
      ...(record.validTo && record.validTo <= record.validFrom
        ? { validTo: t('organizationStructure.dateError') }
        : {}),
      ...(record.recordId > 0 && record.parentNodeId === record.recordId
        ? { parentNodeId: t('organizationStructure.selfParent') }
        : {}),
    }),
    advancedFilter: {
      fieldLabel: t('organizationStructure.unit'),
      getValue: (record) => record.name,
      matches: (record, value) =>
        `${record.code} ${record.name} ${record.nameAlias ?? ''}`
          .toLocaleLowerCase()
          .includes(value.trim().toLocaleLowerCase()),
    },
  };

  return <ListDetailsTreePage title={t('organizationStructure.nodes')} config={config} />;
}

function descendantNodeIds(nodes: HierarchyNodeRecord[], recordId: number): Set<number> {
  if (recordId <= 0) return new Set();
  const descendants = new Set<number>();
  let changed = true;
  while (changed) {
    changed = false;
    nodes.forEach((node) => {
      if (
        node.parentNodeId != null &&
        (node.parentNodeId === recordId || descendants.has(node.parentNodeId)) &&
        !descendants.has(node.recordId)
      ) {
        descendants.add(node.recordId);
        changed = true;
      }
    });
  }
  return descendants;
}

function PagedLookup<T>({
  name,
  label,
  value,
  disabled,
  records,
  getId,
  getCode,
  getName,
  queryKey,
  onChange,
}: {
  name: string;
  label: string;
  value: DetailValue | undefined;
  disabled: boolean;
  records: T[];
  getId: (record: T) => number;
  getCode: (record: T) => string;
  getName: (record: T) => string;
  queryKey: readonly unknown[];
  onChange: (value: DetailValue) => void;
}): React.ReactElement {
  const selected = records.find((record) => getId(record) === Number(value));
  return (
    <Box>
      <LookupField
        name={name}
        label={label}
        value={Number(value) || undefined}
        disabled={disabled}
        displayMode="select"
        searchable
        sideMode="server"
        lazyLoading
        pageSize={20}
        searchDebounceMs={250}
        queryKey={queryKey}
        options={
          selected
            ? [{ id: getId(selected), code: getCode(selected), name: getName(selected) }]
            : []
        }
        fetchPage={async ({ pageNumber, pageSize, search }) => {
          const term = search.trim().toLocaleLowerCase();
          const filtered = records
            .filter((record) =>
              term
                ? `${getCode(record)} ${getName(record)}`.toLocaleLowerCase().includes(term)
                : true
            )
            .sort((left, right) => getName(left).localeCompare(getName(right)));
          const start = (pageNumber - 1) * pageSize;
          return {
            data: filtered.slice(start, start + pageSize).map((record) => ({
              id: getId(record),
              code: getCode(record),
              name: getName(record),
            })),
            pageNumber,
            totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)),
            totalRecords: filtered.length,
          };
        }}
        onChange={(nextValue) => onChange(nextValue == null ? '' : Number(nextValue))}
      />
    </Box>
  );
}
