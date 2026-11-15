import React from 'react';
import {
  Alert,
  Box,
  Button,
  FormControlLabel,
  MenuItem,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useLocation } from 'react-router-dom';
import { apiClient } from '@core/api/apiClient';
import { useAuth } from '@core/auth/useAuth';
import { PERMISSIONS } from '@core/permissions/permissions';
import type { Contact } from './contactTypes';
import { MasterDetailPage } from '@patterns/master-detail/MasterDetailPage';
import { MasterDetailSection } from '@patterns/master-detail/MasterDetailSection';
import {
  PartyPostalAddressPanel,
  PartyElectronicAddressPanel,
} from '@shared/components/logistics/PartyLogisticsPanels';

function dateValue(year?: number | null, month?: number | null, day?: number | null) {
  return year && month && day
    ? [
        String(year).padStart(4, '0'),
        String(month).padStart(2, '0'),
        String(day).padStart(2, '0'),
      ].join('-')
    : '';
}

interface DetailsProps {
  account: string;
  endpoint: string;
  queryKey: unknown[];
  record?: Contact;
  contacts: Contact[];
  open: (id?: string) => void;
}
export function ContactPersonPage({
  account,
  endpoint,
  queryKey,
  record,
  contacts,
  open,
}: DetailsProps) {
  const { hasPermission } = useAuth();
  const cache = useQueryClient();
  const location = useLocation();
  const [editing, setEditing] = React.useState(
    !record || (Boolean(location.state?.edit) && hasPermission(PERMISSIONS.CONTACT_EDIT))
  );
  const [more, setMore] = React.useState(false);
  const [draft, setDraft] = React.useState({
    name: record?.name ?? '',
    nameAlias: record?.nameAlias ?? '',
    languageId: record?.languageId ?? 'en-US',
    addressBookNames: record?.addressBookNames ?? '',
    initials: record?.initials ?? '',
    birthday: dateValue(record?.birthYear, record?.birthMonth, record?.birthday),
    anniversary: dateValue(
      record?.anniversaryYear,
      record?.anniversaryMonth,
      record?.anniversaryDay
    ),
    inactive: record?.inactive ?? false,
  });
  const [nameParts, setNameParts] = React.useState(
    record?.nameParts ?? { firstName: '', middleName: '', lastNamePrefix: '', lastName: '' }
  );
  const structuredName =
    !record || Boolean(record.nameParts) || Object.values(nameParts).some(Boolean);
  const fullName = structuredName
    ? [nameParts.firstName, nameParts.middleName, nameParts.lastNamePrefix, nameParts.lastName]
        .map((s) => s.trim())
        .filter(Boolean)
        .join(' ')
    : draft.name;
  const genders = ['None', 'Male', 'Female', 'NonSpecific'];
  const statuses = ['None', 'Single', 'Married', 'Divorced', 'Widowed'];
  const enumIndex = (value: number | string | null | undefined, options: string[]) =>
    typeof value === 'string' ? options.indexOf(value) : (value ?? 0);
  const [gender, setGender] = React.useState(enumIndex(record?.gender, genders));
  const [maritalStatus, setMaritalStatus] = React.useState(
    enumIndex(record?.maritalStatus, statuses)
  );
  const save = useMutation({
    mutationFn: async () => {
      const body = {
        ...draft,
        name: fullName,
        nameParts: structuredName ? nameParts : null,
        gender,
        maritalStatus,
        birthday: draft.birthday || null,
        anniversary: draft.anniversary || null,
      };
      return record
        ? (await apiClient.put<number>(`${endpoint}/${record.recId}`, body)).data
        : (await apiClient.post<number>(endpoint, body)).data;
    },
    onSuccess: async (id) => {
      await cache.invalidateQueries({ queryKey });
      setEditing(false);
      if (!record) open(String(id));
    },
  });
  const remove = useMutation({
    mutationFn: () => apiClient.delete(`/v1/ContactPerson/${record!.recId}`),
    onSuccess: async () => {
      await cache.invalidateQueries({ queryKey });
      open();
    },
  });
  const fields = (field: Exclude<keyof typeof draft, 'inactive'>, label: string, type = 'text') => (
    <TextField
      size="small"
      label={label}
      type={type}
      value={draft[field]}
      disabled={!editing}
      slotProps={{ inputLabel: { shrink: true } }}
      onChange={(e) => setDraft({ ...draft, [field]: e.target.value })}
    />
  );
  const nameField = (key: keyof typeof nameParts, label: string) => (
    <TextField
      size="small"
      label={label}
      disabled={!editing}
      value={nameParts[key]}
      onChange={(event) => setNameParts({ ...nameParts, [key]: event.target.value })}
    />
  );
  const writable = hasPermission(record ? PERMISSIONS.CONTACT_EDIT : PERMISSIONS.CONTACT_CREATE);
  return (
    <MasterDetailPage
      records={contacts.map((r) => ({
        id: String(r.recId),
        title: r.contactPersonId,
        subtitle: r.name,
      }))}
      selectedId={record && String(record.recId)}
      onSelect={open}
      title={record?.name ?? 'New Record'}
      subtitle={`Contacts | ${account}`}
      filterLabel="Filter"
      emptyLabel="No contacts"
      viewLabel="Standard view"
      backAction={<Button onClick={() => open()}>Back</Button>}
      actionPane={
        <>
          {writable &&
            (editing ? (
              <Button
                disabled={!fullName.trim() || !draft.languageId.trim() || save.isPending}
                onClick={() => save.mutate()}
              >
                Save
              </Button>
            ) : (
              <Button onClick={() => setEditing(true)}>Edit</Button>
            ))}
          {record && hasPermission(PERMISSIONS.CONTACT_CREATE) && (
            <Button onClick={() => open('new')}>New</Button>
          )}
          {record && hasPermission(PERMISSIONS.CONTACT_DELETE) && (
            <Button
              disabled={remove.isPending}
              onClick={() => {
                if (window.confirm('Delete this contact?')) remove.mutate();
              }}
            >
              Delete
            </Button>
          )}
        </>
      }
    >
      {(save.error || remove.error) && (
        <Alert severity="error">{(save.error || remove.error)?.message}</Alert>
      )}
      <MasterDetailSection title="General">
        <Stack spacing={2}>
          <Box
            sx={{
              display: 'grid',
              gap: 2,
              alignItems: 'start',
              gridTemplateColumns: {
                xs: '1fr',
                sm: 'repeat(2, minmax(0, 1fr))',
                lg: 'repeat(5, minmax(0, 1fr))',
              },
            }}
          >
            <Stack spacing={2}>
              <Typography variant="overline">Contact</Typography>
              <TextField
                size="small"
                label="Contact ID"
                value={record?.contactPersonId ?? 'Automatic'}
                disabled
              />
              <TextField size="small" label="Contact for" value={account} disabled />
            </Stack>
            <Stack spacing={2}>
              <Typography variant="overline">Name details</Typography>
              {!structuredName && fields('name', 'Name')}
              {nameField('firstName', 'First name')}
              {nameField('middleName', 'Middle name')}
              {nameField('lastNamePrefix', 'Last name prefix')}
            </Stack>
            <Stack spacing={2} sx={{ pt: { lg: 6 } }}>
              {nameField('lastName', 'Last name')}
              {fields('nameAlias', 'Arabic name')}
              {more && fields('initials', 'Initials')}
            </Stack>
            <Stack spacing={2}>
              <Typography variant="overline">Other information</Typography>
              {fields('addressBookNames', 'Address books')}
              {fields('languageId', 'Language')}
            </Stack>
            <Stack spacing={2}>
              <Typography variant="overline">Personal information</Typography>
              <TextField
                select
                size="small"
                label="Gender"
                disabled={!editing}
                value={gender}
                onChange={(e) => setGender(Number(e.target.value))}
              >
                {genders.map((label, index) => (
                  <MenuItem key={label} value={index}>
                    {label}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                select
                size="small"
                label="Marital status"
                disabled={!editing}
                value={maritalStatus}
                onChange={(e) => setMaritalStatus(Number(e.target.value))}
              >
                {statuses.map((label, index) => (
                  <MenuItem key={label} value={index}>
                    {label}
                  </MenuItem>
                ))}
              </TextField>

              {more && (
                <>
                  {fields('birthday', 'Birthday', 'date')}
                  {fields('anniversary', 'Anniversary', 'date')}
                </>
              )}
              <Typography variant="overline">Status</Typography>
              <FormControlLabel
                label="Inactive"
                control={
                  <Switch
                    disabled={!editing}
                    checked={draft.inactive}
                    onChange={(e) => setDraft({ ...draft, inactive: e.target.checked })}
                  />
                }
              />
            </Stack>
          </Box>
          <Button sx={{ alignSelf: 'flex-start' }} onClick={() => setMore(!more)}>
            {more ? 'Show fewer fields' : 'Show more fields'}
          </Button>
        </Stack>
      </MasterDetailSection>
      {!record && (
        <MasterDetailSection title="Addresses" defaultExpanded={false}>
          <Typography variant="body2">Save the contact to add addresses.</Typography>
        </MasterDetailSection>
      )}
      {record && (
        <>
          <MasterDetailSection title="Addresses" defaultExpanded={false}>
            <PartyPostalAddressPanel
              partyId={record.party}
              editing={editing && writable}
              storageKey="contactPerson.addresses"
            />
          </MasterDetailSection>
          <MasterDetailSection title="Contact information" defaultExpanded={false}>
            <PartyElectronicAddressPanel
              partyId={record.party}
              editing={editing && writable}
              storageKey="contactPerson.communication"
            />
          </MasterDetailSection>
        </>
      )}
    </MasterDetailPage>
  );
}
