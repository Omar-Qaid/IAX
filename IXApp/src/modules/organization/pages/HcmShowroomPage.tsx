import React, { useState } from 'react';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { localizedName } from '@shared/utilities/localizedName';
import {
  PartyElectronicAddressPanel,
  PartyPostalAddressPanel,
} from '@shared/components/logistics/PartyLogisticsPanels';
import { HcmShowroomAssignmentsPanel } from '../components/HcmShowroomAssignmentsPanel';
import { hcmShowroomApi, type HcmShowroomRecord } from '../api/hcmShowroomApi';

const textValue = (value: string | number | boolean | undefined): string => String(value ?? '');

const emptyShowroom = (): HcmShowroomRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recordId: 0,
  personnelNumber: '',
  party: 0,
  name: '',
  nameAlias: '',
  description: '',
  isActive: true,
});

export function HcmShowroomPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  return <HcmShowroomContent key={company} company={company} />;
}

function HcmShowroomContent({ company }: { company: string }): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const [filtersVisible, setFiltersVisible] = useState(false);
  const config: EnterpriseListDetailsConfig<HcmShowroomRecord> = {
    recordTableName: 'HcmShowroom',
    onSearch: () => setFiltersVisible((visible) => !visible),
    dataSource: {
      type: 'remote',
      key: `hcm-showrooms-${company}`,
      load: (signal) => hcmShowroomApi.list(signal),
      create: hcmShowroomApi.create,
      update: hcmShowroomApi.update,
      delete: hcmShowroomApi.delete,
    },
    createRecord: emptyShowroom,
    numberSequence: { key: 'HcmShowroom', field: 'personnelNumber' },
    getPrimaryText: (record) => localizedName(record, isRtl),
    getSecondaryText: (record) => record.description ?? '',
    matchesSearch: (record, query) =>
      `${record.personnelNumber} ${record.name ?? ''} ${record.nameAlias ?? ''} ${record.description ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      nameAlias: record.nameAlias ?? '',
      description: record.description ?? '',
      isActive: record.isActive,
    }),
    setValues: (record, values) => ({
      ...record,
      nameAlias: textValue(values.nameAlias) || null,
      description: textValue(values.description) || null,
      isActive: Boolean(values.isActive),
    }),
    headerFields: [
      {
        id: 'personnelNumber',
        label: t('hcmShowrooms.fields.personnelNumber'),
        width: 180,
        disabled: true,
        getValue: (record) => record.personnelNumber,
        setValue: (record, value) => ({ ...record, personnelNumber: textValue(value) }),
      },
      {
        id: 'name',
        label: t('hcmShowrooms.fields.name'),
        width: 'minmax(320px, 560px)',
        getValue: (record) => record.name ?? '',
        getDisplayValue: (record) => localizedName(record, isRtl),
        setValue: (record, value) => ({ ...record, name: textValue(value) }),
      },
    ],
    sections: ({ record, editing }) => [
      {
        id: 'details',
        title: t('hcmShowrooms.sections.details'),
        groups: [
          {
            id: 'general',
            fields: [
              { name: 'nameAlias', label: t('hcmShowrooms.fields.nameAlias') },
              { name: 'description', label: t('hcmShowrooms.fields.description') },
              { name: 'isActive', label: t('hcmWorkers.fields.active'), type: 'boolean' },
            ],
          },
        ],
      },
      {
        id: 'primaryAssignments',
        title: t('hcmShowrooms.assignments.primaryTitle'),
        minHeight: 220,
        content: (
          <HcmShowroomAssignmentsPanel
            showroomId={record.recordId}
            company={company}
            showFilterRow={filtersVisible}
          />
        ),
      },
      {
        id: 'assignmentHistory',
        title: t('hcmShowrooms.assignments.historyTitle'),
        minHeight: 220,
        content: (
          <HcmShowroomAssignmentsPanel
            showroomId={record.recordId}
            company={company}
            history
            showFilterRow={filtersVisible}
          />
        ),
      },
      {
        id: 'addresses',
        title: t('hcmWorkers.sections.addresses'),
        minHeight: 145,
        content: (
          <PartyPostalAddressPanel
            partyId={record.party}
            editing={editing}
            storageKey="organization.showroom.addresses"
            showFilterRow={filtersVisible}
          />
        ),
      },
      {
        id: 'contacts',
        title: t('hcmWorkers.sections.contacts'),
        minHeight: 145,
        content: (
          <PartyElectronicAddressPanel
            partyId={record.party}
            editing={editing}
            storageKey="organization.showroom.contacts"
            showFilterRow={filtersVisible}
          />
        ),
      },
    ],
    permissions: {
      view: 'Organization.Showrooms.View',
      create: 'Organization.Showrooms.Create',
      edit: 'Organization.Showrooms.Edit',
      delete: 'Organization.Showrooms.Delete',
    },
    validate: (record): Record<string, string> => {
      if (!record.name?.trim()) {
        return { name: t('validation.required', { field: t('hcmShowrooms.fields.name') }) };
      }
      return {};
    },
    advancedFilter: {
      fieldLabel: t('hcmShowrooms.fields.name'),
      getValue: (record) => record.name ?? '',
      matches: (record, value) =>
        `${record.name ?? ''} ${record.nameAlias ?? ''}`
          .toLocaleLowerCase()
          .includes(value.trim().toLocaleLowerCase()),
    },
  };

  return <ListDetailsPage variant="enterprise" title={t('hcmShowrooms.title')} config={config} />;
}
