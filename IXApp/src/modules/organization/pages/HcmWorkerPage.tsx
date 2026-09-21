import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useAppStore } from '@app/store/useAppStore';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { hcmWorkerApi, type HcmLookupOption, type HcmWorkerRecord } from '../api/hcmWorkerApi';
import { PartyPostalAddressPanel, PartyElectronicAddressPanel } from '@shared/components/logistics/PartyLogisticsPanels';
import { HcmWorkerAssignmentsPanel } from '../components/HcmWorkerAssignmentsPanel';

const numberValue = (value: DetailValue | undefined): number => Number(value) || 0;
const textValue = (value: DetailValue | undefined): string => String(value ?? '');
const dateValue = (value: string | null): string => value?.slice(0, 10) ?? '';

const emptyWorker = (): HcmWorkerRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recordId: 0,
  personnelNumber: '',
  person: 0,
  name: null,
  nameAlias: null,
  occupationId: 0,
  genderId: 0,
  nationalityId: 0,
  hireDate: null,
  birthDate: null,
  userId: null,
  isActive: true,
});

export function HcmWorkerPage(): React.ReactElement {
  const company = useAppStore((state) => state.currentCompany);
  return <HcmWorkerContent key={company} company={company} />;
}

function HcmWorkerContent({ company }: { company: string }): React.ReactElement {
  const { t } = useAppTranslation();
  const lookups = {
    occupations: useQuery({
      queryKey: ['hcm-workers', company, 'occupations'],
      queryFn: ({ signal }) => hcmWorkerApi.lookup('Occupation', signal),
    }),
    genders: useQuery({
      queryKey: ['hcm-workers', company, 'genders'],
      queryFn: ({ signal }) => hcmWorkerApi.lookup('Gender', signal),
    }),
    nationalities: useQuery({
      queryKey: ['hcm-workers', company, 'nationalities'],
      queryFn: ({ signal }) => hcmWorkerApi.lookup('Nationality', signal),
    }),
  };

  const lookupField = (
    name: string,
    label: string,
    options: HcmLookupOption[],
    loading: boolean,
    required = true
  ) => ({
    name,
    label,
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
        name={name}
        label={label}
        value={numberValue(value)}
        onChange={(next) => onChange(Number(next) || 0)}
        options={options.map((option) => ({
          id: option.id,
          code: option.code ?? '',
          name: option.name ?? '',
        }))}
        required={required}
        disabled={disabled || loading}
        displayMode="select"
      />
    ),
  });

  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'employment',
        title: t('hcmWorkers.sections.employment'),
        groups: [
          {
            id: 'organization',
            title: t('hcmWorkers.groups.organization'),
            fields: [
              lookupField(
                'occupationId',
                t('hcmWorkers.fields.occupation'),
                lookups.occupations.data ?? [],
                lookups.occupations.isLoading
              ),
            ],
          },
          {
            id: 'personal',
            title: t('hcmWorkers.groups.personal'),
            fields: [
              lookupField(
                'genderId',
                t('hcmWorkers.fields.gender'),
                lookups.genders.data ?? [],
                lookups.genders.isLoading
              ),
              lookupField(
                'nationalityId',
                t('hcmWorkers.fields.nationality'),
                lookups.nationalities.data ?? [],
                lookups.nationalities.isLoading
              ),
              { name: 'birthDate', label: t('hcmWorkers.fields.birthDate'), type: 'date' },
            ],
          },
          {
            id: 'dates',
            title: t('hcmWorkers.groups.dates'),
            fields: [
              { name: 'hireDate', label: t('hcmWorkers.fields.hireDate'), type: 'date' },
              { name: 'isActive', label: t('hcmWorkers.fields.active'), type: 'boolean' },
            ],
          },
        ],
      },
    ],
    [
      lookups.genders.data,
      lookups.genders.isLoading,
      lookups.nationalities.data,
      lookups.nationalities.isLoading,
      lookups.occupations.data,
      lookups.occupations.isLoading,
      t,
    ]
  );

  const config: EnterpriseListDetailsConfig<HcmWorkerRecord> = {
    recordTableName: 'HcmWorker',
    dataSource: {
      type: 'remote',
      key: `hcm-workers-${company}`,
      load: (signal) => hcmWorkerApi.list(signal),
      create: hcmWorkerApi.create,
      update: hcmWorkerApi.update,
      delete: hcmWorkerApi.delete,
    },
    createRecord: emptyWorker,
    numberSequence: { key: 'HcmWorker', field: 'personnelNumber' },
    getPrimaryText: (record) => record.name?.trim() || record.personnelNumber,
    getSecondaryText: (record) => record.personnelNumber,
    matchesSearch: (record, query) =>
      `${record.personnelNumber} ${record.name ?? ''} ${record.nameAlias ?? ''} ${record.occupationName ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      occupationId: record.occupationId,
      genderId: record.genderId,
      nationalityId: record.nationalityId,
      hireDate: dateValue(record.hireDate),
      birthDate: dateValue(record.birthDate),
      isActive: record.isActive,
    }),
    setValues: (record, values) => ({
      ...record,
      occupationId: numberValue(values.occupationId),
      genderId: numberValue(values.genderId),
      nationalityId: numberValue(values.nationalityId),
      hireDate: textValue(values.hireDate) || null,
      birthDate: textValue(values.birthDate) || null,
      isActive: Boolean(values.isActive),
    }),
    headerFields: [
      {
        id: 'personnelNumber',
        label: t('hcmWorkers.fields.personnelNumber'),
        width: 180,
        disabled: true,
        getValue: (record) => record.personnelNumber,
        setValue: (record, value) => ({ ...record, personnelNumber: textValue(value) }),
      },
      {
        id: 'name',
        label: t('hcmWorkers.fields.name'),
        width: 'minmax(320px, 520px)',
        getValue: (record) => record.name ?? '',
        setValue: (record, value) => ({ ...record, name: textValue(value) }),
      },
    ],
    sections: ({ record, editing }) => [
      ...sections,
      {
        id: 'organizationAssignments',
        title: 'Organization assignments',
        minHeight: 220,
        content: <HcmWorkerAssignmentsPanel workerId={record.recordId} company={company} />,
      },
      {
        id: 'addresses',
        title: t('customerDetails.sections.addresses', 'Addresses'),
        minHeight: 145,
        content: <PartyPostalAddressPanel partyId={record.person} editing={editing} storageKey="organization.worker.addresses" />,
      },
      {
        id: 'contacts',
        title: t('customerDetails.sections.contacts', 'Contact information'),
        minHeight: 145,
        content: <PartyElectronicAddressPanel partyId={record.person} editing={editing} storageKey="organization.worker.contacts" />,
      },
    ],
    permissions: {
      view: 'Organization.Employees.View',
      create: 'Organization.Employees.Create',
      edit: 'Organization.Employees.Edit',
      delete: 'Organization.Employees.Delete',
    },
    validate: (record) => ({
      ...(record.occupationId <= 0
        ? { occupationId: t('validation.required', { field: t('hcmWorkers.fields.occupation') }) }
        : {}),
      ...(record.genderId <= 0
        ? { genderId: t('validation.required', { field: t('hcmWorkers.fields.gender') }) }
        : {}),
      ...(record.nationalityId <= 0
        ? { nationalityId: t('validation.required', { field: t('hcmWorkers.fields.nationality') }) }
        : {}),
    }),
    advancedFilter: {
      fieldLabel: t('hcmWorkers.fields.name'),
      getValue: (record) => record.name ?? record.personnelNumber,
      matches: (record, value) =>
        (record.name ?? record.personnelNumber)
          .toLocaleLowerCase()
          .includes(value.trim().toLocaleLowerCase()),
    },
  };

  return <ListDetailsPage variant="enterprise" title={t('hcmWorkers.title')} config={config} />;
}
