import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ListDetailsListGridPage } from '@patterns/list-details-listgrid/ListDetailsListGridPage';
import type {
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { inventItemGroupApi, type InventItemGroupRecord } from '../api/inventItemGroupApi';
import {
  InventItemGroupPostingGrid,
  type PostingRowDefinition,
} from '../components/InventItemGroupPostingGrid';

const emptyGroup = (): InventItemGroupRecord => ({
  id: `new-${crypto.randomUUID()}`,
  itemGroupId: '',
  name: '',
  postings: [],
});
const text = (value: DetailValue): string => String(value ?? '');

const salesRows: PostingRowDefinition[] = [
  { accountType: 19, label: 'Cost of units, delivered' },
  { accountType: 20, label: 'Cost of goods sold, delivered' },
  { accountType: 21, label: 'Cost of units, invoiced' },
  { accountType: 22, label: 'Cost of goods sold, invoiced' },
  { accountType: 23, label: 'Revenue' },
  { accountType: 24, label: 'Discount' },
];
const purchaseRows: PostingRowDefinition[] = [
  { accountType: 0, label: 'Cost of purchased materials received' },
  { accountType: 1, label: 'Purchase expenditure, un-invoiced' },
  { accountType: 2, label: 'Cost of purchased materials invoiced' },
  { accountType: 3, label: 'Purchase expenditure for product' },
  { accountType: 4, label: 'Discount' },
  { accountType: 27, label: 'Fixed receipt price profit' },
  { accountType: 28, label: 'Fixed receipt price loss' },
  { accountType: 39, label: 'Purchase price variance' },
  { accountType: 48, label: 'Fixed asset receipt' },
  { accountType: 58, label: 'Prepayment' },
  { accountType: 59, label: 'Purchase expenditure for expense' },
];
const inventoryRows: PostingRowDefinition[] = [
  { accountType: 9, label: 'Inventory receipt' },
  { accountType: 10, label: 'Inventory expenditure, profit' },
  { accountType: 7, label: 'Inventory issue' },
  { accountType: 8, label: 'Inventory expenditure, loss' },
  { accountType: 38, label: 'Inventory cost revaluation' },
  { accountType: 41, label: 'Inter-unit payable' },
  { accountType: 42, label: 'Inter-unit receivable' },
  { accountType: 43, label: 'Cost change variance' },
  { accountType: 47, label: 'Inventory rounding variance' },
];

export function InventItemGroupPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const accounts = useQuery({
    queryKey: ['inventory-item-group-ledger-accounts'],
    queryFn: ({ signal }) => inventItemGroupApi.ledgerAccounts(signal),
    staleTime: 300000,
  });
  const config = useMemo<EnterpriseListDetailsConfig<InventItemGroupRecord>>(
    () => ({
      recordTableName: 'InventItemGroup',
      dataSource: {
        type: 'remote',
        key: 'inventory-item-groups',
        load: inventItemGroupApi.list,
        create: inventItemGroupApi.create,
        update: inventItemGroupApi.update,
        delete: inventItemGroupApi.delete,
      },
      createRecord: emptyGroup,
      getPrimaryText: (record) => record.itemGroupId,
      getSecondaryText: (record) => record.name,
      matchesSearch: (record, query) =>
        `${record.itemGroupId} ${record.name}`
          .toLocaleLowerCase()
          .includes(query.trim().toLocaleLowerCase()),
      getValues: (): DetailValues => ({}),
      setValues: (record) => record,
      headerFields: [
        {
          id: 'itemGroupId',
          label: t('inventItemGroup.fields.id', 'Item group'),
          width: 180,
          getValue: (record) => record.itemGroupId,
          setValue: (record, value) => ({ ...record, itemGroupId: text(value) }),
        },
        {
          id: 'name',
          label: t('inventItemGroup.fields.name', 'Name'),
          width: 'minmax(320px, 520px)',
          getValue: (record) => record.name,
          setValue: (record, value) => ({ ...record, name: text(value) }),
        },
      ],
      sections: ({ record, editing, onRecordChange }) => [
        {
          id: 'sales-order',
          title: t('inventItemGroup.sections.sales', 'Sales order'),
          defaultExpanded: true,
          detailsPadding: 10,
          content: (
            <InventItemGroupPostingGrid
              record={record}
              editing={editing}
              rows={salesRows}
              ledgerAccounts={accounts.data ?? []}
              storageKey="inventory.item-groups.sales"
              onRecordChange={onRecordChange}
            />
          ),
        },
        {
          id: 'purchase-order',
          title: t('inventItemGroup.sections.purchase', 'Purchase order'),
          defaultExpanded: true,
          detailsPadding: 10,
          content: (
            <InventItemGroupPostingGrid
              record={record}
              editing={editing}
              rows={purchaseRows}
              ledgerAccounts={accounts.data ?? []}
              storageKey="inventory.item-groups.purchase"
              onRecordChange={onRecordChange}
            />
          ),
        },
        {
          id: 'inventory',
          title: t('inventItemGroup.sections.inventory', 'Inventory'),
          defaultExpanded: true,
          detailsPadding: 10,
          content: (
            <InventItemGroupPostingGrid
              record={record}
              editing={editing}
              rows={inventoryRows}
              ledgerAccounts={accounts.data ?? []}
              storageKey="inventory.item-groups.inventory"
              onRecordChange={onRecordChange}
            />
          ),
        },
      ],
      permissions: {
        view: PERMISSIONS.INVENTORY_TRANSACTION_VIEW,
        create: PERMISSIONS.INVENTORY_TRANSACTION_CREATE,
        edit: PERMISSIONS.INVENTORY_TRANSACTION_EDIT,
        delete: PERMISSIONS.INVENTORY_TRANSACTION_DELETE,
      },
      validate: (record) => ({
        ...(!record.itemGroupId.trim()
          ? {
              itemGroupId: t('validation.required', {
                field: t('inventItemGroup.fields.id', 'Item group'),
              }),
            }
          : {}),
        ...(!record.name.trim()
          ? { name: t('validation.required', { field: t('inventItemGroup.fields.name', 'Name') }) }
          : {}),
      }),
      presentation: {
        mode: 'list',
        listWidth: 275,
        listResizable: true,
        recordHeaderMinHeight: 105,
      },
    }),
    [accounts.data, t]
  );
  return (
    <ListDetailsListGridPage title={t('inventItemGroup.title', 'Item groups')} config={config} />
  );
}
