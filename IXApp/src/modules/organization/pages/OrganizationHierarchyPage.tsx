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
      label: 'Root organization unit',
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
          label="Root organization unit"
          value={numberValue(value)}
          onChange={(next) => onChange(Number(next) || 0)}
          options={(units.data ?? []).map((unit) => ({
            id: unit.id,
            code: unit.code,
            name: unit.name,
          }))}
          required
          disabled={disabled || units.isLoading}
          displayMode="select"
        />
      ),
    }),
    [units.data, units.isLoading]
  );
  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'rootNode',
        title: 'Root organization node',
        groups: [
          {
            id: 'rootNodeDetails',
            title: 'Initial membership',
            fields: [
              rootUnitField,
              { name: 'validFrom', label: 'Valid from', type: 'date' },
              { name: 'validTo', label: 'Valid to', type: 'date' },
            ],
          },
        ],
      },
    ],
    [rootUnitField]
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
    getPrimaryText: (record) => record.name,
    getSecondaryText: (record) => record.code,
    matchesSearch: (record, query) =>
      `${record.code} ${record.name} ${record.purpose}`
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
        label: 'Code',
        width: 170,
        getValue: (record) => record.code,
        setValue: (record, value) => ({ ...record, code: textValue(value) }),
      },
      {
        id: 'name',
        label: 'Name',
        width: 'minmax(320px, 520px)',
        getValue: (record) => record.name,
        setValue: (record, value) => ({ ...record, name: textValue(value) }),
      },
      {
        id: 'purpose',
        label: 'Purpose',
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
        title: 'Organization hierarchy nodes',
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
      ...(!record.code.trim() ? { code: 'Code is required.' } : {}),
      ...(!record.name.trim() ? { name: 'Name is required.' } : {}),
      ...(!record.purpose.trim() ? { purpose: 'Purpose is required.' } : {}),
      ...(record.recordId === 0 && record.rootOrganizationUnitId <= 0
        ? { rootOrganizationUnitId: 'Root organization unit is required.' }
        : {}),
      ...(record.recordId === 0 && !record.validFrom
        ? { validFrom: 'Valid from is required.' }
        : {}),
      ...(record.recordId === 0 && record.validTo && record.validTo <= record.validFrom
        ? { validTo: 'Valid to must be later than valid from.' }
        : {}),
    }),
    advancedFilter: {
      fieldLabel: 'Name',
      getValue: (record) => record.name,
      matches: (record, value) =>
        record.name.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
  };
  return <ListDetailsPage variant="enterprise" title="Organization hierarchies" config={config} />;
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
        headerName: 'Organization unit',
        minWidth: 260,
        flex: 1,
        editable: true,
        type: 'singleSelect',
        valueOptions: units.map((unit) => ({
          value: unit.id,
          label: `${unit.code} — ${unit.name}`,
        })),
        renderCell: ({ value }) => {
          const unit = units.find((item) => item.id === Number(value));
          return unit ? `${unit.code} — ${unit.name}` : '';
        },
      },
      {
        field: 'parentNodeId',
        headerName: 'Parent node',
        width: 250,
        editable: true,
        type: 'singleSelect',
        valueOptions: [
          { value: null, label: 'Root node' },
          ...nodes.map((node) => {
            const unit = units.find((item) => item.id === node.organizationUnitId);
            return {
              value: node.id,
              label: `#${node.id} — ${unit ? `${unit.code} — ${unit.name}` : `Organization unit ${node.organizationUnitId}`}`,
            };
          }),
        ],
        renderCell: ({ value }) => {
          if (value == null) return 'Root node';
          const parent = nodes.find((node) => node.id === Number(value));
          if (!parent) return '';
          const unit = units.find((item) => item.id === parent.organizationUnitId);
          return unit ? `${unit.code} — ${unit.name}` : `Node #${parent.id}`;
        },
      },
      { field: 'validFrom', headerName: 'Valid from', width: 135, editable: true, type: 'date' },
      { field: 'validTo', headerName: 'Valid to', width: 135, editable: true, type: 'date' },
    ],
    [nodes, units]
  );
  const save = async (values: Partial<HierarchyNodeRow>, isNew: boolean) => {
    const organizationUnitId = Number(values.organizationUnitId) || 0;
    const parentNodeId = values.parentNodeId == null ? null : Number(values.parentNodeId);
    const validFrom = String(values.validFrom ?? '');
    const validTo = values.validTo ? String(values.validTo) : null;
    if (organizationUnitId <= 0) throw new Error('Organization unit is required.');
    if (!validFrom) throw new Error('Valid from is required.');
    if (validTo && validTo <= validFrom) throw new Error('Valid to must be later than valid from.');
    if (!isNew && parentNodeId === Number(values.id))
      throw new Error('A node cannot be its own parent.');
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
      addLabel="Add node"
      removeLabel="Close node"
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
