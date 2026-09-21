import React, { useMemo, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Alert, MenuItem, Stack, TextField, Typography } from '@mui/material';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import {
  organizationStructureApi as api,
  type OrganizationPosition,
  type OrganizationRole,
} from '../api/organizationStructureApi';

interface UnitRecord {
  id: string;
  recordId: number;
  code: string;
  name: string;
  nameAR: string;
  type: number;
  validFrom: string;
  validTo: string | null;
}

const today = () => {
  const date = new Date();
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
};
const numberValue = (value: DetailValue): number => Number(value) || 0;
const textValue = (value: DetailValue): string => String(value ?? '');

export function OrganizationUnitsPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  return <OrganizationUnitsContent key={company} company={company} />;
}

function OrganizationUnitsContent({ company }: { company: string }): React.ReactElement {
  const { t } = useAppTranslation();
  const label = (key: string) => t(`organizationUnits.${key}`);
  const [asOf, setAsOf] = useState(today);
  const [selected, setSelected] = useState<UnitRecord | null>(null);
  const [hierarchy, setHierarchy] = useState('');
  const positions = useQuery({
    queryKey: ['organization-structure', company, 'positions', asOf],
    queryFn: ({ signal }) => api.positions(asOf, signal),
  });
  const roles = useQuery({
    queryKey: ['organization-structure', company, 'roles'],
    queryFn: ({ signal }) => api.roles(signal),
  });
  const hierarchies = useQuery({
    queryKey: ['organization-structure', company, 'hierarchies'],
    queryFn: ({ signal }) => api.hierarchies(signal),
  });
  const ancestors = useQuery({
    queryKey: ['organization-structure', company, 'ancestors', hierarchy, selected?.id, asOf],
    queryFn: ({ signal }) => api.ancestors(Number(hierarchy), selected!.recordId, asOf, signal),
    enabled: Boolean(hierarchy && selected),
  });
  const typeOptions = Array.from({ length: 9 }, (_, index) => ({
    value: String(index + 1),
    label: label(`types.${index + 1}`),
  }));
  const emptyUnit = (): UnitRecord => ({
    id: `new-${crypto.randomUUID()}`,
    recordId: 0,
    code: '',
    name: '',
    nameAR: '',
    type: 1,
    validFrom: asOf,
    validTo: null,
  });
  const config: EnterpriseListDetailsConfig<UnitRecord> = {
    recordTableName: 'OrganizationUnit',
    filterStorageKey: 'organization-units',
    dataSource: {
      type: 'remote',
      key: `organization-units-${company}-${asOf}`,
      load: async (signal) =>
        (await api.units(asOf, signal)).map((unit) => ({
          ...unit,
          id: String(unit.id),
          recordId: unit.id,
          nameAR: '',
          validFrom: asOf,
          validTo: null,
        })),
      create: async (record) => {
        const recordId = await api.create({
          code: record.code.trim(),
          name: record.name.trim(),
          nameAR: record.nameAR.trim() || null,
          type: record.type,
          validFrom: record.validFrom,
          validTo: record.validTo,
        });
        return { ...record, id: String(recordId), recordId };
      },
      update: async (record) => {
        await api.update(record.recordId, {
          code: record.code.trim(),
          name: record.name.trim(),
          type: record.type,
        });
        return record;
      },
      delete: async (record) => {
        await api.close(record.recordId, asOf);
      },
    },
    createRecord: emptyUnit,
    getPrimaryText: (record) => record.name,
    getSecondaryText: (record) => record.code,
    matchesSearch: (record, query) =>
      `${record.code} ${record.name} ${record.nameAR}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      type: record.type,
      nameAR: record.nameAR,
      validFrom: record.validFrom,
      validTo: record.validTo ?? '',
    }),
    setValues: (record, values) => ({
      ...record,
      type: numberValue(values.type),
      nameAR: textValue(values.nameAR),
      validFrom: textValue(values.validFrom),
      validTo: textValue(values.validTo) || null,
    }),
    headerFields: [
      {
        id: 'code',
        label: label('code'),
        width: 180,
        getValue: (record) => record.code,
        setValue: (record, value) => ({ ...record, code: textValue(value) }),
      },
      {
        id: 'name',
        label: label('name'),
        width: 'minmax(320px, 520px)',
        getValue: (record) => record.name,
        setValue: (record, value) => ({ ...record, name: textValue(value) }),
      },
    ],
    onSelectionChange: setSelected,
    actionPaneEndContent: (
      <TextField
        size="small"
        type="date"
        label={label('asOf')}
        value={asOf}
        slotProps={{ inputLabel: { shrink: true } }}
        onChange={(event) => {
          if (event.target.value) setAsOf(event.target.value);
        }}
      />
    ),
    sections: ({ record, editing }) => [
      {
        id: 'configuration',
        title: label('general'),
        groups: [
          {
            id: 'classification',
            title: label('type'),
            fields: [
              {
                name: 'type',
                label: label('type'),
                type: 'select',
                options: typeOptions,
              },
              ...(record.recordId === 0
                ? [
                    { name: 'nameAR', label: label('nameAR') },
                    { name: 'validFrom', label: label('validFrom'), type: 'date' as const },
                    { name: 'validTo', label: label('exclusiveEnd'), type: 'date' as const },
                  ]
                : []),
            ],
          },
        ],
      },
      {
        id: 'hierarchy',
        title: label('hierarchy'),
        content: (
          <Stack spacing={1}>
            <TextField
              select
              size="small"
              label={label('hierarchy')}
              value={hierarchy}
              disabled={editing}
              onChange={(event) => setHierarchy(event.target.value)}
              sx={{ maxWidth: 350 }}
            >
              <MenuItem value="">{label('chooseHierarchy')}</MenuItem>
              {(hierarchies.data ?? []).map((item) => (
                <MenuItem key={item.id} value={String(item.id)}>
                  {item.name}
                </MenuItem>
              ))}
            </TextField>
            {(hierarchies.error || ancestors.error) && (
              <Alert severity="error">{(hierarchies.error || ancestors.error)?.message}</Alert>
            )}
            {hierarchy && record.recordId > 0 && (
              <Typography variant="body2">
                {ancestors.isFetching
                  ? t('common.loading')
                  : ancestors.data?.length
                    ? [...ancestors.data]
                        .reverse()
                        .map((unit) => unit.name)
                        .join(' / ')
                    : label('noMembership')}
              </Typography>
            )}
          </Stack>
        ),
      },
      {
        id: 'positions',
        title: label('positions'),
        content: positions.error ? (
          <Alert severity="error">{positions.error.message}</Alert>
        ) : (
          <OrganizationUnitPositionsPanel
            organizationUnitId={record.recordId}
            positions={(positions.data ?? []).filter(
              (position) => position.organizationUnitId === record.recordId
            )}
            roles={roles.data ?? []}
            disabled={editing || positions.isLoading || roles.isLoading}
            onRefresh={() => positions.refetch()}
          />
        ),
      },
    ],
    permissions: {
      view: 'Organization.Structure.View',
      create: 'Organization.Structure.Create',
      edit: 'Organization.Structure.Edit',
      delete: 'Organization.Structure.Edit',
    },
    validate: (record) => ({
      ...(!record.code.trim() || record.code.length > 50 ? { code: label('codeError') } : {}),
      ...(!record.name.trim() || record.name.length > 200 ? { name: label('nameError') } : {}),
      ...(record.nameAR.length > 200 ? { nameAR: label('nameError') } : {}),
      ...(record.type <= 0 ? { type: label('required') } : {}),
      ...(!record.validFrom ? { validFrom: label('required') } : {}),
      ...(record.validTo && record.validTo <= record.validFrom
        ? { validTo: label('dateError') }
        : {}),
    }),
    advancedFilter: {
      fieldLabel: label('name'),
      getValue: (record) => record.name,
      matches: (record, value) =>
        record.name.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
  };

  return <ListDetailsPage key={asOf} variant="enterprise" title={label('title')} config={config} />;
}

type PositionRow = Omit<OrganizationPosition, 'id'> & { id: string };

function OrganizationUnitPositionsPanel({
  organizationUnitId,
  positions,
  roles,
  disabled,
  onRefresh,
}: {
  organizationUnitId: number;
  positions: OrganizationPosition[];
  roles: OrganizationRole[];
  disabled: boolean;
  onRefresh: () => Promise<unknown>;
}): React.ReactElement {
  const gridRef = useRef<DataGridHandle>(null);
  const [selectedIds, setSelectedIds] = useState<(string | number)[]>([]);
  const rows = useMemo<PositionRow[]>(
    () => positions.map((position) => ({ ...position, id: String(position.id) })),
    [positions]
  );
  const columns = useMemo<ColumnDef<PositionRow>[]>(
    () => [
      { field: 'code', headerName: 'Code', width: 160, editable: true },
      { field: 'name', headerName: 'Name', minWidth: 230, flex: 1, editable: true },
      {
        field: 'roleId',
        headerName: 'Organization role',
        minWidth: 210,
        editable: true,
        type: 'singleSelect',
        valueOptions: roles.map((role) => ({
          value: role.id,
          label: `${role.code} — ${role.name}`,
        })),
      },
      { field: 'validFrom', headerName: 'Valid from', width: 135, editable: true, type: 'date' },
      { field: 'validTo', headerName: 'Valid to', width: 135, editable: true, type: 'date' },
    ],
    [roles]
  );
  const save = async (values: Partial<PositionRow>, isNew: boolean) => {
    const code = String(values.code ?? '').trim();
    const name = String(values.name ?? '').trim();
    const roleId = Number(values.roleId) || 0;
    const validFrom = String(values.validFrom ?? '');
    const validTo = values.validTo ? String(values.validTo) : null;
    if (!code) throw new Error('Code is required.');
    if (!name) throw new Error('Name is required.');
    if (roleId <= 0) throw new Error('Organization role is required.');
    if (!validFrom) throw new Error('Valid from is required.');
    if (validTo && validTo <= validFrom) throw new Error('Valid to must be later than valid from.');
    const payload = { code, name, organizationUnitId, roleId, validFrom, validTo };
    if (isNew) await api.createPosition(payload);
    else await api.updatePosition(Number(values.id), payload);
    await onRefresh();
  };
  const close = async () => {
    const id = Number(selectedIds.at(-1));
    if (!id) return;
    await api.closePosition(id, today());
    setSelectedIds([]);
    await onRefresh();
  };
  return (
    <TabularDetailPanel
      showFilterRow={false}
      rows={rows}
      columns={columns}
      addLabel="Add position"
      removeLabel="Close position"
      selectedIds={selectedIds}
      onSelectionChange={setSelectedIds}
      onAdd={() => gridRef.current?.startAddRow()}
      onRemove={close}
      onRowSave={save}
      onNewRow={() => ({
        id: `new-${crypto.randomUUID()}`,
        organizationUnitId,
        roleId: 0,
        code: '',
        name: '',
        validFrom: today(),
        validTo: null,
      })}
      gridRef={gridRef}
      masterForm
      disabled={disabled || organizationUnitId <= 0}
      height={250}
      storageKey="organization-unit-positions"
    />
  );
}
