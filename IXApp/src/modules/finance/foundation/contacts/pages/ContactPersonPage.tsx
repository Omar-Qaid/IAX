import React from 'react';
import { Alert, Button, Stack, Typography } from '@mui/material';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { apiClient } from '@core/api/apiClient';
import { useAuth } from '@core/auth/useAuth';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { localizedName } from '@shared/utilities/localizedName';
import {
  PartyPostalAddressPanel,
  PartyElectronicAddressPanel,
} from '@shared/components/logistics/PartyLogisticsPanels';
import type { Contact } from '../contactTypes';
import { customerQuickCreateApi } from '@modules/finance/accounts-receivable/customer/api/customerQuickCreateApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '@modules/finance/accounts-receivable/routes/accountsReceivableRoutePaths';

interface DetailsProps {
  account: string;
  endpoint: string;
  queryKey: unknown[];
  record?: Contact;
  contacts: Contact[];
  open: (id?: string) => void;
}
interface ContactRecord extends Contact {
  id: string;
  firstName: string;
  middleName: string;
  lastNamePrefix: string;
  lastName: string;
  birthdayDate: string;
  anniversaryDate: string;
}
const genders = ['None', 'Male', 'Female', 'NonSpecific'];
const statuses = ['None', 'Single', 'Married', 'Divorced', 'Widowed'];
const enumIndex = (value: number | string | null | undefined, options: string[]) =>
  typeof value === 'string' ? Math.max(0, options.indexOf(value)) : (value ?? 0);
const dateValue = (year?: number | null, month?: number | null, day?: number | null) =>
  year && month && day
    ? `${String(year).padStart(4, '0')}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`
    : '';
const toRecord = (contact: Contact): ContactRecord => ({
  ...contact,
  id: String(contact.recId),
  firstName: contact.nameParts?.firstName ?? '',
  middleName: contact.nameParts?.middleName ?? '',
  lastNamePrefix: contact.nameParts?.lastNamePrefix ?? '',
  lastName: contact.nameParts?.lastName ?? '',
  birthdayDate: dateValue(contact.birthYear, contact.birthMonth, contact.birthday),
  anniversaryDate: dateValue(
    contact.anniversaryYear,
    contact.anniversaryMonth,
    contact.anniversaryDay
  ),
});
const emptyContact = (): ContactRecord =>
  toRecord({
    recId: 0,
    contactPersonId: '',
    party: 0,
    name: '',
    nameAlias: '',
    languageId: 'en-US',
    addressBookNames: '',
    initials: '',
    birthYear: null,
    birthMonth: null,
    birthday: null,
    anniversaryYear: null,
    anniversaryMonth: null,
    anniversaryDay: null,
    telephone: null,
    extension: null,
    nameParts: { firstName: '', middleName: '', lastNamePrefix: '', lastName: '' },
    gender: 0,
    maritalStatus: 0,
    inactive: false,
    vendorPortalAccessAllowed: false,
  });
const namePartsOf = (row: ContactRecord) => ({
  firstName: row.firstName,
  middleName: row.middleName,
  lastNamePrefix: row.lastNamePrefix,
  lastName: row.lastName,
});
const fullName = (row: ContactRecord) =>
  row.nameParts || Object.values(namePartsOf(row)).some(Boolean)
    ? Object.values(namePartsOf(row))
        .map((value) => value.trim())
        .filter(Boolean)
        .join(' ')
    : row.name;

/** Same enterprise page configuration and lifecycle as CustTablePage. */
export function ContactPersonPage({
  account,
  endpoint,
  queryKey,
  record,
  contacts,
  open,
}: DetailsProps) {
  const cache = useQueryClient();
  const { hasPermission } = useAuth();
  const { isRtl } = useAppTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const { kind, accountNum } = useParams();
  const [more, setMore] = React.useState(false);
  const sourceKey = JSON.stringify([...queryKey, record?.recId ?? 'new']);
  const customerNavigation = useMutation({
    mutationFn: async () => {
      const customers = await cache.fetchQuery({
        queryKey: ['contact-owner-customer', ...queryKey],
        queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
      });
      const customer = customers.find((row) => row.accountNumber === accountNum);
      if (!customer) throw new Error('Customer not found for this contact account.');
      return customer;
    },
    onSuccess: (customer) =>
      navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customer(String(customer.recId))),
  });
  const load = async (signal?: AbortSignal) =>
    (await apiClient.get<{ contacts: Contact[] }>(endpoint, { signal })).data.contacts.map(
      toRecord
    );
  const persist = async (row: ContactRecord, create: boolean): Promise<ContactRecord> => {
    const body = {
      name: fullName(row),
      nameAlias: row.nameAlias,
      languageId: row.languageId,
      addressBookNames: row.addressBookNames,
      initials: row.initials,
      nameParts:
        row.nameParts || Object.values(namePartsOf(row)).some(Boolean) ? namePartsOf(row) : null,
      gender: enumIndex(row.gender, genders),
      maritalStatus: enumIndex(row.maritalStatus, statuses),
      birthday: row.birthdayDate || null,
      anniversary: row.anniversaryDate || null,
      inactive: row.inactive,
    };
    const result = create
      ? await apiClient.post<number>(endpoint, body)
      : await apiClient.put<number>(`${endpoint}/${row.recId}`, body);
    await cache.invalidateQueries({ queryKey });
    const refreshed = await load();
    return (
      refreshed.find((item) => item.recId === Number(result.data)) ?? {
        ...row,
        recId: Number(result.data),
        id: String(result.data),
        name: body.name,
      }
    );
  };
  const status = useMutation({
    mutationFn: (row: ContactRecord) => persist(row, false),
    onSuccess: async () => {
      await cache.invalidateQueries({ queryKey: ['list-details', sourceKey] });
    },
  });
  const initialRecords = contacts.map(toRecord);
  const config: EnterpriseListDetailsConfig<ContactRecord> = {
    recordTableName: 'ContactPerson',
    getAuditRecordId: (row) => row.recId,
    initialMode: !record
      ? 'new'
      : location.state?.edit && hasPermission(PERMISSIONS.CONTACT_EDIT)
        ? 'edit'
        : 'view',
    initialSelectedId: record ? String(record.recId) : '__new__',
    interactionLocked: status.isPending || customerNavigation.isPending,
    dataSource: {
      type: 'remote',
      key: sourceKey,
      initialRecords,
      load,
      create: (row) => persist(row, true),
      update: (row) => persist(row, false),
      delete: async (row) => {
        await apiClient.delete(`/v1/ContactPerson/${row.recId}`);
        await cache.invalidateQueries({ queryKey });
        open();
      },
    },
    createRecord: emptyContact,
    getPrimaryText: (row) => localizedName(row, isRtl),
    getSecondaryText: (row) => row.contactPersonId,
    matchesSearch: (row, query) =>
      `${row.contactPersonId} ${row.name} ${row.nameAlias}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (row) => ({
      contactPersonId: row.contactPersonId || 'Automatic',
      account,
      name: row.name,
      firstName: row.firstName,
      middleName: row.middleName,
      lastNamePrefix: row.lastNamePrefix,
      lastName: row.lastName,
      nameAlias: row.nameAlias,
      languageId: row.languageId,
      addressBookNames: row.addressBookNames,
      initials: row.initials ?? '',
      gender: enumIndex(row.gender, genders),
      maritalStatus: enumIndex(row.maritalStatus, statuses),
      birthdayDate: row.birthdayDate,
      anniversaryDate: row.anniversaryDate,
      inactive: row.inactive,
    }),
    setValues: (row, values: DetailValues) => ({
      ...row,
      name: String(values.name ?? ''),
      firstName: String(values.firstName ?? ''),
      middleName: String(values.middleName ?? ''),
      lastNamePrefix: String(values.lastNamePrefix ?? ''),
      lastName: String(values.lastName ?? ''),
      nameAlias: String(values.nameAlias ?? ''),
      languageId: String(values.languageId ?? ''),
      addressBookNames: String(values.addressBookNames ?? ''),
      initials: String(values.initials ?? ''),
      gender: Number(values.gender),
      maritalStatus: Number(values.maritalStatus),
      birthdayDate: String(values.birthdayDate ?? ''),
      anniversaryDate: String(values.anniversaryDate ?? ''),
      inactive: Boolean(values.inactive),
    }),
    headerFields: [
      {
        id: 'summary',
        label: 'Contacts',
        type: 'display',
        getValue: (row) => `${row.contactPersonId || 'New Record'} | ${account}`,
        setValue: (row) => row,
      },
    ],
    sections: ({ record: row, editing }) => [
      {
        id: 'general',
        title: 'General',
        columns: 6,
        minHeight: 320,
        groups: [
          {
            id: 'contact',
            title: 'Contact',
            fields: [
              { name: 'contactPersonId', label: 'Contact ID', disabled: true },
              { name: 'account', label: 'Contact for', disabled: true },
            ],
          },
          {
            id: 'name',
            fields: [
              ...(!row.nameParts && row.recId > 0 ? [{ name: 'name', label: 'Name' }] : []),
              { name: 'firstName', label: 'First name' },
              { name: 'middleName', label: 'Middle name' },
              { name: 'lastNamePrefix', label: 'Last name prefix' },
            ],
          },
          {
            id: 'last-name',
            fields: [
              { name: 'lastName', label: 'Last name' },
              { name: 'nameAlias', label: 'Arabic name' },
            ],
          },
          {
            id: 'other',
            title: 'Other information',
            fields: [
              { name: 'addressBookNames', label: 'Address books' },
              { name: 'languageId', label: 'Language' },
              ...(more ? [{ name: 'initials', label: 'Initials' }] : []),
            ],
          },
          {
            id: 'personal',
            title: 'Personal information',
            fields: [
              {
                name: 'gender',
                label: 'Gender',
                type: 'select',
                options: genders.map((label, value) => ({ label, value: String(value) })),
              },
              {
                name: 'maritalStatus',
                label: 'Marital status',
                type: 'select',
                options: statuses.map((label, value) => ({ label, value: String(value) })),
              },
              ...(more ? [{ name: 'birthdayDate', label: 'Birthday', type: 'date' as const }] : []),
            ],
          },
          {
            id: 'status',
            title: 'Status',
            fields: [
              { name: 'inactive', label: 'Inactive', type: 'boolean' },
              ...(more
                ? [{ name: 'anniversaryDate', label: 'Anniversary', type: 'date' as const }]
                : []),
            ],
          },
        ],
        link: (
          <Button onClick={() => setMore((value) => !value)}>
            {more ? 'Show fewer fields' : 'Show more fields'}
          </Button>
        ),
      },
      {
        id: 'addresses',
        title: 'Addresses',
        content: (
          <Stack>
            {!row.party && (
              <Typography variant="caption">Save the contact to add addresses.</Typography>
            )}
            <PartyPostalAddressPanel
              partyId={row.party}
              editing={editing && row.party > 0}
              storageKey="contactPerson.addresses"
            />
          </Stack>
        ),
      },
      {
        id: 'communication',
        title: 'Contact information',
        content: (
          <Stack>
            {!row.party && (
              <Typography variant="caption">
                Save the contact to add contact information.
              </Typography>
            )}
            <PartyElectronicAddressPanel
              partyId={row.party}
              editing={editing && row.party > 0}
              storageKey="contactPerson.communication"
            />
          </Stack>
        ),
      },
    ],
    commands: (row) => [
      {
        id: 'contact',
        label: 'Contact',
        requiresSelection: true,
        ribbonPersistenceKey: 'contactPerson.ribbon',
        groups: [
          {
            id: 'maintain',
            label: 'Maintain',
            actions: [
              {
                id: 'inactivate',
                label: 'Inactivate',
                disabled: !row || !hasPermission(PERMISSIONS.CONTACT_EDIT) || row.inactive,
                onClick: () => {
                  if (row) status.mutate({ ...row, inactive: true });
                },
              },
              {
                id: 'activate',
                label: 'Activate',
                disabled: !row || !hasPermission(PERMISSIONS.CONTACT_EDIT) || !row.inactive,
                onClick: () => {
                  if (row) status.mutate({ ...row, inactive: false });
                },
              },
            ],
          },
          {
            id: 'accounts',
            label: 'Accounts',
            actions: [
              {
                id: 'customer',
                label: 'Customer',
                disabled:
                  kind !== 'customer' ||
                  !hasPermission(PERMISSIONS.CUSTOMER_VIEW) ||
                  customerNavigation.isPending,
                onClick: () => customerNavigation.mutate(),
              },
              { id: 'vendor', label: 'Vendor', disabled: true },
            ],
          },
          {
            id: 'related',
            label: 'Related information',
            actions: [{ id: 'contacts', label: 'View contacts', onClick: () => open() }],
          },
        ],
      },
    ],
    permissions: {
      view: PERMISSIONS.CONTACT_VIEW,
      create: PERMISSIONS.CONTACT_CREATE,
      edit: PERMISSIONS.CONTACT_EDIT,
      delete: PERMISSIONS.CONTACT_DELETE,
    },
    validate: (row) => ({
      ...(!fullName(row).trim() ? { firstName: 'Contact name is required.' } : {}),
      ...(!row.languageId.trim() ? { languageId: 'Language is required.' } : {}),
    }),
    onSelectionChange: (row) => {
      if (row && row.recId !== record?.recId) open(row.id);
    },
    onCancel: !record ? () => open() : undefined,
    backCommand: { onClick: () => open() },
    detailHeader:
      status.error || customerNavigation.error ? (
        <Alert severity="error">{(status.error || customerNavigation.error)?.message}</Alert>
      ) : undefined,
    presentation: {
      mode: 'list',
      listInitiallyVisible: Boolean(record),
      listWidth: 232,
      listMinWidth: 200,
      listMaxWidth: 360,
      listResizable: true,
      compactRecordHeader: true,
      recordHeaderMinHeight: 42,
    },
  };
  return <ListDetailsPage variant="enterprise" title="Contacts" config={config} />;
}
