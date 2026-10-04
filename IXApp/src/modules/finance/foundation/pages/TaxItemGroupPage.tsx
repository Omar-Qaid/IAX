import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ListDetailsListGridPage } from '@patterns/list-details-listgrid/ListDetailsListGridPage';
import type { DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { taxItemGroupApi, type TaxItemGroupRecord } from '../api/taxItemGroupApi';
import { taxTableApi } from '../api/taxTableApi';
import { TaxItemGroupLinesPanel } from '../components/TaxItemGroupLinesPanel';

const emptyItemGroup = (): TaxItemGroupRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  taxItemGroup: '',
  name: '',
  source: 0,
  euSalesListType: 1,
  lines: [],
});

export function TaxItemGroupPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const taxCodes = useQuery({
    queryKey: ['sales-tax-codes'],
    queryFn: ({ signal }) => taxTableApi.list(signal),
    staleTime: 300000,
  });
  const reportingOptions = useMemo(() => [
    { value: '0', label: t('taxItemGroup.options.notSpecified', 'Not specified') },
    { value: '1', label: t('taxItemGroup.options.item', 'Item') },
    { value: '2', label: t('taxItemGroup.options.service', 'Service') },
    { value: '3', label: t('taxItemGroup.options.triangulated', 'Triangulated') },
  ], [t]);
  const config = useMemo<EnterpriseListDetailsConfig<TaxItemGroupRecord>>(() => ({
    recordTableName: 'TaxItemGroupHeading',
    dataSource: {
      type: 'remote', key: 'item-sales-tax-groups', load: taxItemGroupApi.list,
      create: taxItemGroupApi.create, update: taxItemGroupApi.update, delete: taxItemGroupApi.delete,
    },
    createRecord: emptyItemGroup,
    getPrimaryText: (record) => record.taxItemGroup,
    getSecondaryText: (record) => record.name,
    matchesSearch: (record, query) => `${record.taxItemGroup} ${record.name}`.toLowerCase().includes(query.toLowerCase()),
    getValues: (record): DetailValues => ({}),
    setValues: (record) => record,
    headerFields: [
      {
        id: 'taxItemGroup', label: t('taxItemGroup.fields.group', 'Item sales tax group'),
        getValue: (record) => record.taxItemGroup,
        setValue: (record, value) => record.recId ? record : { ...record, taxItemGroup: String(value).toUpperCase() },
      },
      {
        id: 'name', label: t('taxItemGroup.fields.description', 'Description'),
        getValue: (record) => record.name,
        setValue: (record, value) => ({ ...record, name: String(value) }),
      },
      {
        id: 'euSalesListType', label: t('taxItemGroup.fields.reportingType', 'Reporting type'),
        type: 'select', options: reportingOptions,
        getValue: (record) => String(record.euSalesListType),
        setValue: (record, value) => ({ ...record, euSalesListType: Number(value) || 0 }),
      },
    ],
    sections: ({ record, editing, onRecordChange }) => [{
      id: 'setup',
      title: t('taxItemGroup.sections.setup', 'Setup'),
      defaultExpanded: true,
      detailsPadding: '8px 10px 12px',
      content: (
        <TaxItemGroupLinesPanel
          record={record}
          editing={editing}
          taxCodes={taxCodes.data ?? []}
          onRecordChange={onRecordChange}
        />
      ),
    }],
    permissions: {
      view: PERMISSIONS.TAX_ITEM_GROUP_VIEW,
      create: PERMISSIONS.TAX_ITEM_GROUP_CREATE,
      edit: PERMISSIONS.TAX_ITEM_GROUP_EDIT,
      delete: PERMISSIONS.TAX_ITEM_GROUP_DELETE,
    },
    validate: (record) => ({
      ...(!record.taxItemGroup.trim()
        ? { taxItemGroup: t('validation.required', { field: t('taxItemGroup.fields.group', 'Item sales tax group') }) }
        : {}),
      ...(!record.name.trim()
        ? { name: t('validation.required', { field: t('taxItemGroup.fields.description', 'Description') }) }
        : {}),
    }),
    advancedFilter: {
      fieldLabel: t('taxItemGroup.fields.group', 'Item sales tax group'),
      getValue: (record) => record.taxItemGroup,
      matches: (record, value) => `${record.taxItemGroup} ${record.name}`.toLowerCase().includes(value.trim().toLowerCase()),
    },
    presentation: { mode: 'list', listWidth: 240, listResizable: true, recordHeaderMinHeight: 96 },
  }), [reportingOptions, t, taxCodes.data]);

  return <ListDetailsListGridPage title={t('taxItemGroup.title', 'Item sales tax groups')} config={config} />;
}
