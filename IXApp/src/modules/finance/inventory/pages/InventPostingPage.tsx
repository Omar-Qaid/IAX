import React from 'react';
import { MenuItem, TextField } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { TabbedChoiceGridPage } from '@patterns/tabbed-choice-grid';
import type { ChoiceGridTab } from '@patterns/tabbed-choice-grid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { usePermission } from '@core/permissions/usePermission';
import { PERMISSIONS } from '@core/permissions/permissions';
import {
  inventPostingApi,
  type InventPostingRule,
  type PostingLookup,
} from '../api/inventPostingApi';

const choice = (id: number, label: string) => ({ id: String(id), label });
const tabs: ChoiceGridTab[] = [
  {
    id: 'sales',
    label: 'Sales order',
    choices: [
      choice(19, 'Cost of units, delivered'),
      choice(20, 'Cost of goods sold, delivered'),
      choice(21, 'Cost of units, invoiced'),
      choice(22, 'Cost of goods sold, invoiced'),
      choice(23, 'Revenue'),
      choice(25, 'Commission'),
      choice(24, 'Discount'),
      choice(26, 'Commission offset'),
      choice(30, 'Deferred revenue on delivery'),
      choice(31, 'Deferred revenue offset on delivery'),
      choice(37, 'Deferred sales tax on delivery'),
      choice(73, 'Customer prepayment'),
    ],
  },
  {
    id: 'purchase',
    label: 'Purchase order',
    choices: [
      choice(0, 'Cost of purchased materials received'),
      choice(1, 'Purchase expenditure, un-invoiced'),
      choice(2, 'Cost of purchased materials invoiced'),
      choice(3, 'Purchase expenditure for product'),
      choice(4, 'Discount'),
      choice(27, 'Fixed receipt price profit'),
      choice(28, 'Fixed receipt price loss'),
      choice(29, 'Fixed receipt price offset'),
      choice(32, 'Charges'),
      choice(33, 'Stock variation'),
      choice(39, 'Purchase price variance'),
      choice(48, 'Fixed asset receipt'),
      choice(58, 'Prepayment'),
      choice(59, 'Purchase expenditure for expense'),
    ],
  },
  {
    id: 'inventory',
    label: 'Inventory',
    choices: [
      choice(9, 'Inventory receipt'),
      choice(10, 'Inventory profit'),
      choice(7, 'Inventory issue'),
      choice(8, 'Inventory loss'),
      choice(38, 'Inventory cost revaluation'),
      choice(41, 'Inter-unit payable'),
      choice(42, 'Inter-unit receivable'),
      choice(43, 'Cost change variance'),
      choice(47, 'Rounding variance'),
      choice(49, 'Fixed asset issue'),
      choice(60, 'Moving average price difference'),
      choice(61, 'Moving average cost revaluation'),
    ],
  },
  {
    id: 'production',
    label: 'Production',
    choices: [
      choice(11, 'Picking list'),
      choice(12, 'Picking list offset'),
      choice(13, 'Report as finished'),
      choice(14, 'Report as finished offset'),
      choice(15, 'Production issue'),
      choice(16, 'Production issue offset'),
      choice(17, 'Production receipt'),
      choice(18, 'Production receipt offset'),
      choice(40, 'Production variance'),
      choice(56, 'Lean WIP service receipt'),
      choice(57, 'Lean WIP service clearing'),
    ],
  },
  {
    id: 'standard-cost',
    label: 'Standard cost variance',
    choices: [
      choice(39, 'Purchase price variance'),
      choice(40, 'Production variance'),
      choice(43, 'Cost change variance'),
      choice(44, 'Lot size variance'),
      choice(45, 'Quantity variance'),
      choice(46, 'Substitution variance'),
      choice(47, 'Rounding variance'),
    ],
  },
];
const scopeOptions = [
  { id: '0', label: 'Table' },
  { id: '1', label: 'Group' },
  { id: '2', label: 'All' },
];
const emptyRule = (accountType: number): InventPostingRule => ({
  id: `new-${crypto.randomUUID()}`,
  inventAccountType: accountType,
  itemCode: 2,
  itemRelation: '',
  categoryRelation: '0',
  custVendCode: 2,
  custVendRelation: '',
  taxGroupId: '',
  ledgerDimension: '0',
  mainAccount: '',
});

export function InventPostingPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const client = useQueryClient();
  const [tabId, setTabId] = React.useState('sales');
  const [accountType, setAccountType] = React.useState(19);
  const [selectedIds, setSelectedIds] = React.useState<(string | number)[]>([]);
  const [draft, setDraft] = React.useState<InventPostingRule | null>(null);
  const [saving, setSaving] = React.useState(false);
  const [error, setError] = React.useState('');
  const canCreate = usePermission(PERMISSIONS.INVENTORY_TRANSACTION_CREATE).hasPermission;
  const canEdit = usePermission(PERMISSIONS.INVENTORY_TRANSACTION_EDIT).hasPermission;
  const canDelete = usePermission(PERMISSIONS.INVENTORY_TRANSACTION_DELETE).hasPermission;
  const queryKey = ['inventory-postings', accountType];
  const rules = useQuery({
    queryKey,
    queryFn: ({ signal }) => inventPostingApi.list(accountType, signal),
  });
  const lookups = useQuery({
    queryKey: ['inventory-posting-lookups'],
    queryFn: ({ signal }) => inventPostingApi.lookups(signal),
    staleTime: 300000,
  });
  const selected = draft ?? (rules.data ?? []).find((row) => selectedIds.includes(row.id)) ?? null;
  const shownRows = draft
    ? draft.id.startsWith('new-')
      ? [...(rules.data ?? []), draft]
      : (rules.data ?? []).map((row) => (row.id === draft.id ? draft : row))
    : (rules.data ?? []);
  const contextChange = React.useCallback((nextTab: string, nextChoice: string) => {
    setTabId(nextTab);
    setAccountType(Number(nextChoice));
    setSelectedIds([]);
    setDraft(null);
    setError('');
  }, []);
  const field = (
    row: InventPostingRule,
    key: keyof InventPostingRule,
    options?: PostingLookup[],
    disabled = false
  ) => {
    if (!draft || draft.id !== row.id) return String(row[key] ?? '');
    return (
      <TextField
        select={Boolean(options)}
        variant="standard"
        size="small"
        fullWidth
        value={String(draft[key] ?? '')}
        disabled={disabled}
        onChange={(event) =>
          setDraft((current) =>
            current
              ? {
                  ...current,
                  [key]:
                    key === 'itemCode' || key === 'custVendCode'
                      ? Number(event.target.value)
                      : event.target.value,
                  ...(key === 'itemCode'
                    ? { itemRelation: '' }
                    : {}),
                  ...(key === 'custVendCode'
                    ? { custVendRelation: '' }
                    : {}),
                }
              : current
          )
        }
      >
        {options?.map((option) => (
          <MenuItem key={option.id} value={option.id}>
            {option.label}
          </MenuItem>
        ))}
      </TextField>
    );
  };
  const itemRelations =
    selected?.itemCode === 0
      ? lookups.data?.items
      : selected?.itemCode === 1
        ? lookups.data?.itemGroups
        : [];
  const accountRelations =
    tabId === 'sales'
      ? selected?.custVendCode === 0
        ? lookups.data?.customers
        : selected?.custVendCode === 1
          ? lookups.data?.customerGroups
          : []
      : tabId === 'purchase'
        ? selected?.custVendCode === 0
          ? lookups.data?.vendors
          : selected?.custVendCode === 1
            ? lookups.data?.vendorGroups
            : []
        : [];
  const columns: ColumnDef<InventPostingRule>[] = [
    {
      field: 'itemCode',
      headerName: 'Item code',
      width: 145,
      renderCell: ({ row }) =>
        draft?.id === row.id
          ? field(row, 'itemCode', scopeOptions)
          : scopeOptions.find((x) => x.id === String(row.itemCode))?.label,
    },
    {
      field: 'itemRelation',
      headerName: 'Item relation',
      width: 180,
      renderCell: ({ row }) => field(row, 'itemRelation', itemRelations, row.itemCode === 2),
    },
    {
      field: 'categoryRelation',
      headerName: 'Category relation',
      width: 170,
      renderCell: ({ row }) =>
        draft?.id === row.id
          ? field(row, 'categoryRelation', [
              { id: '0', label: '' },
              ...(lookups.data?.categories ?? []),
            ])
          : lookups.data?.categories.find((category) => category.id === row.categoryRelation)
              ?.label ?? '',
    },
    {
      field: 'custVendCode',
      headerName: 'Account code',
      width: 150,
      renderCell: ({ row }) =>
        draft?.id === row.id
          ? field(row, 'custVendCode', scopeOptions, tabId !== 'sales' && tabId !== 'purchase')
          : scopeOptions.find((x) => x.id === String(row.custVendCode))?.label,
    },
    {
      field: 'custVendRelation',
      headerName: 'Account relation',
      width: 180,
      renderCell: ({ row }) =>
        field(
          row,
          'custVendRelation',
          accountRelations,
          row.custVendCode === 2 || (tabId !== 'sales' && tabId !== 'purchase')
        ),
    },
    {
      field: 'taxGroupId',
      headerName: 'Sales tax group',
      width: 155,
      renderCell: ({ row }) =>
        field(row, 'taxGroupId', [{ id: '', label: '' }, ...(lookups.data?.taxGroups ?? [])]),
    },
    {
      field: 'mainAccount',
      headerName: 'Main account',
      minWidth: 180,
      flex: 1,
      renderCell: ({ row }) =>
        draft?.id === row.id
          ? field(row, 'ledgerDimension', lookups.data?.ledgerAccounts ?? [])
          : row.mainAccount,
    },
  ];
  const save = async () => {
    if (!draft) return;
    setSaving(true);
    setError('');
    try {
      const saved = draft.id.startsWith('new-')
        ? await inventPostingApi.create(draft)
        : await inventPostingApi.update(draft);
      await client.invalidateQueries({ queryKey });
      setDraft(null);
      setSelectedIds([saved.id]);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : String(reason));
    } finally {
      setSaving(false);
    }
  };
  const remove = async () => {
    const row = (rules.data ?? []).find((item) => selectedIds.includes(item.id));
    if (!row) return;
    setSaving(true);
    try {
      await inventPostingApi.delete(row);
      await client.invalidateQueries({ queryKey });
      setSelectedIds([]);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : String(reason));
    } finally {
      setSaving(false);
    }
  };
  return (
    <TabbedChoiceGridPage
      title={t('inventPosting.title', 'Posting')}
      chooserLabel={t('inventPosting.select', 'Select')}
      tabs={tabs}
      rows={shownRows}
      columns={columns}
      loading={rules.isLoading}
      error={error || (rules.isError ? rules.error.message : '') || (lookups.isError ? lookups.error.message : '')}
      editing={Boolean(draft)}
      saving={saving}
      canCreate={canCreate}
      canEdit={canEdit}
      canDelete={canDelete}
      selectedRowIds={selectedIds}
      initialTabId="sales"
      initialChoiceId="19"
      storageKey="inventory.posting.rules"
      onContextChange={contextChange}
      onSelectionChange={setSelectedIds}
      onEdit={() => selected && setDraft({ ...selected })}
      onNew={() => {
        const row = emptyRule(accountType);
        setDraft(row);
        setSelectedIds([row.id]);
      }}
      onDelete={() => void remove()}
      onSave={() => void save()}
      onCancel={() => setDraft(null)}
      onRefresh={() => void rules.refetch()}
    />
  );
}
