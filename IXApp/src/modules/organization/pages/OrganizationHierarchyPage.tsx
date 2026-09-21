import React, { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailFieldConfig,
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { LookupField } from '@shared/components/lookups/LookupField';
import { localizedName } from '@shared/utilities/localizedName';
import { organizationStructureApi as api } from '../api/organizationStructureApi';
import { OrganizationHierarchyNodesPage } from './OrganizationHierarchyNodesPage';

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

const today = () => new Date().toISOString().slice(0, 10);
const numberValue = (value: DetailValue | undefined): number => Number(value) || 0;
const textValue = (value: DetailValue | undefined): string => String(value ?? '');

export function OrganizationHierarchyPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  return <OrganizationHierarchyContent key={company} company={company} />;
}

function OrganizationHierarchyContent({ company }: { company: string }): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const navigate = useNavigate();
  const asOf = today();
  const [activeHierarchyId, setActiveHierarchyId] = useState<number | null>(null);
  const units = useQuery({
    queryKey: ['organization-structure', company, 'units', asOf],
    queryFn: ({ signal }) => api.units(asOf, signal),
  });
  const rootUnitField = useMemo<DetailFieldConfig>(
    () => ({
      name: 'rootOrganizationUnitId',
      label: t('organizationStructure.rootUnit'),
      renderOwnLabel: true,
      render: ({ value, disabled, onChange }) => {
        const available = units.data ?? [];
        const selected = available.find((unit) => unit.id === Number(value));
        return (
          <LookupField
            name="rootOrganizationUnitId"
            label={t('organizationStructure.rootUnit')}
            value={value || undefined}
            disabled={disabled || units.isLoading}
            required
            displayMode="select"
            searchable
            sideMode="server"
            lazyLoading
            pageSize={20}
            searchDebounceMs={250}
            queryKey={['organization-hierarchy-root-unit', company, asOf]}
            options={
              selected
                ? [{ id: selected.id, code: selected.code, name: localizedName(selected, isRtl) }]
                : []
            }
            fetchPage={async ({ pageNumber, pageSize, search }) => {
              const term = search.trim().toLocaleLowerCase();
              const filtered = available
                .filter((unit) =>
                  term
                    ? `${unit.code} ${unit.name} ${unit.nameAlias ?? ''}`
                        .toLocaleLowerCase()
                        .includes(term)
                    : true
                )
                .sort((left, right) =>
                  localizedName(left, isRtl).localeCompare(localizedName(right, isRtl))
                );
              const start = (pageNumber - 1) * pageSize;
              return {
                data: filtered.slice(start, start + pageSize).map((unit) => ({
                  id: unit.id,
                  code: unit.code,
                  name: localizedName(unit, isRtl),
                })),
                pageNumber,
                totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)),
                totalRecords: filtered.length,
              };
            }}
            onChange={(nextValue) => onChange(Number(nextValue) || 0)}
          />
        );
      },
    }),
    [asOf, company, isRtl, t, units.data, units.isLoading]
  );
  if (activeHierarchyId != null) {
    return (
      <OrganizationHierarchyNodesPage
        initialHierarchyId={activeHierarchyId}
        onHierarchyChange={setActiveHierarchyId}
        onExit={() => setActiveHierarchyId(null)}
      />
    );
  }
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
  const config: EnterpriseListDetailsConfig<HierarchyRecord> = {
    recordTableName: 'OrganizationHierarchy',
    filterStorageKey: 'organization-hierarchies',
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
    sections: ({ record }) =>
      record.recordId === 0
        ? [
            {
              id: 'rootNode',
              title: t('organizationStructure.rootOrganizationNode'),
              groups: [
                {
                  id: 'rootNodeDetails',
                  title: t('organizationStructure.initialMembership'),
                  fields: [
                    rootUnitField,
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
          ]
        : [],
    commands: (record) => [
      {
        id: 'nodes',
        label: t('organizationStructure.nodes'),
        requiresSelection: true,
        disabled: !record || record.recordId <= 0,
        onClick: (selected) => {
          if (selected?.recordId) setActiveHierarchyId(selected.recordId);
        },
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
