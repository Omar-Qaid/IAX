import React from 'react';
import { Button, Stack, Typography } from '@mui/material';
import type { DetailSectionConfig } from '@patterns/list-details/types';
import {
  PartyPostalAddressPanel,
  PartyElectronicAddressPanel,
} from '@shared/components/logistics/PartyLogisticsPanels';
import { genders, statuses, type ContactRecord } from './contactPersonModel';

export function buildContactPersonHeaderSections({
  row,
  editing,
  more,
  setMore,
}: {
  row: ContactRecord;
  editing: boolean;
  more: boolean;
  setMore: React.Dispatch<React.SetStateAction<boolean>>;
}): DetailSectionConfig[] {
  return [
    {
      id: 'general',
      title: 'General',
      columns: 6,
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
            <Typography variant="caption">Save the contact to add contact information.</Typography>
          )}
          <PartyElectronicAddressPanel
            partyId={row.party}
            editing={editing && row.party > 0}
            storageKey="contactPerson.communication"
          />
        </Stack>
      ),
    },
  ];
}
