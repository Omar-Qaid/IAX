import React from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@core/auth/useAuth';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useListDetailsPage } from '@patterns/list-details/useListDetailsPage';
import type { EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { localizedName } from '@shared/utilities/localizedName';
import { customerQuickCreateApi } from '@modules/finance/accounts-receivable/customer/api/customerQuickCreateApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '@modules/finance/accounts-receivable/routes/accountsReceivableRoutePaths';
import { contactPersonApi } from '../api/contactPersonApi';
import {
  emptyContact,
  fullName,
  toRecord,
  type ContactRecord,
  type ContactPersonDetailsProps,
} from './contactPersonModel';
import { getContactValues, setContactValues } from './ContactPersonHeaderFields';

export function useContactPersonDocument({
  account,
  endpoint,
  queryKey,
  record,
  contacts,
  open,
}: ContactPersonDetailsProps) {
  const cache = useQueryClient();
  const { hasPermission } = useAuth();
  const { isRtl } = useAppTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const { kind, accountNum } = useParams();
  const [more, setMore] = React.useState(false);
  const cancelled = React.useRef(false);
  const navigatedId = React.useRef<number | null>(null);
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
  const load = (signal?: AbortSignal) => contactPersonApi.load(endpoint, signal);
  const persist = async (row: ContactRecord, create: boolean): Promise<ContactRecord> => {
    const recId = await contactPersonApi.save(endpoint, row, create);
    await cache.invalidateQueries({ queryKey });
    const refreshed = await load();
    return (
      refreshed.find((item) => item.recId === recId) ?? {
        ...row,
        recId,
        id: String(recId),
        name: fullName(row),
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
    initialMode:
      !record && hasPermission(PERMISSIONS.CONTACT_CREATE)
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
        await contactPersonApi.remove(row.recId);
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
    getValues: (row) => getContactValues(row, account),
    setValues: setContactValues,
    headerFields: [],
    sections: [],
    validate: (row) => ({
      ...(!fullName(row).trim() ? { firstName: 'Contact name is required.' } : {}),
      ...(!row.languageId.trim() ? { languageId: 'Language is required.' } : {}),
    }),
    onCancel: () => {
      cancelled.current = !record;
      if (!record) open();
    },
  };
  const state = useListDetailsPage(config);
  const busy = state.saving || status.isPending || customerNavigation.isPending;
  React.useEffect(() => {
    if (
      cancelled.current ||
      state.editing ||
      state.isNew ||
      !state.draft?.recId ||
      state.draft.recId === record?.recId
    )
      return;
    if (navigatedId.current === state.draft.recId) return;
    navigatedId.current = state.draft.recId;
    open(String(state.draft.recId));
  }, [record, state.editing, state.isNew, state.draft?.recId, open]);
  const row = state.selected;
  const ribbons = [
    {
      id: 'contact',
      label: 'Contact',
      disabled: busy || state.isNew || !row,
      persistenceKey: 'contactPerson.ribbon',
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
  ];
  return {
    state,
    config,
    busy,
    more,
    setMore,
    ribbons,
    hasPermission,
    actionError: status.error?.message || customerNavigation.error?.message,
    beginEdit: () => {
      if (busy || !hasPermission(PERMISSIONS.CONTACT_EDIT)) return;
      cancelled.current = false;
      state.startEdit();
    },
  };
}
