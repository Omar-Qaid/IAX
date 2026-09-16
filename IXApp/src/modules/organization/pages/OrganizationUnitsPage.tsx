import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Alert, MenuItem, Stack, TextField, Typography } from '@mui/material';
import { useAppStore } from '@app/store/useAppStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import { organizationStructureApi as api } from '../api/organizationStructureApi';

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
  const company = useAppStore((state) => state.currentCompany);
  return <OrganizationUnitsContent key={company} company={company} />;
}

function OrganizationUnitsContent({ company }: { company: string }): React.ReactElement {
  const { t } = useAppTranslation();
  const label = (key: string) => t(`organizationUnits.${key}`);
  const [asOf, setAsOf] = useState(today);
  const [selected, setSelected] = useState<UnitRecord | null>(null);
  const [hierarchy, setHierarchy] = useState('');
  const [selectedPositionIds, setSelectedPositionIds] = useState<(string | number)[]>([]);
  const positions = useQuery({
    queryKey: ['organization-structure', company, 'positions', asOf],
    queryFn: ({ signal }) => api.positions(asOf, signal),
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
          <TabularDetailPanel
            showFilterRow={false}
            rows={(positions.data ?? [])
              .filter((position) => position.organizationUnitId === record.recordId)
              .map((position) => ({ ...position, id: String(position.id) }))}
            columns={[
              { field: 'code', headerName: label('code'), width: 170 },
              { field: 'name', headerName: label('name'), flex: 1 },
              { field: 'validFrom', headerName: label('validFrom'), width: 140 },
              { field: 'validTo', headerName: label('validTo'), width: 140 },
            ]}
            addLabel={t('actions.new')}
            removeLabel={t('actions.delete')}
            selectedIds={selectedPositionIds}
            onSelectionChange={setSelectedPositionIds}
            disabled={editing || positions.isLoading}
            height={250}
            storageKey="organization-unit-positions"
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
