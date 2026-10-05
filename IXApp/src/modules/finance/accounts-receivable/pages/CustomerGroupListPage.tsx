import React, { useMemo } from 'react';
import { Link } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { customerGroupApi, newCustomerGroup, type CustomerGroupRecord } from '../api/customerGroupApi';
import { customerQuickCreateApi } from '../api/customerQuickCreateApi';

const groupQueryKey = ['accounts-receivable', 'customer-groups'] as const;

export function CustomerGroupListPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const queryClient = useQueryClient();
  const groupsQuery = useQuery({ queryKey: groupQueryKey, queryFn: ({ signal }) => customerGroupApi.list(signal) });
  const lookupsQuery = useQuery({
    queryKey: ['accounts-receivable', 'customer-groups', 'field-lookups'],
    queryFn: ({ signal }) => customerQuickCreateApi.groupFieldLookups(signal),
    staleTime: 5 * 60 * 1000,
  });
  const paymentOptions = lookupsQuery.data?.paymentTerms ?? [];
  const taxOptions = lookupsQuery.data?.salesTaxGroups ?? [];
  const columns = useMemo<ColumnDef<CustomerGroupRecord>[]>(() => [
    { field: 'custGroupId', headerName: 'fields.customerGroup', width: 145, pinned: 'left', editable: true, renderCell: ({ row }) => <Link component="button" underline="none" sx={{ color: 'primary.main', fontSize: '0.75rem' }}>{row.custGroupId}</Link> },
    { field: 'name', headerName: 'fields.description', width: 205, editable: true },
    { field: 'paymTermId', headerName: 'fields.termsOfPayment', width: 205, editable: true, type: 'singleSelect', valueOptions: paymentOptions },
    { field: 'invoiceDueInterval', headerName: 'fields.timeBetweenInvoiceDue', width: 210, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'taxGroupId', headerName: 'fields.salesTaxGroup', width: 150, editable: true, type: 'singleSelect', valueOptions: taxOptions },
    { field: 'priceIncludeSalesTax', headerName: 'fields.pricesIncludeTax', width: 130, type: 'boolean', valueGetter: ({ row }) => row.priceIncludeSalesTax === 1 },
    { field: 'defaultWriteOffReason', headerName: 'fields.defaultWriteOffReason', width: 190, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'accountingCurrencyExchange', headerName: 'fields.accountingCurrencyExchange', width: 190, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'reportingCurrencyExchange', headerName: 'fields.reportingCurrencyExchange', width: 190, sortable: false, filterable: false, valueGetter: () => '—' },
  ], [paymentOptions, taxOptions]);

  const config: EnterpriseListConfig<CustomerGroupRecord> = {
    contextLabel: t('pages.customerGroups.title'),
    viewLabel: t('pages.customerGroups.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchMode: 'quick',
    searchFields: [
      { field: 'custGroupId', label: t('fields.groupId') },
      { field: 'name', label: t('fields.name') },
      { field: 'paymTermId', label: t('fields.paymentTerms') },
      { field: 'taxGroupId', label: t('fields.salesTaxGroup') },
    ],
    locale: currentLanguage.code,
    crud: {
      editLabel: t('actions.edit'), newLabel: t('actions.new'), deleteLabel: t('actions.delete'),
      onDelete: async (rows) => {
        for (const row of rows) await customerGroupApi.delete(row);
        await queryClient.invalidateQueries({ queryKey: groupQueryKey });
        await queryClient.invalidateQueries({ queryKey: ['accounts-receivable', 'customer-quick-create', 'lookups'] });
      },
    },
    commands: ['setup', 'forecast', 'productFilters'].map((id) => ({ id, label: t(`customerGroupCommands.${id}`) })),
    utilities: {
      personalizeLabel: t('utilities.personalize'), guideLabel: t('utilities.guide'), notificationsLabel: t('common.notifications'),
      refreshLabel: t('actions.refresh'), openWindowLabel: t('utilities.openWindow'), notificationCount: 0,
    },
    advancedFilter: {
      title: t('filters.title'),
      addLabel: t('actions.add'),
      fieldLabel: t('fields.customerGroup'),
      operatorLabel: t('filters.contains'),
      applyLabel: t('actions.apply'),
      resetLabel: t('actions.reset'),
      getValue: (group) => group.custGroupId,
      matches: (group, value) => group.custGroupId.toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
  };

  return <SimpleListPage
    title={t('pages.customerGroups.title')}
    enterpriseConfig={config}
    dataSource={{
      type: 'controlled', rows: groupsQuery.data ?? [], loading: groupsQuery.isLoading || lookupsQuery.isLoading,
      error: groupsQuery.error instanceof Error ? groupsQuery.error.message
        : lookupsQuery.error instanceof Error ? lookupsQuery.error.message : null,
      refresh: () => { void groupsQuery.refetch(); void lookupsQuery.refetch(); },
    }}
    columns={columns}
    dataGridProps={{
      storageKey: 'accounts-receivable.customer-groups.reference-view',
      hideSidebar: false,
      masterForm: true,
      onNewRow: newCustomerGroup,
      onRowSave: async (values, isNew) => {
        const record = values as CustomerGroupRecord;
        if (isNew) await customerGroupApi.create(record);
        else await customerGroupApi.update(record);
        await queryClient.invalidateQueries({ queryKey: groupQueryKey });
        await queryClient.invalidateQueries({ queryKey: ['accounts-receivable', 'customer-quick-create', 'lookups'] });
      },
    }}
  />;
}
