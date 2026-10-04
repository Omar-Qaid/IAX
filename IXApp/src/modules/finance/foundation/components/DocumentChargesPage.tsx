import React from 'react';
import { Box, Stack, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { queryClient } from '@core/api/queryClient';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useNotifications } from '@shared/hooks/useNotifications';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { currencyApi } from '../api/currencyApi';
import { taxGroupApi } from '../api/taxGroupApi';
import { taxItemGroupApi } from '../api/taxItemGroupApi';
import { markupTransApi, type ChargeDocumentType, type ChargeLevel, type MarkupTransRecord } from '../api/markupTransApi';

export interface DocumentChargesPageProps {
  documentType: ChargeDocumentType;
  level?: ChargeLevel;
  documentRecId: number;
  documentNumber: string;
  documentName?: string;
  onBack: () => void;
  canEdit: boolean;
  onChanged?: () => Promise<unknown> | void;
  defaultCurrencyCode?: string;
  defaultTaxGroup?: string;
  defaultTaxItemGroup?: string;
}

const emptyCharge = (currencyCode: string, taxGroup = '', taxItemGroup = ''): MarkupTransRecord => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, markupCode: '', lineNum: 0,
  transDate: new Date().toISOString(), txt: '', voucher: '', markupCategory: 0,
  moduleType: 0, transRecId: 0, transTableId: 0, currencyCode, value: 0, calculatedAmount: 0,
  keep: 0, mcrBrokerContractFee: 0, taxGroup, taxItemGroup,
  intercompanyRefRecId: 0, intercompanyMarkupValue: 0,
});

export function DocumentChargesPage({ documentType, level = 'header', documentRecId, documentNumber, documentName, onBack, canEdit, onChanged, defaultCurrencyCode, defaultTaxGroup, defaultTaxItemGroup }: DocumentChargesPageProps): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  const sourceKey = `document-charges-${documentType}-${level}-${documentRecId}`;
  const queryKey = ['simple-list', sourceKey] as const;
  const codesQuery = useQuery({ queryKey: ['markup-codes', documentType], queryFn: ({ signal }) => markupTransApi.codes(documentType, signal) });
  const currenciesQuery = useQuery({ queryKey: ['currencies'], queryFn: ({ signal }) => currencyApi.list(signal) });
  const taxGroupsQuery = useQuery({ queryKey: ['tax-groups'], queryFn: ({ signal }) => taxGroupApi.list(signal) });
  const itemTaxGroupsQuery = useQuery({ queryKey: ['tax-item-groups'], queryFn: ({ signal }) => taxItemGroupApi.list(signal) });
  const codeOptions = (codesQuery.data ?? []).map((x) => ({ value: x.markupCode, label: x.markupCode }));
  const codeById = React.useMemo(() => new Map((codesQuery.data ?? []).map((x) => [x.markupCode, x])), [codesQuery.data]);
  const currencyOptions = (currenciesQuery.data ?? []).map((x) => ({ value: x.currencyCode, label: x.currencyCode }));
  const taxGroupOptions = (taxGroupsQuery.data ?? []).map((x) => ({ value: x.taxGroup, label: x.taxGroupName || x.taxGroup }));
  const taxItemGroupOptions = (itemTaxGroupsQuery.data ?? []).map((x) => ({ value: x.taxItemGroup, label: x.name || x.taxItemGroup }));
  const columns = React.useMemo<ColumnDef<MarkupTransRecord>[]>(() => [
    { field: 'markupCode', headerName: 'Charges code', width: 135, type: 'singleSelect', valueOptions: codeOptions, editable: true },
    { field: 'txt', headerName: t('fields.description', 'Description'), width: 190, editable: true },
    { field: 'markupCategory', headerName: 'Category', width: 150, type: 'singleSelect', valueOptions: [{ value: 0, label: 'Fixed' }, { value: 1, label: 'Pcs' }, { value: 2, label: 'Percentage' }], editable: true },
    { field: 'keep', headerName: 'Keep', width: 80, type: 'boolean', editable: true, valueGetter: ({ row }) => row.keep === 1 },
    { field: 'value', headerName: 'Charges value', width: 150, type: 'number', align: 'right', headerAlign: 'right', editable: true },
    { field: 'currencyCode', headerName: t('fields.currency', 'Currency'), width: 105, type: 'singleSelect', valueOptions: currencyOptions, editable: true },
    { field: 'mcrBrokerContractFee', headerName: 'Broker contract fee', width: 145, type: 'boolean', editable: true, valueGetter: ({ row }) => row.mcrBrokerContractFee === 1 },
    { field: 'taxGroup', headerName: 'Sales tax group', width: 170, type: 'singleSelect', valueOptions: taxGroupOptions, editable: true },
    { field: 'taxItemGroup', headerName: 'Item sales tax group', width: 180, type: 'singleSelect', valueOptions: taxItemGroupOptions, editable: true },
  ], [codeOptions, currencyOptions, taxGroupOptions, taxItemGroupOptions, t]);
  const refresh = () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<MarkupTransRecord> = {
    readOnly: !canEdit,
    contextLabel: `Maintain charges | ${level === 'line' ? 'Line | ' : ''}${documentNumber}${documentName ? ` : ${documentName}` : ''}`,
    viewLabel: t('common.standardView', 'Standard view'), filterLabel: t('actions.filter'),
    informationLabel: t('common.information'), searchMode: 'quick', locale: currentLanguage.code,
    searchFields: [
      { field: 'markupCode', label: 'Charges code' }, { field: 'txt', label: t('fields.description', 'Description') },
      { field: 'currencyCode', label: t('fields.currency', 'Currency') }, { field: 'taxGroup', label: 'Sales tax group' },
      { field: 'taxItemGroup', label: 'Item sales tax group' },
    ],
    backCommand: { label: t('actions.back', 'Back'), onClick: onBack },
    recordTableName: 'MarkupTrans', getAuditRecordId: (record) => record.recId,
    crud: {
      editLabel: t('actions.edit'), newLabel: t('actions.new'), deleteLabel: t('actions.delete'),
      onDelete: async (records) => {
        try {
          await Promise.all(records.map((record) => markupTransApi.delete(record)));
          await refresh();
          await onChanged?.();
          notifySuccess(t('messages.deletedSuccessfully'));
        } catch (error) {
          notifyError(error instanceof Error ? error.message : t('errors.deleteFailed'));
        }
      },
    },
    utilities: { personalizeLabel: t('utilities.personalize'), guideLabel: t('utilities.guide'), notificationsLabel: t('common.notifications'), refreshLabel: t('actions.refresh'), openWindowLabel: t('utilities.openWindow'), notificationCount: 0 },
    advancedFilter: {
      title: t('filters.title'), addLabel: t('actions.add'), fieldLabel: 'Charges code', operatorLabel: t('filters.contains'),
      applyLabel: t('actions.apply'), resetLabel: t('actions.reset'), getValue: (record) => record.markupCode,
      matches: (record, value) => `${record.markupCode} ${record.txt}`.toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
  };

  return <SimpleListPage
    title="Maintain charges" enterpriseConfig={config}
    dataSource={{ type: 'remote', key: sourceKey, load: (signal) => markupTransApi.list(documentType, documentRecId, level, signal) }}
    columns={columns} gridHeight={340} contentMinHeight={650}
    dataGridProps={{
      storageKey: `document-charges.${documentType}.simple-list.v1`, masterForm: true,
      rowHeight: 28, headerHeight: 28, hideToolbar: true, hideFooter: true,
      hideSidebarTabs: true, showColumnBorders: true, showCellBorders: true,
      onNewRow: () => emptyCharge(
        defaultCurrencyCode || (currencyOptions[0]?.value as string) || '',
        defaultTaxGroup,
        defaultTaxItemGroup
      ),
      onRowSave: async (partial, isNew) => {
        const record = partial as MarkupTransRecord;
        if (!record.markupCode?.trim()) throw new Error('Charges code is required.');
        record.keep = Number(record.keep ? 1 : 0);
        record.mcrBrokerContractFee = Number(record.mcrBrokerContractFee ? 1 : 0);
        const setup = codeById.get(record.markupCode);
        if (setup) {
          if (!record.txt) record.txt = setup.txt;
          if (!record.taxItemGroup) record.taxItemGroup = setup.taxItemGroup;
        }
        if (isNew || record.recId === 0) await markupTransApi.create(documentType, documentRecId, level, record);
        else await markupTransApi.update(record);
        await refresh();
        await onChanged?.();
        notifySuccess(t('messages.savedSuccessfully'));
      },
    }}
    belowGridContent={(selected) => <ChargeDetails selected={selected} />}
  />;
}

function ChargeDetails({ selected }: { selected: MarkupTransRecord | null }): React.ReactElement {
  return <Stack direction="row" spacing={8} sx={{ pt: 2.25, alignItems: 'flex-start' }}>
    <ChargeDetailsGroup title="Ledger" fields={[
      ['Voucher', selected?.voucher], ['Date', selected?.transDate ? selected.transDate.slice(0, 10) : ''],
      ['Amount in transaction currency', selected?.value?.toFixed(2)], ['Calculated value', selected?.calculatedAmount?.toFixed(2)],
    ]} />
    <ChargeDetailsGroup title="Intercompany" fields={[
      ['Reference', selected?.intercompanyRefRecId || ''], ['Intercompany amount', selected?.intercompanyMarkupValue?.toFixed(2)],
    ]} />
  </Stack>;
}

function ChargeDetailsGroup({ title, fields }: { title: string; fields: [string, unknown][] }): React.ReactElement {
  return <Box sx={{ minWidth: 180 }}>
    <Typography sx={{ fontSize: 11, fontWeight: 700, textTransform: 'uppercase', mb: 1 }}>{title}</Typography>
    <Stack spacing={1.1}>{fields.map(([label, value]) => <Box key={label}>
      <Typography sx={{ fontSize: 11, mb: 0.25 }}>{label}</Typography>
      <Box sx={{ minHeight: 25, minWidth: 135, border: '1px solid', borderColor: 'divider', px: 0.75, py: 0.35, fontSize: 12, textAlign: typeof value === 'string' && value.includes('.') ? 'right' : 'left' }}>{String(value ?? '')}</Box>
    </Box>)}</Stack>
  </Box>;
}
