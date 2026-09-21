import { localizedName } from '@shared/utilities/localizedName';
import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
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
  nameAlias?: string | null;
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
  nameAlias: null,
  organizationUnitId: 0,
  roleId: 0,
  validFrom: today(),
  validTo: null,
});

const toPayload = (record: PositionRecord) => ({
  code: record.code.trim(),
  name: record.name.trim(),
  nameAlias: record.nameAlias?.trim() || null,
  organizationUnitId: record.organizationUnitId,
  roleId: record.roleId,
  validFrom: record.validFrom,
  validTo: record.validTo || null,
});

export function HcmPositionPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  return <HcmPositionContent key={company} company={company} />;
}

function HcmPositionContent({ company }: { company: string }): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
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
        name: localizedName(unit, isRtl),
      })),
    [unitsQuery.data, isRtl]
  );
  const roleOptions = useMemo(
    () =>
      (rolesQuery.data ?? []).map((role) => ({
        id: role.id,
        code: role.code,
        name: localizedName(role, isRtl),
      })),
    [rolesQuery.data, isRtl]
  );
  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'configuration',
        title: t('hcmPositions.sections.configuration'),
        groups: [
          {
            id: 'identity',
            title: t('hcmWorkers.fields.nameAlias'),
            fields: [{ name: 'nameAlias', label: t('hcmWorkers.fields.nameAlias') }],
          },
          {
            id: 'assignment',
            title: t('hcmPositions.groups.organizationAssignment'),
            fields: [
              {
                name: 'organizationUnitId',
                label: t('hcmPositions.fields.organizationUnit'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupField
                    name="organizationUnitId"
                    label={t('hcmPositions.fields.organizationUnit')}
                    value={numberValue(value ?? 0)}
                    onChange={(unitId) => onChange(Number(unitId) || 0)}
                    options={unitOptions}
                    required
                    disabled={disabled || unitsQuery.isLoading}
                    displayMode="select"
                  />
                ),
              },
              {
                name: 'roleId',
                label: t('hcmPositions.fields.role'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupField
                    name="roleId"
                    label={t('hcmPositions.fields.role')}
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
            title: t('hcmPositions.groups.effectiveDates'),
            fields: [
              { name: 'validFrom', label: t('hcmPositions.fields.validFrom'), type: 'date' },
              { name: 'validTo', label: t('hcmPositions.fields.validTo'), type: 'date' },
            ],
          },
        ],
      },
    ],
    [roleOptions, rolesQuery.isLoading, t, unitOptions, unitsQuery.isLoading]
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
    getPrimaryText: (record) => localizedName(record, isRtl),
    getSecondaryText: (record) => record.code,
    matchesSearch: (record, query) =>
      `${record.code} ${record.name} ${record.nameAlias ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      nameAlias: record.nameAlias ?? '',
      organizationUnitId: record.organizationUnitId,
      roleId: record.roleId,
      validFrom: record.validFrom,
      validTo: record.validTo ?? '',
    }),
    setValues: (record, values) => ({
      ...record,
      nameAlias: textValue(values.nameAlias) || null,
      organizationUnitId: numberValue(values.organizationUnitId),
      roleId: numberValue(values.roleId),
      validFrom: textValue(values.validFrom),
      validTo: textValue(values.validTo) || null,
    }),
    headerFields: [
      {
        id: 'code',
        label: t('hcmPositions.fields.code'),
        width: 180,
        getValue: (record) => record.code,
        setValue: (record, value) => ({ ...record, code: textValue(value) }),
      },
      {
        id: 'name',
        label: t('hcmPositions.fields.name'),
        width: 'minmax(320px, 520px)',
        getValue: (record) => record.name,
        getDisplayValue: (record) => localizedName(record, isRtl),
        setValue: (record, value) => ({ ...record, name: textValue(value) }),
      },
    ],
    sections,
    permissions: {
      view: 'Organization.Structure.View',
      create: 'Organization.Structure.Create',
      edit: 'Organization.Structure.Edit',
      delete: 'Organization.Structure.Edit',
    },
    validate: (record) => ({
      ...(!record.code.trim()
        ? { code: t('validation.required', { field: t('hcmPositions.fields.code') }) }
        : {}),
      ...(!record.name.trim()
        ? { name: t('validation.required', { field: t('hcmPositions.fields.name') }) }
        : {}),
      ...(record.organizationUnitId <= 0
        ? {
            organizationUnitId: t('validation.required', {
              field: t('hcmPositions.fields.organizationUnit'),
            }),
          }
        : {}),
      ...(record.roleId <= 0
        ? { roleId: t('validation.required', { field: t('hcmPositions.fields.role') }) }
        : {}),
      ...(!record.validFrom
        ? {
            validFrom: t('validation.required', {
              field: t('hcmPositions.fields.validFrom'),
            }),
          }
        : {}),
      ...(record.validTo && record.validTo <= record.validFrom
        ? { validTo: t('hcmPositions.validation.validToAfterValidFrom') }
        : {}),
    }),
    advancedFilter: {
      fieldLabel: t('hcmPositions.fields.name'),
      getValue: (record) => record.name,
      matches: (record, value) =>
        `${record.name} ${record.nameAlias ?? ''}`
          .toLocaleLowerCase()
          .includes(value.trim().toLocaleLowerCase()),
    },
  };

  return <ListDetailsPage variant="enterprise" title={t('hcmPositions.title')} config={config} />;
}
