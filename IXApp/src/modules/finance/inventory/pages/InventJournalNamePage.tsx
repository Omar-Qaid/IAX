import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { inventJournalNameApi, type InventJournalNameRecord } from '../api/inventJournalNameApi';

const emptyRecord = (): InventJournalNameRecord => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, journalNameId: '', description: '', journalType: 0,
  voucherNumberSequenceTable: 0, voucherNumberSequenceCode: '', detailSummary: 0,
  deletePostedLines: false, reservation: 0,
});

export function InventJournalNamePage(): React.ReactElement {
  const { t } = useAppTranslation();
  const lookups = useQuery({ queryKey: ['inventory-journal-name-lookups'], queryFn: ({ signal }) => inventJournalNameApi.lookups(signal) });
  const config = useMemo<EnterpriseListDetailsConfig<InventJournalNameRecord>>(() => ({
    recordTableName: 'InventJournalName',
    dataSource: { type: 'remote', key: 'inventory-journal-names', load: inventJournalNameApi.list,
      create: inventJournalNameApi.create, update: inventJournalNameApi.update, delete: inventJournalNameApi.delete },
    createRecord: emptyRecord,
    getPrimaryText: (x) => x.journalNameId,
    getSecondaryText: (x) => x.description,
    matchesSearch: (x, query) => `${x.journalNameId} ${x.description}`.toLowerCase().includes(query.toLowerCase()),
    getValues: (x): DetailValues => ({ journalType: String(x.journalType), voucherNumberSequenceTable: String(x.voucherNumberSequenceTable),
      detailSummary: String(x.detailSummary), deletePostedLines: x.deletePostedLines, reservation: String(x.reservation) }),
    setValues: (x, values) => ({ ...x, journalType: Number(values.journalType) || 0,
      voucherNumberSequenceTable: Number(values.voucherNumberSequenceTable) || 0,
      detailSummary: Number(values.detailSummary) || 0, deletePostedLines: values.deletePostedLines === true,
      reservation: Number(values.reservation) || 0 }),
    headerFields: [
      { id: 'journalNameId', label: t('inventJournalName.fields.id', 'Name'), getValue: (x) => x.journalNameId,
        setValue: (x, value) => ({ ...x, journalNameId: String(value).toUpperCase() }) },
      { id: 'description', label: t('inventJournalName.fields.description', 'Description'), getValue: (x) => x.description,
        setValue: (x, value) => ({ ...x, description: String(value) }) },
    ],
    sections: [{ id: 'general', title: t('common.general', 'General'), defaultExpanded: true, columns: 2, groups: [
      { id: 'journal', fields: [
        { name: 'journalType', label: t('inventJournalName.fields.type', 'Journal type'), type: 'select', options: [
          { value: '0', label: t('inventJournalName.types.movement', 'Movement') },
          { value: '1', label: t('inventJournalName.types.adjustment', 'Inventory adjustment') },
          { value: '2', label: t('inventJournalName.types.counting', 'Counting') },
          { value: '3', label: t('inventJournalName.types.transfer', 'Transfer') }] },
        { name: 'voucherNumberSequenceTable', label: t('inventJournalName.fields.voucherSequence', 'Voucher number sequence'), type: 'select',
          options: [{ value: '0', label: t('common.none', 'None') }, ...(lookups.data?.numberSequences ?? []).map((x) => ({ value: String(x.recId), label: `${x.numberSequence} - ${x.description}` }))] },
      ] },
      { id: 'posting', fields: [
        { name: 'detailSummary', label: t('inventJournalName.fields.detailSummary', 'Detail level'), type: 'select', options: [
          { value: '0', label: t('inventJournalName.detail.detail', 'Detail') }, { value: '1', label: t('inventJournalName.detail.summary', 'Summary') }] },
        { name: 'deletePostedLines', label: t('inventJournalName.fields.deletePostedLines', 'Delete lines after posting'), type: 'boolean' },
        { name: 'reservation', label: t('inventJournalName.fields.reservation', 'Reservation'), type: 'select', options: [
          { value: '0', label: t('common.none', 'None') }, { value: '1', label: t('inventJournalName.reservation.automatic', 'Automatic') }] },
      ] },
    ] }],
    permissions: { view: PERMISSIONS.INVENTORY_TRANSACTION_VIEW, create: PERMISSIONS.INVENTORY_TRANSACTION_CREATE,
      edit: PERMISSIONS.INVENTORY_TRANSACTION_EDIT, delete: PERMISSIONS.INVENTORY_TRANSACTION_DELETE },
    validate: (x) => ({ ...(!x.journalNameId.trim() ? { journalNameId: t('validation.required', { field: t('inventJournalName.fields.id', 'Name') }) } : {}),
      ...(!x.description.trim() ? { description: t('validation.required', { field: t('inventJournalName.fields.description', 'Description') }) } : {}) }),
    presentation: { mode: 'list', listWidth: 275, listResizable: true },
  }), [lookups.data, t]);
  return <ListDetailsPage variant="enterprise" title={t('inventJournalName.title', 'Inventory journal names')} config={config} />;
}
