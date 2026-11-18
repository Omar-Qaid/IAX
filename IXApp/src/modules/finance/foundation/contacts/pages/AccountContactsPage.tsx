import React from 'react';
import { Alert, Box, Button, Stack, TextField } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { apiClient } from '@core/api/apiClient';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { PERMISSIONS } from '@core/permissions/permissions';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { ContactPersonPage } from './ContactPersonPage';
import type { Contact } from '../contactTypes';
import { accountContacts } from '../routes/contactRoutes';

export function AccountContactsPage() {
  const { kind = '', accountNum = '', contactId } = useParams();
  const company = useCompanyStore((state) => state.currentCompany);
  const navigate = useNavigate();
  const [filter, setFilter] = React.useState('');
  const { hasPermission } = useAuth();
  const [listError, setListError] = React.useState<string | null>(null);
  const key = ['ContactPerson', company, kind, accountNum];
  const endpoint = `/v1/ContactPerson/accounts/${encodeURIComponent(kind)}/${encodeURIComponent(accountNum)}`;
  const query = useQuery({
    queryKey: key,
    queryFn: async ({ signal }) =>
      (await apiClient.get<{ name: string | null; contacts: Contact[] }>(endpoint, { signal }))
        .data,
  });
  const rows = query.data?.contacts ?? [];
  const contextLabel = [accountNum, query.data?.name].filter(Boolean).join(' : ');
  const open = (id?: string) => navigate(accountContacts(kind, accountNum, id));
  if (query.isError)
    return (
      <Alert severity="error">
        {query.error.message} <Button onClick={() => void query.refetch()}>Retry</Button>
      </Alert>
    );
  if (contactId) {
    if (contactId === 'new' && !hasPermission(PERMISSIONS.CONTACT_CREATE))
      return <Alert severity="warning">You do not have permission to create contacts.</Alert>;
    if (query.isPending) return <Box>Loading contacts…</Box>;
    const record = rows.find((row) => String(row.recId) === contactId);
    if (contactId !== 'new' && !record)
      return <Alert severity="warning">Contact not found for this account.</Alert>;
    return (
      <ContactPersonPage
        key={JSON.stringify([...key, contactId])}
        account={contextLabel}
        endpoint={endpoint}
        queryKey={key}
        record={record}
        contacts={rows}
        open={open}
      />
    );
  }
  return (
    <SimpleListPage
      recordTableName="ContactPerson"
      enterpriseConfig={{
        contextLabel: `Contacts | ${contextLabel}`,
        viewLabel: 'Standard view',
        filterLabel: 'Filter',
        informationLabel: 'Information',
        searchFields: [{ field: 'name', label: 'Name' }],
        utilities: {
          personalizeLabel: 'Personalize',
          guideLabel: 'Guide',
          notificationsLabel: 'Notifications',
          refreshLabel: 'Refresh',
          openWindowLabel: 'Open in new window',
        },
        crud: {
          editLabel: 'Edit',
          newLabel: 'New contact',
          deleteLabel: 'Delete',
          editPermission: PERMISSIONS.CONTACT_EDIT,
          newPermission: PERMISSIONS.CONTACT_CREATE,
          deletePermission: PERMISSIONS.CONTACT_DELETE,
          onEdit: (row) =>
            navigate(accountContacts(kind, accountNum, row.id), { state: { edit: true } }),
          onNew: () => open('new'),
          onDelete: async (selected) => {
            setListError(null);
            try {
              for (const row of selected) await apiClient.delete(`/v1/ContactPerson/${row.recId}`);
            } catch (error) {
              setListError(error instanceof Error ? error.message : 'Unable to delete contact');
            } finally {
              await query.refetch();
            }
          },
        },
      }}
      belowGridContent={listError ? <Alert severity="error">{listError}</Alert> : undefined}
      title={`Contacts | ${contextLabel}`}
      dataSource={{
        type: 'controlled',
        rows: rows
          .filter((r) =>
            [r.name, r.contactPersonId, r.telephone]
              .join(' ')
              .toLowerCase()
              .includes(filter.trim().toLowerCase())
          )
          .map((r) => ({ ...r, id: String(r.recId), contactFor: query.data?.name || accountNum })),
        refresh: () => void query.refetch(),
      }}
      loading={query.isPending}
      columns={[
        {
          field: 'contactPersonId',
          headerName: 'Contact ID',
          renderCell: ({ row }) => (
            <Button onClick={() => open(row.id)}>{row.contactPersonId}</Button>
          ),
        },
        { field: 'name', headerName: 'Name', width: 200 },
        { field: 'contactFor', headerName: 'Contact for' },
        { field: 'telephone', headerName: 'Telephone' },
        { field: 'extension', headerName: 'Extension' },
        {
          field: 'inactive',
          headerName: 'Inactive',
          valueGetter: ({ row }) => (row.inactive ? 'Yes' : 'No'),
        },
        {
          field: 'vendorPortalAccessAllowed',
          headerName: 'Vendor collaboration access',
          valueGetter: ({ row }) => (row.vendorPortalAccessAllowed ? 'Yes' : 'No'),
        },
      ]}
      dataGridProps={{ storageKey: 'ContactPerson.list', mobilePresentation: 'table' }}
      filterBar={
        <Stack direction="row" spacing={1} sx={{ mx: 2.5, my: 1 }}>
          <TextField
            size="small"
            label="Filter"
            value={filter}
            onChange={(e) => setFilter(e.target.value)}
          />
        </Stack>
      }
    />
  );
}
