import { localizedName } from '@shared/utilities/localizedName';
import React, { useState } from 'react';
import { LookupField } from '@shared/components/lookups/LookupField';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsTreePage } from '@patterns/list-details-tree';
import type { DetailValue, DetailValues } from '@patterns/list-details/types';
import type { ListDetailsTreePageConfig } from '@patterns/list-details-tree';
import { organizationStructureApi as api } from '../api/organizationStructureApi';

interface UnitRecord {
  id: string;
  recordId: number;
  code: string;
  name: string;
  nameAlias: string | null;
  type: number;
  parentOrganizationUnitId: number | null;
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
  const { t, isRtl } = useAppTranslation();
  const label = (key: string) => t(`organizationUnits.${key}`);
  const [asOf] = useState(today);
  const [units, setUnits] = useState<UnitRecord[]>([]);
  const typeOptions = Array.from({ length: 9 }, (_, index) => ({
    value: String(index + 1),
    label: label(`types.${index + 1}`),
  }));

  const emptyUnit = (): UnitRecord => ({
    id: `new-${crypto.randomUUID()}`,
    recordId: 0,
    code: '',
    name: '',
    nameAlias: null,
    type: 1,
    parentOrganizationUnitId: null,
    validFrom: asOf,
    validTo: null,
  });

  const config: ListDetailsTreePageConfig<UnitRecord> = {
    recordTableName: 'OrganizationUnit',
    filterStorageKey: 'organization-units',
    dataSource: {
      type: 'remote',
      key: `organization-units-${company}-${asOf}`,
      load: async (signal) => {
        const loaded = (await api.units(asOf, signal)).map((unit) => ({
          ...unit,
          id: String(unit.id),
          recordId: unit.id,
          nameAlias: unit.nameAlias ?? null,
          validFrom: unit.validFrom ? unit.validFrom.split('T')[0] : asOf,
          validTo: unit.validTo ? unit.validTo.split('T')[0] : null,
        }));
        setUnits(loaded);
        return loaded;
      },
      create: async (record) => {
        const recordId = await api.create({
          code: record.code.trim(),
          name: record.name.trim(),
          nameAlias: record.nameAlias?.trim() || null,
          type: record.type,
          parentOrganizationUnitId: record.parentOrganizationUnitId,
          validFrom: record.validFrom,
          validTo: record.validTo,
        });
        return { ...record, id: String(recordId), recordId };
      },
      update: async (record) => {
        await api.update(record.recordId, {
          code: record.code.trim(),
          name: record.name.trim(),
          nameAlias: record.nameAlias?.trim() || null,
          type: record.type,
          parentOrganizationUnitId: record.parentOrganizationUnitId,
          validFrom: record.validFrom,
          validTo: record.validTo,
        });
        return record;
      },
      delete: async (record) => {
        await api.close(record.recordId, asOf);
      },
    },
    createRecord: emptyUnit,
    getPrimaryText: (record) => localizedName(record, isRtl),
    getSecondaryText: (record) => record.code,
    matchesSearch: (record, query) =>
      `${record.code} ${record.name} ${record.nameAlias ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      type: record.type,
      parentOrganizationUnitId: record.parentOrganizationUnitId ?? '',
      nameAlias: record.nameAlias ?? '',
      validFrom: record.validFrom,
      validTo: record.validTo ?? '',
    }),
    setValues: (record, values) => ({
      ...record,
      type: numberValue(values.type),
      parentOrganizationUnitId:
        values.parentOrganizationUnitId === ''
          ? null
          : numberValue(values.parentOrganizationUnitId),
      nameAlias: textValue(values.nameAlias) || null,
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
        getDisplayValue: (record) => localizedName(record, isRtl),
        setValue: (record, value) => ({ ...record, name: textValue(value) }),
      },
    ],

    sections: ({ record }) => [
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
              {
                name: 'parentOrganizationUnitId',
                label: label('parent'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => {
                  const byId = new Map(units.map((unit) => [unit.recordId, unit]));
                  const isDescendant = (candidate: UnitRecord) => {
                    if (record.recordId <= 0) return false;
                    let parentId = candidate.parentOrganizationUnitId;
                    const visited = new Set<number>();
                    while (parentId != null && !visited.has(parentId)) {
                      if (parentId === record.recordId) return true;
                      visited.add(parentId);
                      parentId = byId.get(parentId)?.parentOrganizationUnitId ?? null;
                    }
                    return false;
                  };
                  const available = units.filter(
                    (unit) => unit.recordId !== record.recordId && !isDescendant(unit)
                  );
                  const selectedParent = available.find(
                    (unit) => unit.recordId === record.parentOrganizationUnitId
                  );
                  return (
                    <LookupField
                      name="parentOrganizationUnitId"
                      label={label('parent')}
                      value={Number(value) || undefined}
                      disabled={disabled}
                      displayMode="select"
                      searchable
                      sideMode="server"
                      lazyLoading
                      pageSize={20}
                      searchDebounceMs={250}
                      queryKey={['organization-unit-parent', company, asOf, record.id]}
                      options={
                        selectedParent
                          ? [
                              {
                                id: selectedParent.recordId,
                                code: selectedParent.code,
                                name: localizedName(selectedParent, isRtl),
                              },
                            ]
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
                            id: unit.recordId,
                            code: unit.code,
                            name: localizedName(unit, isRtl),
                          })),
                          pageNumber,
                          totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)),
                          totalRecords: filtered.length,
                        };
                      }}
                      onChange={(nextValue) => onChange(nextValue == null ? '' : Number(nextValue))}
                    />
                  );
                },
              },
              {
                name: 'validFrom',
                label: label('validFrom'),
                type: 'date',
              },
              {
                name: 'validTo',
                label: label('exclusiveEnd'),
                type: 'date',
              },
              { name: 'nameAlias', label: t('hcmWorkers.fields.nameAlias') },
            ],
          },
        ],
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
      ...((record.nameAlias?.length ?? 0) > 200 ? { nameAlias: label('nameError') } : {}),
      ...(record.type <= 0 ? { type: label('required') } : {}),
      ...(!record.validFrom ? { validFrom: label('required') } : {}),
      ...(record.validTo && record.validTo <= record.validFrom
        ? { validTo: label('dateError') }
        : {}),
    }),
    tree: {
      getParentId: (record) =>
        record.parentOrganizationUnitId == null ? null : String(record.parentOrganizationUnitId),
      getLabel: (record) => localizedName(record, isRtl),
      getSecondaryText: (record) => record.code,
      compare: (left, right) =>
        localizedName(left, isRtl).localeCompare(localizedName(right, isRtl)),
      initiallyExpanded: 'all',
      ariaLabel: label('title'),
    },
    presentation: {
      mode: 'list',
      listWidth: 330,
      listMinWidth: 240,
      listMaxWidth: 520,
      listWidthStorageKey: 'organization-units-tree',
    },
    advancedFilter: {
      fieldLabel: label('name'),
      getValue: (record) => record.name,
      matches: (record, value) =>
        `${record.name} ${record.nameAlias ?? ''}`
          .toLocaleLowerCase()
          .includes(value.trim().toLocaleLowerCase()),
    },
  };

  return <ListDetailsTreePage key={asOf} title={label('title')} config={config} />;
}
