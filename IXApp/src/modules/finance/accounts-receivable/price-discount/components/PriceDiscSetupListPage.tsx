import React, { useCallback, useMemo } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import {
  SimpleListPage,
  type EnterpriseListConfig,
  type EnterpriseListSearchField,
} from '@patterns/simple-list/SimpleListPage';
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

type SetupRecord = (TradeAgreementJournal | PriceDiscountGroup | TradeAgreementJournalName) & {
  journalNum?: string;
  journalName?: string;
  groupId?: string;
  posted?: number;
  priceDiscPriceAttributeEnable?: number;
};
type SetupKind = 'journals' | 'groups' | 'names';

type Translate = (key: string) => string;

const RELATION_KEYS = [
  'salesPrice',
  'lineDiscount',
  'multilineDiscount',
  'totalDiscount',
  'purchasePrice',
  'purchaseLineDiscount',
  'purchaseMultilineDiscount',
  'purchaseTotalDiscount',
] as const;
const relationValueOptions = (t: Translate, count: number) =>
  RELATION_KEYS.slice(0, count).map((key, value) => ({
    value,
    label: t('priceDiscount.relations.' + key),
  }));
const moduleValueOptions = (t: Translate) =>
  (['vendor', 'customer', 'item'] as const).map((key, index) => ({
    value: index + 1,
    label: t('priceDiscount.modules.' + key),
  }));

export function PriceDiscSetupListPage({ kind }: { kind: SetupKind }): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const queryClient = useQueryClient();
  const { notifyError, notifySuccess } = useNotifications();
  const queryKey = ['accounts-receivable', 'price-disc-setup', kind] as const;
  const api =
    kind === 'journals'
      ? tradeAgreementJournalApi
      : kind === 'groups'
        ? priceDiscountGroupApi
        : tradeAgreementJournalNameApi;
  const query = useQuery<SetupRecord[]>({
    queryKey,
    queryFn: async ({ signal }) => {
      if (kind === 'journals') return tradeAgreementJournalApi.list(signal);
      if (kind === 'groups') return priceDiscountGroupApi.list(signal);
      return tradeAgreementJournalNameApi.list(signal);
    },
  });
  const isPostedJournal = useCallback(
    (record: SetupRecord): boolean => kind === 'journals' && record.posted === 1,
    [kind]
  );

  const columns = useMemo<ColumnDef<SetupRecord>[]>(() => {
    if (kind === 'journals')
      return [
        {
          field: 'journalNum',
          headerName: 'priceDiscount.columns.journalNum',
          width: 180,
          pinned: 'left',
          editable: true,
          renderEditCell: ({ row, value, onChange, disabled }) => (
            <input
              aria-label={t('priceDiscount.columns.journalNum')}
              value={String(value ?? '')}
              disabled={disabled || isPostedJournal(row)}
              onChange={(event) => onChange(event.target.value)}
            />
          ),
        },
        {
          field: 'journalName',
          headerName: 'priceDiscount.columns.journalName',
          width: 150,
          editable: true,
          renderEditCell: ({ row, value, onChange, disabled }) => (
            <input
              aria-label={t('priceDiscount.columns.journalName')}
              value={String(value ?? '')}
              disabled={disabled || isPostedJournal(row)}
              onChange={(event) => onChange(event.target.value)}
            />
          ),
        },
        {
          field: 'name',
          headerName: 'priceDiscount.columns.name',
          minWidth: 200,
          flex: 1,
          editable: true,
          renderEditCell: ({ row, value, onChange, disabled }) => (
            <input
              aria-label={t('priceDiscount.columns.name')}
              value={String(value ?? '')}
              disabled={disabled || isPostedJournal(row)}
              onChange={(event) => onChange(event.target.value)}
            />
          ),
        },
        {
          field: 'defaultRelation',
          headerName: 'priceDiscount.columns.relation',
          width: 180,
          type: 'singleSelect',
          valueOptions: relationValueOptions(t, 4),
          editable: true,
          renderEditCell: ({ row, value, onChange, disabled }) => (
            <select
              aria-label={t('priceDiscount.columns.relation')}
              value={Number(value ?? 0)}
              disabled={disabled || isPostedJournal(row)}
              onChange={(event) => onChange(Number(event.target.value))}
            >
              {relationValueOptions(t, 4).map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          ),
        },
        {
          field: 'priceGroup',
          headerName: 'priceDiscount.columns.priceGroup',
          width: 140,
          editable: true,
          renderEditCell: ({ row, value, onChange, disabled }) => (
            <input
              aria-label={t('priceDiscount.columns.priceGroup')}
              value={String(value ?? '')}
              disabled={disabled || isPostedJournal(row)}
              onChange={(event) => onChange(event.target.value)}
            />
          ),
        },
        {
          field: 'posted',
          headerName: 'priceDiscount.columns.posted',
          width: 100,
          type: 'boolean',
          valueGetter: ({ row }) => row.posted === 1,
        },
        {
          field: 'postedDate',
          headerName: 'priceDiscount.columns.postedDate',
          width: 180,
          type: 'date',
        },
      ];
    if (kind === 'groups')
      return [
        {
          field: 'groupId',
          headerName: 'priceDiscount.columns.groupId',
          width: 160,
          pinned: 'left',
          editable: true,
        },
        {
          field: 'name',
          headerName: 'priceDiscount.columns.name',
          minWidth: 200,
          flex: 1,
          editable: true,
        },
        {
          field: 'module',
          headerName: 'priceDiscount.columns.module',
          width: 150,
          type: 'singleSelect',
          valueOptions: moduleValueOptions(t),
          editable: true,
        },
        {
          field: 'type',
          headerName: 'priceDiscount.columns.relation',
          width: 180,
          type: 'singleSelect',
          valueOptions: relationValueOptions(t, RELATION_KEYS.length),
          editable: true,
        },
        {
          field: 'retailPricingPriorityNumber',
          headerName: 'priceDiscount.columns.retailPricingPriorityNumber',
          width: 160,
          type: 'number',
          editable: true,
        },
      ];
    return [
      {
        field: 'journalName',
        headerName: 'priceDiscount.columns.journalName',
        width: 180,
        pinned: 'left',
        editable: true,
      },
      {
        field: 'name',
        headerName: 'priceDiscount.columns.name',
        minWidth: 220,
        flex: 1,
        editable: true,
      },
      {
        field: 'defaultRelation',
        headerName: 'priceDiscount.columns.defaultRelation',
        width: 180,
        type: 'singleSelect',
        valueOptions: relationValueOptions(t, 4),
        editable: true,
      },
      {
        field: 'priceDiscPriceAttributeEnable',
        headerName: 'priceDiscount.columns.priceDiscPriceAttributeEnable',
        width: 190,
        type: 'boolean',
        valueGetter: ({ row }) => row.priceDiscPriceAttributeEnable === 1,
        editable: true,
        renderEditCell: ({ value, onChange, disabled }) => (
          <input
            aria-label={t('priceDiscount.columns.priceDiscPriceAttributeEnable')}
            type="checkbox"
            checked={value === true || Number(value) === 1}
            disabled={disabled}
            onChange={(event) => onChange(event.target.checked ? 1 : 0)}
          />
        ),
      },
    ];
  }, [kind, isPostedJournal, t]);

  const labels =
    kind === 'journals'
      ? {
          title: t('priceDiscount.titles.journals'),
          table: 'PriceDiscAdmTable',
          permission: [
            PERMISSIONS.TRADE_AGREEMENT_JOURNAL_VIEW,
            PERMISSIONS.TRADE_AGREEMENT_JOURNAL_CREATE,
            PERMISSIONS.TRADE_AGREEMENT_JOURNAL_EDIT,
            PERMISSIONS.TRADE_AGREEMENT_JOURNAL_DELETE,
          ] as const,
        }
      : kind === 'groups'
        ? {
            title: t('priceDiscount.titles.groups'),
            table: 'PriceDiscGroup',
            permission: [
              PERMISSIONS.PRICE_DISCOUNT_GROUP_VIEW,
              PERMISSIONS.PRICE_DISCOUNT_GROUP_CREATE,
              PERMISSIONS.PRICE_DISCOUNT_GROUP_EDIT,
              PERMISSIONS.PRICE_DISCOUNT_GROUP_DELETE,
            ] as const,
          }
        : {
            title: t('priceDiscount.titles.names'),
            table: 'PriceDiscAdmName',
            permission: [
              PERMISSIONS.TRADE_AGREEMENT_JOURNAL_NAME_VIEW,
              PERMISSIONS.TRADE_AGREEMENT_JOURNAL_NAME_CREATE,
              PERMISSIONS.TRADE_AGREEMENT_JOURNAL_NAME_EDIT,
              PERMISSIONS.TRADE_AGREEMENT_JOURNAL_NAME_DELETE,
            ] as const,
          };

  const searchFields: EnterpriseListSearchField<SetupRecord>[] =
    kind === 'groups'
      ? [
          { field: 'groupId', label: t('priceDiscount.columns.groupId') },
          { field: 'name', label: t('priceDiscount.columns.name') },
        ]
      : [
          { field: 'journalName', label: t('priceDiscount.columns.journalName') },
          { field: 'name', label: t('priceDiscount.columns.name') },
          ...(kind === 'journals'
            ? [{ field: 'journalNum' as const, label: t('priceDiscount.columns.journalNum') }]
            : []),
        ];

  const config: EnterpriseListConfig<SetupRecord> = {
    recordTableName: labels.table,
    contextLabel: labels.title,
    searchMode: 'quick',
    searchFields,
    crud: {
      editPermission: labels.permission[2],
      newPermission: labels.permission[1],
      deletePermission: labels.permission[3],
      onDelete: async (rows) => {
        try {
          for (const row of rows) {
            if (!isNewRecord(row) && isPostedJournal(row))
              throw new Error(t('priceDiscount.postedJournalCannotBeDeleted'));
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
    advancedFilter: {
      fieldLabel:
        kind === 'groups'
          ? t('priceDiscount.columns.groupId')
          : t('priceDiscount.columns.journalNum'),
      operatorLabel: t('filters.contains'),
      getValue: (row) =>
        kind === 'groups'
          ? (row as PriceDiscountGroup).groupId
          : kind === 'journals'
            ? (row as TradeAgreementJournal).journalNum
            : (row as TradeAgreementJournalName).journalName,
      matches: (row, value) =>
        `${'groupId' in row ? row.groupId : 'journalNum' in row ? row.journalNum : row.journalName} ${row.name}`
          .toLocaleLowerCase(currentLanguage.code)
          .includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
    readOnly: false,
  };

  return (
    <SimpleListPage
      title={labels.title}
      enterpriseConfig={config}
      dataSource={{
        type: 'controlled',
        rows: query.data ?? [],
        loading: query.isLoading,
        error: query.error instanceof Error ? query.error.message : null,
        refresh: () => {
          void query.refetch();
        },
      }}
      columns={columns}
      dataGridProps={{
        storageKey: `accounts-receivable.${kind}.reference-view`,
        hideSidebar: false,
        masterForm: true,
        onNewRow: () => {
          if (kind === 'journals') return newTradeAgreementJournal() as SetupRecord;
          if (kind === 'groups') return newPriceDiscountGroup() as SetupRecord;
          return newTradeAgreementJournalName() as SetupRecord;
        },
        onRowSave: async (values, isNew) => {
          const record = values as SetupRecord;
          if (!isNew && kind === 'journals') {
            const persisted = query.data?.find((row) => row.id === record.id);
            if (persisted && isPostedJournal(persisted))
              throw new Error('Posted journals cannot be edited.');
          }
          if (isNew) await api.create(record as never);
          else await api.update(record as never);
          await queryClient.invalidateQueries({ queryKey });
        },
      }}
    />
  );

  function isNewRecord(record: SetupRecord): boolean {
    return record.id.startsWith('new-');
  }
}

export const setupPermissions = {
  journals: PERMISSIONS.TRADE_AGREEMENT_JOURNAL_VIEW,
  groups: PERMISSIONS.PRICE_DISCOUNT_GROUP_VIEW,
  names: PERMISSIONS.TRADE_AGREEMENT_JOURNAL_NAME_VIEW,
};
