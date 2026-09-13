import React from 'react';
import { ListDetailsListGridPage } from '@patterns/list-details-listgrid/ListDetailsListGridPage';
import { ListGridField } from '@patterns/list-details-listgrid/ListGridField';
import type { EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { PERMISSIONS } from '@core/permissions/permissions';
import {
  customerPostingProfileApi as api,
  newPostingProfile,
  type CustomerPostingProfile,
} from '../api/customerPostingProfileApi';
import { CustomerPostingAccountsPanel } from '../components/CustomerPostingAccountsPanel';

export function CustomerPostingProfilesPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  return <CompanyProfiles key={company} company={company} />;
}
function CompanyProfiles({ company }: { company: string }): React.ReactElement {
  const { t } = useAppTranslation();
  const [lineEditing, setLineEditing] = React.useState(false);
  const [existing, setExisting] = React.useState(false);
  const label = (name: string) => t(`customerPostingProfiles.fields.${name}`);
  const config: EnterpriseListDetailsConfig<CustomerPostingProfile> = {
    interactionLocked: lineEditing,
    onSelectionChange: (record) => setExisting(Boolean(record?.recId)),
    recordTableName: 'CustLedger',
    showAttachmentAction: false,
    dataSource: {
      type: 'remote',
      key: `customer-posting-profiles:${company}`,
      load: api.list,
      create: api.create,
      update: api.update,
      delete: api.delete,
    },
    createRecord: () => newPostingProfile(company),
    getPrimaryText: (record) => record.postingProfile,
    getSecondaryText: (record) => record.name,
    matchesSearch: (record, query) =>
      `${record.postingProfile} ${record.name}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record) => ({
      settlement: record.settlement === 1,
      interest: record.interest === 1,
      collectionLetter: record.collectionLetter === 1,
      close: '',
    }),
    setValues: (record, values) => ({
      ...record,
      settlement: values.settlement ? 1 : 0,
      interest: values.interest ? 1 : 0,
      collectionLetter: values.collectionLetter ? 1 : 0,
    }),
    headerFields: [
      {
        id: 'postingProfile',
        label: label('postingProfile'),
        width: 98,
        disabled: existing,
        getValue: (record) => record.postingProfile,
        setValue: (record, value) =>
          record.recId ? record : { ...record, postingProfile: String(value) },
      },
      {
        id: 'name',
        label: label('name'),
        width: 206,
        getValue: (record) => record.name,
        setValue: (record, value) => ({ ...record, name: String(value) }),
      },
    ],
    sections: ({ record, editing }) => [
      {
        id: 'setup',
        title: t('customerPostingProfiles.setup'),
        defaultExpanded: true,
        detailsPadding: '9px 10px 16px',
        content: (
          <CustomerPostingAccountsPanel
            key={record.id}
            profile={record}
            masterEditing={editing}
            onEditingChange={setLineEditing}
          />
        ),
      },
      {
        id: 'restrictions',
        title: t('customerPostingProfiles.restrictions'),
        defaultExpanded: true,
        gridTemplateColumns: 'repeat(4,206px)',
        columnGap: '47px',
        groups: [
          ...(['settlement', 'interest', 'collectionLetter'] as const).map((name) => ({
            id: name,
            fields: [{ name, label: label(name), type: 'boolean' as const }],
          })),
          {
            id: 'close',
            fields: [
              {
                name: 'close',
                label: label('close'),
                type: 'display',
                disabled: true,
                renderOwnLabel: true,
                render: () => <ListGridField label={label('close')} value="" disabled underlined />,
              },
            ],
          },
        ],
      },
    ],
    permissions: {
      view: PERMISSIONS.CUSTOMER_POSTING_PROFILE_VIEW,
      create: PERMISSIONS.CUSTOMER_POSTING_PROFILE_CREATE,
      edit: PERMISSIONS.CUSTOMER_POSTING_PROFILE_EDIT,
      delete: PERMISSIONS.CUSTOMER_POSTING_PROFILE_DELETE,
    },
    validate: (record) => {
      const errors: Record<string, string> = {};
      for (const [field, max] of [
        ['postingProfile', 10],
        ['name', 60],
      ] as const) {
        if (!record[field].trim())
          errors[field] = t('validation.required', { field: label(field) });
        else if (record[field].length > max)
          errors[field] = t('customerPostingProfiles.maxLength', { field: label(field), max });
      }
      return errors;
    },
  };
  return <ListDetailsListGridPage title={t('customerPostingProfiles.title')} config={config} />;
}
