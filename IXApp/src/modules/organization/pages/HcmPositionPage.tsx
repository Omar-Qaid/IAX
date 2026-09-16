import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAppStore } from '@app/store/useAppStore';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { organizationStructureApi as api } from '../api/organizationStructureApi';

interface PositionRecord {
  id: string;
  recordId: number;
  code: string;
  name: string;
  organizationUnitId: number;
  roleId: number;
  validFrom: string;
  validTo: string | null;
}

const today = () => new Date().toISOString().slice(0, 10);
const numberValue = (value: DetailValue): number => Number(value) || 0;
const textValue = (value: DetailValue): string => String(value ?? '');

const emptyPosition = (): PositionRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recordId: 0,
  code: '',
  name: '',
  organizationUnitId: 0,
  roleId: 0,
  validFrom: today(),
  validTo: null,
});

const toPayload = (record: PositionRecord) => ({
  code: record.code.trim(),
  name: record.name.trim(),
  organizationUnitId: record.organizationUnitId,
  roleId: record.roleId,
  validFrom: record.validFrom,
  validTo: record.validTo || null,
});

export function HcmPositionPage(): React.ReactElement {
  const company = useAppStore((state) => state.currentCompany);
  return <HcmPositionContent key={company} company={company} />;
}

function HcmPositionContent({ company }: { company: string }): React.ReactElement {
  const unitsQuery = useQuery({
    queryKey: ['organization-structure', company, 'units', 'position-lookup'],
    queryFn: ({ signal }) => api.units(today(), signal),
  });
  const rolesQuery = useQuery({
    queryKey: ['organization-structure', company, 'roles'],
    queryFn: ({ signal }) => api.roles(signal),
  });
  const unitOptions = useMemo(
    () =>
      (unitsQuery.data ?? []).map((unit) => ({
        id: unit.id,
        code: unit.code,
        name: unit.name,
      })),
    [unitsQuery.data]
  );
  const roleOptions = useMemo(
    () =>
      (rolesQuery.data ?? []).map((role) => ({
        id: role.id,
        code: role.code,
        name: role.name,
      })),
    [rolesQuery.data]
  );
  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'configuration',
        title: 'Configuration',
        groups: [
          {
            id: 'assignment',
            title: 'Organization assignment',
            fields: [
              {
                name: 'organizationUnitId',
                label: 'Organization unit',
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupField
                    name="organizationUnitId"
                    label="Organization unit"
                    value={numberValue(value ?? 0)}
                    onChange={(unitId) => onChange(Number(unitId) || 0)}
                    options={unitOptions}
                    required
                    disabled={disabled || unitsQuery.isLoading}
                  />
                ),
              },
              {
                name: 'roleId',
                label: 'Role',
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupField
                    name="roleId"
                    label="Role"
                    value={numberValue(value ?? 0)}
                    onChange={(roleId) => onChange(Number(roleId) || 0)}
                    options={roleOptions}
                    required
                    disabled={disabled || rolesQuery.isLoading}
                    displayMode="select"
                  />
                ),
              },
            ],
          },
          {
            id: 'effectiveDates',
            title: 'Effective dates',
            fields: [
              { name: 'validFrom', label: 'Valid from', type: 'date' },
              { name: 'validTo', label: 'Valid to (exclusive)', type: 'date' },
            ],
          },
        ],
      },
    ],
    [roleOptions, rolesQuery.isLoading, unitOptions, unitsQuery.isLoading]
  );

  const config: EnterpriseListDetailsConfig<PositionRecord> = {
    recordTableName: 'HcmPosition',
    dataSource: {
      type: 'remote',
      key: `organization-positions-${company}`,
      load: async (signal) =>
        (await api.positions(today(), signal)).map((position) => ({
          ...position,
          id: String(position.id),
          recordId: position.id,
        })),
      create: async (record) => {
        const recordId = await api.createPosition(toPayload(record));
        return { ...record, id: String(recordId), recordId };
      },
      update: async (record) => {
        await api.updatePosition(record.recordId, toPayload(record));
        return record;
      },
      delete: async (record) => {
        await api.closePosition(record.recordId, today());
      },
    },
    createRecord: emptyPosition,
    getPrimaryText: (record) => record.name,
    getSecondaryText: (record) => record.code,
    matchesSearch: (record, query) =>
      `${record.code} ${record.name}`.toLocaleLowerCase().includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      organizationUnitId: record.organizationUnitId,
      roleId: record.roleId,
      validFrom: record.validFrom,
      validTo: record.validTo ?? '',
    }),
    setValues: (record, values) => ({
      ...record,
      organizationUnitId: numberValue(values.organizationUnitId),
      roleId: numberValue(values.roleId),
      validFrom: textValue(values.validFrom),
      validTo: textValue(values.validTo) || null,
    }),
    headerFields: [
      {
        id: 'code',
        label: 'Code',
        width: 180,
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
    ],
    sections,
    crud: {
      editLabel: 'Edit',
      newLabel: 'New',
      deleteLabel: 'Close',
      saveLabel: 'Save',
      cancelLabel: 'Cancel',
    },
    permissions: {
      view: 'Organization.Structure.View',
      create: 'Organization.Structure.Create',
      edit: 'Organization.Structure.Edit',
      delete: 'Organization.Structure.Edit',
    },
    validate: (record) => ({
      ...(!record.code.trim() ? { code: 'Code is required.' } : {}),
      ...(!record.name.trim() ? { name: 'Name is required.' } : {}),
      ...(record.organizationUnitId <= 0
        ? { organizationUnitId: 'Organization unit is required.' }
        : {}),
      ...(record.roleId <= 0 ? { roleId: 'Role is required.' } : {}),
      ...(!record.validFrom ? { validFrom: 'Valid from is required.' } : {}),
      ...(record.validTo && record.validTo <= record.validFrom
        ? { validTo: 'Valid to must be later than Valid from.' }
        : {}),
    }),
    advancedFilter: {
      fieldLabel: 'Name',
      getValue: (record) => record.name,
      matches: (record, value) =>
        record.name.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
  };

  return <ListDetailsPage variant="enterprise" title="Positions" config={config} />;
}
