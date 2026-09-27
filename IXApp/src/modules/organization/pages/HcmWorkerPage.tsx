import React, { useMemo, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { localizedName } from '@shared/utilities/localizedName';
import { hcmWorkerApi, type HcmLookupOption, type HcmWorkerRecord } from '../api/hcmWorkerApi';
import {
  PartyPostalAddressPanel,
  PartyElectronicAddressPanel,
} from '@shared/components/logistics/PartyLogisticsPanels';
import { HcmWorkerAssignmentsPanel } from '../components/HcmWorkerAssignmentsPanel';
import { HcmWorkerAssignmentDiagram } from '../components/HcmWorkerAssignmentDiagram';

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
  managerWorkerId: null,
  departmentId: null,
  showroomId: null,
  genderId: 0,
  nationalityId: 0,
  hireDate: null,
  birthDate: null,
  userId: null,
  isActive: true,
});

export function HcmWorkerPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  return <HcmWorkerContent key={company} company={company} />;
}

function HcmWorkerContent({ company }: { company: string }): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const queryClient = useQueryClient();
  const [assignmentFiltersVisible, setAssignmentFiltersVisible] = useState(false);
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
    departments: useQuery({
      queryKey: ['hcm-workers', company, 'departments'],
      queryFn: ({ signal }) => hcmWorkerApi.lookup('HcmDepartment', signal),
    }),
    showrooms: useQuery({
      queryKey: ['hcm-workers', company, 'showrooms'],
      queryFn: ({ signal }) => hcmWorkerApi.lookup('HcmShowroom', signal),
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
          name: localizedName(option, isRtl),
          description: [option.name, option.nameAlias].filter(Boolean).join(' '),
        }))}
        required={required}
        disabled={disabled || loading}
        displayMode="select"
        sideMode="client"
        searchable
        lazyLoading
        pageSize={25}
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
            id: 'personal',
            title: t('hcmWorkers.groups.personal'),
            fields: [
              { name: 'nameAlias', label: t('hcmWorkers.fields.nameAlias') },
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
            id: 'organization',
            title: t('hcmWorkers.groups.organization'),
            fields: [
              {
                name: 'managerWorkerId',
                label: t('hcmWorkers.assignments.manager'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupField
                    name="managerWorkerId"
                    label={t('hcmWorkers.assignments.manager')}
                    value={numberValue(value)}
                    onChange={(next) => onChange(Number(next) || 0)}
                    fetchPage={({ pageNumber, pageSize, search, signal }) =>
                      hcmWorkerApi.managerLookup({
                        pageNumber,
                        pageSize,
                        search,
                        selectedId: numberValue(value),
                        signal,
                      })
                    }
                    queryKey={['hcm-workers', company, 'manager-lookup', numberValue(value)]}
                    required={false}
                    disabled={disabled}
                    displayMode="select"
                    sideMode="server"
                    searchable
                    lazyLoading
                    pageSize={25}
                  />
                ),
              },
              lookupField(
                'departmentId',
                t('hcmWorkers.fields.department'),
                lookups.departments.data ?? [],
                lookups.departments.isLoading,
                false
              ),
              lookupField(
                'occupationId',
                t('hcmWorkers.fields.occupation'),
                lookups.occupations.data ?? [],
                lookups.occupations.isLoading
              ),
              lookupField(
                'showroomId',
                t('hcmWorkers.fields.showroom'),
                lookups.showrooms.data ?? [],
                lookups.showrooms.isLoading,
                false
              ),
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
      lookups.departments.data,
      lookups.departments.isLoading,
      lookups.occupations.data,
      lookups.occupations.isLoading,
      lookups.showrooms.data,
      lookups.showrooms.isLoading,
      isRtl,
      t,
    ]
  );

  const refreshAssignmentHistory = async (workerId: number): Promise<void> => {
    await Promise.all([
      queryClient.invalidateQueries({
        queryKey: ['hcm-worker-organization-assignments-v1', company, workerId],
      }),
      queryClient.invalidateQueries({
        queryKey: ['hcm-worker-showroom-assignments', company, workerId],
      }),
      queryClient.invalidateQueries({
        queryKey: ['hcm-worker-assignment-chain', company, workerId],
      }),
    ]);
  };

  const createWorker = async (record: HcmWorkerRecord): Promise<HcmWorkerRecord> => {
    const saved = await hcmWorkerApi.create(record);
    await refreshAssignmentHistory(saved.recordId);
    return saved;
  };

  const updateWorker = async (record: HcmWorkerRecord): Promise<HcmWorkerRecord> => {
    const saved = await hcmWorkerApi.update(record);
    await refreshAssignmentHistory(saved.recordId);
    return saved;
  };

  const config: EnterpriseListDetailsConfig<HcmWorkerRecord> = {
    recordTableName: 'HcmWorker',
    onSearch: () => setAssignmentFiltersVisible((visible) => !visible),
    dataSource: {
      type: 'remote',
      key: `hcm-workers-${company}`,
      load: (signal) => hcmWorkerApi.list(signal),
      create: createWorker,
      update: updateWorker,
      delete: hcmWorkerApi.delete,
    },
    createRecord: emptyWorker,
    numberSequence: { key: 'HcmWorker', field: 'personnelNumber' },
    getPrimaryText: (record) => localizedName(record, isRtl) || record.personnelNumber,
    getSecondaryText: (record) => record.personnelNumber,
    matchesSearch: (record, query) =>
      `${record.personnelNumber} ${record.name ?? ''} ${record.nameAlias ?? ''} ${record.occupationName ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      occupationId: record.occupationId,
      managerWorkerId: record.managerWorkerId ?? 0,
      departmentId: record.departmentId ?? 0,
      showroomId: record.showroomId ?? 0,
      genderId: record.genderId,
      nationalityId: record.nationalityId,
      nameAlias: record.nameAlias ?? '',
      hireDate: dateValue(record.hireDate),
      birthDate: dateValue(record.birthDate),
      isActive: record.isActive,
    }),
    setValues: (record, values) => ({
      ...record,
      occupationId: numberValue(values.occupationId),
      managerWorkerId: numberValue(values.managerWorkerId) || null,
      departmentId: numberValue(values.departmentId) || null,
      showroomId: numberValue(values.showroomId) || null,
      genderId: numberValue(values.genderId),
      nationalityId: numberValue(values.nationalityId),
      nameAlias: textValue(values.nameAlias) || null,
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
        getDisplayValue: (record) => localizedName(record, isRtl),
        setValue: (record, value) => ({ ...record, name: textValue(value) }),
      },
    ],
    sections: ({ record, editing }) => [
      ...sections,
      {
        id: 'assignmentDiagram',
        title: t('hcmWorkers.assignments.diagramTitle'),
        minHeight: 220,
        content: (
          <HcmWorkerAssignmentDiagram
            key={record.recordId}
            workerId={record.recordId}
            company={company}
          />
        ),
      },
      {
        id: 'organizationAssignments',
        title: t('hcmWorkers.assignments.organizationTitle'),
        minHeight: 220,
        content: (
          <HcmWorkerAssignmentsPanel
            key={record.recordId}
            workerId={record.recordId}
            company={company}
            type="organization"
            editing={editing}
            showFilterRow={assignmentFiltersVisible}
          />
        ),
      },
      {
        id: 'showroomAssignments',
        title: t('hcmWorkers.assignments.showroomTitle'),
        minHeight: 220,
        content: (
          <HcmWorkerAssignmentsPanel
            key={record.recordId}
            workerId={record.recordId}
            company={company}
            type="showroom"
            editing={editing}
            showFilterRow={assignmentFiltersVisible}
          />
        ),
      },
      {
        id: 'addresses',
        title: t('hcmWorkers.sections.addresses'),
        minHeight: 145,
        content: (
          <PartyPostalAddressPanel
            partyId={record.person}
            editing={editing}
            storageKey="organization.worker.addresses"
            showFilterRow={assignmentFiltersVisible}
          />
        ),
      },
      {
        id: 'contacts',
        title: t('hcmWorkers.sections.contacts'),
        minHeight: 145,
        content: (
          <PartyElectronicAddressPanel
            partyId={record.person}
            editing={editing}
            storageKey="organization.worker.contacts"
            showFilterRow={assignmentFiltersVisible}
          />
        ),
      },
    ],
    permissions: {
      view: 'Organization.Employees.View',
      create: 'Organization.Employees.Create',
      edit: 'Organization.Employees.Edit',
      delete: 'Organization.Employees.Delete',
    },
    validate: (record) => ({
      ...(!record.name?.trim()
        ? { name: t('validation.required', { field: t('hcmWorkers.fields.name') }) }
        : {}),
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
        `${record.name ?? record.personnelNumber} ${record.nameAlias ?? ''}`
          .toLocaleLowerCase()
          .includes(value.trim().toLocaleLowerCase()),
    },
  };

  return <ListDetailsPage variant="enterprise" title={t('hcmWorkers.title')} config={config} />;
}
