import React, { useMemo } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useNotifications } from '@shared/hooks/useNotifications';
import {
  newPriceDiscountGroup,
  newTradeAgreementJournal,
  newTradeAgreementJournalName,
  priceDiscountGroupApi,
  tradeAgreementJournalApi,
  tradeAgreementJournalNameApi,
  type PriceDiscountGroup,
  type TradeAgreementJournal,
  type TradeAgreementJournalName,
} from '../api/priceDiscSetupApi';

type SetupRecord = TradeAgreementJournal | PriceDiscountGroup | TradeAgreementJournalName;
type SetupKind = 'journals' | 'groups' | 'names';

const relationOptions = [
  { value: 0, label: 'Sales price' },
  { value: 1, label: 'Line discount' },
  { value: 2, label: 'Multiline discount' },
  { value: 3, label: 'Total discount' },
];
const groupRelationOptions = [relationOptions[0], ...relationOptions.slice(1, 4), { value: 4, label: 'Purchase price' }, { value: 5, label: 'Purchase line discount' }, { value: 6, label: 'Purchase multiline discount' }, { value: 7, label: 'Purchase total discount' }];

export function PriceDiscSetupListPage({ kind }: { kind: SetupKind }): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const queryClient = useQueryClient();
  const { notifyError, notifySuccess } = useNotifications();
  const queryKey = ['accounts-receivable', 'price-disc-setup', kind] as const;
  const api = kind === 'journals' ? tradeAgreementJournalApi : kind === 'groups' ? priceDiscountGroupApi : tradeAgreementJournalNameApi;
  const query = useQuery({ queryKey, queryFn: ({ signal }) => api.list(signal) });
  const isPostedJournal = (record: SetupRecord): boolean =>
    kind === 'journals' && (record as TradeAgreementJournal).posted === 1;

  const columns = useMemo<ColumnDef<SetupRecord>[]>(() => {
    if (kind === 'journals') return [
      { field: 'journalNum', headerName: 'Journal number', width: 180, pinned: 'left', editable: true, renderEditCell: ({ row, value, onChange, disabled }) => <input aria-label="Journal number" value={String(value ?? '')} disabled={disabled || isPostedJournal(row)} onChange={(event) => onChange(event.target.value)} /> },
      { field: 'journalName', headerName: 'Journal name', width: 150, editable: true, renderEditCell: ({ row, value, onChange, disabled }) => <input aria-label="Journal name" value={String(value ?? '')} disabled={disabled || isPostedJournal(row)} onChange={(event) => onChange(event.target.value)} /> },
      { field: 'name', headerName: 'Description', minWidth: 200, flex: 1, editable: true, renderEditCell: ({ row, value, onChange, disabled }) => <input aria-label="Description" value={String(value ?? '')} disabled={disabled || isPostedJournal(row)} onChange={(event) => onChange(event.target.value)} /> },
      { field: 'defaultRelation', headerName: 'Relation', width: 180, type: 'singleSelect', valueOptions: relationOptions, editable: true, renderEditCell: ({ row, value, onChange, disabled }) => <select aria-label="Relation" value={Number(value ?? 0)} disabled={disabled || isPostedJournal(row)} onChange={(event) => onChange(Number(event.target.value))}>{relationOptions.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}</select> },
      { field: 'priceGroup', headerName: 'Price group', width: 140, editable: true, renderEditCell: ({ row, value, onChange, disabled }) => <input aria-label="Price group" value={String(value ?? '')} disabled={disabled || isPostedJournal(row)} onChange={(event) => onChange(event.target.value)} /> },
      { field: 'posted', headerName: 'Posted', width: 100, type: 'boolean', valueGetter: ({ row }) => row.posted === 1 },
      { field: 'postedDate', headerName: 'Posted date', width: 180, type: 'date' },
    ];
    if (kind === 'groups') return [
      { field: 'groupId', headerName: 'Group ID', width: 160, pinned: 'left', editable: true },
      { field: 'name', headerName: 'Description', minWidth: 200, flex: 1, editable: true },
      { field: 'module', headerName: 'Module', width: 150, type: 'singleSelect', valueOptions: [{ value: 1, label: 'Vendor' }, { value: 2, label: 'Customer' }, { value: 3, label: 'Item' }], editable: true },
      { field: 'type', headerName: 'Relation', width: 180, type: 'singleSelect', valueOptions: groupRelationOptions, editable: true },
      { field: 'retailPricingPriorityNumber', headerName: 'Pricing priority', width: 160, type: 'number', editable: true },
    ];
    return [
      { field: 'journalName', headerName: 'Journal name', width: 180, pinned: 'left', editable: true },
      { field: 'name', headerName: 'Description', minWidth: 220, flex: 1, editable: true },
      { field: 'defaultRelation', headerName: 'Default relation', width: 180, type: 'singleSelect', valueOptions: relationOptions, editable: true },
      { field: 'priceDiscPriceAttributeEnable', headerName: 'Price attributes enabled', width: 190, type: 'boolean', valueGetter: ({ row }) => row.priceDiscPriceAttributeEnable === 1, editable: true, renderEditCell: ({ value, onChange, disabled }) => <input aria-label="Price attributes enabled" type="checkbox" checked={value === true || Number(value) === 1} disabled={disabled} onChange={(event) => onChange(event.target.checked ? 1 : 0)} /> },
    ];
  }, [kind]);

  const labels = kind === 'journals'
    ? { title: 'Trade agreement journals', table: 'PriceDiscAdmTable', permission: [PERMISSIONS.TRADE_AGREEMENT_JOURNAL_VIEW, PERMISSIONS.TRADE_AGREEMENT_JOURNAL_CREATE, PERMISSIONS.TRADE_AGREEMENT_JOURNAL_EDIT, PERMISSIONS.TRADE_AGREEMENT_JOURNAL_DELETE] as const }
    : kind === 'groups'
      ? { title: 'Price/discount groups', table: 'PriceDiscGroup', permission: [PERMISSIONS.PRICE_DISCOUNT_GROUP_VIEW, PERMISSIONS.PRICE_DISCOUNT_GROUP_CREATE, PERMISSIONS.PRICE_DISCOUNT_GROUP_EDIT, PERMISSIONS.PRICE_DISCOUNT_GROUP_DELETE] as const }
      : { title: 'Trade agreement journal names', table: 'PriceDiscAdmName', permission: [PERMISSIONS.TRADE_AGREEMENT_JOURNAL_NAME_VIEW, PERMISSIONS.TRADE_AGREEMENT_JOURNAL_NAME_CREATE, PERMISSIONS.TRADE_AGREEMENT_JOURNAL_NAME_EDIT, PERMISSIONS.TRADE_AGREEMENT_JOURNAL_NAME_DELETE] as const };

  const config: EnterpriseListConfig<SetupRecord> = {
    recordTableName: labels.table,
    contextLabel: labels.title,
    viewLabel: 'Standard view',
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchMode: 'quick',
    searchFields: kind === 'groups'
      ? [{ field: 'groupId', label: 'Group ID' }, { field: 'name', label: 'Description' }]
      : [{ field: 'journalName', label: 'Journal name' }, { field: 'name', label: 'Description' }, ...(kind === 'journals' ? [{ field: 'journalNum', label: 'Journal number' }] : [])],
    locale: currentLanguage.code,
    crud: {
      editLabel: t('actions.edit'), newLabel: t('actions.new'), deleteLabel: t('actions.delete'),
      editPermission: labels.permission[2], newPermission: labels.permission[1], deletePermission: labels.permission[3],
      onDelete: async (rows) => {
        try {
          for (const row of rows) {
            if (!isNewRecord(row) && isPostedJournal(row)) throw new Error('Posted journals cannot be deleted.');
            await api.delete(row as never);
          }
          await queryClient.invalidateQueries({ queryKey });
          notifySuccess(t('messages.deletedSuccessfully'));
        } catch (error) {
          notifyError(error instanceof Error ? error.message : t('errors.deleteFailed'));
        }
      },
    },
    commands: ['setup', 'options'].map((id) => ({ id, label: t(`customerGroupCommands.${id}`) })),
    utilities: {
      personalizeLabel: t('utilities.personalize'), guideLabel: t('utilities.guide'), notificationsLabel: t('common.notifications'),
      refreshLabel: t('actions.refresh'), openWindowLabel: t('utilities.openWindow'), notificationCount: 0,
    },
    advancedFilter: {
      title: t('filters.title'), addLabel: t('actions.add'), fieldLabel: kind === 'groups' ? 'Group ID' : 'Journal number',
      operatorLabel: t('filters.contains'), applyLabel: t('actions.apply'), resetLabel: t('actions.reset'),
      getValue: (row) => kind === 'groups' ? (row as PriceDiscountGroup).groupId : kind === 'journals' ? (row as TradeAgreementJournal).journalNum : (row as TradeAgreementJournalName).journalName,
      matches: (row, value) => `${'groupId' in row ? row.groupId : 'journalNum' in row ? row.journalNum : row.journalName} ${row.name}`.toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
    readOnly: false,
  };

  return <SimpleListPage
    title={labels.title}
    recordTableName={labels.table}
    enterpriseConfig={config}
    dataSource={{ type: 'controlled', rows: query.data ?? [], loading: query.isLoading, error: query.error instanceof Error ? query.error.message : null, refresh: () => { void query.refetch(); } }}
    columns={columns}
    dataGridProps={{
      storageKey: `accounts-receivable.${kind}.reference-view`, hideSidebar: false, masterForm: true,
      onNewRow: () => {
        if (kind === 'journals') return newTradeAgreementJournal() as SetupRecord;
        if (kind === 'groups') return newPriceDiscountGroup() as SetupRecord;
        return newTradeAgreementJournalName() as SetupRecord;
      },
      onRowSave: async (values, isNew) => {
        const record = values as SetupRecord;
        if (!isNew && kind === 'journals') {
          const persisted = query.data?.find((row) => row.id === record.id);
          if (persisted && isPostedJournal(persisted)) throw new Error('Posted journals cannot be edited.');
        }
        if (isNew) await api.create(record as never);
        else await api.update(record as never);
        await queryClient.invalidateQueries({ queryKey });
      },
    }}
  />;

  function isNewRecord(record: SetupRecord): boolean {
    return record.id.startsWith('new-');
  }
}

export const setupPermissions = {
  journals: PERMISSIONS.TRADE_AGREEMENT_JOURNAL_VIEW,
  groups: PERMISSIONS.PRICE_DISCOUNT_GROUP_VIEW,
  names: PERMISSIONS.TRADE_AGREEMENT_JOURNAL_NAME_VIEW,
};

export { relationOptions };
