import React from 'react';
import { Alert, Box, Button, CircularProgress, Typography } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { usePermission } from '@core/permissions/usePermission';
import { PERMISSIONS } from '@core/permissions/permissions';
import { ListGridDetailsPanel } from '@patterns/list-details-listgrid/ListDetailsListGridPage';
import { ListGridField } from '@patterns/list-details-listgrid/ListGridField';
import { listGridControlSx, listGridLabelSx } from '@patterns/list-details-listgrid/fieldStyles';
import { LookupField } from '@shared/components/lookups/LookupField';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import type { ColumnDef } from '@shared/components/data-grid/types';
import {
  customerPostingAccountApi as api,
  newPostingAccount,
  type CustomerPostingAccount,
  type CustomerPostingProfile,
} from '../api/customerPostingProfileApi';

export function CustomerPostingAccountsPanel({
  profile,
  masterEditing,
  onEditingChange,
}: {
  profile: CustomerPostingProfile;
  masterEditing: boolean;
  onEditingChange: (editing: boolean) => void;
}): React.ReactElement {
  const { t } = useAppTranslation();
  const client = useQueryClient();
  const canCreate = usePermission(PERMISSIONS.CUSTOMER_POSTING_PROFILE_CREATE).hasPermission;
  const canEdit = usePermission(PERMISSIONS.CUSTOMER_POSTING_PROFILE_EDIT).hasPermission;
  const canDelete = usePermission(PERMISSIONS.CUSTOMER_POSTING_PROFILE_DELETE).hasPermission;
  const queryKey = ['customer-posting-accounts', profile.dataAreaId, profile.postingProfile];
  const accounts = useQuery({
    queryKey,
    queryFn: ({ signal }) => api.forProfile(profile.postingProfile, signal),
    enabled: profile.recId > 0,
  });
  const [selectedId, setSelectedId] = React.useState<string | null>(null);
  const [draft, setDraft] = React.useState<CustomerPostingAccount | null>(null);
  const [busy, setBusy] = React.useState(false);
  const [error, setError] = React.useState('');
  const [deleting, setDeleting] = React.useState(false);
  const rows = accounts.data ?? [];
  const selected = draft ?? rows.find((row) => row.id === selectedId) ?? rows[0] ?? null;
  const editing = draft != null;
  const code = selected?.accountCode ?? 2;
  const references = useQuery({
    queryKey: ['customer-posting-references', profile.dataAreaId, code],
    queryFn: ({ signal }) => api.references(code, signal),
    enabled: editing && code !== 2,
  });
  useUnsavedChanges(editing || busy);
  React.useEffect(() => {
    onEditingChange(editing || busy);
    return () => onEditingChange(false);
  }, [editing, busy, onEditingChange]);
  const label = (field: string) => t(`customerPostingProfiles.fields.${field}`);
  const options = [0, 1, 2].map((value) => ({
    value: String(value),
    label: t(`customerPostingProfiles.accountCodes.${value}`),
  }));
  const columns: ColumnDef<CustomerPostingAccount>[] = [
    {
      field: 'accountCode',
      headerName: 'customerPostingProfiles.fields.accountCode',
      width: 147,
      renderCell: ({ row }) =>
        options.find((option) => option.value === String(row.accountCode))?.label ??
        row.accountCode,
    },
    { field: 'num', headerName: 'customerPostingProfiles.fields.num', minWidth: 175, flex: 1 },
  ];
  const begin = (row: CustomerPostingAccount) => {
    setDraft({ ...row });
    setError('');
    onEditingChange(true);
  };
  const cancel = () => {
    setDraft(null);
    setError('');
    onEditingChange(false);
  };
  const save = async () => {
    if (!draft || busy) return;
    setError('');
    if (draft.num.length > 20 || draft.collectionLetterCourse.length > 10) {
      setError(t('customerPostingProfiles.invalidLength'));
      return;
    }
    const dimensions = [
      'summaryLedgerDimension',
      'clearingLedgerDimension',
      'vatPrepaymentsLedgerDimension',
      'liabilitiesForDiscountLedgerDimension',
      'custInterest',
    ] as const;
    if (dimensions.some((field) => !Number.isSafeInteger(draft[field]) || draft[field] < 0)) {
      setError(t('customerPostingProfiles.invalidIdentifier'));
      return;
    }
    setBusy(true);
    try {
      const payload = { ...draft };
      if (payload.accountCode === 2) payload.num = '';
      else {
        // Refresh before saving: do not accept a stale/deleted reference or an arbitrary typed code.
        const result = await references.refetch({ throwOnError: true });
        const reference = result.data?.find((option) => String(option.id) === payload.num);
        if (!reference) {
          setError(t('customerPostingProfiles.selectReference'));
          return;
        }
        payload.num = String(reference.id);
      }
      await client.cancelQueries({ queryKey });
      const saved = await (payload.recId ? api.update(payload) : api.create(payload));
      client.setQueryData<CustomerPostingAccount[]>(queryKey, (current = []) =>
        payload.recId
          ? current.map((row) => (row.id === saved.id ? saved : row))
          : [...current, saved]
      );
      setSelectedId(saved.id);
      setDraft(null);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : String(reason));
    } finally {
      setBusy(false);
    }
  };
  const remove = async () => {
    if (!selected || busy) return;
    setBusy(true);
    setError('');
    try {
      await client.cancelQueries({ queryKey });
      await api.delete(selected);
      client.setQueryData<CustomerPostingAccount[]>(queryKey, (current = []) =>
        current.filter((row) => row.id !== selected.id)
      );
      setSelectedId(null);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : String(reason));
    } finally {
      setBusy(false);
      setDeleting(false);
    }
  };
  const field = (
    name:
      | 'accountCode'
      | 'num'
      | 'summaryLedgerDimension'
      | 'clearingLedgerDimension'
      | 'vatPrepaymentsLedgerDimension'
      | 'liabilitiesForDiscountLedgerDimension'
      | 'collectionLetterCourse'
      | 'custInterest',
    underlined = false
  ) => {
    if (!selected) return null;
    if (name === 'num' && editing)
      return (
        <Box sx={{ minWidth: 0, maxWidth: 153 }}>
          <Typography component="label" sx={listGridLabelSx}>
            {label('num')}
            {code !== 2 ? ' *' : ''}
          </Typography>
          <Box
            sx={{
              ...listGridControlSx,
              '& .MuiInputLabel-root': { display: 'none' },
            }}
          >
            <LookupField
              key={code}
              name="num"
              label={label('num')}
              value={code === 2 ? '' : selected.num}
              options={code === 2 ? [] : (references.data ?? [])}
              disabled={busy || code === 2}
              loading={references.isFetching}
              required={code !== 2}
              error={code !== 2 && references.isError}
              helperText={code !== 2 && references.isError ? references.error.message : undefined}
              displayMode="select"
              searchable
              sideMode="client"
              lazyLoading={false}
              onChange={(value) =>
                setDraft((current) =>
                  current ? { ...current, num: String(value ?? '') } : current
                )
              }
            />
          </Box>
          {code !== 2 && references.isError && (
            <Button onClick={() => void references.refetch()}>{t('actions.refresh')}</Button>
          )}
        </Box>
      );
    const numeric = !['num', 'accountCode', 'collectionLetterCourse'].includes(name);
    return (
      <ListGridField
        label={label(name)}
        value={numeric && selected[name] === 0 ? '' : selected[name]}
        editing={editing && !busy}
        numeric={numeric}
        underlined={underlined}
        options={name === 'accountCode' ? options : undefined}
        onChange={(value) =>
          setDraft((current) =>
            current
              ? {
                  ...current,
                  [name]: numeric || name === 'accountCode' ? Number(value) : value,
                  ...(name === 'accountCode' && Number(value) !== current.accountCode
                    ? { num: '' }
                    : {}),
                }
              : current
          )
        }
      />
    );
  };
  if (!profile.recId)
    return <Alert severity="info">{t('customerPostingProfiles.saveProfileFirst')}</Alert>;
  if (accounts.isLoading) return <CircularProgress size={22} />;
  if (accounts.isError)
    return (
      <Alert
        severity="error"
        action={<Button onClick={() => void accounts.refetch()}>{t('actions.refresh')}</Button>}
      >
        {accounts.error.message}
      </Alert>
    );
  const shownRows = draft
    ? draft.recId
      ? rows.map((row) => (row.id === draft.id ? draft : row))
      : [...rows, draft]
    : rows;
  return (
    <>
      {error && (
        <Alert severity="error" sx={{ mb: 1 }}>
          {error}
        </Alert>
      )}
      <ListGridDetailsPanel
        rows={shownRows}
        columns={columns}
        storageKey="customer-posting-profiles.accounts"
        selectedIds={selected ? [selected.id] : []}
        onSelectionChange={(ids) => {
          if (!editing && !busy) setSelectedId(String(ids[0] ?? ''));
        }}
        addLabel={t('actions.add')}
        removeLabel={t('actions.remove')}
        disabled={busy || masterEditing}
        onAdd={
          canCreate && !editing
            ? () => begin(newPostingAccount(profile.postingProfile, profile.dataAreaId))
            : undefined
        }
        onRemove={canDelete && !editing ? () => setDeleting(true) : undefined}
        actions={
          editing
            ? [
                {
                  id: 'save',
                  label: t('actions.save'),
                  ariaLabel: t('customerPostingProfiles.saveAccount'),
                  onClick: () => void save(),
                },
                {
                  id: 'cancel',
                  label: t('actions.cancel'),
                  ariaLabel: t('customerPostingProfiles.cancelAccount'),
                  onClick: cancel,
                },
              ]
            : canEdit
              ? [
                  {
                    id: 'edit',
                    label: t('actions.edit'),
                    ariaLabel: t('customerPostingProfiles.editAccount'),
                    disabled: !selected,
                    onClick: () => selected && begin(selected),
                  },
                ]
              : []
        }
        details={
          selected ? (
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: {
                  xs: '1fr',
                  sm: 'repeat(2,minmax(0,1fr))',
                  xl: 'repeat(4,minmax(0,1fr))',
                },
                columnGap: { xs: 2, xl: '47px' },
                rowGap: '10px',
                maxWidth: 965,
              }}
            >
              <Box>
                {field('accountCode', true)}
                <Box sx={{ mt: '10px' }}>{field('summaryLedgerDimension')}</Box>
              </Box>
              <Box>
                {field('num', true)}
                <Box sx={{ mt: '10px' }}>{field('clearingLedgerDimension')}</Box>
              </Box>
              <Box sx={{ pt: { xs: 0, xl: '56px' } }}>{field('vatPrepaymentsLedgerDimension')}</Box>
              <Box sx={{ pt: { xs: 0, xl: '56px' }, display: 'grid', gap: '10px' }}>
                {field('liabilitiesForDiscountLedgerDimension')}
                {field('collectionLetterCourse', true)}
                {field('custInterest', true)}
              </Box>
            </Box>
          ) : (
            <Alert severity="info">{t('customerPostingProfiles.noAccounts')}</Alert>
          )
        }
      />
      <ConfirmationDialog
        open={deleting}
        onClose={() => setDeleting(false)}
        onConfirm={() => void remove()}
        loading={busy}
        message={t('customerPostingProfiles.removeAccount')}
      />
    </>
  );
}
