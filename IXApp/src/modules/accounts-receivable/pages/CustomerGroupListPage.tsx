import React, { useMemo } from 'react';
import { Link, Typography } from '@mui/material';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { MOCK_CUSTOMER_GROUPS, type CustomerGroup } from '@mocks/data/customerGroups';

export function CustomerGroupListPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const columns = useMemo<ColumnDef<CustomerGroup>[]>(() => [
    { field: 'groupId', headerName: 'fields.customerGroup', width: 145, pinned: 'left', renderCell: ({ row }) => <Link component="button" underline="none" sx={{ color: 'primary.main', fontSize: '0.75rem' }}>{row.groupId}</Link> },
    { field: 'description', headerName: 'fields.description', width: 205, renderCell: ({ row }) => row.name },
    { field: 'paymentTerms', headerName: 'fields.termsOfPayment', width: 205 },
    { field: 'invoiceDueInterval', headerName: 'fields.timeBetweenInvoiceDue', width: 210, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'defaultTaxGroup', headerName: 'fields.defaultTaxGroup', width: 150, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'pricesIncludeTax', headerName: 'fields.pricesIncludeTax', width: 130, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'defaultWriteOffReason', headerName: 'fields.defaultWriteOffReason', width: 190, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'accountingCurrencyExchange', headerName: 'fields.accountingCurrencyExchange', width: 190, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'reportingCurrencyExchange', headerName: 'fields.reportingCurrencyExchange', width: 190, sortable: false, filterable: false, valueGetter: () => '—' },
  ], [currentLanguage.code]);

  const config: EnterpriseListConfig<CustomerGroup> = {
    contextLabel: t('pages.customerGroups.title'),
    viewLabel: t('pages.customerGroups.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchMode: 'quick',
    searchFields: [
      { field: 'groupId', label: t('fields.groupId') },
      { field: 'name', label: t('fields.name') },
      { field: 'description', label: t('fields.description') },
      { field: 'paymentTerms', label: t('fields.paymentTerms') },
    ],
    locale: currentLanguage.code,
    crud: { editLabel: t('actions.edit'), newLabel: t('actions.new'), deleteLabel: t('actions.delete') },
    commands: ['setup', 'forecast', 'productFilters', 'options'].map((id) => ({ id, label: t(`customerGroupCommands.${id}`) })),
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
      matches: (group, value) => group.groupId.toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
    relatedInformation: {
      title: t('relatedInformation.title'),
      sections: (group) => [
        {
          id: 'groupDetails',
          label: t('fields.description'),
          defaultExpanded: true,
          content: <Typography sx={{ fontSize: '0.75rem', color: 'text.secondary', whiteSpace: 'pre-line' }}>{group?.description ?? t('common.notProvided')}</Typography>,
        },
        { id: 'relationships', label: t('relatedInformation.relationships') },
        { id: 'statistics', label: t('relatedInformation.statistics') },
        { id: 'creditStatistics', label: t('relatedInformation.creditStatistics') },
        { id: 'contacts', label: t('relatedInformation.contacts') },
        { id: 'classificationBalances', label: t('relatedInformation.classificationBalances') },
      ],
    },
  };

  return <SimpleListPage
    variant="enterprise"
    title={t('pages.customerGroups.title')}
    enterpriseConfig={config}
    dataGridProps={{ rows: MOCK_CUSTOMER_GROUPS, columns, storageKey: 'accounts-receivable.customer-groups.reference-view', hideSidebar: true }}
  />;
}
