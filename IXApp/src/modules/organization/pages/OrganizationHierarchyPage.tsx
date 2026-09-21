import { localizedName } from '@shared/utilities/localizedName';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import React, { useMemo, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import {
  organizationStructureApi as api,
  type OrganizationHierarchyNode,
  type OrganizationUnit,
} from '../api/organizationStructureApi';

interface HierarchyRecord {
  id: string;
  recordId: number;
  code: string;
  name: string;
  nameAlias?: string | null;
  purpose: string;
  rootOrganizationUnitId: number;
  validFrom: string;
  validTo: string | null;
}
type HierarchyNodeRow = Omit<OrganizationHierarchyNode, 'id'> & { id: string };
const today = () => new Date().toISOString().slice(0, 10);
const numberValue = (value: DetailValue | undefined): number => Number(value) || 0;
const textValue = (value: DetailValue | undefined): string => String(value ?? '');

export function OrganizationHierarchyPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  return <OrganizationHierarchyContent key={company} company={company} />;
}

function OrganizationHierarchyContent({ company }: { company: string }): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const asOf = today();
  const [selected, setSelected] = useState<HierarchyRecord | null>(null);
  const units = useQuery({
    queryKey: ['organization-structure', company, 'units', asOf],
    queryFn: ({ signal }) => api.units(asOf, signal),
  });
  const nodes = useQuery({
    queryKey: ['organization-structure', company, 'hierarchy-nodes', selected?.recordId, asOf],
    queryFn: ({ signal }) => api.nodes(selected!.recordId, asOf, signal),
    enabled: Boolean(selected?.recordId),
  });
  const emptyHierarchy = (): HierarchyRecord => ({
    id: `new-${crypto.randomUUID()}`,
    recordId: 0,
    code: '',
    name: '',
    purpose: '',
    rootOrganizationUnitId: 0,
    validFrom: asOf,
    validTo: null,
  });
  const rootUnitField = useMemo(
    () => ({
      name: 'rootOrganizationUnitId',
      label: t('organizationStructure.rootUnit'),
      renderOwnLabel: true,
      render: ({
        value,
        disabled,
        onChange,
      }: {
        value: DetailValue | undefined;
        disabled: boolean;
        onChange: (value: DetailValue) => void;
      }) => (
        <AppLookupField
          name="rootOrganizationUnitId"
          label={t('organizationStructure.rootUnit')}
          value={numberValue(value)}
          onChange={(next) => onChange(Number(next) || 0)}
          options={(units.data ?? []).map((unit) => ({
            id: unit.id,
            code: unit.code,
            name: localizedName(unit, isRtl),
          }))}
          required
          disabled={disabled || units.isLoading}
          displayMode="select"
        />
      ),
    }),
    [units.data, units.isLoading, t, isRtl]
  );
  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'rootNode',
        title: t('organizationStructure.rootOrganizationNode'),
        groups: [
          {
            id: 'rootNodeDetails',
            title: t('organizationStructure.initialMembership'),
            fields: [
              rootUnitField,
              { name: 'validFrom', label: t('organizationStructure.validFrom'), type: 'date' },
              { name: 'validTo', label: t('organizationStructure.validTo'), type: 'date' },
            ],
          },
        ],
      },
    ],
    [rootUnitField, t]
  );
  const config: EnterpriseListDetailsConfig<HierarchyRecord> = {
    recordTableName: 'OrganizationHierarchy',
    dataSource: {
      type: 'remote',
      key: `organization-hierarchies-${company}`,
      load: async (signal) =>
        (await api.hierarchies(signal)).map((row) => ({
          ...row,
          id: String(row.id),
          recordId: row.id,
          rootOrganizationUnitId: 0,
          validFrom: asOf,
          validTo: null,
        })),
      create: async (record) => {
        const recordId = await api.createHierarchyWithRootNode({
          code: record.code.trim(),
          name: record.name.trim(),
          purpose: record.purpose.trim(),
          organizationUnitId: record.rootOrganizationUnitId,
          validFrom: record.validFrom,
          validTo: record.validTo,
        });
        return { ...record, id: String(recordId), recordId };
      },
      update: async (record) => {
        await api.updateHierarchy(record.recordId, {
          code: record.code.trim(),
          name: record.name.trim(),
          purpose: record.purpose.trim(),
        });
        return record;
      },
    },
    createRecord: emptyHierarchy,
    getPrimaryText: (record) => localizedName(record, isRtl),
    getSecondaryText: (record) => record.code,
    matchesSearch: (record, query) =>
      `${record.code} ${record.name} ${record.nameAlias ?? ''} ${record.purpose}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      rootOrganizationUnitId: record.rootOrganizationUnitId,
      validFrom: record.validFrom,
      validTo: record.validTo ?? '',
    }),
    setValues: (record, values) => ({
      ...record,
      rootOrganizationUnitId: numberValue(values.rootOrganizationUnitId),
      validFrom: textValue(values.validFrom),
      validTo: textValue(values.validTo) || null,
    }),
    headerFields: [
      {
        id: 'code',
        label: t('organizationStructure.code'),
        width: 170,
        getValue: (record) => record.code,
        setValue: (record, value) => ({ ...record, code: textValue(value) }),
      },
      {
        id: 'name',
        label: t('organizationStructure.name'),
        width: 'minmax(320px, 520px)',
        getValue: (record) => record.name,
        getDisplayValue: (record) => localizedName(record, isRtl),
        setValue: (record, value) => ({ ...record, name: textValue(value) }),
      },
      {
        id: 'purpose',
        label: t('organizationStructure.purpose'),
        width: 230,
        getValue: (record) => record.purpose,
        setValue: (record, value) => ({ ...record, purpose: textValue(value) }),
      },
    ],
    onSelectionChange: setSelected,
    sections: ({ record }) => [
      ...(record.recordId === 0 ? sections : []),
      {
        id: 'nodes',
        title: t('organizationStructure.nodes'),
        minHeight: 280,
        content: (
          <HierarchyNodesPanel
            hierarchyId={record.recordId}
            nodes={nodes.data ?? []}
            units={units.data ?? []}
            loading={nodes.isLoading}
            onRefresh={() => nodes.refetch()}
          />
        ),
      },
    ],
    permissions: {
      view: 'Organization.Structure.View',
      create: 'Organization.Structure.Create',
      edit: 'Organization.Structure.Edit',
    },
    validate: (record) => ({
      ...(!record.code.trim() ? { code: t('organizationStructure.codeRequired') } : {}),
      ...(!record.name.trim() ? { name: t('organizationStructure.nameRequired') } : {}),
      ...(!record.purpose.trim() ? { purpose: t('organizationStructure.purposeRequired') } : {}),
      ...(record.recordId === 0 && record.rootOrganizationUnitId <= 0
        ? { rootOrganizationUnitId: t('organizationStructure.rootUnitRequired') }
        : {}),
      ...(record.recordId === 0 && !record.validFrom
        ? { validFrom: t('organizationStructure.validFromRequired') }
        : {}),
      ...(record.recordId === 0 && record.validTo && record.validTo <= record.validFrom
        ? { validTo: t('organizationStructure.dateError') }
        : {}),
    }),
    advancedFilter: {
      fieldLabel: t('organizationStructure.name'),
      getValue: (record) => record.name,
      matches: (record, value) =>
        `${record.name} ${record.nameAlias ?? ''}`
          .toLocaleLowerCase()
          .includes(value.trim().toLocaleLowerCase()),
    },
  };
  return (
    <ListDetailsPage
      variant="enterprise"
      title={t('organizationStructure.hierarchiesTitle')}
      config={config}
    />
  );
}

function HierarchyNodesPanel({
  hierarchyId,
  nodes,
  units,
  loading,
  onRefresh,
}: {
  hierarchyId: number;
  nodes: OrganizationHierarchyNode[];
  units: OrganizationUnit[];
  loading: boolean;
  onRefresh: () => Promise<unknown>;
}): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const gridRef = useRef<DataGridHandle>(null);
  const [selectedIds, setSelectedIds] = useState<(string | number)[]>([]);
  const rows = useMemo<HierarchyNodeRow[]>(
    () => nodes.map((node) => ({ ...node, id: String(node.id) })),
    [nodes]
  );
  const columns = useMemo<ColumnDef<HierarchyNodeRow>[]>(
    () => [
      {
        field: 'organizationUnitId',
        headerName: t('organizationStructure.unit'),
        minWidth: 260,
        flex: 1,
        editable: true,
        type: 'singleSelect',
        valueOptions: units.map((unit) => ({
          value: unit.id,
          label: `${unit.code} — ${localizedName(unit, isRtl)}`,
        })),
        renderCell: ({ value }) => {
          const unit = units.find((item) => item.id === Number(value));
          return unit ? `${unit.code} — ${localizedName(unit, isRtl)}` : '';
        },
      },
      {
        field: 'parentNodeId',
        headerName: t('organizationStructure.parentNode'),
        width: 250,
        editable: true,
        type: 'singleSelect',
        valueOptions: [
          { value: null, label: t('organizationStructure.rootNode') },
          ...nodes.map((node) => {
            const unit = units.find((item) => item.id === node.organizationUnitId);
            return {
              value: node.id,
              label: `#${node.id} — ${unit ? `${unit.code} — ${localizedName(unit, isRtl)}` : t('organizationStructure.unitReference', { id: node.organizationUnitId })}`,
            };
          }),
        ],
        renderCell: ({ value }) => {
          if (value == null) return t('organizationStructure.rootNode');
          const parent = nodes.find((node) => node.id === Number(value));
          if (!parent) return '';
          const unit = units.find((item) => item.id === parent.organizationUnitId);
          return unit
            ? `${unit.code} — ${localizedName(unit, isRtl)}`
            : t('organizationStructure.nodeReference', { id: parent.id });
        },
      },
      {
        field: 'validFrom',
        headerName: t('organizationStructure.validFrom'),
        width: 135,
        editable: true,
        type: 'date',
      },
      {
        field: 'validTo',
        headerName: t('organizationStructure.validTo'),
        width: 135,
        editable: true,
        type: 'date',
      },
    ],
    [nodes, units, t, isRtl]
  );
  const save = async (values: Partial<HierarchyNodeRow>, isNew: boolean) => {
    const organizationUnitId = Number(values.organizationUnitId) || 0;
    const parentNodeId = values.parentNodeId == null ? null : Number(values.parentNodeId);
    const validFrom = String(values.validFrom ?? '');
    const validTo = values.validTo ? String(values.validTo) : null;
    if (organizationUnitId <= 0) throw new Error(t('organizationStructure.unitRequired'));
    if (!validFrom) throw new Error(t('organizationStructure.validFromRequired'));
    if (validTo && validTo <= validFrom) throw new Error(t('organizationStructure.dateError'));
    if (!isNew && parentNodeId === Number(values.id))
      throw new Error(t('organizationStructure.selfParent'));
    if (isNew)
      await api.createNode({ hierarchyId, organizationUnitId, parentNodeId, validFrom, validTo });
    else
      await api.updateNode(Number(values.id), {
        organizationUnitId,
        parentNodeId,
        validFrom,
        validTo,
      });
    await onRefresh();
  };
  const close = async () => {
    const id = Number(selectedIds.at(-1));
    if (!id) return;
    await api.closeNode(id, today());
    setSelectedIds([]);
    await onRefresh();
  };
  return (
    <TabularDetailPanel
      rows={rows}
      columns={columns}
      addLabel={t('organizationStructure.addNode')}
      removeLabel={t('organizationStructure.closeNode')}
      selectedIds={selectedIds}
      onSelectionChange={setSelectedIds}
      onAdd={() => gridRef.current?.startAddRow()}
      onRemove={close}
      onRowSave={save}
      onNewRow={() => ({
        id: `new-${crypto.randomUUID()}`,
        hierarchyId,
        organizationUnitId: 0,
        parentNodeId: null,
        validFrom: today(),
        validTo: null,
      })}
      gridRef={gridRef}
      masterForm
      disabled={hierarchyId <= 0 || loading}
      storageKey="organization.hierarchy.nodes"
      height={250}
    />
  );
}
